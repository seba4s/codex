using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using CODEX.Player;
using CODEX.Systems;
using CODEX.Tutorial;

namespace CODEX.Editor
{
    public static class TutorialSceneConfigurator
    {
        const float BlockWidth = 28f;
        const float GroundY    = -3f;
        const float PlayerY    = -2.2f;

        // ═══════════════════════════════════════════════════════════════
        //  ENTRY POINT
        // ═══════════════════════════════════════════════════════════════

        [MenuItem("CODEX/Configure Tutorial Scene")]
        public static void Configure()
        {
            if (!EditorUtility.DisplayDialog(
                    "Configurar Tutorial",
                    "Configura Player, Cámara, Suelo, Fondos y Transiciones.\n\n¿Continuar?",
                    "Sí", "Cancelar")) return;

            int steps = 0;
            TryRun("Player",            ConfigurePlayer,              ref steps);
            TryRun("Cámara",            ConfigureCamera,              ref steps);
            TryRun("Layer de plataformas", AssignGroundLayer,         ref steps);
            TryRun("Paredes/límites",   AddBoundaryWalls,             ref steps);
            TryRun("Fondos",            ConfigureBackgrounds,         ref steps);
            TryRun("ZoneTransitions",   WireZoneTransitions,          ref steps);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("¡Configuración lista!",
                $"{steps}/6 pasos completados.\n\n" +
                "Pendientes manuales:\n" +
                "• Asignar sprites a los SpriteRenderer de fondos y plataformas\n" +
                "• Asignar prefab de proyectil en ShootingSystem → Projectile Prefab\n" +
                "• Crear layer 'ground' si no existe aún\n" +
                "• Asignar targets en TutorialShootingBlock (Bloque 2)\n" +
                "• Asignar enemies[] en TutorialEnemyCounter (Bloques 4 y 7)\n" +
                "• Asignar collectibles[] en TutorialCollectionBlock (Bloque 3)",
                "Entendido");
        }

        static void TryRun(string label, System.Action action, ref int counter)
        {
            try   { action(); counter++; }
            catch (System.Exception e)
            { Debug.LogError($"[Configurator] Error en '{label}': {e.Message}\n{e.StackTrace}"); }
        }

        // ═══════════════════════════════════════════════════════════════
        //  1. PLAYER
        // ═══════════════════════════════════════════════════════════════

        static void ConfigurePlayer()
        {
            var player = GameObject.FindGameObjectWithTag("Player") ?? GameObject.Find("Player");
            if (player == null) { Debug.LogError("[Configurator] No se encontró el GameObject 'Player'."); return; }

            // Tag
            player.tag = "Player";

            // ── Rigidbody2D (primero para satisfacer RequireComponent de PlayerController)
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb == null) rb = player.AddComponent<Rigidbody2D>();
            rb.freezeRotation             = true;
            rb.gravityScale               = 3f;
            rb.collisionDetectionMode     = CollisionDetectionMode2D.Continuous;
            rb.interpolation              = RigidbodyInterpolation2D.Interpolate;

            // ── Collider (cuerpo del jugador, no trigger)
            var col = player.GetComponent<BoxCollider2D>();
            if (col == null) col = player.AddComponent<BoxCollider2D>();
            col.size   = new Vector2(0.28f, 0.44f);
            col.offset = new Vector2(0f, 0f);

            // ── GroundCheckPoint
            var gcp = player.transform.Find("GroundCheckPoint");
            if (gcp == null)
            {
                var gcpGO = new GameObject("GroundCheckPoint");
                gcpGO.transform.SetParent(player.transform);
                gcpGO.transform.localPosition = new Vector3(0f, -0.22f, 0f);
                gcp = gcpGO.transform;
            }

            // ── FirePoint (para ShootingSystem)
            var fp = player.transform.Find("FirePoint");
            if (fp == null)
            {
                var fpGO = new GameObject("FirePoint");
                fpGO.transform.SetParent(player.transform);
                fpGO.transform.localPosition = new Vector3(0.5f, 0f, 0f);
                fp = fpGO.transform;
            }

