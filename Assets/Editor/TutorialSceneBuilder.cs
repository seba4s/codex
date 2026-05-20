using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using CODEX.Tutorial;
using CODEX.Systems;
using CODEX.Enemies;

namespace CODEX.Editor
{
    public static class TutorialSceneBuilder
    {
        // ─── Layout ────────────────────────────────────────────────────────
        private const float BlockWidth = 28f;
        private const float GroundY    = -3f;
        private const float PlayerY    = -2.2f;
        private const float PlatformH  = 0.4f;
        private const float GateH      = 4f;

        static int GroundLayerIndex => LayerMask.NameToLayer("ground");

        // ═══════════════════════════════════════════════════════════════════
        //  ENTRY POINT
        // ═══════════════════════════════════════════════════════════════════

        [MenuItem("CODEX/Build Tutorial Scene")]
        public static void BuildScene()
        {
            if (!EditorUtility.DisplayDialog(
                    "Construir Tutorial",
                    "Crea los 8 bloques del tutorial en la escena activa.\n\n¿Continuar?",
                    "Sí, construir", "Cancelar"))
                return;

            EnsureGroundLayer();

            var backgrounds = GetOrCreate("[BACKGROUNDS]", null);

            // Canvas UI — si falla no detiene la construcción de bloques
            GameObject dialoguePanel = null;
            TextMeshProUGUI dialogueText = null;
            GameObject hintPanel = null;
            TextMeshProUGUI hintText = null;
            TextMeshProUGUI progressText = null;

            try
            {
                (dialoguePanel, dialogueText, hintPanel, hintText, progressText) = BuildCanvas();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[TutorialSceneBuilder] No se pudo crear el Canvas automáticamente: " + e.Message +
                                 "\nCrea el Canvas UI manualmente y asigna los paneles en cada script.");
            }

            // Construir cada bloque de forma independiente
            TryBuild("Bloque 1", () => BuildBlock1(backgrounds, dialoguePanel, dialogueText));
            TryBuild("Bloque 2", () => BuildBlock2(backgrounds, dialoguePanel, dialogueText, hintPanel, hintText, progressText));
            TryBuild("Bloque 3", () => BuildBlock3(backgrounds, dialoguePanel, dialogueText));
            TryBuild("Bloque 4", () => BuildBlock4(backgrounds, dialoguePanel, dialogueText));
            TryBuild("Bloque 5", () => BuildBlock5(backgrounds, dialoguePanel, dialogueText));
            TryBuild("Bloque 6", () => BuildBlock6(backgrounds, dialoguePanel, dialogueText));
            TryBuild("Bloque 7", () => BuildBlock7(backgrounds, dialoguePanel, dialogueText));
            TryBuild("Bloque 8", () => BuildBlock8(backgrounds, dialoguePanel, dialogueText));

            // Reposicionar Player
            var player = GameObject.FindGameObjectWithTag("Player") ?? GameObject.Find("Player");
            if (player != null)
                player.transform.position = new Vector3(BlockStart(1) + 2f, PlayerY, 0);

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Debug.Log("[TutorialSceneBuilder] ¡Escena construida! Revisa la consola para errores parciales.");

            EditorUtility.DisplayDialog("¡Listo!",
                "Jerarquía creada.\n\n" +
                "Pasos manuales restantes:\n" +
                "• Tag 'Player' en el GameObject Player\n" +
                "• Layer 'ground' en cada plataforma\n" +
                "• CameraFollow en Main Camera → campo Target = Player\n" +
                "• Referencias en TutorialShootingBlock (targets, UI)\n" +
                "• Referencias en TutorialEnemyCounter (enemies[], gate)\n" +
                "• Referencias en TutorialCollectionBlock (collectibles[], gate)\n" +
                "• ZoneTransition de cada bloque → verificar objectsToEnable\n" +
                "• Portal_Salida → OnActivated → SceneTransition",
                "Entendido");
        }

