using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

namespace CODEX.Editor
{
    /// <summary>
    /// Crea las 8 escenas del tutorial en Assets/Scenes/Tutorial/.
    /// Menú: CODEX > Tutorial > Crear 8 escenas del tutorial
    /// </summary>
    public static class TutorialSceneCreator
    {
        private const string ScenesFolder = "Assets/Scenes/Tutorial";

        private static readonly (string name, string description)[] Blocks =
        {
            ("T01_Materializacion",      "Bloque 1 - Materialización y Movimiento Básico"),
            ("T02_Disparo",              "Bloque 2 - Sistema de Disparo (Arma de Purga)"),
            ("T03_RecoleccionDatos",     "Bloque 3 - Recolección de Datos"),
            ("T04_DanoYEsquive",         "Bloque 4 - Daño, Salud y Esquive"),
            ("T05_Terminal",             "Bloque 5 - Interacción con Terminales"),
            ("T06_PlataformasEspeciales","Bloque 6 - Plataformas Especiales y Sectores Dañados"),
            ("T07_EnemigoCombinados",    "Bloque 7 - Enemigos Combinados"),
            ("T08_PuertoSalida",         "Bloque 8 - Puerto de Salida y Cierre Narrativo"),
        };

        [MenuItem("CODEX/Tutorial/Crear 8 escenas del tutorial")]
        public static void CreateAllScenes()
        {
            if (!EditorUtility.DisplayDialog(
                    "Crear Escenas Tutorial",
                    "Esto creará 8 escenas en Assets/Scenes/Tutorial/.\n" +
                    "Las escenas existentes con el mismo nombre serán sobreescritas.\n\n¿Continuar?",
                    "Sí, crear", "Cancelar"))
                return;

            // Crear la carpeta si no existe
            if (!AssetDatabase.IsValidFolder(ScenesFolder))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                    AssetDatabase.CreateFolder("Assets", "Scenes");
                AssetDatabase.CreateFolder("Assets/Scenes", "Tutorial");
            }

            // Guardar escena actual
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            for (int i = 0; i < Blocks.Length; i++)
            {
                CreateScene(i, Blocks[i].name, Blocks[i].description);
            }

            AssetDatabase.Refresh();
            AddScenesToBuildSettings();

            EditorUtility.DisplayDialog(
                "Escenas creadas",
                $"Se crearon {Blocks.Length} escenas en {ScenesFolder}.\n\n" +
                "Revisa el Build Settings — las escenas fueron agregadas automáticamente.",
                "OK");
        }

        private static void CreateScene(int blockIndex, string sceneName, string description)
        {
            string scenePath = $"{ScenesFolder}/{sceneName}.unity";

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ── Cámara principal ─────────────────────────────────────
            var camGO = new GameObject("Main Camera");
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.12f);
            camGO.tag = "MainCamera";
            camGO.AddComponent<AudioListener>();
            camGO.AddComponent<CODEX.Systems.CameraFollow>();
            camGO.transform.position = new Vector3(0, 0, -10);

            // ── Luz 2D global ─────────────────────────────────────────
            var lightGO = new GameObject("Global Light 2D");
            var light2D = lightGO.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
            light2D.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Global;
            light2D.intensity = 1f;

            // ── TutorialManager (persistente entre escenas) ───────────
            var tmGO = new GameObject("TutorialManager");
            tmGO.AddComponent<CODEX.Tutorial.TutorialManager>();

            // ── CheckpointManager ─────────────────────────────────────
            var cpGO = new GameObject("CheckpointManager");
            var defaultSpawn = new GameObject("DefaultSpawn");
            defaultSpawn.transform.position = new Vector3(-8f, 0f, 0f);
            var cpMgr = cpGO.AddComponent<CODEX.Tutorial.CheckpointManager>();
            SerializedObject cpSO = new SerializedObject(cpMgr);
            cpSO.FindProperty("defaultSpawn").objectReferenceValue = defaultSpawn.transform;
            cpSO.ApplyModifiedProperties();