            // ── PlayerController
            var pc = player.GetComponent<PlayerController>();
            if (pc == null) pc = player.AddComponent<PlayerController>();
            if (pc != null)
            {
                var so = new SerializedObject(pc);
                so.FindProperty("groundCheckPoint").objectReferenceValue = gcp;
                so.FindProperty("groundLayer").intValue = LayerMask.GetMask("ground");
                var anim = player.GetComponent<Animator>();
                if (anim != null) so.FindProperty("animator").objectReferenceValue = anim;
                so.ApplyModifiedProperties();
            }

            // ── ShootingSystem
            var ss = player.GetComponent<ShootingSystem>();
            if (ss == null) ss = player.AddComponent<ShootingSystem>();
            if (ss != null)
            {
                var so = new SerializedObject(ss);
                so.FindProperty("firePoint").objectReferenceValue = fp;
                // targetLayers: todo excepto Player y ground
                so.FindProperty("targetLayers").intValue =
                    LayerMask.GetMask("Default") | LayerMask.GetMask("Enemy");
                so.ApplyModifiedProperties();
            }

            // ── PlayerHealth
            if (player.GetComponent<PlayerHealth>() == null)
                player.AddComponent<PlayerHealth>();

            EditorUtility.SetDirty(player);
            Debug.Log("[Configurator] Player configurado.");
        }

        // ═══════════════════════════════════════════════════════════════
        //  2. CÁMARA
        // ═══════════════════════════════════════════════════════════════

        static void ConfigureCamera()
        {
            var cam = Camera.main;
            if (cam == null) { Debug.LogError("[Configurator] No se encontró la Main Camera."); return; }

            var cf = cam.GetComponent<CameraFollow>();
            if (cf == null) cf = cam.gameObject.AddComponent<CameraFollow>();
            if (cf == null) return;

            var player = GameObject.FindGameObjectWithTag("Player") ?? GameObject.Find("Player");

            var so = new SerializedObject(cf);
            if (player != null)
                so.FindProperty("target").objectReferenceValue = player.transform;
            so.FindProperty("smoothSpeed").floatValue      = 5f;
            so.FindProperty("offset").vector2Value         = new Vector2(2f, 1f);
            so.FindProperty("minBounds").vector2Value      = new Vector2(-8f,  -10f);
            so.FindProperty("maxBounds").vector2Value      = new Vector2(BlockWidth * 8f + 8f, 10f);
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(cam.gameObject);
            Debug.Log("[Configurator] CameraFollow configurado.");
        }

        // ═══════════════════════════════════════════════════════════════
        //  3. LAYER DE PLATAFORMAS
        // ═══════════════════════════════════════════════════════════════

        static void AssignGroundLayer()
        {
            int groundLayer = LayerMask.NameToLayer("ground");
            if (groundLayer == -1)
            {
                Debug.LogWarning("[Configurator] Layer 'ground' no existe. Créalo en Edit > Project Settings > Tags and Layers.");
                return;
            }

            string[] platformPrefixes = { "Ground_", "Plataforma_", "Suelo_", "FallingPlatform_" };
            var allGOs = Object.FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None);
            int count = 0;

            foreach (var bc in allGOs)
            {
                if (bc.isTrigger) continue;
                string name = bc.gameObject.name;
                foreach (var prefix in platformPrefixes)
                {
                    if (name.StartsWith(prefix))
                    {
                        bc.gameObject.layer = groundLayer;
                        EditorUtility.SetDirty(bc.gameObject);
                        count++;
                        break;
                    }
                }
            }

