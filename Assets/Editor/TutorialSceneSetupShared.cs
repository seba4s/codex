using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using TMPro;
using CODEX.Tutorial;
using CODEX.Player;
using CODEX.Systems;
using CODEX.Enemies;

/// <summary>
/// Métodos compartidos entre T05–T08 SceneSetup.
/// Cámara, suelo, luz, jugador, canvas, LUMA, HUD, managers, transitions.
/// T04 mantiene sus métodos propios para no romper lo existente.
/// </summary>
public static class TutorialSceneSetupShared
{
    public const float FloorY  = -1.5f;
    public const float GroundY = -2.5f;
    public const float CamSize = 5f;

    // ── SUELO ─────────────────────────────────────────────────────────────────

    public static void SetupGround(string suffix, float width = 40f, float centerX = 0f)
    {
        string gName = $"Ground_{suffix}";
        if (GameObject.Find(gName) == null)
        {
            var go = new GameObject(gName);
            Undo.RegisterCreatedObjectUndo(go, "Create Ground");
            go.transform.position   = new Vector3(centerX, GroundY, 0f);
            go.transform.localScale = new Vector3(width, 1f, 1f);
            int groundLayer = LayerMask.NameToLayer("ground");
            go.layer = groundLayer >= 0 ? groundLayer : 0;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
            sr.color  = new Color(0.10f, 0.16f, 0.30f);
            go.AddComponent<BoxCollider2D>();
        }

        string wName = $"Wall_Left_{suffix}";
        if (GameObject.Find(wName) == null)
        {
            var wallX = centerX - width * 0.5f - 0.5f;
            var go = new GameObject(wName);
            Undo.RegisterCreatedObjectUndo(go, "Create Left Wall");
            go.transform.position   = new Vector3(wallX, 0f, 0f);
            go.transform.localScale = new Vector3(1f, 20f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
            sr.color  = new Color(0.10f, 0.16f, 0.30f);
            go.AddComponent<BoxCollider2D>();
        }
    }

    // ── CÁMARA ────────────────────────────────────────────────────────────────

    public static void SetupCamera()
    {
        if (Camera.main != null) return;
        var camGO = new GameObject("Main Camera");
        Undo.RegisterCreatedObjectUndo(camGO, "Create Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        camGO.AddComponent<AudioListener>();
        cam.orthographic       = true;
        cam.orthographicSize   = CamSize;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.backgroundColor    = new Color(0.02f, 0.02f, 0.08f);
    }

    // ── FONDO ─────────────────────────────────────────────────────────────────

    public static void SetupBackground(string spritePath)
    {
        if (GameObject.Find("Background") != null) return;
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
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
            bg.transform.localScale = new Vector3(
                w / sprite.bounds.size.x,
                h / sprite.bounds.size.y, 1f);
        }
        else
        {
            bg.transform.localScale = new Vector3(18f, 10f, 1f);
            if (sprite == null)
                Debug.LogWarning($"[CODEX Setup] Fondo no encontrado en: {spritePath}. " +
                                 "Asigna el sprite manualmente en el Inspector.");
        }
    }

    // ── LUZ GLOBAL ────────────────────────────────────────────────────────────

    public static void SetupGlobalLight()
    {
        if (GameObject.Find("Global Light 2D") != null) return;
        var go = new GameObject("Global Light 2D");
        Undo.RegisterCreatedObjectUndo(go, "Create Global Light");
        var light = go.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.intensity = 1f;
        light.color     = Color.white;
    }

    // ── SPAWN POINT ───────────────────────────────────────────────────────────

    public static Transform SetupSpawnPoint(Vector3 pos)
    {
        var existing = GameObject.Find("SpawnPoint");
        var go = existing ?? new GameObject("SpawnPoint");
        if (existing == null) Undo.RegisterCreatedObjectUndo(go, "Create SpawnPoint");
        go.transform.position = pos;
        return go.transform;
    }

    // ── JUGADOR ───────────────────────────────────────────────────────────────

    public static GameObject SetupPlayer(Transform spawnPoint)
    {
        var existing = GameObject.FindGameObjectWithTag("Player");
        if (existing != null) return existing;

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/Tutorial/Player/Player_Tutorial.prefab");
        if (prefab == null)
        {
            Debug.LogWarning("[CODEX Setup] Player_Tutorial.prefab no encontrado en " +
                             "Assets/Prefabs/Tutorial/Player/. Créalo con PlayerPrefabCreator.");
            return null;
        }

        var player = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        Undo.RegisterCreatedObjectUndo(player, "Create Player");
        player.transform.position = spawnPoint != null
            ? spawnPoint.position
            : new Vector3(-7f, FloorY, 0f);

        var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            "Assets/Movimiento/Player_Tutorial.controller");
        if (ctrl != null)
        {
            var anim = player.GetComponentInChildren<Animator>(true)
                    ?? player.AddComponent<Animator>();
            anim.runtimeAnimatorController = ctrl;
        }

        if (player.GetComponent<PlayerHealth>() == null)
            Undo.AddComponent<PlayerHealth>(player);

        return player;
    }

