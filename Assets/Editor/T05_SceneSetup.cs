using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using CODEX.Tutorial;
using CODEX.Tutorial.Blocks;
using CODEX.Player;
using CODEX.Systems;

/// <summary>
/// Configura T05_Terminal – Interacción con terminales de reparación.
/// El jugador activa el terminal para desbloquear la puerta de salida.
/// Mecánica nueva respecto a T04: presionar [E] en un terminal.
/// Menú: CODEX → Setup Escenas → T05 – Terminal
/// </summary>
public static class T05_SceneSetup
{
    [MenuItem("CODEX/Setup Escenas/T05 – Terminal")]
    public static void Setup()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.name.Contains("T05"))
        {
            bool ok = EditorUtility.DisplayDialog("CODEX – Setup T05",
                $"La escena activa es '{scene.name}'.\n¿Continuar de todas formas?",
                "Sí", "Cancelar");
            if (!ok) return;
        }

        Undo.SetCurrentGroupName("CODEX T05 Setup");
        int group = Undo.GetCurrentGroup();

        // ── Infraestructura común ──────────────────────────────────────────────
        TutorialSceneSetupShared.SetupGround("T05");
        TutorialSceneSetupShared.SetupCamera();
        TutorialSceneSetupShared.SetupBackground("Assets/Sprites/UI/Tutorial/fondo5.png");
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

        // ── Mecánica específica T05 ────────────────────────────────────────────

        // Terminal de reparación — el jugador presiona [E] para interactuar
        TutorialSceneSetupShared.SetupRepairTerminal(
            "Terminal_T05",
            new Vector3(-2f, TutorialSceneSetupShared.FloorY + 1f, 0f));

        // Puerta bloqueada (muro sólido) — Block05 la oculta al activar el terminal
        var blockedDoor = SetupBlockedDoor();

        // SceneLoader de salida — Block05 lo activa al interactuar con el terminal
        var openDoor = TutorialSceneSetupShared.SetupSceneLoader(
            "OpenDoor_T05_to_T06",
            new Vector3(9f, 0f, 0f));
        openDoor.SetActive(false);

        // Block05 — enlaza luma + puertas y escucha OnTerminalActivated
        SetupBlock05(luma, blockedDoor, openDoor);

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        EditorUtility.DisplayDialog("CODEX – T05 Listo",
            "T05_Terminal configurada.\n\n" +
            "✓ Suelo, cámara, luz, fondo (fondo5.png)\n" +
            "✓ Jugador con PlayerHealth\n" +
            "✓ LUMA, HUD, TutorialManager, CheckpointManager, SceneTransition\n" +
            "✓ Terminal_T05 en (-2, FloorY+1) — presionar [E] para activar\n" +
            "✓ BlockedDoor_T05 (muro rojo) en x=5 — bloquea el paso físicamente\n" +
            "✓ OpenDoor/SceneLoader en x=9 — inactivo hasta usar el terminal\n" +
            "✓ Block05_Terminal conectado (luma, blockedDoor, openDoor)", "OK");
    }

    // ── PUERTA BLOQUEADA (muro sólido) ───────────────────────────────────────

    static GameObject SetupBlockedDoor()
    {
        const string name = "BlockedDoor_T05";
        if (GameObject.Find(name) != null) return GameObject.Find(name);

        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create BlockedDoor");
        go.transform.position   = new Vector3(5f, 0f, 0f);
        go.transform.localScale = new Vector3(1f, 8f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = CODEX.Editor.EditorSpriteHelper.GetWhiteSprite();
        sr.color        = new Color(0.60f, 0.12f, 0.12f);   // rojo oscuro — barrera bloqueada
        sr.sortingOrder = 1;

        go.AddComponent<BoxCollider2D>();   // sólido (no trigger) — bloquea físicamente
        return go;
    }

    // ── BLOCK 05 ─────────────────────────────────────────────────────────────

    static void SetupBlock05(LumaGuide luma, GameObject blockedDoor, GameObject openDoor)
    {
        var existing = GameObject.Find("BlockManager");
        var bmGO = existing ?? new GameObject("BlockManager");
        if (existing == null) Undo.RegisterCreatedObjectUndo(bmGO, "Create BlockManager");

        var block = bmGO.GetComponent<Block05_Terminal>()
                 ?? Undo.AddComponent<Block05_Terminal>(bmGO);

        var so = new SerializedObject(block);
        so.FindProperty("luma").objectReferenceValue        = luma;
        so.FindProperty("blockedDoor").objectReferenceValue = blockedDoor;
        so.FindProperty("openDoor").objectReferenceValue    = openDoor;
        so.ApplyModifiedProperties();
    }
}
