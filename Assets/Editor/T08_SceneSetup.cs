using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using CODEX.Tutorial;
using CODEX.Tutorial.Blocks;
using CODEX.Player;
using CODEX.Systems;

/// <summary>
/// Configura T08_PuertoSalida – Cierre narrativo del tutorial.
/// 3 terminales → FirstFileKeyEvent → cutscene emocional → Nivel1_DiscoDuro.
/// Block08_PuertoSalida reinicia el contador de terminales en Start() para que
/// los 3 de T08 sean los que disparan la secuencia (T05 activó 1 antes).
/// Menú: CODEX → Setup Escenas → T08 – Puerto de Salida
/// </summary>
public static class T08_SceneSetup
{
    [MenuItem("CODEX/Setup Escenas/T08 – Puerto de Salida")]
    public static void Setup()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.name.Contains("T08"))
        {
            bool ok = EditorUtility.DisplayDialog("CODEX – Setup T08",
                $"La escena activa es '{scene.name}'.\n¿Continuar de todas formas?",
                "Sí", "Cancelar");
            if (!ok) return;
        }

        Undo.SetCurrentGroupName("CODEX T08 Setup");
        int group = Undo.GetCurrentGroup();

        // ── Infraestructura común ──────────────────────────────────────────────
        TutorialSceneSetupShared.SetupGround("T08", 50f, 2f);
        TutorialSceneSetupShared.SetupCamera();
        TutorialSceneSetupShared.SetupBackground("Assets/Sprites/UI/Tutorial/fondo8.png");
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

        // ── Mecánica específica T08 ────────────────────────────────────────────

        // 3 terminales de reparación distribuidos en el área
        // NOTA: Block08_PuertoSalida.Start() llama TutorialManager.ResetTerminals()
        //       para que la cuenta empiece en 0 aquí (T05 ya activó 1 terminal antes).
        TutorialSceneSetupShared.SetupRepairTerminal(
            "Terminal_T08_A",
            new Vector3(-5f, TutorialSceneSetupShared.FloorY + 0.5f, 0f));
        TutorialSceneSetupShared.SetupRepairTerminal(
            "Terminal_T08_B",
            new Vector3(0f, TutorialSceneSetupShared.FloorY + 0.5f, 0f));
        TutorialSceneSetupShared.SetupRepairTerminal(
            "Terminal_T08_C",
            new Vector3(5f, TutorialSceneSetupShared.FloorY + 0.5f, 0f));

        // Puerto de salida — visual que aparece tras la secuencia narrativa
        var exitPort = SetupExitPort();

        // Panel "primer archivo" — aparece en el cutscene emocional
        var (filePanel, fileContent, filePanelGroup) = SetupFilePanel(canvas);

        // FirstFileKeyEvent: coordina el cutscene y la transición a Nivel1_DiscoDuro
        SetupFirstFileKeyEvent(filePanel, fileContent, filePanelGroup, luma, exitPort, player);

        // Block08: diálogo de introducción al área del puerto
        SetupBlock08(luma);

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        EditorUtility.DisplayDialog("CODEX – T08 Listo",
            "T08_PuertoSalida configurada.\n\n" +
            "✓ 3 terminales: Terminal_T08_A (-5), B (0), C (5)\n" +
            "✓ ExitPort (cyan) en x=9, y=FloorY+1.5 — inactivo al inicio\n" +
            "✓ FilePanel_T08 (panel oscuro de terminal) en Canvas\n" +
            "✓ FirstFileKeyEvent conectado (filePanel, fileContent, luma, exitPort, player)\n" +
            "✓ Block08_PuertoSalida conectado — reinicia contador de terminales al iniciar\n" +
            "✓ Al activar los 3 terminales → secuencia narrativa → Nivel1_DiscoDuro\n\n" +
            "⚠ IMPORTANTE: añade 'Nivel1_DiscoDuro' a File → Build Settings si no existe.", "OK");
    }

    // ── PUERTO DE SALIDA (visual) ─────────────────────────────────────────────

    static GameObject SetupExitPort()
    {
        if (GameObject.Find("ExitPort") != null) return GameObject.Find("ExitPort");

        var go = new GameObject("ExitPort");
        Undo.RegisterCreatedObjectUndo(go, "Create ExitPort");
        go.transform.position   = new Vector3(9f, TutorialSceneSetupShared.FloorY + 1.5f, 0f);
        go.transform.localScale = new Vector3(2f, 3f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
        sr.color        = new Color(0.10f, 0.92f, 1f, 0.85f);   // cyan brillante — puerto USB activo
        sr.sortingOrder = 3;

        // Inactivo: FirstFileKeyEvent lo activa al terminar la secuencia narrativa.
        // No necesita SceneLoader propio — FirstFileKeyEvent gestiona la transición directamente.
        go.SetActive(false);
        return go;
    }

    // ── FILE PANEL (terminal de texto del cutscene) ───────────────────────────

    static (GameObject panel, TextMeshProUGUI content, CanvasGroup group) SetupFilePanel(
        GameObject canvas)
    {
        const string panelName = "FilePanel_T08";
        if (GameObject.Find(panelName) != null)
        {
            var ex = GameObject.Find(panelName);
            return (ex,
                    ex.GetComponentInChildren<TextMeshProUGUI>(),
                    ex.GetComponent<CanvasGroup>());
        }

        // Panel central oscuro — simula una ventana de terminal
        var panelGO = TutorialSceneSetupShared.CreateUIPanel(
            panelName, canvas.transform,
            new Vector2(0.10f, 0.15f), new Vector2(0.90f, 0.85f));
        panelGO.GetComponent<Image>().color = new Color(0.03f, 0.04f, 0.10f, 0.97f);

        // CanvasGroup para fade-in/out si se necesita en el futuro
        var cg = panelGO.AddComponent<CanvasGroup>();

        // Encabezado de archivo
        var titleGO = TutorialSceneSetupShared.CreateTMPChild(
            "FileTitle", panelGO.transform,
            new Vector2(0.04f, 0.82f), new Vector2(0.96f, 0.96f));
        var titleTMP = titleGO.GetComponent<TextMeshProUGUI>();
        titleTMP.text      = "[ ARCHIVO DETECTADO ]";
        titleTMP.fontSize  = 20;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color     = new Color(0.4f, 0.9f, 1f);           // cyan — cabecera de terminal
        titleTMP.alignment = TextAlignmentOptions.Center;

        // Contenido del archivo — FirstFileKeyEvent escribe aquí en runtime
        var contentGO = TutorialSceneSetupShared.CreateTMPChild(
            "FileContent", panelGO.transform,
            new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.78f));
        var contentTMP = contentGO.GetComponent<TextMeshProUGUI>();
        contentTMP.fontSize  = 22;
        contentTMP.color     = new Color(0.78f, 1f, 0.78f);       // verde pálido — texto archivo
        contentTMP.alignment = TextAlignmentOptions.TopLeft;

        panelGO.SetActive(false);   // FirstFileKeyEvent lo activa en PlayFinalSequence()
        return (panelGO, contentTMP, cg);
    }

    // ── FIRST FILE KEY EVENT ──────────────────────────────────────────────────

    static void SetupFirstFileKeyEvent(
        GameObject filePanel, TextMeshProUGUI fileContent, CanvasGroup filePanelGroup,
        LumaGuide luma, GameObject exitPort, GameObject player)
    {
        if (Object.FindAnyObjectByType<FirstFileKeyEvent>() != null) return;

        var go = new GameObject("FirstFileKeyEvent");
        Undo.RegisterCreatedObjectUndo(go, "Create FirstFileKeyEvent");

        var fke = go.AddComponent<FirstFileKeyEvent>();
        var so  = new SerializedObject(fke);
        so.FindProperty("filePanel").objectReferenceValue      = filePanel;
        so.FindProperty("fileContent").objectReferenceValue    = fileContent;
        so.FindProperty("filePanelGroup").objectReferenceValue = filePanelGroup;
        so.FindProperty("luma").objectReferenceValue           = luma;
        so.FindProperty("exitPort").objectReferenceValue       = exitPort;

        if (player != null)
        {
            var ctrl = player.GetComponent<PlayerController>();
            if (ctrl != null)
                so.FindProperty("player").objectReferenceValue = ctrl;
            else
                Debug.LogWarning("[T08 Setup] PlayerController no encontrado en el Player. " +
                                 "Asígnalo manualmente en el Inspector de FirstFileKeyEvent.player.");
        }

        so.ApplyModifiedProperties();
    }

    // ── BLOCK 08 ─────────────────────────────────────────────────────────────

    static void SetupBlock08(LumaGuide luma)
    {
        var existing = GameObject.Find("BlockManager");
        var bmGO = existing ?? new GameObject("BlockManager");
        if (existing == null) Undo.RegisterCreatedObjectUndo(bmGO, "Create BlockManager");

        var block = bmGO.GetComponent<Block08_PuertoSalida>()
                 ?? Undo.AddComponent<Block08_PuertoSalida>(bmGO);

        var so = new SerializedObject(block);
        so.FindProperty("luma").objectReferenceValue = luma;
        so.ApplyModifiedProperties();
    }
}
