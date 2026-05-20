using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace CODEX.Editor
{
    /// <summary>
    /// Configura T02_Disparo: reposiciona objetos existentes y agrega lo que falta.
    /// Menú: CODEX > Tutorial > Setup T02 - Disparo
    /// </summary>
    public static class T02_Setup
    {
        // Mismas constantes que T01
        private const float GroundY      = -3f;
        private const float FloorY       = -1.5f;   // donde pisan los pies del jugador
        private const float PlatformH    = 0.4f;

        private static readonly Color ColGround   = new Color(0.10f, 0.16f, 0.30f);
        private static readonly Color ColPlatform = new Color(0.15f, 0.25f, 0.50f);
        private static readonly Color ColDoor     = new Color(0.20f, 0.10f, 0.40f);  // puerta bloqueada: violeta oscuro
        private static readonly Color ColNeonBlue = new Color(0.20f, 0.70f, 1.00f);
        private static readonly Color ColNeonCyan = new Color(0.00f, 1.00f, 0.90f);

        [MenuItem("CODEX/Tutorial/Setup T02 - Disparo")]
        public static void Setup()
        {
            var scene = EditorSceneManager.OpenScene(
                "Assets/Scenes/Tutorial/T02_Disparo.unity", OpenSceneMode.Single);

            // ── 1. Puerta invisible (solo collider, con mensaje de LUMA) ──
            var blockedDoor = GameObject.Find("BlockedDoor");
            if (blockedDoor == null)
            {
                blockedDoor      = new GameObject("BlockedDoor");
                blockedDoor.layer = LayerMask.NameToLayer("Default");
            }
            blockedDoor.transform.position   = new Vector3(7f, 0f, 0f);
            blockedDoor.transform.localScale = new Vector3(0.5f, 8f, 1f);

            // Quitar SpriteRenderer si existe (la puerta es invisible)
            var oldSR = blockedDoor.GetComponent<SpriteRenderer>();
            if (oldSR != null) UnityEngine.Object.DestroyImmediate(oldSR);

            // Asegurar BoxCollider2D sólido
            var doorCol = blockedDoor.GetComponent<BoxCollider2D>();
            if (doorCol == null) doorCol = blockedDoor.AddComponent<BoxCollider2D>();
            doorCol.isTrigger = false;

            // InvisibleDoorBlocker
            var blocker = blockedDoor.GetComponent<CODEX.Tutorial.InvisibleDoorBlocker>()
                       ?? blockedDoor.AddComponent<CODEX.Tutorial.InvisibleDoorBlocker>();

            // ── 2. Reposicionar OpenDoor (trigger de salida a T03) ────────
            var openDoor = GameObject.Find("OpenDoor_T02_to_T03");
            if (openDoor != null)
            {
                // Lo movemos al extremo derecho como zona de salida
                openDoor.transform.position   = new Vector3(16f, 0f, 0f);
                openDoor.transform.localScale = new Vector3(3f, 10f, 1f);
            }

            // ── 3. Reposicionar SpawnPoint ────────────────────────────────
            var spawnPoint = GameObject.Find("SpawnPoint");
            if (spawnPoint != null)
                spawnPoint.transform.position = new Vector3(-7f, FloorY + 0.5f, 0f);

            // ── 4. Reposicionar LUMA junto al jugador ─────────────────────
            var luma = GameObject.Find("LUMA_Guide");
            if (luma != null)
                luma.transform.position = new Vector3(-4f, FloorY + 2f, 0f);

            // ── 5. Suelo continuo ─────────────────────────────────────────
            if (GameObject.Find("Suelo_T02") == null)
            {
                CreateBlock("Suelo_T02",
                    new Vector3(4f, GroundY - 0.5f, 0f),
                    new Vector3(32f, 1f, 1f),
                    ColGround, "ground");
            }

            // ── 6. Suelo zona de salida (después de la puerta) ────────────
            if (GameObject.Find("Suelo_Salida") == null)
            {
                CreateBlock("Suelo_Salida",
                    new Vector3(12f, FloorY - 0.2f, 0f),
                    new Vector3(8f, PlatformH, 1f),
                    ColPlatform, "ground");
            }

            // Pared izquierda y derecha
            if (GameObject.Find("Pared_Izquierda_T02") == null)
                CreateBlock("Pared_Izquierda_T02", new Vector3(-10f, 0f, 0f), new Vector3(1f, 20f, 1f), ColGround, "");
            if (GameObject.Find("Pared_Derecha_T02") == null)
                CreateBlock("Pared_Derecha_T02", new Vector3(20f, 0f, 0f), new Vector3(1f, 20f, 1f), ColGround, "");

            // ── 7. Instanciar el jugador ──────────────────────────────────
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/CODIGO7.prefab");
            var existingPlayer = GameObject.FindGameObjectWithTag("Player");

            if (existingPlayer == null && playerPrefab != null)
            {
                var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                Vector3 spawnPos = spawnPoint != null
                    ? spawnPoint.transform.position
                    : new Vector3(-7f, FloorY + 0.5f, 0f);
                player.transform.position = spawnPos;

                // Conectar CameraFollow
                var camFollow = Camera.main?.GetComponent<CODEX.Systems.CameraFollow>();
                if (camFollow != null)
                {
                    var so = new SerializedObject(camFollow);
                    var t = so.FindProperty("target");
                    if (t != null) { t.objectReferenceValue = player.transform; so.ApplyModifiedProperties(); }
                }
            }

            // ── 8. Instanciar enemigo estático frente a la puerta ────────
            var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/Tutorial/Enemies/InfectedFile_Basic.prefab");

            GameObject enemyGO = GameObject.Find("InfectedFile_Bloqueo");
            if (enemyPrefab != null && enemyGO == null)
            {
                enemyGO = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab);
                enemyGO.name = "InfectedFile_Bloqueo";
            }
            if (enemyGO != null)
            {
                enemyGO.transform.position = new Vector3(5f, FloorY + 0.5f, 0f);

                // Asegurar EnemyAnimator
                if (enemyGO.GetComponent<CODEX.Enemies.EnemyAnimator>() == null)
                    enemyGO.AddComponent<CODEX.Enemies.EnemyAnimator>();

                var infectedFile = enemyGO.GetComponent<CODEX.Enemies.InfectedFile>();

                // Conectar InvisibleDoorBlocker al enemigo y a LUMA
                if (blocker != null && infectedFile != null)
                {
                    var blockerSO = new SerializedObject(blocker);
                    blockerSO.FindProperty("targetEnemy").objectReferenceValue = infectedFile;
                    if (luma != null)
                        blockerSO.FindProperty("luma").objectReferenceValue =
                            luma.GetComponent<CODEX.Tutorial.LumaGuide>();
                    blockerSO.ApplyModifiedProperties();
                }

                // Conectar al BlockManager
                var blockMgr = GameObject.Find("BlockManager");
                if (blockMgr != null)
                {
                    var block02 = blockMgr.GetComponent<CODEX.Tutorial.Blocks.Block02_Disparo>();
                    if (block02 != null)
                    {
                        var so = new SerializedObject(block02);
                        so.FindProperty("blockerEnemy").objectReferenceValue = infectedFile;
                        so.ApplyModifiedProperties();
                    }
                }
            }

            // ── 9. Paneles de luz decorativos ─────────────────────────────
            float[] panelX = { -5f, 0f, 9f, 13f };
            for (int i = 0; i < panelX.Length; i++)
            {
                string pName = $"Panel_Neon_T02_{i}";
                if (GameObject.Find(pName) != null) continue;

                var panel = CreateDecoration(pName,
                    new Vector3(panelX[i], 4f, 0f),
                    new Vector3(0.15f, 3.5f, 1f),
                    i % 2 == 0 ? ColNeonBlue : ColNeonCyan);

                var lightGO = new GameObject("Light");
                lightGO.transform.SetParent(panel.transform);
                lightGO.transform.localPosition = Vector3.zero;
                var l = lightGO.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
                l.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point;
                l.color = ColNeonBlue;
                l.pointLightOuterRadius = 3f;
                l.intensity = 0.8f;
            }

            // ── 10. Ajustar cámara ────────────────────────────────────────
            var cam = Camera.main;
            if (cam != null)
            {
                cam.orthographicSize = 6f;
                cam.transform.position = new Vector3(0f, 0f, -10f);
                var cf = cam.GetComponent<CODEX.Systems.CameraFollow>();
                if (cf != null)
                {
                    var so = new SerializedObject(cf);
                    so.FindProperty("minBounds").vector2Value = new Vector2(-10f, -5f);
                    so.FindProperty("maxBounds").vector2Value = new Vector2(20f, 8f);
                    so.ApplyModifiedProperties();
                }
            }

            // ── 11. Asegurarse que BlockedDoor conectado en BlockManager ──
            var bm = GameObject.Find("BlockManager");
            if (bm != null && blockedDoor != null && openDoor != null)
            {
                var b02 = bm.GetComponent<CODEX.Tutorial.Blocks.Block02_Disparo>();
                if (b02 != null)
                {
                    var so = new SerializedObject(b02);
                    so.FindProperty("blockedDoor").objectReferenceValue = blockedDoor;
                    so.FindProperty("openDoor").objectReferenceValue    = openDoor;
                    if (luma != null)
                        so.FindProperty("luma").objectReferenceValue =
                            luma.GetComponent<CODEX.Tutorial.LumaGuide>();
                    so.ApplyModifiedProperties();
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("T02 lista",
                "T02_Disparo configurada:\n\n" +
                "✓ Suelo y paredes añadidos\n" +
                "✓ Puerta invisible (InvisibleDoorBlocker) en x=7\n" +
                "  → LUMA avisa si el jugador intenta pasar\n" +
                "  → Se abre automáticamente al matar al enemigo\n" +
                "✓ Trigger de salida (x=16)\n" +
                "✓ Enemigo InfectedFile_Bloqueo en x=5 (con EnemyAnimator)\n" +
                "✓ Jugador instanciado con cámara conectada\n" +
                "✓ BlockManager con referencias actualizadas\n\n" +
                "PENDIENTE:\n" +
                "• Ejecuta CODEX > Tutorial > Setup T02 - Animaciones\n" +
                "  para crear el AnimatorController del enemigo.",
                "OK");
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private static GameObject CreateBlock(string name, Vector3 pos, Vector3 scale,
                                              Color color, string layerName)
        {
            var go = new GameObject(name);
            go.transform.position   = pos;
            go.transform.localScale = scale;

            if (!string.IsNullOrEmpty(layerName))
            {
                int idx = LayerMask.NameToLayer(layerName);
                if (idx >= 0) go.layer = idx;
            }

            var sr  = go.AddComponent<SpriteRenderer>();
            sr.sprite = CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
            sr.color  = color;

            go.AddComponent<BoxCollider2D>();
            return go;
        }

        private static GameObject CreateDecoration(string name, Vector3 pos, Vector3 scale, Color color)
        {
            var go = new GameObject(name);
            go.transform.position   = pos;
            go.transform.localScale = scale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
            sr.color        = color;
            sr.sortingOrder = -1;
            return go;
        }
    }
}
