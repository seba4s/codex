using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using CODEX.Tutorial;
using CODEX.Tutorial.Blocks;
using CODEX.Player;
using CODEX.Systems;

/// <summary>
/// Configura T06_PlataformasEspeciales – Abismo con 4 plataformas colapsables.
/// Diseño: suelo izquierdo → abismo (x=0→8) → 4 FallingPlatforms → suelo derecho.
/// Al cruzar el abismo, CheckpointTrigger llama Block06.OnSequenceCleared.
/// Menú: CODEX → Setup Escenas → T06 – Plataformas
/// </summary>
public static class T06_SceneSetup
{
    private const float FloorY  = TutorialSceneSetupShared.FloorY;   // -1.5f
    private const float GroundY = TutorialSceneSetupShared.GroundY;  // -2.5f

    [MenuItem("CODEX/Setup Escenas/T06 – Plataformas")]
    public static void Setup()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.name.Contains("T06"))
        {
            bool ok = EditorUtility.DisplayDialog("CODEX – Setup T06",
                $"La escena activa es '{scene.name}'.\n¿Continuar de todas formas?",
                "Sí", "Cancelar");
            if (!ok) return;
        }

        Undo.SetCurrentGroupName("CODEX T06 Setup");
        int group = Undo.GetCurrentGroup();

        // ── Infraestructura común ──────────────────────────────────────────────
        SetupSplitGround();   // suelo dividido con abismo — no usa SetupGround compartido
        TutorialSceneSetupShared.SetupCamera();
        TutorialSceneSetupShared.SetupBackground("Assets/Sprites/UI/Tutorial/fondo6.png");
        TutorialSceneSetupShared.SetupGlobalLight();

        var spawnPoint = TutorialSceneSetupShared.SetupSpawnPoint(
            new Vector3(-7f, FloorY, 0f));
        var player = TutorialSceneSetupShared.SetupPlayer(spawnPoint);
        var canvas = TutorialSceneSetupShared.SetupCanvas();
        var luma   = TutorialSceneSetupShared.SetupLUMA(canvas);
                     TutorialSceneSetupShared.SetupHUD(canvas, player);

        TutorialSceneSetupShared.SetupTutorialManager();
        TutorialSceneSetupShared.SetupCheckpointManager();
        TutorialSceneSetupShared.SetupSceneTransition(canvas);

        // ── Mecánica específica T06 ────────────────────────────────────────────
        SetupFallingPlatforms();
        SetupDeathZone();

        var checkpoint = SetupCheckpointTrigger();

        TutorialSceneSetupShared.SetupSceneLoader(
            "SceneLoader_T06_to_T07", new Vector3(15f, 0f, 0f));

        var block06 = SetupBlock06(luma);

        // C3 FIX: conectar CheckpointTrigger.OnPlayerPassed → Block06.OnSequenceCleared
        // Antes debía hacerse manualmente en Inspector; ahora el setup lo hace automáticamente.
        if (checkpoint != null && block06 != null)
            UnityEventTools.AddPersistentListener(
                checkpoint.OnPlayerPassed, block06.OnSequenceCleared);

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        EditorUtility.DisplayDialog("CODEX – T06 Listo",
            "T06_PlataformasEspeciales configurada.\n\n" +
            "✓ Suelo izquierdo (x=-20 → 0) + muro izquierdo\n" +
            "✓ Abismo (x=0 → 8) — vacío con DeathZone trigger en y=-8\n" +
            "✓ 4 FallingPlatforms en x=1,3,5,7 sobre el abismo\n" +
            "✓ Suelo derecho (x=8 → 24) con tierra firme\n" +
            "✓ CheckpointTrigger_T06 en x=10 (respawn: SpawnPoint_AfterGap)\n" +
            "✓ CheckpointTrigger.OnPlayerPassed → Block06.OnSequenceCleared (conectado)\n" +
            "✓ SceneLoader_T06_to_T07 en x=15\n" +
            "✓ Block06_Plataformas conectado (luma, platformSequenceStart)", "OK");
    }

    // ── SUELO DIVIDIDO (izquierdo + derecho con abismo) ──────────────────────

    static void SetupSplitGround()
    {
        // Suelo izquierdo: x de -20 a 0 (width=20, centerX=-10)
        if (GameObject.Find("Ground_T06_Left") == null)
        {
            var go = new GameObject("Ground_T06_Left");
            Undo.RegisterCreatedObjectUndo(go, "Create Ground Left");
            go.transform.position   = new Vector3(-10f, GroundY, 0f);
            go.transform.localScale = new Vector3(20f, 1f, 1f);
            int layer = LayerMask.NameToLayer("ground");
            go.layer = layer >= 0 ? layer : 0;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
            sr.color  = new Color(0.10f, 0.16f, 0.30f);
            go.AddComponent<BoxCollider2D>();
        }

        // Muro izquierdo — evita que el jugador salga por la izquierda
        if (GameObject.Find("Wall_Left_T06") == null)
        {
            var go = new GameObject("Wall_Left_T06");
            Undo.RegisterCreatedObjectUndo(go, "Create Wall Left");
            go.transform.position   = new Vector3(-21f, 0f, 0f);
            go.transform.localScale = new Vector3(1f, 20f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
            sr.color  = new Color(0.10f, 0.16f, 0.30f);
            go.AddComponent<BoxCollider2D>();
        }

        // Suelo derecho: x de 8 a 24 (width=16, centerX=16)
        if (GameObject.Find("Ground_T06_Right") == null)
        {
            var go = new GameObject("Ground_T06_Right");
            Undo.RegisterCreatedObjectUndo(go, "Create Ground Right");
            go.transform.position   = new Vector3(16f, GroundY, 0f);
            go.transform.localScale = new Vector3(16f, 1f, 1f);
            int layer = LayerMask.NameToLayer("ground");
            go.layer = layer >= 0 ? layer : 0;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
            sr.color  = new Color(0.10f, 0.16f, 0.30f);
            go.AddComponent<BoxCollider2D>();
        }
    }

    // ── PLATAFORMAS COLAPSABLES ───────────────────────────────────────────────

    static void SetupFallingPlatforms()
    {
        // 4 plataformas sobre el abismo — x=1,3,5,7 (gap x=0 a x=8)
        float[] xPos = { 1f, 3f, 5f, 7f };
        for (int i = 0; i < xPos.Length; i++)
        {
            string pName = $"FallingPlatform_{i}";
            if (GameObject.Find(pName) != null) continue;

            var go = new GameObject(pName);
            Undo.RegisterCreatedObjectUndo(go, "Create " + pName);
            go.transform.position   = new Vector3(xPos[i], FloorY, 0f);
            go.transform.localScale = new Vector3(1.5f, 0.4f, 1f);

            int layer = LayerMask.NameToLayer("ground");
            go.layer = layer >= 0 ? layer : 0;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
            sr.color        = new Color(0.85f, 0.38f, 0.08f);   // naranja — sector corrupto/inestable
            sr.sortingOrder = 1;

            go.AddComponent<BoxCollider2D>();

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType    = RigidbodyType2D.Kinematic;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            var fp = go.AddComponent<FallingPlatform>();
            var so = new SerializedObject(fp);
            so.FindProperty("fallDelay").floatValue  = 0.4f;
            so.FindProperty("resetDelay").floatValue = 3f;
            so.ApplyModifiedProperties();
        }
    }

    // ── DEATH ZONE ────────────────────────────────────────────────────────────

    static void SetupDeathZone()
    {
        if (GameObject.Find("DeathZone_T06") != null) return;

        var go = new GameObject("DeathZone_T06");
        Undo.RegisterCreatedObjectUndo(go, "Create DeathZone");
        go.transform.position   = new Vector3(0f, -8f, 0f);
        go.transform.localScale = new Vector3(60f, 2f, 1f);

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        // NOTA: añadir un script de muerte/respawn que llame CheckpointManager si existe.
        // Por defecto el jugador cae y el CheckpointManager gestiona el respawn al activarse el trigger.
        Debug.Log("[T06 Setup] DeathZone_T06 creada. " +
                  "Si tienes un script de DeathZone (KillZone, Pit, etc.), añádelo manualmente.");
    }

    // ── CHECKPOINT TRIGGER ────────────────────────────────────────────────────

    static CheckpointTrigger SetupCheckpointTrigger()
    {
        if (GameObject.Find("CheckpointTrigger_T06") != null)
        {
            var found = GameObject.Find("CheckpointTrigger_T06");
            return found.GetComponent<CheckpointTrigger>();
        }

        // Punto de reaparición al otro lado del abismo
        var spawnAfterGap = new GameObject("SpawnPoint_AfterGap");
        Undo.RegisterCreatedObjectUndo(spawnAfterGap, "Create SpawnPoint AfterGap");
        spawnAfterGap.transform.position = new Vector3(9f, FloorY, 0f);

        // Trigger invisible al inicio del suelo derecho
        var go = new GameObject("CheckpointTrigger_T06");
        Undo.RegisterCreatedObjectUndo(go, "Create CheckpointTrigger");
        go.transform.position   = new Vector3(10f, 0f, 0f);
        go.transform.localScale = new Vector3(2f, 8f, 1f);

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        var ct = go.AddComponent<CheckpointTrigger>();
        var so = new SerializedObject(ct);
        so.FindProperty("spawnPoint").objectReferenceValue = spawnAfterGap.transform;
        so.ApplyModifiedProperties();

        return ct;
    }

    // ── BLOCK 06 ─────────────────────────────────────────────────────────────

    static Block06_Plataformas SetupBlock06(LumaGuide luma)
    {
        var existing = GameObject.Find("BlockManager");
        var bmGO = existing ?? new GameObject("BlockManager");
        if (existing == null) Undo.RegisterCreatedObjectUndo(bmGO, "Create BlockManager");

        var block = bmGO.GetComponent<Block06_Plataformas>()
                 ?? Undo.AddComponent<Block06_Plataformas>(bmGO);

        var firstPlatform = GameObject.Find("FallingPlatform_0");

        var so = new SerializedObject(block);
        so.FindProperty("luma").objectReferenceValue = luma;
        if (firstPlatform != null)
            so.FindProperty("platformSequenceStart").objectReferenceValue = firstPlatform.transform;
        so.ApplyModifiedProperties();

        return block;
    }
}
