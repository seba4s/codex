using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace CODEX.Editor
{
    /// <summary>
    /// Pobla T01_Materializacion con el contenido completo del Bloque 1.
    /// Menú: CODEX > Tutorial > Setup Bloque 1 - Materialización
    /// </summary>
    public static class Block01_Setup
    {
        // Constantes de layout — ajusta si el jugador (Scale Y:3) lo requiere
        private const float GroundY      = -3f;
        private const float PlayerFloorY = -1.5f;   // pies del jugador sobre el suelo
        private const float PlatformH   = 0.4f;     // grosor de cada plataforma

        // Colores del mundo digital
        private static readonly Color ColGround    = new Color(0.10f, 0.16f, 0.30f);
        private static readonly Color ColPlatform  = new Color(0.15f, 0.25f, 0.50f);
        private static readonly Color ColWallPanel = new Color(0.05f, 0.08f, 0.20f);
        private static readonly Color ColNeonBlue  = new Color(0.20f, 0.70f, 1.00f);
        private static readonly Color ColNeonCyan  = new Color(0.00f, 1.00f, 0.90f);

        [MenuItem("CODEX/Tutorial/Setup Bloque 1 - Materialización")]
        public static void SetupBlock01()
        {
            // Abrir la escena
            string scenePath = "Assets/Scenes/Tutorial/T01_Materializacion.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Limpiar contenedor de contenido anterior
            var oldContent = GameObject.Find("Contenido_Bloque1");
            if (oldContent != null) Object.DestroyImmediate(oldContent);

            var root = new GameObject("Contenido_Bloque1");

            // ════════════════════════════════════
            //  FONDO DE ESCENA
            // ════════════════════════════════════
            var bgGO = new GameObject("Fondo");
            bgGO.transform.SetParent(root.transform);
            bgGO.transform.position = new Vector3(4f, 0f, 1f);  // z=1 → detrás de todo

            var bgSR = bgGO.AddComponent<SpriteRenderer>();
            bgSR.sortingOrder = -10;

            var fondo1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Tutorial/fondo1.png");
            if (fondo1 != null)
            {
                bgSR.sprite = fondo1;
                bgSR.color  = Color.white;
                // fondo1.png es 1536x1024 a 100 ppu = 15.36 x 10.24 unidades
                // La cámara tiene orthographicSize=6 → visible 12 unidades de alto
                // Escalamos para cubrir la cámara
                bgGO.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            }
            else
            {
                bgSR.sprite = CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
                bgSR.color  = new Color(0.05f, 0.05f, 0.12f);
                bgGO.transform.localScale = new Vector3(35f, 15f, 1f);
                Debug.LogWarning("[Block01_Setup] fondo1.png no encontrado. " +
                                 "Ejecuta CODEX > Configurar Sprites primero.");
            }

            // ════════════════════════════════════
            //  SUELO Y PAREDES (reemplazar el base)
            // ════════════════════════════════════

            // Suelo continuo
            CreateBlock(root, "Suelo_Principal",
                new Vector3(4f, GroundY - 0.5f, 0f),
                new Vector3(50f, 1f),
                ColGround, "ground");

            // Abismo al final (sin suelo entre x=18 y x=22) → para el SceneLoader
            // Panel izquierdo de la pared (decoración)
            CreateBlock(root, "Pared_Izquierda",
                new Vector3(-10f, 0f, 0f),
                new Vector3(1f, 20f),
                ColWallPanel, "");
            CreateBlock(root, "Pared_Derecha",
                new Vector3(20f, 0f, 0f),
                new Vector3(1f, 20f),
                ColWallPanel, "");

            // Techo decorativo (sin collider)
            CreateDecoration(root, "Techo",
                new Vector3(5f, 6f, 0f),
                new Vector3(32f, 0.5f),
                ColWallPanel);

            // ════════════════════════════════════
            //  PANELES NEON (decoración lateral)
            // ════════════════════════════════════
            float[] panelX = { -6f, -3f, 3f, 8f, 13f };
            for (int i = 0; i < panelX.Length; i++)
            {
                var panel = CreateDecoration(root, $"Panel_Neon_{i}",
                    new Vector3(panelX[i], 4f, 0f),
                    new Vector3(0.15f, 3.5f),
                    i % 2 == 0 ? ColNeonBlue : ColNeonCyan);
                // Luz puntual pequeña sobre el panel
                AddPointLight(panel, ColNeonBlue, 3f, 0.8f);
            }

            // ════════════════════════════════════
            //  PLATAFORMA DE INICIO (zona segura)
            // ════════════════════════════════════
            // Plataforma amplia donde CODIGO-7 materializa
            CreateBlock(root, "Plataforma_Inicio",
                new Vector3(-6f, PlayerFloorY - 0.2f, 0f),
                new Vector3(6f, PlatformH),
                ColPlatform, "ground");

            // ════════════════════════════════════
            //  PRIMER SALTO OBLIGATORIO
            //  "a 3 metros hay una plataforma elevada de 1 bloque"
            // ════════════════════════════════════
            // Hueco en el suelo entre x=-3 y x=0 (fuerza el salto)
            CreateBlock(root, "Plataforma_Salto1",
                new Vector3(1.5f, PlayerFloorY + 0.8f, 0f),   // 0.8 más alta que el suelo
                new Vector3(3f, PlatformH),
                ColPlatform, "ground");

            // Flecha decorativa apuntando a la plataforma (sprite temporal con texto)
            // (se reemplazará por asset gráfico)

            // ════════════════════════════════════
            //  DOS PLATAFORMAS ESCALONADAS
            //  "confirmación de que el jugador entendió el salto"
            // ════════════════════════════════════
            CreateBlock(root, "Plataforma_Escalon2",
                new Vector3(6f, PlayerFloorY + 1.6f, 0f),   // escalón medio
                new Vector3(2.5f, PlatformH),
                ColPlatform, "ground");

            CreateBlock(root, "Plataforma_Escalon3",
                new Vector3(10f, PlayerFloorY + 2.4f, 0f),  // escalón alto
                new Vector3(2.5f, PlatformH),
                ColPlatform, "ground");

            // ════════════════════════════════════
            //  ZONA SEGURA AL OTRO LADO
            // ════════════════════════════════════
            CreateBlock(root, "Zona_Segura",
                new Vector3(15f, PlayerFloorY + 2.4f, 0f),
                new Vector3(8f, PlatformH),
                ColPlatform, "ground");

            // Checkpoint aquí
            var cpTrigger = new GameObject("Checkpoint_PostSaltos");
            cpTrigger.transform.SetParent(root.transform);
            cpTrigger.transform.position = new Vector3(13f, PlayerFloorY + 3.5f, 0f);
            cpTrigger.transform.localScale = new Vector3(1.5f, 3f, 1f);
            var cpCol = cpTrigger.AddComponent<BoxCollider2D>();
            cpCol.isTrigger = true;
            var spawnPt = new GameObject("SpawnPoint_PostSaltos");
            spawnPt.transform.SetParent(cpTrigger.transform);
            spawnPt.transform.position = new Vector3(13f, PlayerFloorY + 3.8f, 0f);
            var cpComp = cpTrigger.AddComponent<CODEX.Tutorial.CheckpointTrigger>();
            var cpSO = new SerializedObject(cpComp);
            cpSO.FindProperty("spawnPoint").objectReferenceValue = spawnPt.transform;
            cpSO.ApplyModifiedProperties();

            // ════════════════════════════════════
            //  PASILLO DE SALIDA (hacia T02)
            // ════════════════════════════════════
            CreateBlock(root, "Pasillo_Salida",
                new Vector3(21f, PlayerFloorY + 2.4f, 0f),
                new Vector3(4f, PlatformH),
                ColPlatform, "ground");

            // ════════════════════════════════════
            //  SPAWN DEL JUGADOR
            // ════════════════════════════════════
            var existingSpawn = GameObject.Find("PlayerSpawn");
            if (existingSpawn != null)
                existingSpawn.transform.position = new Vector3(-7f, PlayerFloorY + 0.5f, 0f);

            // Instanciar el prefab del jugador si existe
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/CODIGO7.prefab");
            if (playerPrefab != null)
            {
                // Eliminar jugador anterior si existe
                var oldPlayer = GameObject.FindGameObjectWithTag("Player");
                if (oldPlayer != null) Object.DestroyImmediate(oldPlayer);

                var spawnPos = existingSpawn != null
                    ? existingSpawn.transform.position
                    : new Vector3(-7f, PlayerFloorY + 0.5f, 0f);

                var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                player.transform.position = spawnPos;

                // Conectar CameraFollow al jugador
                var camFollow = Camera.main?.GetComponent<CODEX.Systems.CameraFollow>();
                if (camFollow != null)
                {
                    var cfSO = new SerializedObject(camFollow);
                    var targetProp = cfSO.FindProperty("target");
                    if (targetProp != null)
                    {
                        targetProp.objectReferenceValue = player.transform;
                        cfSO.ApplyModifiedProperties();
                    }
                }
            }
            else
            {
                Debug.LogWarning("[Block01_Setup] No se encontró Assets/Prefabs/Player/CODIGO7.prefab — " +
                                 "ejecuta primero CODEX > Crear Prefabs del Jugador.");
            }

            // ════════════════════════════════════
            //  POSICIONAR LUMA JUNTO AL JUGADOR
            // ════════════════════════════════════
            var luma = GameObject.Find("LUMA");
            if (luma != null)
                luma.transform.position = new Vector3(-5f, PlayerFloorY + 2.5f, 0f);

            // ════════════════════════════════════
            //  GESTOR DEL BLOQUE (crearlo ANTES de la UI para que SetupBootScreenUI lo encuentre)
            // ════════════════════════════════════
            var blockMgr = new GameObject("Block01_Manager");
            blockMgr.transform.SetParent(root.transform);
            var block01 = blockMgr.AddComponent<CODEX.Tutorial.Blocks.Block01_Materializacion>();

            // Asignar referencia a LUMA
            var lumaComp = luma?.GetComponent<CODEX.Tutorial.LumaGuide>();
            if (lumaComp != null)
            {
                var so = new SerializedObject(block01);
                so.FindProperty("luma").objectReferenceValue = lumaComp;
                so.ApplyModifiedProperties();
            }

            // ════════════════════════════════════
            //  PANTALLA DE DIAGNÓSTICO (UI) — después del manager para poder conectarla
            // ════════════════════════════════════
            SetupBootScreenUI(root);

            // ════════════════════════════════════
            //  CONFIGURAR CÁMARA
            // ════════════════════════════════════
            var mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.position = new Vector3(-5f, 0f, -10f);
                mainCam.orthographicSize = 6f;

                var camFollow = mainCam.GetComponent<CODEX.Systems.CameraFollow>();
                if (camFollow != null)
                {
                    var cfSO = new SerializedObject(camFollow);
                    cfSO.FindProperty("offset").vector2Value      = new Vector2(2f, 1f);
                    cfSO.FindProperty("minBounds").vector2Value   = new Vector2(-10f, -5f);
                    cfSO.FindProperty("maxBounds").vector2Value   = new Vector2(25f, 8f);
                    cfSO.FindProperty("smoothSpeed").floatValue   = 5f;
                    cfSO.ApplyModifiedProperties();
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[Block01_Setup] T01_Materializacion poblada correctamente.");
            bool playerPlaced = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/CODIGO7.prefab") != null;
            EditorUtility.DisplayDialog("Bloque 1 listo",
                "T01_Materializacion configurada.\n\n" +
                (playerPlaced
                    ? "✓ Jugador CODIGO7 instanciado en la escena.\n✓ CameraFollow conectado.\n\n" +
                      "PENDIENTE:\n• Asigna un Sprite real al Body del jugador\n• Verifica las posiciones en Play Mode"
                    : "⚠ El prefab del jugador no existe aún.\nEjecuta primero: CODEX > Crear Prefabs del Jugador\nLuego vuelve a correr este setup."),
                "OK");
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private static GameObject CreateBlock(GameObject parent, string name,
            Vector3 pos, Vector3 scale, Color color, string layerName)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            go.transform.position = pos;
            go.transform.localScale = scale;

            if (!string.IsNullOrEmpty(layerName))
            {
                int layerIdx = LayerMask.NameToLayer(layerName);
                if (layerIdx >= 0) go.layer = layerIdx;
            }

            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = color;
            sr.sprite = CreateSquareSprite();

            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            return go;
        }

        private static GameObject CreateDecoration(GameObject parent, string name,
            Vector3 pos, Vector3 scale, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            go.transform.position = pos;
            go.transform.localScale = scale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = color;
            sr.sprite = CreateSquareSprite();
            sr.sortingOrder = -1;

            return go;
        }

        private static void AddPointLight(GameObject parent, Color color, float range, float intensity)
        {
            var lightGO = new GameObject("Light");
            lightGO.transform.SetParent(parent.transform);
            lightGO.transform.localPosition = Vector3.zero;

            var light2D = lightGO.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
            light2D.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point;
            light2D.color = color;
            light2D.pointLightOuterRadius = range;
            light2D.intensity = intensity;
        }

        private static Sprite CreateSquareSprite()
        {
            return CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
        }

        private static void SetupBootScreenUI(GameObject parent)
        {
            // Canvas de la pantalla de diagnóstico (fullscreen, se desactiva después del boot)
            var canvasGO = new GameObject("BootScreen_Canvas");
            canvasGO.transform.SetParent(parent.transform);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            var cgComp = canvasGO.AddComponent<CanvasGroup>();

            // Fondo negro
            var bg = new GameObject("Background");
            bg.transform.SetParent(canvasGO.transform, false);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = Color.black;
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;

            // Texto de diagnóstico (verde sobre negro)
            var textGO = new GameObject("BootText");
            textGO.transform.SetParent(canvasGO.transform, false);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "";
            tmp.fontSize = 22;
            tmp.color = new Color(0f, 1f, 0.4f);  // verde terminal
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.1f, 0.35f);
            textRect.anchorMax = new Vector2(0.9f, 0.7f);
            textRect.offsetMin = textRect.offsetMax = Vector2.zero;

            // Agregar el componente DiagnosticBootScreen
            var boot = canvasGO.AddComponent<CODEX.Tutorial.DiagnosticBootScreen>();
            var bootSO = new SerializedObject(boot);
            bootSO.FindProperty("screenRoot").objectReferenceValue = canvasGO;
            bootSO.FindProperty("bootText").objectReferenceValue = tmp;
            bootSO.FindProperty("canvasGroup").objectReferenceValue = cgComp;
            bootSO.ApplyModifiedProperties();

            // Conectar al Block01_Manager
            var blockMgr = GameObject.Find("Block01_Manager");
            if (blockMgr != null)
            {
                var block01 = blockMgr.GetComponent<CODEX.Tutorial.Blocks.Block01_Materializacion>();
                if (block01 != null)
                {
                    var so = new SerializedObject(block01);
                    so.FindProperty("bootScreen").objectReferenceValue = boot;
                    so.ApplyModifiedProperties();
                }
            }
        }
    }
}
