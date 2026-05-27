using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CODEX.Editor
{
    /// <summary>
    /// Encuentra y elimina componentes con script faltante ("Missing Script")
    /// en todas las escenas del proyecto y en los prefabs de Assets/.
    /// </summary>
    public static class MissingScriptCleaner
    {
        // ── Escenas ───────────────────────────────────────────────────────────────

        [MenuItem("CODEX/Limpieza/Eliminar Missing Scripts en todas las escenas")]
        public static void CleanAllScenes()
        {
            string currentPath = EditorSceneManager.GetActiveScene().path;
            bool wasDirty = EditorSceneManager.GetActiveScene().isDirty;

            if (wasDirty)
            {
                bool save = EditorUtility.DisplayDialog(
                    "Escena sin guardar",
                    "La escena actual tiene cambios sin guardar. ¿Guardar antes de continuar?",
                    "Guardar", "Cancelar");
                if (!save) return;
                EditorSceneManager.SaveOpenScenes();
            }

            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
            int totalRemoved = 0;
            var report = new List<string>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                int removed = CleanScene(scene.name, report);
                totalRemoved += removed;

                if (removed > 0)
                    EditorSceneManager.SaveScene(scene);
            }

            // Volver a la escena original
            if (!string.IsNullOrEmpty(currentPath))
                EditorSceneManager.OpenScene(currentPath, OpenSceneMode.Single);

            // Resumen
            string summary = totalRemoved == 0
                ? "✅ No se encontraron Missing Scripts en ninguna escena."
                : $"✅ {totalRemoved} Missing Script(s) eliminados.\n\n" + string.Join("\n", report);

            Debug.Log("[MissingScriptCleaner] " + summary);
            EditorUtility.DisplayDialog("Missing Scripts — Resultado", summary, "OK");
        }

        // ── Solo la escena activa ─────────────────────────────────────────────────

        [MenuItem("CODEX/Limpieza/Eliminar Missing Scripts en la escena activa")]
        public static void CleanActiveScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            var report = new List<string>();
            int removed = CleanScene(scene.name, report);

            if (removed > 0)
                EditorSceneManager.SaveScene(scene);

            string summary = removed == 0
                ? $"✅ No se encontraron Missing Scripts en '{scene.name}'."
                : $"✅ {removed} Missing Script(s) eliminados de '{scene.name}'.\n\n" + string.Join("\n", report);

            Debug.Log("[MissingScriptCleaner] " + summary);
            EditorUtility.DisplayDialog("Missing Scripts — Resultado", summary, "OK");
        }

        // ── Prefabs en Assets/ ────────────────────────────────────────────────────

        [MenuItem("CODEX/Limpieza/Eliminar Missing Scripts en Prefabs")]
        public static void CleanPrefabs()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            int totalRemoved = 0;
            var report = new List<string>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefab);
                if (count > 0)
                {
                    totalRemoved += count;
                    report.Add($"  · {path}  ({count} eliminado/s)");
                    EditorUtility.SetDirty(prefab);
                    Debug.Log($"[MissingScriptCleaner] Prefab '{path}' — {count} Missing Script(s) eliminados.");
                }
            }

            AssetDatabase.SaveAssets();

            string summary = totalRemoved == 0
                ? "✅ No se encontraron Missing Scripts en ningún Prefab."
                : $"✅ {totalRemoved} Missing Script(s) eliminados de Prefabs.\n\n" + string.Join("\n", report);

            Debug.Log("[MissingScriptCleaner] " + summary);
            EditorUtility.DisplayDialog("Missing Scripts (Prefabs) — Resultado", summary, "OK");
        }

        // ── Core ──────────────────────────────────────────────────────────────────

        private static int CleanScene(string sceneName, List<string> report)
        {
            int total = 0;
            var roots = UnityEngine.SceneManagement.SceneManager
                            .GetActiveScene().GetRootGameObjects();

            foreach (var root in roots)
            {
                int count = CleanGameObject(root);
                if (count > 0)
                {
                    total += count;
                    report.Add($"  · [{sceneName}] {root.name}  ({count} eliminado/s)");
                    Debug.Log($"[MissingScriptCleaner] [{sceneName}] '{root.name}' — {count} Missing Script(s) eliminados.");
                }
            }

            return total;
        }

        private static int CleanGameObject(GameObject go)
        {
            int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);

            foreach (Transform child in go.transform)
                count += CleanGameObject(child.gameObject);

            return count;
        }
    }
}
