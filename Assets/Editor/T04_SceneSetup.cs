using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using TMPro;
using CODEX.Tutorial;
using CODEX.Tutorial.Blocks;
using CODEX.Player;                                            // BREAKING: era CODEX.Systems (HealthSystem eliminado)

/// <summary>
/// Configura T04_DanoYEsquive. Mismo layout que T01/T02/T03.
/// Menú: CODEX → Setup Escenas → T04 – Daño y Esquive
/// </summary>
public static class T04_SceneSetup
{
    private const float FloorY  = -1.5f;
    private const float GroundY = -2.5f;
    private const float CamSize = 5f;

    [MenuItem("CODEX/Setup Escenas/T04 – Daño y Esquive")]
    public static void Setup()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.name.Contains("T04"))
        {
            bool ok = EditorUtility.DisplayDialog("CODEX – Setup T04",
                $"La escena activa es '{scene.name}'.\n¿Continuar de todas formas?",
                "Sí", "Cancelar");
            if (!ok) return;
        }

        Undo.SetCurrentGroupName("CODEX T04 Setup");
        int group = Undo.GetCurrentGroup();

        SetupGround();
        SetupCamera();
        SetupBackground();
        SetupGlobalLight();
        var spawnPoint = SetupSpawnPoint();
        var player = SetupPlayer(spawnPoint);
        var canvas = SetupCanvas();
        var luma   = SetupLUMA(canvas);
        var hud    = SetupHUD(canvas, player);
        SetupGroundHazards();
        var openDoor = SetupOpenDoor();
        SetupBlock04(luma, hud);
        SetupTutorialManager();

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        EditorUtility.DisplayDialog("CODEX – T04 Listo",
            "T04 configurada.\n\n" +
            "✓ Mismo layout que T01/T02/T03\n" +
            "✓ fondo4.png\n" +
            "✓ 3 peligros de suelo pulsantes\n" +
            "✓ LUMA enseña sobre daño y esquive (SHIFT)\n" +
            "✓ Salida abierta al final del nivel\n" +
            "✓ HUD con segmentos de salud e indicador de dash", "OK");
    }

    // ── SUELO ────────────────────────────────────────────────────────────────

    static void SetupGround()
    {
        if (GameObject.Find("Ground_T04") == null)
        {
            var go = new GameObject("Ground_T04");
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

        if (GameObject.Find("Wall_Left_T04") == null)
        {
            var go = new GameObject("Wall_Left_T04");
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

    // ── FONDO ────────────────────────────────────────────────────────────────

    static void SetupBackground()
    {
        if (GameObject.Find("Background") != null) return;
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Tutorial/fondo4.png");
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
        if (prefab == null) { Debug.LogWarning("[CODEX T04] Player_Tutorial.prefab no encontrado."); return null; }

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

        if (player.GetComponent<PlayerHealth>() == null)          // REFACTOR: era HealthSystem
            Undo.AddComponent<PlayerHealth>(player);

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

    // ── HUD ──────────────────────────────────────────────────────────────────

    static TutorialHUD SetupHUD(GameObject canvas, GameObject player)
    {
        var existing = Object.FindAnyObjectByType<TutorialHUD>();
        if (existing != null) return existing;

        // Contenedor HUD en la esquina superior izquierda
        var hudRoot = new GameObject("TutorialHUD");
        Undo.RegisterCreatedObjectUndo(hudRoot, "Create TutorialHUD");
        hudRoot.transform.SetParent(canvas.transform, false);

        var hudComp = hudRoot.AddComponent<TutorialHUD>();

        // ── Salud: 5 segmentos en la esquina superior izquierda ──
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
            var rt = seg.AddComponent<RectTransform>();
            float xMin = i * 0.20f;
            float xMax = xMin + 0.18f;
            rt.anchorMin = new Vector2(xMin, 0f);
            rt.anchorMax = new Vector2(xMax, 1f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = seg.AddComponent<Image>();
            img.color = new Color(0.2f, 0.8f, 1f);
            segments[i] = seg;
        }

        // ── Indicador de dash — oculto hasta primer golpe ──
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
        dlTMP.alignment = TMPro.TextAlignmentOptions.Center;
        dashRoot.SetActive(false);

        // Asignar referencias al TutorialHUD vía SerializedObject
        var so = new SerializedObject(hudComp);
        var segsArr = so.FindProperty("healthSegments");
        segsArr.arraySize = 5;
        for (int i = 0; i < 5; i++)
            segsArr.GetArrayElementAtIndex(i).objectReferenceValue = segments[i];
        so.FindProperty("dashIndicatorRoot").objectReferenceValue = dashRoot;
        so.ApplyModifiedProperties();

        // Asegurar que el PlayerHealth del player esté presente     // REFACTOR: era HealthSystem
        if (player != null)
        {
            var ph = player.GetComponent<PlayerHealth>();
            if (ph == null) ph = Undo.AddComponent<PlayerHealth>(player);
        }

        return hudComp;
    }

    // ── PELIGROS DE SUELO ────────────────────────────────────────────────────

    static void SetupGroundHazards()
    {
        // 3 chorros de energía corruptora — distribuidos entre spawn y salida
        (string name, Vector3 pos)[] hazards =
        {
            ("GroundHazard_0", new Vector3(-3f, FloorY, 0f)),
            ("GroundHazard_1", new Vector3( 1f, FloorY, 0f)),
            ("GroundHazard_2", new Vector3( 5f, FloorY, 0f)),
        };

        foreach (var (hName, hPos) in hazards)
        {
            if (GameObject.Find(hName) != null) continue;

            var go = new GameObject(hName);
            Undo.RegisterCreatedObjectUndo(go, "Create GroundHazard");
            go.transform.position   = hPos;
            go.transform.localScale = new Vector3(1.2f, 2f, 1f);

            // Visual — rojo/naranja
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale    = Vector3.one;
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite       = CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
            sr.color        = new Color(1f, 0.25f, 0.05f, 0.85f);
            sr.sortingOrder = 2;

            // Collider trigger (GroundHazard lo habilita/deshabilita con el ciclo)
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            // Script
            var hazardComp = go.AddComponent<GroundHazard>();
            var so = new SerializedObject(hazardComp);
            so.FindProperty("activeTime").floatValue   = 1f;
            so.FindProperty("inactiveTime").floatValue = 1.5f;
            so.FindProperty("damageAmount").intValue   = 1;
            so.FindProperty("visualEffect").objectReferenceValue = visual;
            so.ApplyModifiedProperties();
        }
    }

    // ── PUERTA DE SALIDA ─────────────────────────────────────────────────────

    static GameObject SetupOpenDoor()
    {
        var go = GameObject.Find("OpenDoor_T04_to_T05");
        if (go == null)
        {
            go = new GameObject("OpenDoor_T04_to_T05");
            Undo.RegisterCreatedObjectUndo(go, "Create OpenDoor");
        }
        go.transform.position   = new Vector3(9f, 0f, 0f);
        go.transform.localScale = new Vector3(2f, 12f, 1f);

        var col = go.GetComponent<BoxCollider2D>();
        if (col == null) col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        if (go.GetComponent<TutorialSceneLoader>() == null)
            go.AddComponent<TutorialSceneLoader>();

        // En T04 la puerta siempre está abierta: el reto es cruzar los peligros
        go.SetActive(true);
        return go;
    }

    // ── BLOCK 04 ─────────────────────────────────────────────────────────────

    static void SetupBlock04(LumaGuide luma, TutorialHUD hud)
    {
        var bmGO = GameObject.Find("BlockManager");
        if (bmGO == null)
        {
            bmGO = new GameObject("BlockManager");
            Undo.RegisterCreatedObjectUndo(bmGO, "Create BlockManager");
        }

        var block = bmGO.GetComponent<Block04_DanoYEsquive>()
                 ?? Undo.AddComponent<Block04_DanoYEsquive>(bmGO);

        var so = new SerializedObject(block);
        so.FindProperty("luma").objectReferenceValue = luma;
        so.FindProperty("hud").objectReferenceValue  = hud;
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
