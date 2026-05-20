using UnityEditor;
using UnityEngine;

public static class PlayerSettingsSetup
{
    [MenuItem("CODEX/Setup/Resolución HD (1920x1080)")]
    public static void Setup()
    {
        PlayerSettings.defaultScreenWidth  = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.fullScreenMode      = FullScreenMode.ExclusiveFullScreen;
        PlayerSettings.runInBackground     = false;
        PlayerSettings.resizableWindow     = false;

        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("CODEX – Player Settings",
            "Resolución configurada:\n\n" +
            "• 1920 × 1080\n" +
            "• Exclusive Fullscreen\n\n" +
            "Haz Build and Run de nuevo.", "OK");
    }
}