            // ── Punto de aparición del jugador ────────────────────────
            var spawnGO = new GameObject("PlayerSpawn");
            spawnGO.transform.position = new Vector3(-8f, 1f, 0f);

            // ── Suelo base ────────────────────────────────────────────
            CreatePlatform("Ground", new Vector3(0, -3f, 0), new Vector3(30f, 1f, 1f));

            // ── Paredes invisibles (límite izquierdo) ─────────────────
            CreateWall("WallLeft",  new Vector3(-16f, 0f, 0f));
            CreateWall("WallRight", new Vector3(16f,  0f, 0f));

            // ── LUMA ──────────────────────────────────────────────────
            var lumaGO = new GameObject("LUMA");
            lumaGO.transform.position = new Vector3(-6f, 2f, 0f);
            var lumaSprite = lumaGO.AddComponent<SpriteRenderer>();
            lumaSprite.color = new Color(0.4f, 0.9f, 1f);
            lumaGO.AddComponent<CODEX.Tutorial.LumaGuide>();

            // ── Trigger de carga de siguiente escena ──────────────────
            var loaderGO = new GameObject("SceneLoader_Exit");
            loaderGO.transform.position = new Vector3(14f, 0f, 0f);
            loaderGO.transform.localScale = new Vector3(2f, 8f, 1f);
            var loaderCol = loaderGO.AddComponent<BoxCollider2D>();
            loaderCol.isTrigger = true;
            loaderGO.AddComponent<CODEX.Tutorial.TutorialSceneLoader>();

            // ── Canvas HUD (mínimo — se activan elementos por evento) ─
            var canvasGO = new GameObject("HUD_Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            canvasGO.AddComponent<CODEX.Tutorial.TutorialHUD>();

            // ── Nota descriptiva del bloque ───────────────────────────
            var noteGO = new GameObject($"[BLOQUE {blockIndex + 1}] {description}");
            noteGO.transform.position = Vector3.zero;

            // ── Contenedor de contenido del bloque ────────────────────
            new GameObject($"Contenido_Bloque{blockIndex + 1}");

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[TutorialSceneCreator] Creada: {scenePath}");
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private static GameObject CreatePlatform(string name, Vector3 position, Vector3 scale)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.layer = LayerMask.NameToLayer("ground") >= 0
                ? LayerMask.NameToLayer("ground")
                : 0;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.2f, 0.3f, 0.5f);
            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
            return go;
        }

        private static GameObject CreateWall(string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            go.transform.localScale = new Vector3(1f, 20f, 1f);
            go.layer = LayerMask.NameToLayer("ground") >= 0
                ? LayerMask.NameToLayer("ground")
                : 0;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
            return go;
        }

        private static void AddScenesToBuildSettings()
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(
                EditorBuildSettings.scenes);

            foreach (var (name, _) in Blocks)
            {
                string path = $"{ScenesFolder}/{name}.unity";
                bool alreadyAdded = false;
                foreach (var s in scenes)
                {
                    if (s.path == path) { alreadyAdded = true; break; }
                }
                if (!alreadyAdded)
                    scenes.Add(new EditorBuildSettingsScene(path, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[TutorialSceneCreator] Escenas agregadas al Build Settings.");
        }

        [MenuItem("CODEX/Tutorial/Eliminar escena Tutorial.unity antigua")]
        public static void DeleteOldTutorialScene()
        {
            string oldPath = "Assets/Scenes/Tutorial.unity";
            if (!File.Exists(Path.GetFullPath(oldPath)))
            {
                EditorUtility.DisplayDialog("No encontrada",
                    $"No existe el archivo:\n{oldPath}", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "Eliminar Tutorial.unity",
                    $"¿Eliminar la escena antigua?\n{oldPath}\n\nEsta acción no se puede deshacer.",
                    "Sí, eliminar", "Cancelar"))
                return;

            AssetDatabase.DeleteAsset(oldPath);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Listo", "Tutorial.unity eliminado.", "OK");
        }
    }
}