    // ── CANVAS ────────────────────────────────────────────────────────────────

    public static GameObject SetupCanvas()
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

    // ── LUMA ──────────────────────────────────────────────────────────────────

    public static LumaGuide SetupLUMA(GameObject canvas)
    {
        var existing = Object.FindAnyObjectByType<LumaGuide>();
        if (existing != null) return existing;

        var panelGO = CreateUIPanel("LUMA_DialoguePanel", canvas.transform,
            new Vector2(0f, 0f), new Vector2(1f, 0.22f));
        panelGO.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.15f, 0.88f);
        panelGO.SetActive(false);

        var speakerGO  = CreateTMPChild("SpeakerName", panelGO.transform,
            new Vector2(0.02f, 0.55f), new Vector2(0.98f, 0.95f));
        var speakerTMP = speakerGO.GetComponent<TextMeshProUGUI>();
        speakerTMP.fontSize  = 22;
        speakerTMP.fontStyle = FontStyles.Bold;
        speakerTMP.color     = new Color(0.4f, 0.9f, 1f);

        var msgGO  = CreateTMPChild("MessageText", panelGO.transform,
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

    // ── HUD ───────────────────────────────────────────────────────────────────

    public static TutorialHUD SetupHUD(GameObject canvas, GameObject player)
    {
        var existing = Object.FindAnyObjectByType<TutorialHUD>();
        if (existing != null) return existing;

        var hudRoot = new GameObject("TutorialHUD");
        Undo.RegisterCreatedObjectUndo(hudRoot, "Create TutorialHUD");
        hudRoot.transform.SetParent(canvas.transform, false);
        var hudComp = hudRoot.AddComponent<TutorialHUD>();

        // 5 segmentos de salud — esquina superior izquierda
        var healthRoot = new GameObject("HealthBar");
        Undo.RegisterCreatedObjectUndo(healthRoot, "Create HealthBar");
        healthRoot.transform.SetParent(canvas.transform, false);
        var healthRT = healthRoot.AddComponent<RectTransform>();
        healthRT.anchorMin = new Vector2(0.01f, 0.90f);
        healthRT.anchorMax = new Vector2(0.20f, 0.99f);
        healthRT.offsetMin = healthRT.offsetMax = Vector2.zero;

        var segments = new GameObject[5];
        for (int i = 0; i < 5; i++)
        {
            var seg = new GameObject($"HealthSeg_{i}");
            Undo.RegisterCreatedObjectUndo(seg, "Create HealthSeg");
            seg.transform.SetParent(healthRoot.transform, false);
            var rt   = seg.AddComponent<RectTransform>();
            float xMin = i * 0.20f;
            rt.anchorMin = new Vector2(xMin, 0f);
            rt.anchorMax = new Vector2(xMin + 0.18f, 1f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            seg.AddComponent<Image>().color = new Color(0.2f, 0.8f, 1f);
            segments[i] = seg;
        }

        // Indicador de dash
        var dashRoot = new GameObject("DashIndicator");
        Undo.RegisterCreatedObjectUndo(dashRoot, "Create DashIndicator");
        dashRoot.transform.SetParent(canvas.transform, false);
        var dashRT = dashRoot.AddComponent<RectTransform>();
        dashRT.anchorMin = new Vector2(0.01f, 0.80f);
        dashRT.anchorMax = new Vector2(0.20f, 0.89f);
        dashRT.offsetMin = dashRT.offsetMax = Vector2.zero;
        dashRoot.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.15f, 0.85f);

        var dashLabel = new GameObject("DashLabel");
        Undo.RegisterCreatedObjectUndo(dashLabel, "Create DashLabel");
        dashLabel.transform.SetParent(dashRoot.transform, false);
        var dlRT = dashLabel.AddComponent<RectTransform>();
        dlRT.anchorMin = Vector2.zero; dlRT.anchorMax = Vector2.one;
        dlRT.offsetMin = dlRT.offsetMax = Vector2.zero;
        var dlTMP = dashLabel.AddComponent<TextMeshProUGUI>();
        dlTMP.text      = "[SHIFT] Esquivar";
        dlTMP.fontSize  = 18;
        dlTMP.color     = new Color(0.4f, 0.9f, 1f);
        dlTMP.alignment = TextAlignmentOptions.Center;
        dashRoot.SetActive(false);

        var so = new SerializedObject(hudComp);
        var segsArr = so.FindProperty("healthSegments");
        segsArr.arraySize = 5;
        for (int i = 0; i < 5; i++)
            segsArr.GetArrayElementAtIndex(i).objectReferenceValue = segments[i];
        so.FindProperty("dashIndicatorRoot").objectReferenceValue = dashRoot;
        so.ApplyModifiedProperties();

        if (player != null && player.GetComponent<PlayerHealth>() == null)
            Undo.AddComponent<PlayerHealth>(player);

        return hudComp;
    }

    // ── TUTORIAL MANAGER ──────────────────────────────────────────────────────

    public static void SetupTutorialManager()
    {
        if (Object.FindAnyObjectByType<TutorialManager>() != null) return;
        var go = new GameObject("TutorialManager");
        Undo.RegisterCreatedObjectUndo(go, "Create TutorialManager");
        go.AddComponent<TutorialManager>();
    }

    // ── CHECKPOINT MANAGER ────────────────────────────────────────────────────

    public static void SetupCheckpointManager()
    {
        if (Object.FindAnyObjectByType<CheckpointManager>() != null) return;
        var go = new GameObject("CheckpointManager");
        Undo.RegisterCreatedObjectUndo(go, "Create CheckpointManager");
        go.AddComponent<CheckpointManager>();
    }

    // ── SCENE TRANSITION ──────────────────────────────────────────────────────

    public static void SetupSceneTransition(GameObject canvas)
    {
        if (Object.FindAnyObjectByType<SceneTransition>() != null) return;

        var stGO = new GameObject("SceneTransition");
        Undo.RegisterCreatedObjectUndo(stGO, "Create SceneTransition");
        var st = stGO.AddComponent<SceneTransition>();

        // Panel overlay negro para fade
        var overlayGO = CreateUIPanel("ST_Overlay", canvas.transform, Vector2.zero, Vector2.one);
        var overlayImg = overlayGO.GetComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0f);
        overlayImg.raycastTarget = false;
        overlayGO.GetComponent<RectTransform>().SetAsLastSibling();

        var so = new SerializedObject(st);
        so.FindProperty("panelOverlay").objectReferenceValue = overlayImg;
        so.ApplyModifiedProperties();
    }

