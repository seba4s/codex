using UnityEngine;
using UnityEditor;

public static class CreateTutorialPrefabs
{
    [MenuItem("Tools/Create Tutorial Prefabs")]
    public static void Create()
    {
        CreateFolders();
        CreatePlayerPrefab();
        CreateEnemyPrefab();
        CreateProjectilePrefab();
        CreateDataPrefab();
        CreateTerminalPrefab();
        Debug.Log("Prefabs creados en Assets/Prefabs/Tutorial/");
        EditorUtility.DisplayDialog("Listo", "Prefabs creados en Assets/Prefabs/Tutorial/", "OK");
    }

    static void CreateFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Tutorial"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Tutorial");

        string[] subs = { "Player", "Enemies", "Projectiles", "Data", "Interactables" };
        foreach (string s in subs)
        {
            string path = "Assets/Prefabs/Tutorial/" + s;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder("Assets/Prefabs/Tutorial", s);
        }
        AssetDatabase.Refresh();
    }

    static void CreatePlayerPrefab()
    {
        GameObject go = new GameObject("Player_Tutorial");
        go.tag = "Player";
        go.layer = LayerMask.NameToLayer("Player");

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        go.AddComponent<CapsuleCollider2D>();
        go.AddComponent<CODEX.Player.PlayerController>();
        go.AddComponent<CODEX.Player.ShootingSystem>();

        PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/Tutorial/Player/Player_Tutorial.prefab");
        Object.DestroyImmediate(go);
    }

    static void CreateEnemyPrefab()
    {
        GameObject go = new GameObject("InfectedFile_Basic");
        go.tag = "Enemy";

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;

        go.AddComponent<CODEX.Enemies.InfectedFile>();

        PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/Tutorial/Enemies/InfectedFile_Basic.prefab");
        Object.DestroyImmediate(go);
    }

    static void CreateProjectilePrefab()
    {
        GameObject go = new GameObject("Projectile_SHOT01");
        go.tag = "Projectile";

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.15f;
        col.isTrigger = true;

        go.AddComponent<CODEX.Systems.Projectile>();

        PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/Tutorial/Projectiles/Projectile_SHOT01.prefab");
        Object.DestroyImmediate(go);
    }

    static void CreateDataPrefab()
    {
        GameObject go = new GameObject("Data_Basic");

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;
        col.isTrigger = true;

        go.AddComponent<CODEX.Systems.DataCollectible>();

        PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/Tutorial/Data/Data_Basic.prefab");
        Object.DestroyImmediate(go);
    }

    static void CreateTerminalPrefab()
    {
        GameObject go = new GameObject("RepairTerminal_01");
        go.tag = "Interactable";

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.5f, 2f);
        col.isTrigger = true;

        go.AddComponent<CODEX.Systems.RepairTerminal>();

        PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/Tutorial/Interactables/RepairTerminal_01.prefab");
        Object.DestroyImmediate(go);
    }
}
