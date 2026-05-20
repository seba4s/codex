using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Configura Build Settings con todas las escenas del proyecto.
/// Menú: CODEX → Setup → Configurar Build Settings
/// </summary>
public static class BuildSettingsSetup
{
    [MenuItem("CODEX/Setup/Configurar Build Settings")]
    public static void Setup()
    {
        // Orden de escenas en el build
        string[] scenePaths =
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Tutorial/T01_Materializacion.unity",
            "Assets/Scenes/Tutorial/T02_Disparo.unity",
            "Assets/Scenes/Tutorial/T03_RecoleccionDatos.unity",
            "Assets/Scenes/Tutorial/T04_DanoYEsquive.unity",
            "Assets/Scenes/Tutorial/T05_Terminal.unity",
            "Assets/Scenes/Tutorial/T06_PlataformasEspeciales.unity",
            "Assets/Scenes/Tutorial/T07_EnemigoCombinados.unity",
            "Assets/Scenes/Tutorial/T08_PuertoSalida.unity",
            "Assets/Scenes/EscogerPartida.unity",
            "Assets/Scenes/Mejoras.unity",
            "Assets/Scenes/Opciones.unity",
        };

        var validScenes = new List<EditorBuildSettingsScene>();
        var missing     = new List<string>();

        foreach (var path in scenePaths)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
                validScenes.Add(new EditorBuildSettingsScene(path, true));
            else
                missing.Add(path);
        }

        EditorBuildSettings.scenes = validScenes.ToArray();

        string msg = $"Build Settings actualizado.\n{validScenes.Count} escenas añadidas.";
        if (missing.Count > 0)
            msg += "\n\nNo encontradas (ignoradas):\n• " + string.Join("\n• ", missing);

        EditorUtility.DisplayDialog("CODEX – Build Settings", msg, "OK");
    }
}