    // ── SCENE LOADER (trigger de salida) ──────────────────────────────────────

    public static GameObject SetupSceneLoader(string goName, Vector3 pos,
        bool requireCondition = false, string conditionMsg = "Completa el objetivo primero.")
    {
        var go = GameObject.Find(goName) ?? new GameObject(goName);
        Undo.RegisterCreatedObjectUndo(go, "Create " + goName);
        go.transform.position   = pos;
        go.transform.localScale = new Vector3(2f, 12f, 1f);

        if (go.GetComponent<BoxCollider2D>() == null)
            go.AddComponent<BoxCollider2D>().isTrigger = true;
        else
            go.GetComponent<BoxCollider2D>().isTrigger = true;

        var loader = go.GetComponent<TutorialSceneLoader>()
                  ?? Undo.AddComponent<TutorialSceneLoader>(go);

        if (requireCondition)
        {
            var so = new SerializedObject(loader);
            so.FindProperty("requireCondition").boolValue         = true;
            so.FindProperty("conditionNotMetMessage").stringValue = conditionMsg;
            so.ApplyModifiedProperties();
        }
        return go;
    }

    // ── REPAIR TERMINAL ───────────────────────────────────────────────────────

    public static GameObject SetupRepairTerminal(string goName, Vector3 pos)
    {
        if (GameObject.Find(goName) != null) return GameObject.Find(goName);

        var go = new GameObject(goName);
        Undo.RegisterCreatedObjectUndo(go, "Create " + goName);
        go.transform.position   = pos;
        go.transform.localScale = new Vector3(1f, 2f, 1f);

        // Visual — verde/cyan
        var visual = new GameObject("Visual");
        visual.transform.SetParent(go.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale    = Vector3.one;
        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite       = CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
        sr.color        = new Color(0.15f, 0.75f, 0.4f);
        sr.sortingOrder = 2;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        go.AddComponent<RepairTerminal>();
        return go;
    }

    // ── COLECTIBLE DE DATOS ───────────────────────────────────────────────────

    public static GameObject SetupDataCollectible(string goName, Vector3 pos, int value = 1)
    {
        if (GameObject.Find(goName) != null) return GameObject.Find(goName);

        var go = new GameObject(goName);
        Undo.RegisterCreatedObjectUndo(go, "Create " + goName);
        go.transform.position   = pos;
        go.transform.localScale = Vector3.one * 0.5f;

        var visual = new GameObject("Visual");
        visual.transform.SetParent(go.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale    = Vector3.one;
        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite       = CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
        sr.color        = new Color(0.9f, 0.85f, 0.1f);   // amarillo — dato
        sr.sortingOrder = 3;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.5f;

        var dc = go.AddComponent<DataCollectible>();
        var so = new SerializedObject(dc);
        so.FindProperty("dataValue").intValue = value;
        so.ApplyModifiedProperties();

        return go;
    }

    // ── ENEMY INFECTADO ───────────────────────────────────────────────────────

    public static GameObject SetupEnemy(string goName, Vector3 pos,
        InfectedFile.EnemyType type = InfectedFile.EnemyType.TypeA_Static)
    {
        if (GameObject.Find(goName) != null) return GameObject.Find(goName);

        var go = new GameObject(goName);
        Undo.RegisterCreatedObjectUndo(go, "Create " + goName);
        go.transform.position   = pos;
        go.transform.localScale = new Vector3(1f, 1.5f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
        sr.color  = type == InfectedFile.EnemyType.TypeA_Static ? new Color(0.8f, 0.2f, 0.8f)
                  : type == InfectedFile.EnemyType.TypeB_Patrol   ? new Color(1f,   0.4f, 0.1f)
                  : new Color(1f, 0.1f, 0.1f);   // TypeC/D

        go.AddComponent<BoxCollider2D>();

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1f;
        rb.constraints  = RigidbodyConstraints2D.FreezeRotation;

        var enemy = go.AddComponent<InfectedFile>();
        var so    = new SerializedObject(enemy);
        so.FindProperty("enemyType").enumValueIndex = (int)type;
        so.ApplyModifiedProperties();

        return go;
    }

    // ── HELPERS UI ────────────────────────────────────────────────────────────

    public static GameObject CreateUIPanel(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>();
        return go;
    }

    public static GameObject CreateTMPChild(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax)
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
