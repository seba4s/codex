using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using CODEX.UI;

/// <summary>
/// Crea la barra de vida con sprites de ui x1.png (ya cortado en el editor).
/// Menú: CODEX → Setup UI → Barra de Vida (Sprites)
/// </summary>
public static class HealthBarSpriteSetup
{
    private const string SheetPath = "Assets/Sprites/UI/AssetsPersonajes/UI/ui x1.png";

    [MenuItem("CODEX/Setup UI/Barra de Vida (Sprites)")]
    public static void Setup()
    {
        // ── 1. Cargar todos los sub-sprites del sheet ──────────────────────
        var allAssets = AssetDatabase.LoadAllAssetsAtPath(SheetPath);
        var sprites = new List<(int index, Sprite sprite)>();

        foreach (var obj in allAssets)
        {
            if (obj is Sprite s)
            {
                // Nombre formato "ui x1_N" — extraer N
                var parts = s.name.Split('_');
                if (parts.Length > 0 && int.TryParse(parts[parts.Length - 1], out int idx))
                {
                    if (idx == 1) continue; // corrupto, saltar
                    sprites.Add((idx, s));
                }
            }
        }

        if (sprites.Count == 0)
        {
            EditorUtility.DisplayDialog("Barra de Vida",
                "No se encontraron sub-sprites en:\n" + SheetPath +
                "\n\nEn Unity: selecciona ui x1.png → Inspector → Sprite Mode: Multiple → Sprite Editor → Slice → Apply.",
                "OK");
            return;
        }

        // Ordenar por índice: 0, 2, 3, 4 … 27
        sprites.Sort((a, b) => a.index.CompareTo(b.index));
        Sprite[] ordered = sprites.Select(x => x.sprite).ToArray();

        Debug.Log($"[CODEX] Barra de vida: {ordered.Length} sprites cargados " +
                  $"({sprites[0].index} → {sprites[sprites.Count - 1].index}, sin el índice 1)");

        // ── 2. Buscar Canvas (preferir HUD_Canvas Screen Space Overlay) ───
        Canvas canvas = null;
        var hudGO = GameObject.Find("HUD_Canvas");
        if (hudGO != null) canvas = hudGO.GetComponent<Canvas>();
        if (canvas == null)
        {
            foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay) { canvas = c; break; }
            }
        }
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Barra de Vida",
                "No hay Canvas Screen Space Overlay en la escena.\nEjecuta primero el Setup de la escena.", "OK");
            return;
        }

        // ── 3. Eliminar barra anterior si existe ───────────────────────────
        var old = GameObject.Find("HealthBar_Sprite");
        if (old != null) Undo.DestroyObjectImmediate(old);

        // ── 4. Crear contenedor ────────────────────────────────────────────
        var root = new GameObject("HealthBar_Sprite");
        Undo.RegisterCreatedObjectUndo(root, "Create HealthBar");
        root.transform.SetParent(canvas.transform, false);

        var rt = root.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0f, 1f);
        rt.anchorMax        = new Vector2(0f, 1f);
        rt.pivot            = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(20f, -20f);

        // Tamaño proporcional al sprite (escala ×3)
        Sprite fullSprite = ordered[ordered.Length - 1]; // último = más lleno (índice 27)
        float aspect = fullSprite.rect.width / fullSprite.rect.height;
        rt.sizeDelta = new Vector2(96f * aspect, 96f);

        // ── 5. Image que muestra el sprite actual ──────────────────────────
        var img = root.AddComponent<Image>();
        img.sprite        = fullSprite;
        img.preserveAspect = true;
        img.raycastTarget  = false;

        // ── 6. Componente HealthBarSprite ──────────────────────────────────
        var hbs = root.AddComponent<HealthBarSprite>();
        var so  = new SerializedObject(hbs);
        so.FindProperty("displayImage").objectReferenceValue = img;

        var arr = so.FindProperty("healthSprites");
        arr.arraySize = ordered.Length;
        for (int i = 0; i < ordered.Length; i++)
            arr.GetArrayElementAtIndex(i).objectReferenceValue = ordered[i];

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(root);

        EditorUtility.DisplayDialog("Barra de Vida",
            $"Barra creada con {ordered.Length} sprites.\n\n" +
            $"• Vida llena : {fullSprite.name}\n" +
            $"• Vida vacía : {ordered[0].name}\n\n" +
            "Al recibir daño el sprite cambia automáticamente.\n" +
            "Al llegar a 0 se muestra el sprite vacío y el jugador muere.", "OK");
    }
}
