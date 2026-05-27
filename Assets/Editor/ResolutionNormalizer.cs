using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace CODEX.Editor
{
    /// <summary>
    /// Normaliza la resolución de referencia (1920×1080) en todas las escenas del proyecto:
    ///   · CanvasScaler → Scale With Screen Size, 1920×1080, Match 0.5
    ///   · Muestra el orthographic size de cada cámara principal (no lo toca)
    ///   · Opción separada para imponer un orthographic size en todas las cámaras
    /// </summary>
    public static class ResolutionNormalizer
    {
        private const int TARGET_W = 1920;
        private const int TARGET_H = 1080;

        // ── Menú principal ────────────────────────────────────────────────────────

        [MenuItem("CODEX/Resolución/1) Normalizar Canvas → 1920×1080 (todas las escenas)")]
        public static void NormalizeAllCanvases()
        {
            string currentScenePath = EditorSceneManager.GetActiveScene().path;
            bool dirty = EditorSceneManager.GetActiveScene().isDirty;

            if (dirty)
            {
                bool save = EditorUtility.DisplayDialog(
                    "Escena sin guardar",
                    "La escena actual tiene cambios sin guardar. ¿Guardar antes de continuar?",
                    "Guardar", "Cancelar");

                if (!save) return;
                EditorSceneManager.SaveOpenScenes();
            }

            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
            int fixed_count = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                bool sceneChanged = false;
                var scalers = Object.FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include);

                foreach (var scaler in scalers)
                {
                    bool changed = false;

                    if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
                    { scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; changed = true; }

                    if (scaler.referenceResolution != new Vector2(TARGET_W, TARGET_H))
                    { scaler.referenceResolution = new Vector2(TARGET_W, TARGET_H); changed = true; }

                    if (scaler.screenMatchMode != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
                    { scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; changed = true; }

                    if (!Mathf.Approximately(scaler.matchWidthOrHeight, 0.5f))
                    { scaler.matchWidthOrHeight = 0.5f; changed = true; }

                    if (changed)
                    {
                        EditorUtility.SetDirty(scaler);
                        sceneChanged = true;
                        fixed_count++;
                        Debug.Log($"[ResNorm] Canvas ajustado en <b>{scene.name}</b> → {scaler.gameObject.name}");
                    }
                }

                // Reportar cámara (solo info, sin modificar)
                var cam = Camera.main;
                if (cam != null && cam.orthographic)
                    Debug.Log($"[ResNorm] {scene.name} — Cámara orthographicSize = <b>{cam.orthographicSize}</b>");

                if (sceneChanged)
                    EditorSceneManager.SaveScene(scene);
            }

            // Volver a la escena original
            if (!string.IsNullOrEmpty(currentScenePath))
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);

            EditorUtility.DisplayDialog(
                "Resolución normalizada",
                $"✅ {fixed_count} Canvas ajustados a 1920×1080.\n\nRevisa la consola para ver el orthographicSize de cada cámara.",
                "OK");
        }

        // ── Imponer orthographic size en todas las cámaras ───────────────────────

        [MenuItem("CODEX/Resolución/2) Imponer Orthographic Size en todas las cámaras...")]
        public static void SetOrthographicSizeAll()
        {
            float targetSize = 0f;

            bool confirmed = EditorUtility.DisplayDialog(
                "Orthographic Size",
                "Esta opción sobreescribe el Orthographic Size de TODAS las cámaras principales.\n\n" +
                "Usa la opción de menú solo después de anotar el valor que ves en la consola " +
                "tras ejecutar la opción 1.\n\n" +
                "¿Continuar? (cambia el valor en el campo Size del componente Camera manualmente " +
                "si prefieres hacerlo escena por escena).",
                "Continuar", "Cancelar");

            if (!confirmed) return;

            targetSize = GetFloatFromUser("Orthographic Size objetivo para 1920×1080", 270f);
            if (targetSize <= 0f) return;

            string currentScenePath = EditorSceneManager.GetActiveScene().path;
            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                bool changed = false;

                // Buscar todas las cámaras con orthographic
                var cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include);
                foreach (var cam in cameras)
                {
                    if (!cam.orthographic) continue;
                    if (Mathf.Approximately(cam.orthographicSize, targetSize)) continue;

                    cam.orthographicSize = targetSize;
                    EditorUtility.SetDirty(cam);
                    changed = true;
                    Debug.Log($"[ResNorm] {scene.name} — {cam.gameObject.name} → orthographicSize = {targetSize}");
                }

                if (changed)
                    EditorSceneManager.SaveScene(scene);
            }

            if (!string.IsNullOrEmpty(currentScenePath))
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);

            EditorUtility.DisplayDialog("Listo", $"Orthographic Size = {targetSize} aplicado a todas las escenas.", "OK");
        }

        // ── Project Settings: resolución por defecto en build ────────────────────

        [MenuItem("CODEX/Resolución/3) Establecer resolución de build → 1920×1080")]
        public static void SetBuildResolution()
        {
            PlayerSettings.defaultScreenWidth  = TARGET_W;
            PlayerSettings.defaultScreenHeight = TARGET_H;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;

            Debug.Log($"[ResNorm] Build resolution → {TARGET_W}×{TARGET_H}, FullScreenWindow");
            EditorUtility.DisplayDialog("Build Resolution",
                $"✅ Resolución de build: {TARGET_W}×{TARGET_H} (FullScreen Window).\n\n" +
                "Guarda el proyecto para que el cambio persista (Ctrl+S).",
                "OK");
        }

        // ── Helper: pedir float por pantalla ─────────────────────────────────────

        private static float GetFloatFromUser(string label, float defaultValue)
        {
            // No hay InputDialog nativo en Unity Editor — cambia defaultValue en el script.
            Debug.Log($"[ResNorm] {label}: usando valor {defaultValue}. " +
                      "Para cambiarlo edita 'defaultValue' en ResolutionNormalizer.cs → GetFloatFromUser.");
            return defaultValue;
        }
    }
}
