using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using TMPro;
using CODEX.Tutorial;
using CODEX.Tutorial.Blocks;
using CODEX.Enemies;
using CODEX.Systems;

/// <summary>
/// Configura T02_Disparo con el mismo layout que T01.
/// Menú: CODEX → Setup Escenas → T02 – Sistema de Disparo
/// </summary>
public static class T02_SceneSetup
{
    // Mismas constantes de layout que T01
    private const float FloorY       = -1.5f;   // Y donde pisan los pies
    private const float GroundY      = -2.5f;   // Y centro del suelo
    private const float CamSize      = 5f;

    [MenuItem("CODEX/Setup Escenas/T02 – Sistema de Disparo")]
    public static void Setup()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.name.Contains("T02"))
        {
            bool ok = EditorUtility.DisplayDialog("CODEX – Setup T02",
                $"La escena activa es '{scene.name}'.\n¿Continuar de todas formas?",
                "Sí", "Cancelar");
            if (!ok) return;
        }

        Undo.SetCurrentGroupName("CODEX T02 Setup");
        int group = Undo.GetCurrentGroup();

        SetupGround();
        SetupCamera();
        SetupBackground();
        SetupGlobalLight();
        var spawnPoint  = SetupSpawnPoint();
        var player      = SetupPlayer(spawnPoint);
        var canvas      = SetupCanvas();
        var luma        = SetupLUMA(canvas);
        var enemy       = SetupEnemy();
        var blockedDoor = SetupBlockedDoor(enemy, luma);
        var openDoor    = SetupOpenDoor();
        SetupBlock02(luma, enemy, blockedDoor, openDoor);
        SetupTutorialManager();

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        EditorUtility.DisplayDialog("CODEX – T02 Listo",
            "T02 configurada con el mismo layout que T01.\n\n" +
            "✓ Suelo, cámara y fondo\n" +
            "✓ Jugador en SpawnPoint (-7, -0.5)\n" +
            "✓ Enemigo en (3, FloorY)\n" +
            "✓ Puerta invisible en (6.5, 0)\n" +
            "✓ Salida a T03 en (9, 0) — inactiva hasta matar enemigo\n" +
            "✓ LUMA con diálogos de T02\n\n" +
            "Asigna EnemyController.controller al Animator del enemigo.", "OK");
    }

    // ── SUELO Y PAREDES ──────────────────────────────────────────────────────

    static void SetupGround()
    {
        // Suelo principal
        if (GameObject.Find("Ground_T02") == null)
        {
            var go = new GameObject("Ground_T02");
            Undo.RegisterCreatedObjectUndo(go, "Create Ground");
            go.transform.position   = new Vector3(0f, GroundY, 0f);
            go.transform.localScale = new Vector3(40f, 1f, 1f);
            go.layer = LayerMask.NameToLayer("ground") >= 0 ? LayerMask.NameToLayer("ground") : 0;

            var sr    = go.AddComponent<SpriteRenderer>();
            sr.sprite = CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
            sr.color  = new Color(0.10f, 0.16f, 0.30f);
            sr.sortingOrder = 0;
            go.AddComponent<BoxCollider2D>();
        }

        // Pared izquierda
        if (GameObject.Find("Wall_Left_T02") == null)
        {
            var go = new GameObject("Wall_Left_T02");
            Undo.RegisterCreatedObjectUndo(go, "Create Left Wall");
            go.transform.position   = new Vector3(-10f, 0f, 0f);
            go.transform.localScale = new Vector3(1f, 20f, 1f);
            var sr    = go.AddComponent<SpriteRenderer>();
            sr.sprite = CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
            sr.color  = new Color(0.10f, 0.16f, 0.30f);
            go.AddComponent<BoxCollider2D>();
        }
    }

    // ── CÁMARA ───────────────────────────────────────────────────────────────

    static void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var camGO = new GameObject("Main Camera");
            Undo.RegisterCreatedObjectUndo(camGO, "Create Camera");
            camGO.tag = "MainCamera";
            cam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
        }

        cam.orthographic       = true;
        cam.orthographicSize   = CamSize;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.backgroundColor    = new Color(0.02f, 0.02f, 0.08f);

        // Misma configuración que T01: cámara fija
        var follow = cam.GetComponent<CameraFollow>();
        if (follow != null) follow.enabled = false;
    }

    // ── FONDO ────────────────────────────────────────────────────────────────

    static void SetupBackground()
    {
        if (GameObject.Find("Background") != null) return;

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Tutorial/fondo2.png");
        var bg     = new GameObject("Background");
        Undo.RegisterCreatedObjectUndo(bg, "Create Background");

        var sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.sortingOrder = -10;
        bg.transform.position = Vector3.zero;

        var cam = Camera.main;
        if (cam != null && sprite != null)
        {
            float h = cam.orthographicSize * 2f;
            float w = h * cam.aspect;
            bg.transform.localScale = new Vector3(w / sprite.bounds.size.x,
                                                   h / sprite.bounds.size.y, 1f);
        }
        else
        {
            bg.transform.localScale = new Vector3(18f, 10f, 1f);
        }

        if (sprite == null)
            Debug.LogWarning("[CODEX T02] fondo2.png no encontrado. Ejecuta CODEX > Configurar Sprites.");
    }

    // ── LUZ GLOBAL ───────────────────────────────────────────────────────────

    static void SetupGlobalLight()
    {
        if (GameObject.Find("Global Light 2D") != null) return;

        var go = new GameObject("Global Light 2D");
        Undo.RegisterCreatedObjectUndo(go, "Create Global Light");

        var light = go.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.intensity = 1f;
        light.color     = Color.white;
    }

    // ── SPAWN POINT ──────────────────────────────────────────────────────────

    static Transform SetupSpawnPoint()
    {
        var go = GameObject.Find("SpawnPoint");
        if (go == null)
        {
            go = new GameObject("SpawnPoint");
            Undo.RegisterCreatedObjectUndo(go, "Create SpawnPoint");
        }
        go.transform.position = new Vector3(-7f, FloorY, 0f);
        return go.transform;
    }

    // ── JUGADOR ──────────────────────────────────────────────────────────────

    static GameObject SetupPlayer(Transform spawnPoint)
    {
        var existing = GameObject.FindGameObjectWithTag("Player");
        if (existing != null) return existing;

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/Tutorial/Player/Player_Tutorial.prefab");

        if (prefab == null)
        {
            Debug.LogWarning("[CODEX T02] Player_Tutorial.prefab no encontrado.");
            return null;
        }

        var player = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        Undo.RegisterCreatedObjectUndo(player, "Create Player");
        player.transform.position = spawnPoint != null
            ? spawnPoint.position
            : new Vector3(-7f, FloorY, 0f);

        // Animator controller
        var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            "Assets/Movimiento/Player_Tutorial.controller");
        if (ctrl != null)
        {
            var anim = player.GetComponentInChildren<Animator>(true);
            if (anim == null) anim = player.AddComponent<Animator>();
            anim.runtimeAnimatorController = ctrl;
        }
        else
        {
            Debug.LogWarning("[CODEX T02] Player_Tutorial.controller no encontrado.");
        }

        return player;
    }

    // ── CANVAS ───────────────────────────────────────────────────────────────

    static GameObject SetupCanvas()
    {
        var existing = GameObject.Find("HUD_Canvas");
        if (existing != null) return existing;

        var go = new GameObject("HUD_Canvas");
        Undo.RegisterCreatedObjectUndo(go, "Create Canvas");

        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight  = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    // ── LUMA ─────────────────────────────────────────────────────────────────

    static LumaGuide SetupLUMA(GameObject canvas)
    {
        var existing = Object.FindAnyObjectByType<LumaGuide>();
        if (existing != null) return existing;

        var panelGO = CreateUIPanel("LUMA_DialoguePanel", canvas.transform,
            new Vector2(0f, 0f), new Vector2(1f, 0.22f));
        panelGO.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.15f, 0.88f);
        panelGO.SetActive(false);

        var speakerGO = CreateTMPChild("SpeakerName", panelGO.transform,
            new Vector2(0.02f, 0.55f), new Vector2(0.98f, 0.95f));
        var speakerTMP = speakerGO.GetComponent<TextMeshProUGUI>();
        speakerTMP.fontSize  = 22;
        speakerTMP.fontStyle = FontStyles.Bold;
        speakerTMP.color     = new Color(0.4f, 0.9f, 1f);

        var msgGO = CreateTMPChild("MessageText", panelGO.transform,
            new Vector2(0.02f, 0.05f), new Vector2(0.98f, 0.52f));
        var msgTMP = msgGO.GetComponent<TextMeshProUGUI>();
        msgTMP.fontSize = 20;
        msgTMP.color    = Color.white;

        var lumaGO = new GameObject("LUMA_Guide");
        Undo.RegisterCreatedObjectUndo(lumaGO, "Create LUMA_Guide");
        lumaGO.transform.position = new Vector3(-4f, FloorY + 2f, 0f);

        var guide = lumaGO.AddComponent<LumaGuide>();
        var so    = new SerializedObject(guide);
        so.FindProperty("dialoguePanel").objectReferenceValue = panelGO;
        so.FindProperty("speakerText").objectReferenceValue   = speakerTMP;
        so.FindProperty("messageText").objectReferenceValue   = msgTMP;
        so.ApplyModifiedProperties();

        return guide;
    }

    // ── ENEMIGO ──────────────────────────────────────────────────────────────

    static InfectedFile SetupEnemy()
    {
        var existing = GameObject.Find("InfectedFile_Blocker");
        if (existing != null) return existing.GetComponent<InfectedFile>();

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/Tutorial/Enemies/InfectedFile_Basic.prefab");

        GameObject go;
        if (prefab != null)
        {
            go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            Undo.RegisterCreatedObjectUndo(go, "Create Enemy");
        }
        else
        {
            // Fallback visual: rectángulo rojo
            go = new GameObject();
            Undo.RegisterCreatedObjectUndo(go, "Create Enemy Fallback");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
            sr.color  = new Color(0.9f, 0.15f, 0.15f);
            sr.sortingOrder = 1;
            go.transform.localScale = new Vector3(1f, 2f, 1f);
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            go.AddComponent<InfectedFile>();
        }

        go.name = "InfectedFile_Blocker";
        go.transform.position = new Vector3(3f, FloorY, 0f);

        // Agregar EnemyAnimator si no lo tiene
        if (go.GetComponent<CODEX.Enemies.EnemyAnimator>() == null)
            go.AddComponent<CODEX.Enemies.EnemyAnimator>();

        // Asignar AnimatorController si existe
        var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            "Assets/Animation/Enemy/EnemyController.controller");
        if (ctrl != null)
        {
            var anim = go.GetComponentInChildren<Animator>(true);
            if (anim != null) anim.runtimeAnimatorController = ctrl;
        }

        return go.GetComponent<InfectedFile>();
    }

    // ── PUERTA BLOQUEADA (invisible) ─────────────────────────────────────────

    static GameObject SetupBlockedDoor(InfectedFile enemy, LumaGuide luma)
    {
        var go = GameObject.Find("BlockedDoor");
        if (go == null)
        {
            go = new GameObject("BlockedDoor");
            Undo.RegisterCreatedObjectUndo(go, "Create BlockedDoor");
        }

        // Posición: entre el enemigo y la salida, en el medio de la pantalla
        go.transform.position   = new Vector3(6.5f, 0f, 0f);
        go.transform.localScale = new Vector3(0.3f, 12f, 1f);

        // Sin SpriteRenderer — completamente invisible
        var oldSR = go.GetComponent<SpriteRenderer>();
        if (oldSR != null) Undo.DestroyObjectImmediate(oldSR);

        var col = go.GetComponent<BoxCollider2D>();
        if (col == null) col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = false;

        // InvisibleDoorBlocker
        var blocker = go.GetComponent<CODEX.Tutorial.InvisibleDoorBlocker>()
                   ?? Undo.AddComponent<CODEX.Tutorial.InvisibleDoorBlocker>(go);

        if (enemy != null || luma != null)
        {
            var so = new SerializedObject(blocker);
            if (enemy != null)
                so.FindProperty("targetEnemy").objectReferenceValue = enemy;
            if (luma != null)
                so.FindProperty("luma").objectReferenceValue = luma;
            so.ApplyModifiedProperties();
        }

        return go;
    }

    // ── PUERTA DE SALIDA A T03 ────────────────────────────────────────────────

    static GameObject SetupOpenDoor()
    {
        var go = GameObject.Find("OpenDoor_T02_to_T03");
        if (go == null)
        {
            go = new GameObject("OpenDoor_T02_to_T03");
            Undo.RegisterCreatedObjectUndo(go, "Create OpenDoor");
        }

        go.transform.position   = new Vector3(9f, 0f, 0f);
        go.transform.localScale = new Vector3(2f, 12f, 1f);

        var col = go.GetComponent<BoxCollider2D>();
        if (col == null) col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        if (go.GetComponent<TutorialSceneLoader>() == null)
            go.AddComponent<TutorialSceneLoader>();

        go.SetActive(false);   // Se activa cuando el enemigo muere
        return go;
    }

    // ── BLOCK 02 ─────────────────────────────────────────────────────────────

    static void SetupBlock02(LumaGuide luma, InfectedFile enemy,
                              GameObject blockedDoor, GameObject openDoor)
    {
        var bmGO = GameObject.Find("BlockManager");
        if (bmGO == null)
        {
            bmGO = new GameObject("BlockManager");
            Undo.RegisterCreatedObjectUndo(bmGO, "Create BlockManager");
        }

        var block = bmGO.GetComponent<Block02_Disparo>()
                 ?? Undo.AddComponent<Block02_Disparo>(bmGO);

        var so = new SerializedObject(block);
        so.FindProperty("luma").objectReferenceValue         = luma;
        so.FindProperty("blockerEnemy").objectReferenceValue = enemy;
        so.FindProperty("blockedDoor").objectReferenceValue  = blockedDoor;
        so.FindProperty("openDoor").objectReferenceValue     = openDoor;
        so.ApplyModifiedProperties();
    }

    // ── TUTORIAL MANAGER ─────────────────────────────────────────────────────

    static void SetupTutorialManager()
    {
        if (Object.FindAnyObjectByType<TutorialManager>() != null) return;
        var go = new GameObject("TutorialManager");
        Undo.RegisterCreatedObjectUndo(go, "Create TutorialManager");
        go.AddComponent<TutorialManager>();
    }

    // ── HELPERS UI ────────────────────────────────────────────────────────────

    static GameObject CreateUIPanel(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        var rt       = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>();
        return go;
    }

    static GameObject CreateTMPChild(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        var rt       = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        go.AddComponent<TextMeshProUGUI>();
        return go;
    }
}
