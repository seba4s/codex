using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using TMPro;
using CODEX.Tutorial;
using CODEX.Tutorial.Blocks;
using CODEX.Systems;

/// <summary>
/// Configura T03_RecoleccionDatos. Mismo layout que T01/T02.
/// Menú: CODEX → Setup Escenas → T03 – Recolección de Datos
/// </summary>
public static class T03_SceneSetup
{
    private const float FloorY  = -1.5f;
    private const float GroundY = -2.5f;
    private const float CamSize = 5f;

    [MenuItem("CODEX/Setup Escenas/T03 – Recolección de Datos")]
    public static void Setup()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.name.Contains("T03"))
        {
            bool ok = EditorUtility.DisplayDialog("CODEX – Setup T03",
                $"La escena activa es '{scene.name}'.\n¿Continuar de todas formas?",
                "Sí", "Cancelar");
            if (!ok) return;
        }

        Undo.SetCurrentGroupName("CODEX T03 Setup");
        int group = Undo.GetCurrentGroup();

        SetupGround();
        SetupCamera();
        SetupBackground();
        SetupGlobalLight();
        var spawnPoint = SetupSpawnPoint();
        var player     = SetupPlayer(spawnPoint);
        var canvas     = SetupCanvas();
        var luma       = SetupLUMA(canvas);
        var openDoor   = SetupOpenDoor();
        SetupDataCollectibles();
        SetupBlock03(luma, openDoor);
        SetupTutorialManager();

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        EditorUtility.DisplayDialog("CODEX – T03 Listo",
            "T03 configurada.\n\n" +
            "✓ Mismo layout que T01/T02\n" +
            "✓ fondo3.png\n" +
            "✓ 5 fragmentos de datos esparcidos\n" +
            "✓ Salida se activa al recoger todos\n" +
            "✓ LUMA con diálogos de T03", "OK");
    }

    // ── SUELO ────────────────────────────────────────────────────────────────

    static void SetupGround()
    {
        if (GameObject.Find("Ground_T03") == null)
        {
            var go = new GameObject("Ground_T03");
            Undo.RegisterCreatedObjectUndo(go, "Create Ground");
            go.transform.position   = new Vector3(0f, GroundY, 0f);
            go.transform.localScale = new Vector3(40f, 1f, 1f);
            int groundLayer = LayerMask.NameToLayer("ground");
            go.layer = groundLayer >= 0 ? groundLayer : 0;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
            sr.color  = new Color(0.10f, 0.16f, 0.30f);
            go.AddComponent<BoxCollider2D>();
        }

        if (GameObject.Find("Wall_Left_T03") == null)
        {
            var go = new GameObject("Wall_Left_T03");
            Undo.RegisterCreatedObjectUndo(go, "Create Left Wall");
            go.transform.position   = new Vector3(-10f, 0f, 0f);
            go.transform.localScale = new Vector3(1f, 20f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
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
        var follow = cam.GetComponent<CameraFollow>();
        if (follow != null) follow.enabled = false;
    }

    // ── FONDO ────────────────────────────────────────────────────────────────

    static void SetupBackground()
    {
        if (GameObject.Find("Background") != null) return;
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Tutorial/fondo3.png");
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
            bg.transform.localScale = new Vector3(18f, 10f, 1f);
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
        var go = GameObject.Find("SpawnPoint") ?? new GameObject("SpawnPoint");
        Undo.RegisterCreatedObjectUndo(go, "Create SpawnPoint");
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
        if (prefab == null) { Debug.LogWarning("[CODEX T03] Player_Tutorial.prefab no encontrado."); return null; }

        var player = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        Undo.RegisterCreatedObjectUndo(player, "Create Player");
        player.transform.position = spawnPoint != null ? spawnPoint.position : new Vector3(-7f, FloorY, 0f);

        var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Movimiento/Player_Tutorial.controller");
        if (ctrl != null)
        {
            var anim = player.GetComponentInChildren<Animator>(true);
            if (anim == null) anim = player.AddComponent<Animator>();
            anim.runtimeAnimatorController = ctrl;
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

        var speakerGO  = CreateTMPChild("SpeakerName", panelGO.transform, new Vector2(0.02f, 0.55f), new Vector2(0.98f, 0.95f));
        var speakerTMP = speakerGO.GetComponent<TextMeshProUGUI>();
        speakerTMP.fontSize = 22; speakerTMP.fontStyle = FontStyles.Bold;
        speakerTMP.color    = new Color(0.4f, 0.9f, 1f);

        var msgGO  = CreateTMPChild("MessageText", panelGO.transform, new Vector2(0.02f, 0.05f), new Vector2(0.98f, 0.52f));
        var msgTMP = msgGO.GetComponent<TextMeshProUGUI>();
        msgTMP.fontSize = 20; msgTMP.color = Color.white;

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

    // ── FRAGMENTOS DE DATOS ──────────────────────────────────────────────────

    static void SetupDataCollectibles()
    {
        // 5 fragmentos distribuidos por la escena
        Vector3[] positions =
        {
            new Vector3(-5f,  FloorY + 1.5f, 0f),
            new Vector3(-2f,  FloorY + 1.5f, 0f),
            new Vector3( 1f,  FloorY + 1.5f, 0f),
            new Vector3( 4f,  FloorY + 1.5f, 0f),
            new Vector3( 7f,  FloorY + 1.5f, 0f),
        };

        int playerLayer = LayerMask.NameToLayer("Player");
        LayerMask playerMask = playerLayer >= 0 ? (1 << playerLayer) : 1;

        for (int i = 0; i < positions.Length; i++)
        {
            string itemName = $"DataFragment_{i}";
            if (GameObject.Find(itemName) != null) continue;

            var go = new GameObject(itemName);
            Undo.RegisterCreatedObjectUndo(go, "Create DataFragment");
            go.transform.position = positions[i];

            // Visual — cuadrado cian flotante
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale    = new Vector3(0.4f, 0.4f, 1f);

            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite       = CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
            sr.color        = new Color(0f, 1f, 0.9f, 0.9f);
            sr.sortingOrder = 2;

            // Collider trigger
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size      = new Vector2(1f, 1f);

            // DataCollectible
            var dc = go.AddComponent<DataCollectible>();
            var so = new SerializedObject(dc);
            so.FindProperty("dataValue").intValue       = 1;
            so.FindProperty("collectRadius").floatValue = 1f;
            so.FindProperty("playerLayer").intValue     = playerMask;
            so.FindProperty("floatAmplitude").floatValue = 0.15f;
            so.FindProperty("floatSpeed").floatValue     = 2f;
            so.ApplyModifiedProperties();
        }
    }

    // ── PUERTA DE SALIDA ─────────────────────────────────────────────────────

    static GameObject SetupOpenDoor()
    {
        var go = GameObject.Find("OpenDoor_T03_to_T04");
        if (go == null)
        {
            go = new GameObject("OpenDoor_T03_to_T04");
            Undo.RegisterCreatedObjectUndo(go, "Create OpenDoor");
        }
        go.transform.position   = new Vector3(9f, 0f, 0f);
        go.transform.localScale = new Vector3(2f, 12f, 1f);

        var col = go.GetComponent<BoxCollider2D>();
        if (col == null) col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        if (go.GetComponent<TutorialSceneLoader>() == null)
            go.AddComponent<TutorialSceneLoader>();

        go.SetActive(false);
        return go;
    }

    // ── BLOCK 03 ─────────────────────────────────────────────────────────────

    static void SetupBlock03(LumaGuide luma, GameObject openDoor)
    {
        var bmGO = GameObject.Find("BlockManager");
        if (bmGO == null)
        {
            bmGO = new GameObject("BlockManager");
            Undo.RegisterCreatedObjectUndo(bmGO, "Create BlockManager");
        }

        var block = bmGO.GetComponent<Block03_RecoleccionDatos>()
                 ?? Undo.AddComponent<Block03_RecoleccionDatos>(bmGO);

        var so = new SerializedObject(block);
        so.FindProperty("luma").objectReferenceValue     = luma;
        so.FindProperty("openDoor").objectReferenceValue = openDoor;
        so.FindProperty("dataRequired").intValue         = 5;
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

    static GameObject CreateUIPanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>();
        return go;
    }

    static GameObject CreateTMPChild(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.AddComponent<TextMeshProUGUI>();
        return go;
    }
}
