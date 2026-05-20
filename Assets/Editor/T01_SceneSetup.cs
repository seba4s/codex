using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using CODEX.Tutorial;
using CODEX.Tutorial.Blocks;
using CODEX.Systems;

public static class T01_SceneSetup
{
    [MenuItem("CODEX/Setup Escenas/T01 – Materialización")]
    public static void Setup()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        if (!scene.name.Contains("T01"))
        {
            bool proceed = EditorUtility.DisplayDialog(
                "CODEX – Setup T01",
                $"La escena activa es '{scene.name}', no T01_Materializacion.\n¿Continuar de todas formas?",
                "Sí, continuar", "Cancelar");
            if (!proceed) return;
        }

        Undo.SetCurrentGroupName("CODEX T01 Setup");
        int group = Undo.GetCurrentGroup();

        var spawnPoint = SetupSpawnPoint();
        var player     = SetupPlayer(spawnPoint);
        var canvas     = SetupCanvas();
        var bootScreen = SetupBootScreen(canvas);
        var luma       = SetupLUMA(canvas);
        var door       = SetupDoor();
        SetupBlock01(spawnPoint, bootScreen, luma, door);
        SetupTutorialManager();
        SetupCamera(player);

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        EditorUtility.DisplayDialog(
            "CODEX – T01 Listo",
            "Escena configurada.\n\n" +
            "Revisa en la Hierarchy:\n" +
            "• Player_Tutorial  →  spawn (-7, 0)\n" +
            "• HUD_Canvas  →  BootScreen + LUMA panel\n" +
            "• BlockManager  →  Block01_Materializacion\n" +
            "• Door_T01_to_T02  →  trigger invisible (inactivo al inicio)\n" +
            "• TutorialManager\n\n" +
            "⚠ Arrastra Door_T01_to_T02 en la escena para colocarla donde\n" +
            "  termina el suelo visible en el fondo (borde derecho del nivel).",
            "OK");
    }

    // ─── BACKGROUND ───────────────────────────────────────────────────────────

    static void SetupBackground()
    {
        if (GameObject.Find("Background") != null) return;

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Sprites/UI/Tutorial/fondo1.png");

        var bg = new GameObject("Background");
        Undo.RegisterCreatedObjectUndo(bg, "Create Background");

        var sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.sortingOrder = -10;
        bg.transform.position = Vector3.zero;

        var cam = Camera.main;
        if (cam != null && sprite != null)
        {
            float h  = cam.orthographicSize * 2f;
            float w  = h * cam.aspect;
            float sh = sprite.bounds.size.y;
            float sw = sprite.bounds.size.x;
            bg.transform.localScale = new Vector3(w / sw, h / sh, 1f);
        }
        else
        {
            bg.transform.localScale = new Vector3(18f, 10f, 1f);
        }

        if (sprite == null)
            Debug.LogWarning("[CODEX] fondo1.png no encontrado. Asigna el sprite manualmente en Background.");
    }

    // ─── SPAWN POINT ─────────────────────────────────────────────────────────

    static Transform SetupSpawnPoint()
    {
        var go = GameObject.Find("SpawnPoint");
        if (go == null)
        {
            go = new GameObject("SpawnPoint");
            Undo.RegisterCreatedObjectUndo(go, "Create SpawnPoint");
        }
        go.transform.position = new Vector3(-7f, 0f, 0f);
        return go.transform;
    }

    // ─── PLAYER ──────────────────────────────────────────────────────────────

    static GameObject SetupPlayer(Transform spawnPoint)
    {
        var existing = GameObject.FindGameObjectWithTag("Player");
        if (existing != null) return existing;

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/Tutorial/Player/Player_Tutorial.prefab");

        if (prefab == null)
        {
            Debug.LogWarning("[CODEX] Player_Tutorial.prefab no encontrado. Arrástralo manualmente.");
            return null;
        }

        var player = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        Undo.RegisterCreatedObjectUndo(player, "Create Player");
        player.transform.position = spawnPoint != null
            ? spawnPoint.position
            : new Vector3(-7f, 0f, 0f);

        SetupBackground(); // el background se crea después del player para evitar Z-fighting

        return player;
    }

    // ─── CANVAS ──────────────────────────────────────────────────────────────

    static GameObject SetupCanvas()
    {
        var existing = GameObject.Find("HUD_Canvas");
        if (existing != null) return existing;

        var go = new GameObject("HUD_Canvas");
        Undo.RegisterCreatedObjectUndo(go, "Create Canvas");

        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight  = 0.5f;

        go.AddComponent<GraphicRaycaster>();

        return go;
    }

    // ─── BOOT SCREEN ─────────────────────────────────────────────────────────

    static DiagnosticBootScreen SetupBootScreen(GameObject canvas)
    {
        var existing = Object.FindAnyObjectByType<DiagnosticBootScreen>();
        if (existing != null) return existing;

        // Root del panel
        var rootGO = CreateUIPanel("BootScreen_Root", canvas.transform,
            Vector2.zero, Vector2.one, Vector2.zero);
        rootGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.95f);

        var cg = rootGO.AddComponent<CanvasGroup>();

        // Texto de boot
        var textGO = new GameObject("BootText");
        Undo.RegisterCreatedObjectUndo(textGO, "Create BootText");
        textGO.transform.SetParent(rootGO.transform, false);

        var rt = textGO.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.1f, 0.2f);
        rt.anchorMax        = new Vector2(0.9f, 0.8f);
        rt.offsetMin        = Vector2.zero;
        rt.offsetMax        = Vector2.zero;

        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.fontSize    = 28;
        tmp.color       = new Color(0f, 1f, 0.2f);
        tmp.alignment   = TextAlignmentOptions.TopLeft;
        tmp.text        = "";

        // Componente
        var boot = rootGO.AddComponent<DiagnosticBootScreen>();
        var so   = new SerializedObject(boot);
        so.FindProperty("screenRoot").objectReferenceValue  = rootGO;
        so.FindProperty("bootText").objectReferenceValue    = tmp;
        so.FindProperty("canvasGroup").objectReferenceValue = cg;
        so.ApplyModifiedProperties();

        return boot;
    }

    // ─── LUMA GUIDE ──────────────────────────────────────────────────────────

    static LumaGuide SetupLUMA(GameObject canvas)
    {
        var existing = Object.FindAnyObjectByType<LumaGuide>();
        if (existing != null) return existing;

        // Panel de diálogo (en el Canvas)
        var panelGO = CreateUIPanel("LUMA_DialoguePanel", canvas.transform,
            new Vector2(0f, 0f), new Vector2(1f, 0.22f), Vector2.zero);
        panelGO.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.15f, 0.88f);
        panelGO.SetActive(false);

        var speakerGO = CreateTMPChild("SpeakerName", panelGO.transform,
            new Vector2(0.02f, 0.55f), new Vector2(0.98f, 0.95f));
        speakerGO.GetComponent<TextMeshProUGUI>().fontSize    = 22;
        speakerGO.GetComponent<TextMeshProUGUI>().fontStyle   = FontStyles.Bold;
        speakerGO.GetComponent<TextMeshProUGUI>().color       = new Color(0.4f, 0.9f, 1f);

        var msgGO = CreateTMPChild("MessageText", panelGO.transform,
            new Vector2(0.02f, 0.05f), new Vector2(0.98f, 0.52f));
        msgGO.GetComponent<TextMeshProUGUI>().fontSize = 20;
        msgGO.GetComponent<TextMeshProUGUI>().color    = Color.white;

        // GameObject flotante de LUMA (fuera del Canvas, en world space)
        var lumaGO = GameObject.Find("LUMA_Guide");
        if (lumaGO == null)
        {
            lumaGO = new GameObject("LUMA_Guide");
            Undo.RegisterCreatedObjectUndo(lumaGO, "Create LUMA_Guide");
        }
        lumaGO.transform.position = new Vector3(2f, 3f, 0f);

        var guide = lumaGO.AddComponent<LumaGuide>();
        var so    = new SerializedObject(guide);
        so.FindProperty("dialoguePanel").objectReferenceValue = panelGO;
        so.FindProperty("speakerText").objectReferenceValue   = speakerGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("messageText").objectReferenceValue   = msgGO.GetComponent<TextMeshProUGUI>();
        so.ApplyModifiedProperties();

        return guide;
    }

    // ─── BLOCK 01 ────────────────────────────────────────────────────────────

    // ─── DOOR TO T02 ─────────────────────────────────────────────────────────

    static GameObject SetupDoor()
    {
        var existing = GameObject.Find("Door_T01_to_T02");
        if (existing != null) return existing;

        var go = new GameObject("Door_T01_to_T02");
        Undo.RegisterCreatedObjectUndo(go, "Create Door T01");

        // Posición en el borde derecho visible — ajusta X en la escena si es necesario
        go.transform.position = new Vector3(120f, 0f, 0f);

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(4f, 30f);

        go.AddComponent<TutorialSceneLoader>();

        // Empieza desactivada; Block01 la activa cuando LUMA termina
        go.SetActive(false);

        return go;
    }

    // ─── BLOCK 01 ────────────────────────────────────────────────────────────

    static void SetupBlock01(Transform spawnPoint, DiagnosticBootScreen boot, LumaGuide luma, GameObject door)
    {
        var bmGO = GameObject.Find("BlockManager");
        if (bmGO == null)
        {
            bmGO = new GameObject("BlockManager");
            Undo.RegisterCreatedObjectUndo(bmGO, "Create BlockManager");
        }

        var block = bmGO.GetComponent<Block01_Materializacion>()
                    ?? Undo.AddComponent<Block01_Materializacion>(bmGO);

        var so = new SerializedObject(block);
        so.FindProperty("bootScreen").objectReferenceValue = boot;
        so.FindProperty("luma").objectReferenceValue       = luma;
        so.FindProperty("spawnPoint").objectReferenceValue = spawnPoint;
        so.FindProperty("doorToT02").objectReferenceValue  = door;
        so.ApplyModifiedProperties();
    }

    // ─── TUTORIAL MANAGER ────────────────────────────────────────────────────

    static void SetupTutorialManager()
    {
        if (Object.FindAnyObjectByType<TutorialManager>() != null) return;

        var go = new GameObject("TutorialManager");
        Undo.RegisterCreatedObjectUndo(go, "Create TutorialManager");
        go.AddComponent<TutorialManager>();
    }

    // ─── CAMERA ──────────────────────────────────────────────────────────────

    static void SetupCamera(GameObject player)
    {
        var cam = Camera.main;

        if (cam == null)
        {
            var camGO = new GameObject("Main Camera");
            Undo.RegisterCreatedObjectUndo(camGO, "Create Main Camera");
            camGO.tag = "MainCamera";
            cam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
            cam.orthographic       = true;
            cam.orthographicSize   = 5f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }
        else
        {
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }

        // Las escenas del tutorial usan cámara fija — desactivar CameraFollow si existe
        var follow = cam.GetComponent<CameraFollow>();
        if (follow != null)
        {
            follow.enabled = false;
            Debug.Log("[CODEX] CameraFollow desactivado. Cámara fija en (0,0,-10).");
        }
    }

    // ─── HELPERS UI ──────────────────────────────────────────────────────────

    static GameObject CreateUIPanel(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);

        var rt         = go.AddComponent<RectTransform>();
        rt.anchorMin   = anchorMin;
        rt.anchorMax   = anchorMax;
        rt.pivot       = pivot == Vector2.zero ? new Vector2(0.5f, 0.5f) : pivot;
        rt.offsetMin   = Vector2.zero;
        rt.offsetMax   = Vector2.zero;

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