        static void TryBuild(string label, System.Action build)
        {
            try   { build(); }
            catch (System.Exception e)
            {
                Debug.LogError($"[TutorialSceneBuilder] Error en {label}: {e.Message}\n{e.StackTrace}");
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        //  BLOQUES
        // ═══════════════════════════════════════════════════════════════════

        static void BuildBlock1(GameObject bgs, GameObject dp, TextMeshProUGUI dt)
        {
            float ox = BlockStart(1);
            var root = GetOrCreate("[BLOQUE_1]", null);

            var bg1 = CreateBG("Background_Bloque1", bgs.transform);

            CreatePlatform("Ground_Bloque1",      root.transform, ox + 14f, GroundY,        30f, PlatformH);
            CreatePlatform("Plataforma_Inicio",   root.transform, ox + 2f,  GroundY,         6f, PlatformH);
            CreatePlatform("Plataforma_Salto_1",  root.transform, ox + 11f, GroundY + 1.5f,  4f, PlatformH);
            CreatePlatform("Plataforma_Salto_2",  root.transform, ox + 19f, GroundY + 3f,    4f, PlatformH);

            Pos(GetOrCreate("SpawnPoint_Player",  root.transform), ox + 2f, PlayerY);
            CreateCheckpoint("Checkpoint_1", root.transform, ox + 4f, GroundY + 1f);

            CreateDialogueTrigger("TriggerDialogo_LUMA", root.transform,
                ox + 1f, GroundY + 1f, 3f, 4f,
                "Sistema en línea. Soy LUMA.\nUsa A/D para moverte,\nEspacio para saltar, Shift para dash.",
                dp, dt);

            var sp2 = Pos(GetOrCreate("SpawnPoint_Bloque2", root.transform), BlockStart(2) + 2f, PlayerY);

            CreateZoneTransition("TransicionBloque1_a_2", root.transform,
                ox + BlockWidth - 0.5f, GroundY + 1f, 1f, 10f,
                sp2.transform, new[] { bg1 }, null);
        }

        static void BuildBlock2(GameObject bgs, GameObject dp, TextMeshProUGUI dt,
                                 GameObject hp, TextMeshProUGUI ht, TextMeshProUGUI pt)
        {
            float ox = BlockStart(2);
            var root = GetOrCreate("[BLOQUE_2]", null);

            var bg2 = CreateBG("Background_Bloque2", bgs.transform);
            bg2.SetActive(false);

            CreatePlatform("Ground_Bloque2", root.transform, ox + 14f, GroundY, 30f, PlatformH);
            CreateCheckpoint("Checkpoint_2", root.transform, ox + 2f, GroundY + 1f);

            CreateDialogueTrigger("TriggerDialogo_Disparo", root.transform,
                ox + 1f, GroundY + 1f, 3f, 4f,
                "¡Archivos infectados detectados!\nApunta con el ratón.\nClic para disparar.",
                dp, dt);

            for (int i = 0; i < 3; i++)
                CreateShootingTarget($"ShootingTarget_{i + 1}", root.transform,
                    ox + 8f + i * 2.5f, GroundY + 1.5f);

            // TutorialShootingBlock — referencias a asignar manualmente en Inspector
            var sb = GetOrCreate("TutorialShootingBlock_Disparo", root.transform);
            Pos(sb, ox + 10f, GroundY + 1f);
            sb.AddComponent<TutorialShootingBlock>();
            var sbc = sb.AddComponent<BoxCollider2D>();
            sbc.isTrigger = true;
            sbc.size = new Vector2(14f, 6f);

            CreateEnemy("Enemy_EstaticoBloqueaPuerta", root.transform,
                ox + 18f, GroundY + 0.5f, InfectedFile.EnemyType.TypeA_Static);
            CreateGate("Puerta_Bloqueada", root.transform, ox + 20f, GroundY + GateH * 0.5f);

            var sp3 = Pos(GetOrCreate("SpawnPoint_Bloque3", root.transform), BlockStart(3) + 2f, PlayerY);
            CreateZoneTransition("TransicionBloque2_a_3", root.transform,
                ox + BlockWidth - 0.5f, GroundY + 1f, 1f, 10f,
                sp3.transform, new[] { bg2 }, null);
        }

        static void BuildBlock3(GameObject bgs, GameObject dp, TextMeshProUGUI dt)
        {
            float ox = BlockStart(3);
            var root = GetOrCreate("[BLOQUE_3]", null);

            var bg3 = CreateBG("Background_Bloque3", bgs.transform);
            bg3.SetActive(false);

            CreatePlatform("Ground_Bloque3", root.transform, ox + 14f, GroundY, 30f, PlatformH);
            CreateCheckpoint("Checkpoint_3", root.transform, ox + 2f, GroundY + 1f);

            CreateDialogueTrigger("TriggerDialogo_Recoleccion", root.transform,
                ox + 1f, GroundY + 1f, 3f, 4f,
                "Fragmentos de datos corruptos.\n¡Recógelos todos!",
                dp, dt);

            float[] cx = { ox + 5f, ox + 8f, ox + 11f, ox + 14f, ox + 17f, ox + 20f };
            float[] cy = { -1.5f, -0.5f, -1.5f, -2f, -1.5f, -0.8f };
            for (int i = 0; i < 6; i++)
                CreateCollectible($"DataCollectible_{i + 1}", root.transform, cx[i], cy[i]);

            CreateGate("Puerta_Recoleccion", root.transform, ox + 22f, GroundY + GateH * 0.5f);

            // TutorialCollectionBlock — referencias a asignar manualmente
            var cb = GetOrCreate("TutorialCollectionBlock_3", root.transform);
            Pos(cb, ox + 14f, GroundY + 1f);
            cb.AddComponent<TutorialCollectionBlock>();

            var sp4 = Pos(GetOrCreate("SpawnPoint_Bloque4", root.transform), BlockStart(4) + 2f, PlayerY);
            CreateZoneTransition("TransicionBloque3_a_4", root.transform,
                ox + BlockWidth - 0.5f, GroundY + 1f, 1f, 10f,
                sp4.transform, new[] { bg3 }, null);
        }

        static void BuildBlock4(GameObject bgs, GameObject dp, TextMeshProUGUI dt)
        {
            float ox = BlockStart(4);
            var root = GetOrCreate("[BLOQUE_4]", null);

            var bg4 = CreateBG("Background_Bloque4", bgs.transform);
            bg4.SetActive(false);

            CreatePlatform("Ground_Bloque4", root.transform, ox + 14f, GroundY, 30f, PlatformH);
            CreateCheckpoint("Checkpoint_4", root.transform, ox + 2f, GroundY + 1f);

            CreateDialogueTrigger("TriggerDialogo_Dano", root.transform,
                ox + 1f, GroundY + 1f, 3f, 4f,
                "Zona de alto riesgo.\nUsa Shift para dash\ne ignorar el daño.",
                dp, dt);

            CreateSpikeTrap("SpikeTrap_1", root.transform, ox + 8f,  GroundY + PlatformH);
            CreateSpikeTrap("SpikeTrap_2", root.transform, ox + 12f, GroundY + PlatformH);

            CreateEnemy("Enemy_Melee_1", root.transform, ox + 15f, GroundY + 0.5f, InfectedFile.EnemyType.TypeD_Melee);
            CreateEnemy("Enemy_Melee_2", root.transform, ox + 19f, GroundY + 0.5f, InfectedFile.EnemyType.TypeD_Melee);

            CreateGate("Puerta_Bloque4", root.transform, ox + 22f, GroundY + GateH * 0.5f);

            // TutorialEnemyCounter — referencias a asignar manualmente
            var ec = GetOrCreate("TutorialEnemyCounter_4", root.transform);
            Pos(ec, ox + 14f, GroundY + 1f);
            ec.AddComponent<TutorialEnemyCounter>();

            var sp5 = Pos(GetOrCreate("SpawnPoint_Bloque5", root.transform), BlockStart(5) + 2f, PlayerY);
            CreateZoneTransition("TransicionBloque4_a_5", root.transform,
                ox + BlockWidth - 0.5f, GroundY + 1f, 1f, 10f,
                sp5.transform, new[] { bg4 }, null);
        }

        static void BuildBlock5(GameObject bgs, GameObject dp, TextMeshProUGUI dt)
        {
            float ox = BlockStart(5);
            var root = GetOrCreate("[BLOQUE_5]", null);

            var bg5 = CreateBG("Background_Bloque5", bgs.transform);
            bg5.SetActive(false);

            CreatePlatform("Ground_Bloque5", root.transform, ox + 14f, GroundY, 30f, PlatformH);
            CreateCheckpoint("Checkpoint_5", root.transform, ox + 2f, GroundY + 1f);

            CreateDialogueTrigger("TriggerDialogo_Terminal", root.transform,
                ox + 1f, GroundY + 1f, 3f, 4f,
                "Terminal de sistema detectada.\nPresiona E para interactuar.",
                dp, dt);

            var term = GetOrCreate("Terminal_Principal", root.transform);
            Pos(term, ox + 14f, GroundY + 0.5f);
            term.AddComponent<RepairTerminal>();
            var tc = term.AddComponent<BoxCollider2D>();
            tc.isTrigger = true;
            tc.size = new Vector2(1.5f, 2f);
            term.AddComponent<SpriteRenderer>();

            CreateGate("Puerta_Terminal", root.transform, ox + 20f, GroundY + GateH * 0.5f);

            var sp6 = Pos(GetOrCreate("SpawnPoint_Bloque6", root.transform), BlockStart(6) + 2f, PlayerY);
            CreateZoneTransition("TransicionBloque5_a_6", root.transform,
                ox + BlockWidth - 0.5f, GroundY + 1f, 1f, 10f,
                sp6.transform, new[] { bg5 }, null);
        }

        static void BuildBlock6(GameObject bgs, GameObject dp, TextMeshProUGUI dt)
        {
            float ox = BlockStart(6);
            var root = GetOrCreate("[BLOQUE_6]", null);

            var bg6 = CreateBG("Background_Bloque6", bgs.transform);
            bg6.SetActive(false);

            CreatePlatform("Suelo_Inicio_B6", root.transform, ox + 3f,  GroundY, 6f, PlatformH);
            CreatePlatform("Suelo_Final_B6",  root.transform, ox + 23f, GroundY, 6f, PlatformH);
            CreateCheckpoint("Checkpoint_6",  root.transform, ox + 2f, GroundY + 1f);

            CreateDialogueTrigger("TriggerDialogo_Plataformas", root.transform,
                ox + 1f, GroundY + 1f, 3f, 4f,
                "Las plataformas son inestables.\n¡Muévete rápido!",
                dp, dt);

            float[] fpX = { ox + 8f, ox + 10.5f, ox + 13f, ox + 15.5f, ox + 18f };
            for (int i = 0; i < fpX.Length; i++)
            {
                var fp = GetOrCreate($"FallingPlatform_{i + 1}", root.transform);
                fp.transform.position = new Vector3(fpX[i], GroundY + 0.5f, 0);
                fp.AddComponent<FallingPlatform>();
                var bx = fp.AddComponent<BoxCollider2D>();
                bx.size = new Vector2(2f, PlatformH);
                var rb = fp.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.freezeRotation = true;
                fp.AddComponent<SpriteRenderer>();
                int gl = GroundLayerIndex;
                if (gl >= 0) fp.layer = gl;
            }

            var sp7 = Pos(GetOrCreate("SpawnPoint_Bloque7", root.transform), BlockStart(7) + 2f, PlayerY);
            CreateZoneTransition("TransicionBloque6_a_7", root.transform,
                ox + BlockWidth - 0.5f, GroundY + 1f, 1f, 10f,
                sp7.transform, new[] { bg6 }, null);
        }

        static void BuildBlock7(GameObject bgs, GameObject dp, TextMeshProUGUI dt)
        {
            float ox = BlockStart(7);
            var root = GetOrCreate("[BLOQUE_7]", null);

            var bg7 = CreateBG("Background_Bloque7", bgs.transform);
            bg7.SetActive(false);

            CreatePlatform("Ground_Bloque7", root.transform, ox + 14f, GroundY, 30f, PlatformH);
            CreateCheckpoint("Checkpoint_7", root.transform, ox + 2f, GroundY + 1f);

            CreateDialogueTrigger("TriggerDialogo_Combinado", root.transform,
                ox + 1f, GroundY + 1f, 3f, 4f,
                "Objetivo final: elimina todos\nlos archivos infectados.",
                dp, dt);

            var patrol = CreateEnemy("Enemy_Patrol", root.transform,
                ox + 10f, GroundY + 0.5f, InfectedFile.EnemyType.TypeB_Patrol);
            Pos(GetOrCreate("PatrolPoint_0", patrol.transform), ox + 7f,  GroundY + 0.5f);
            Pos(GetOrCreate("PatrolPoint_1", patrol.transform), ox + 16f, GroundY + 0.5f);

            var projEnemy = CreateEnemy("Enemy_Projectile", root.transform,
                ox + 20f, GroundY + 0.5f, InfectedFile.EnemyType.TypeC_Projectile);
            Pos(GetOrCreate("FirePoint", projEnemy.transform), 0f, 0.5f, local: true);

            CreateGate("Puerta_Bloque7", root.transform, ox + 22f, GroundY + GateH * 0.5f);

            var ec7 = GetOrCreate("TutorialEnemyCounter_7", root.transform);
            Pos(ec7, ox + 14f, GroundY + 1f);
            ec7.AddComponent<TutorialEnemyCounter>();

            var sp8 = Pos(GetOrCreate("SpawnPoint_Bloque8", root.transform), BlockStart(8) + 2f, PlayerY);
            CreateZoneTransition("TransicionBloque7_a_8", root.transform,
                ox + BlockWidth - 0.5f, GroundY + 1f, 1f, 10f,
                sp8.transform, new[] { bg7 }, null);
        }

        static void BuildBlock8(GameObject bgs, GameObject dp, TextMeshProUGUI dt)
        {
            float ox = BlockStart(8);
            var root = GetOrCreate("[BLOQUE_8]", null);

            var bg8 = CreateBG("Background_Bloque8", bgs.transform);
            bg8.SetActive(false);

            CreatePlatform("Ground_Bloque8", root.transform, ox + 14f, GroundY, 30f, PlatformH);
            CreateCheckpoint("Checkpoint_8", root.transform, ox + 2f, GroundY + 1f);

            CreateDialogueTrigger("TriggerDialogo_Final", root.transform,
                ox + 1f, GroundY + 1f, 3f, 4f,
                "Tutorial completado.\nEl sistema te espera, agente.",
                dp, dt);

            var portal = GetOrCreate("Portal_Salida", root.transform);
            Pos(portal, ox + 14f, GroundY + 1f);
            portal.AddComponent<RepairTerminal>();
            var pc = portal.AddComponent<BoxCollider2D>();
            pc.isTrigger = true;
            pc.size = new Vector2(2.5f, 3f);
            portal.AddComponent<SpriteRenderer>();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  CANVAS UI
        // ═══════════════════════════════════════════════════════════════════

        static (GameObject dp, TextMeshProUGUI dt,
                GameObject hp, TextMeshProUGUI ht, TextMeshProUGUI pt) BuildCanvas()
        {
            // Reusar canvas existente o crear uno nuevo
            Canvas existing = Object.FindAnyObjectByType<Canvas>();
            GameObject canvasGO;
            if (existing != null)
            {
                canvasGO = existing.gameObject;
            }
            else
            {
                canvasGO = new GameObject("Canvas");
                var canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }

            // DialoguePanel
            var dpGO  = CreateUIPanel("DialoguePanel", canvasGO.transform,
                            new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.25f),
                            new Color(0f, 0f, 0.1f, 0.85f));
            dpGO.SetActive(false);
            var dtGO  = CreateUIText("DialogueText", dpGO.transform,
                            Vector2.zero, Vector2.one, new Vector2(12f, 8f), new Vector2(-12f, -8f),
                            18, out var dtTMP);

            // HintPanel
            var hpGO  = CreateUIPanel("HintPanel", canvasGO.transform,
                            new Vector2(0.3f, 0.75f), new Vector2(0.7f, 0.95f),
                            new Color(0f, 0.1f, 0f, 0.8f));
            hpGO.SetActive(false);
            var htGO  = CreateUIText("HintText",     hpGO.transform,
                            new Vector2(0f, 0.4f), Vector2.one, new Vector2(8f, 4f), new Vector2(-8f, -4f),
                            16, out var htTMP);
            var ptGO  = CreateUIText("ProgressText", hpGO.transform,
                            Vector2.zero, new Vector2(1f, 0.4f), new Vector2(8f, 4f), new Vector2(-8f, -4f),
                            14, out var ptTMP);
            ptTMP.text = "0 / 0";

            return (dpGO, dtTMP, hpGO, htTMP, ptTMP);
        }

        // Creates a Panel (Image) correctly using UI GameObject pattern
        static GameObject CreateUIPanel(string name, Transform parent,
                                        Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        static GameObject CreateUIText(string name, Transform parent,
                                       Vector2 anchorMin, Vector2 anchorMax,
                                       Vector2 offsetMin, Vector2 offsetMax,
                                       float fontSize, out TextMeshProUGUI tmp)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = "";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = fontSize;
            return go;
        }

        // ═══════════════════════════════════════════════════════════════════
        //  HELPERS
        // ═══════════════════════════════════════════════════════════════════

        static float BlockStart(int index) => (index - 1) * BlockWidth;

        static GameObject GetOrCreate(string name, Transform parent)
        {
            var go = GameObject.Find(name);
            if (go == null)
            {
                go = new GameObject(name);
                if (parent != null) go.transform.SetParent(parent, true);
            }
            else if (parent != null && go.transform.parent != parent)
            {
                go.transform.SetParent(parent, true);
            }
            return go;
        }

        // Posiciona un GO en world space y lo devuelve
        static GameObject Pos(GameObject go, float x, float y, bool local = false)
        {
            if (local) go.transform.localPosition = new Vector3(x, y, 0);
            else       go.transform.position       = new Vector3(x, y, 0);
            return go;
        }

        static GameObject CreateBG(string name, Transform parent)
        {
            var go = GetOrCreate(name, parent);
            if (go.GetComponent<SpriteRenderer>() == null)
                go.AddComponent<SpriteRenderer>().sortingOrder = -10;
            return go;
        }

        static void CreatePlatform(string name, Transform parent, float x, float y, float w, float h)
        {
            var go = GetOrCreate(name, parent);
            go.transform.position = new Vector3(x, y, 0);
            if (go.GetComponent<BoxCollider2D>() == null)
                go.AddComponent<BoxCollider2D>().size = new Vector2(w, h);
            if (go.GetComponent<SpriteRenderer>() == null)
                go.AddComponent<SpriteRenderer>();
            int gl = GroundLayerIndex;
            if (gl >= 0) go.layer = gl;
        }

        static void CreateCheckpoint(string name, Transform parent, float x, float y)
        {
            var go = GetOrCreate(name, parent);
            go.transform.position = new Vector3(x, y, 0);
            // Collider primero para satisfacer [RequireComponent] de CheckpointTrigger
            if (go.GetComponent<BoxCollider2D>() == null)
            {
                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(1f, 3f);
            }
            if (go.GetComponent<CheckpointTrigger>() == null)
                go.AddComponent<CheckpointTrigger>();
        }

        static void CreateDialogueTrigger(string name, Transform parent,
            float x, float y, float w, float h,
            string message, GameObject dp, TextMeshProUGUI dt)
        {
            var go = GetOrCreate(name, parent);
            go.transform.position = new Vector3(x, y, 0);

            // Collider primero para satisfacer [RequireComponent(typeof(Collider2D))]
            if (go.GetComponent<BoxCollider2D>() == null)
            {
                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(w, h);
            }

            TutorialDialogueTrigger trig = go.GetComponent<TutorialDialogueTrigger>();
            if (trig == null) trig = go.AddComponent<TutorialDialogueTrigger>();
            if (trig == null)
            {
                Debug.LogWarning($"[TutorialSceneBuilder] No se pudo agregar TutorialDialogueTrigger a '{name}'. Agrégalo manualmente.");
                return;
            }

            try
            {
                var so = new SerializedObject(trig);
                so.FindProperty("message").stringValue = message;
                if (dp != null) so.FindProperty("dialoguePanel").objectReferenceValue = dp;
                if (dt != null) so.FindProperty("dialogueText").objectReferenceValue  = dt;
                so.ApplyModifiedProperties();
            }
            catch
            {
                // Fallback: asignar campos vía reflexión
                SetField(trig, "message",       message);
                if (dp != null) SetField(trig, "dialoguePanel", dp);
                if (dt != null) SetField(trig, "dialogueText",  dt);
                EditorUtility.SetDirty(trig);
            }
        }

        static void CreateZoneTransition(string name, Transform parent,
            float x, float y, float w, float h,
            Transform spawnPoint, GameObject[] disable, GameObject[] enable)
        {
            var go = GetOrCreate(name, parent);
            go.transform.position = new Vector3(x, y, 0);

            // Collider primero para satisfacer [RequireComponent(typeof(Collider2D))]
            if (go.GetComponent<BoxCollider2D>() == null)
            {
                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(w, h);
            }

            ZoneTransition zt = go.GetComponent<ZoneTransition>();
            if (zt == null) zt = go.AddComponent<ZoneTransition>();
            if (zt == null)
            {
                Debug.LogWarning($"[TutorialSceneBuilder] No se pudo agregar ZoneTransition a '{name}'. Agrégalo manualmente.");
                return;
            }

            try
            {
                var so = new SerializedObject(zt);
                so.FindProperty("playerSpawnPoint").objectReferenceValue = spawnPoint;

                var disArr = so.FindProperty("objectsToDisable");
                disArr.arraySize = disable?.Length ?? 0;
                for (int i = 0; i < (disable?.Length ?? 0); i++)
                    disArr.GetArrayElementAtIndex(i).objectReferenceValue = disable[i];

                var enArr = so.FindProperty("objectsToEnable");
                enArr.arraySize = enable?.Length ?? 0;
                for (int i = 0; i < (enable?.Length ?? 0); i++)
                    enArr.GetArrayElementAtIndex(i).objectReferenceValue = enable[i];

                so.ApplyModifiedProperties();
            }
            catch
            {
                SetField(zt, "playerSpawnPoint", spawnPoint);
                EditorUtility.SetDirty(zt);
            }
        }

        static GameObject CreateGate(string name, Transform parent, float x, float y)
        {
            var go = GetOrCreate(name, parent);
            go.transform.position = new Vector3(x, y, 0);
            if (go.GetComponent<BoxCollider2D>() == null)
                go.AddComponent<BoxCollider2D>().size = new Vector2(0.4f, GateH);
            if (go.GetComponent<SpriteRenderer>() == null)
                go.AddComponent<SpriteRenderer>();
            return go;
        }

        static void CreateShootingTarget(string name, Transform parent, float x, float y)
        {
            var go = GetOrCreate(name, parent);
            go.transform.position = new Vector3(x, y, 0);
            if (go.GetComponent<ShootingTarget>() == null)   go.AddComponent<ShootingTarget>();
            if (go.GetComponent<BoxCollider2D>() == null)    go.AddComponent<BoxCollider2D>().size = new Vector2(0.6f, 0.6f);
            if (go.GetComponent<SpriteRenderer>() == null)   go.AddComponent<SpriteRenderer>();
        }

        static void CreateCollectible(string name, Transform parent, float x, float y)
        {
            var go = GetOrCreate(name, parent);
            go.transform.position = new Vector3(x, y, 0);
            if (go.GetComponent<DataCollectible>() == null)  go.AddComponent<DataCollectible>();
            if (go.GetComponent<CircleCollider2D>() == null)
            {
                var col = go.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.4f;
            }
            var vis = GetOrCreate("Visual", go.transform);
            vis.transform.localPosition = Vector3.zero;
            if (vis.GetComponent<SpriteRenderer>() == null)  vis.AddComponent<SpriteRenderer>();
        }

        static GameObject CreateEnemy(string name, Transform parent, float x, float y,
                                       InfectedFile.EnemyType type)
        {
            var go = GetOrCreate(name, parent);
            go.transform.position = new Vector3(x, y, 0);

            InfectedFile inf = go.GetComponent<InfectedFile>();
            if (inf == null) inf = go.AddComponent<InfectedFile>();
            if (inf != null)
            {
                try
                {
                    var so = new SerializedObject(inf);
                    so.FindProperty("enemyType").enumValueIndex = (int)type;
                    so.ApplyModifiedProperties();
                }
                catch
                {
                    SetField(inf, "enemyType", type);
                    EditorUtility.SetDirty(inf);
                }
            }

            if (go.GetComponent<Rigidbody2D>() == null)
            {
                var rb = go.AddComponent<Rigidbody2D>();
                rb.freezeRotation = true;
                if (type == InfectedFile.EnemyType.TypeA_Static)
                    rb.bodyType = RigidbodyType2D.Kinematic;
            }
            if (go.GetComponent<BoxCollider2D>() == null)    go.AddComponent<BoxCollider2D>().size = new Vector2(0.6f, 0.8f);
            if (go.GetComponent<SpriteRenderer>() == null)   go.AddComponent<SpriteRenderer>();
            return go;
        }

        static void CreateSpikeTrap(string name, Transform parent, float x, float y)
        {
            var go = GetOrCreate(name, parent);
            go.transform.position = new Vector3(x, y, 0);
            if (go.GetComponent<SpikeDamage>() == null)      go.AddComponent<SpikeDamage>();
            if (go.GetComponent<BoxCollider2D>() == null)
            {
                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(1.5f, 0.4f);
            }
            if (go.GetComponent<SpriteRenderer>() == null)   go.AddComponent<SpriteRenderer>();
        }

        static void SetField(object target, string fieldName, object value)
        {
            var f = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            f?.SetValue(target, value);
        }

        static void EnsureGroundLayer()
        {
            if (LayerMask.NameToLayer("ground") == -1)
                Debug.LogWarning("[TutorialSceneBuilder] Layer 'ground' no existe. " +
                                 "Créalo en Edit > Project Settings > Tags and Layers antes de correr el Builder.");
        }
    }
}