            Debug.Log($"[Configurator] Layer 'ground' asignado a {count} plataformas.");
        }

        // ═══════════════════════════════════════════════════════════════
        //  4. PAREDES / LÍMITES
        // ═══════════════════════════════════════════════════════════════

        static void AddBoundaryWalls()
        {
            // Pared izquierda — impide que el jugador salga por la izquierda del Bloque 1
            var b1 = GameObject.Find("[BLOQUE_1]");
            var wallParent = b1 != null ? b1.transform : null;

            EnsureWall("Wall_Izquierda_B1", wallParent,
                x: BlockStart(1) - 0.6f, y: GroundY + 5f,
                w: 0.5f, h: 16f);

            // Pared derecha final — detrás del último bloque
            var b8 = GameObject.Find("[BLOQUE_8]");
            EnsureWall("Wall_Derecha_Final", b8 != null ? b8.transform : null,
                x: BlockStart(8) + BlockWidth + 0.6f, y: GroundY + 5f,
                w: 0.5f, h: 16f);

            // Techo global — evita que el jugador vuele fuera de cámara
            EnsureWall("Techo_Global", null,
                x: BlockStart(1) + (BlockWidth * 8f) / 2f, y: GroundY + 12f,
                w: BlockWidth * 8f + 4f, h: 0.5f);

            Debug.Log("[Configurator] Paredes/límites creados.");
        }

        static void EnsureWall(string name, Transform parent, float x, float y, float w, float h)
        {
            var go = GameObject.Find(name);
            if (go == null)
            {
                go = new GameObject(name);
                if (parent != null) go.transform.SetParent(parent);
            }
            go.transform.position = new Vector3(x, y, 0f);

            var col = go.GetComponent<BoxCollider2D>();
            if (col == null) col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = false;
            col.size = new Vector2(w, h);

            EditorUtility.SetDirty(go);
        }

        // ═══════════════════════════════════════════════════════════════
        //  5. FONDOS — posición, escala y activación
        // ═══════════════════════════════════════════════════════════════

        static void ConfigureBackgrounds()
        {
            // Ortho size 5, aspecto 16:9 → viewport ≈ 17.8 × 10 unidades
            // Cada bloque mide BlockWidth = 28 u. → fondo de 40 × 14 cubre todo.
            const float bgW = 40f;
            const float bgH = 14f;
            const float bgZ =  5f;   // detrás del gameplay (z positivo = más lejos en 2D ortho)
            const float bgY = -1f;   // centro vertical aproximado de la cámara

            for (int i = 1; i <= 8; i++)
            {
                var bg = GameObject.Find($"Background_Bloque{i}");
                if (bg == null)
                {
                    Debug.LogWarning($"[Configurator] No se encontró Background_Bloque{i}.");
                    continue;
                }

                // Centro horizontal del bloque
                float centerX = BlockStart(i) + BlockWidth * 0.5f;
                bg.transform.position   = new Vector3(centerX, bgY, bgZ);
                bg.transform.localScale = new Vector3(bgW, bgH, 1f);

                // Ajustar sorting order para que quede detrás de todo
                var sr = bg.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sortingOrder = -100;

                // Solo el Bloque 1 empieza activo
                bool shouldBeActive = (i == 1);
                if (bg.activeSelf != shouldBeActive)
                    bg.SetActive(shouldBeActive);

                EditorUtility.SetDirty(bg);
            }

            Debug.Log("[Configurator] Fondos posicionados. Background_Bloque1 activo, el resto inactivo.");
        }

        // ═══════════════════════════════════════════════════════════════
        //  6. ZONE TRANSITIONS — disable/enable de fondos
        // ═══════════════════════════════════════════════════════════════

        static void WireZoneTransitions()
        {
            for (int i = 1; i <= 7; i++)
            {
                string transName = $"TransicionBloque{i}_a_{i + 1}";
                var transGO = GameObject.Find(transName);
                if (transGO == null) { Debug.LogWarning($"[Configurator] No encontrado: {transName}"); continue; }

                var zt = transGO.GetComponent<ZoneTransition>();
                if (zt == null) { Debug.LogWarning($"[Configurator] {transName} no tiene ZoneTransition."); continue; }

                var bgCurrent = GameObject.Find($"Background_Bloque{i}");
                var bgNext    = GameObject.Find($"Background_Bloque{i + 1}");

                var so = new SerializedObject(zt);

                // objectsToDisable → fondo actual
                var disArr = so.FindProperty("objectsToDisable");
                disArr.arraySize = bgCurrent != null ? 1 : 0;
                if (bgCurrent != null)
                    disArr.GetArrayElementAtIndex(0).objectReferenceValue = bgCurrent;

                // objectsToEnable → fondo siguiente
                var enArr = so.FindProperty("objectsToEnable");
                enArr.arraySize = bgNext != null ? 1 : 0;
                if (bgNext != null)
                    enArr.GetArrayElementAtIndex(0).objectReferenceValue = bgNext;

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(transGO);
            }

            Debug.Log("[Configurator] ZoneTransitions configuradas: cada transición desactiva el fondo actual y activa el siguiente.");
        }

        // ═══════════════════════════════════════════════════════════════
        //  HELPERS
        // ═══════════════════════════════════════════════════════════════

        static float BlockStart(int index) => (index - 1) * BlockWidth;
    }
}
