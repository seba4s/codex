using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using CODEX.Tutorial;
using CODEX.Tutorial.Blocks;
using CODEX.Enemies;
using CODEX.Player;
using CODEX.Systems;

/// <summary>
/// Configura T07_EnemigoCombinados – Corredor con 2 grupos de enemigos.
/// Grupo 1 (TypeA + TypeB) custodia una puerta central.
/// Grupo 2 (TypeB + TypeC) desbloquea el paso al tercer terminal al ser eliminados.
/// OnAllEnemiesDefeated del Grupo 2 → Block07.OnSecondGroupCleared (conectado automáticamente).
/// Menú: CODEX → Setup Escenas → T07 – Enemigos Combinados
/// </summary>
public static class T07_SceneSetup
{
    [MenuItem("CODEX/Setup Escenas/T07 – Enemigos Combinados")]
    public static void Setup()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.name.Contains("T07"))
        {
            bool ok = EditorUtility.DisplayDialog("CODEX – Setup T07",
                $"La escena activa es '{scene.name}'.\n¿Continuar de todas formas?",
                "Sí", "Cancelar");
            if (!ok) return;
        }

        Undo.SetCurrentGroupName("CODEX T07 Setup");
        int group = Undo.GetCurrentGroup();

        // ── Infraestructura común ──────────────────────────────────────────────
        // Corredor amplio: 60 unidades, centrado en x=5 → de -25 a 35
        TutorialSceneSetupShared.SetupGround("T07", 60f, 5f);
        TutorialSceneSetupShared.SetupCamera();
        TutorialSceneSetupShared.SetupBackground("Assets/Sprites/UI/Tutorial/fondo7.png");
        TutorialSceneSetupShared.SetupGlobalLight();

        var spawnPoint = TutorialSceneSetupShared.SetupSpawnPoint(
            new Vector3(-7f, TutorialSceneSetupShared.FloorY, 0f));
        var player = TutorialSceneSetupShared.SetupPlayer(spawnPoint);
        var canvas = TutorialSceneSetupShared.SetupCanvas();
        var luma   = TutorialSceneSetupShared.SetupLUMA(canvas);
                     TutorialSceneSetupShared.SetupHUD(canvas, player);

        TutorialSceneSetupShared.SetupTutorialManager();
        TutorialSceneSetupShared.SetupCheckpointManager();
        TutorialSceneSetupShared.SetupSceneTransition(canvas);

        // ── Grupo 1: TypeA (estático) + TypeB (patrulla) ─────────────────────

        var g1a = TutorialSceneSetupShared.SetupEnemy(
            "Enemy_G1_TypeA",
            new Vector3(-4f, TutorialSceneSetupShared.FloorY, 0f),
            InfectedFile.EnemyType.TypeA_Static);

        var g1b = TutorialSceneSetupShared.SetupEnemy(
            "Enemy_G1_TypeB",
            new Vector3(-2f, TutorialSceneSetupShared.FloorY, 0f),
            InfectedFile.EnemyType.TypeB_Patrol);

        // Puerta central — se elimina cuando el Grupo 1 es derrotado
        var gateMid = SetupGate("Gate_T07_Middle", new Vector3(2f, 0f, 0f));

        // TutorialEnemyCounter Grupo 1 — quita la puerta al derrotar G1
        SetupEnemyCounter("EnemyCounter_Group1", gateMid,
            g1a.GetComponent<InfectedFile>(),
            g1b.GetComponent<InfectedFile>());

        // ── Grupo 2: TypeB (patrulla) + TypeC (proyectiles) ──────────────────

        var g2b = TutorialSceneSetupShared.SetupEnemy(
            "Enemy_G2_TypeB",
            new Vector3(7f, TutorialSceneSetupShared.FloorY, 0f),
            InfectedFile.EnemyType.TypeB_Patrol);

        var g2c = TutorialSceneSetupShared.SetupEnemy(
            "Enemy_G2_TypeC",
            new Vector3(10f, TutorialSceneSetupShared.FloorY, 0f),
            InfectedFile.EnemyType.TypeC_Projectile);

        // TutorialEnemyCounter Grupo 2 — sin puerta; dispara evento para Block07
        var counter2 = SetupEnemyCounter("EnemyCounter_Group2", null,
            g2b.GetComponent<InfectedFile>(),
            g2c.GetComponent<InfectedFile>());

        // ── DataCollectibles dispersos ────────────────────────────────────────

        TutorialSceneSetupShared.SetupDataCollectible(
            "Data_T07_A", new Vector3(-6f, TutorialSceneSetupShared.FloorY + 1f, 0f));
        TutorialSceneSetupShared.SetupDataCollectible(
            "Data_T07_B", new Vector3(3f,  TutorialSceneSetupShared.FloorY + 1f, 0f));
        TutorialSceneSetupShared.SetupDataCollectible(
            "Data_T07_C", new Vector3(9f,  TutorialSceneSetupShared.FloorY + 1f, 0f));

        // ── SceneLoader con condición: Grupo 2 eliminado ──────────────────────

        var loaderGO = TutorialSceneSetupShared.SetupSceneLoader(
            "SceneLoader_T07_to_T08",
            new Vector3(15f, 0f, 0f),
            requireCondition: true,
            conditionMsg: "Derrota todos los archivos infectados antes de continuar.");

        // Asignar enemyCondition al TutorialSceneLoader
        var loader   = loaderGO.GetComponent<TutorialSceneLoader>();
        var loaderSO = new SerializedObject(loader);
        loaderSO.FindProperty("enemyCondition").objectReferenceValue = counter2;
        loaderSO.ApplyModifiedProperties();

        // ── Block 07 ─────────────────────────────────────────────────────────

        var block07 = SetupBlock07(luma);

        // C3 FIX: conectar EnemyCounter_Group2.OnAllEnemiesDefeated → Block07.OnSecondGroupCleared
        // Antes requería wiring manual en Inspector — ahora el setup lo hace automáticamente.
        if (counter2 != null && block07 != null)
            UnityEventTools.AddPersistentListener(
                counter2.OnAllEnemiesDefeated, block07.OnSecondGroupCleared);

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        EditorUtility.DisplayDialog("CODEX – T07 Listo",
            "T07_EnemigoCombinados configurada.\n\n" +
            "✓ Corredor ancho (60u) con muro izquierdo en x=-26\n" +
            "✓ Grupo 1: Enemy_G1_TypeA + Enemy_G1_TypeB\n" +
            "✓ Gate_T07_Middle (púrpura) en x=2 — se oculta al derrotar G1\n" +
            "✓ Grupo 2: Enemy_G2_TypeB + Enemy_G2_TypeC\n" +
            "✓ EnemyCounter_Group2.OnAllEnemiesDefeated → Block07.OnSecondGroupCleared (conectado)\n" +
            "✓ 3 DataCollectibles en x=-6, 3, 9\n" +
            "✓ SceneLoader_T07_to_T08 en x=15 (requiere Grupo 2 eliminado)", "OK");
    }

    // ── GATE (barrera de energía infectada) ───────────────────────────────────

    static GameObject SetupGate(string name, Vector3 pos)
    {
        if (GameObject.Find(name) != null) return GameObject.Find(name);

        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.position   = pos;
        go.transform.localScale = new Vector3(1f, 8f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
        sr.color        = new Color(0.55f, 0.10f, 0.55f, 0.90f);   // púrpura — barrera infectada
        sr.sortingOrder = 1;

        go.AddComponent<BoxCollider2D>();   // sólido — bloquea el paso hasta que el contador lo oculta
        return go;
    }

    // ── ENEMY COUNTER ─────────────────────────────────────────────────────────

    static TutorialEnemyCounter SetupEnemyCounter(
        string goName, GameObject gate, params InfectedFile[] enemies)
    {
        var existing = GameObject.Find(goName);
        var go = existing ?? new GameObject(goName);
        if (existing == null) Undo.RegisterCreatedObjectUndo(go, "Create " + goName);

        var counter = go.GetComponent<TutorialEnemyCounter>()
                   ?? Undo.AddComponent<TutorialEnemyCounter>(go);

        var so  = new SerializedObject(counter);
        var arr = so.FindProperty("enemies");
        arr.arraySize = enemies.Length;
        for (int i = 0; i < enemies.Length; i++)
            arr.GetArrayElementAtIndex(i).objectReferenceValue = enemies[i];

        so.FindProperty("gate").objectReferenceValue = gate;   // null = sin puerta para Grupo 2
        so.ApplyModifiedProperties();

        return counter;
    }

    // ── BLOCK 07 ─────────────────────────────────────────────────────────────

    static Block07_EnemigoCombinados SetupBlock07(LumaGuide luma)
    {
        var existing = GameObject.Find("BlockManager");
        var bmGO = existing ?? new GameObject("BlockManager");
        if (existing == null) Undo.RegisterCreatedObjectUndo(bmGO, "Create BlockManager");

        var block = bmGO.GetComponent<Block07_EnemigoCombinados>()
                 ?? Undo.AddComponent<Block07_EnemigoCombinados>(bmGO);

        var so = new SerializedObject(block);
        so.FindProperty("luma").objectReferenceValue = luma;
        so.ApplyModifiedProperties();

        return block;
    }
}
