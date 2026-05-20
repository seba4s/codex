using UnityEngine;
using UnityEditor;

public static class AssignTutorialBackgrounds
{
    [MenuItem("Tools/Assign Tutorial Backgrounds")]
    public static void Assign()
    {
        string[] bgNames = {
            "[BACKGROUNDS]/BG_Bloque1",
            "[BACKGROUNDS]/BG_Bloque2",
            "[BACKGROUNDS]/BG_Bloque3",
            "[BACKGROUNDS]/BG_Bloque4",
            "[BACKGROUNDS]/BG_Bloque5",
            "[BACKGROUNDS]/BG_Bloque6",
            "[BACKGROUNDS]/BG_Bloque7"
        };

        string[] spritePaths = {
            "Assets/Sprites/UI/Tutorial/fondo1.png",
            "Assets/Sprites/UI/Tutorial/fondo2.png",
            "Assets/Sprites/UI/Tutorial/fondo3.png",
            "Assets/Sprites/UI/Tutorial/fondo4.png",
            "Assets/Sprites/UI/Tutorial/fondo5.png",
            "Assets/Sprites/UI/Tutorial/fondo6.png",
            "Assets/Sprites/UI/Tutorial/fondo7.png"
        };

        // Camera reference for scale
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
        float camHeight = cam != null ? cam.orthographicSize * 2f : 10f;
        float camWidth  = camHeight * (16f / 9f);

        int assigned = 0;

        for (int i = 0; i < bgNames.Length; i++)
        {
            // Find the GameObject
            GameObject go = GameObject.Find(bgNames[i].Split('/')[1]);
            if (go == null)
            {
                // Try by name alone
                go = GameObject.Find("BG_Bloque" + (i + 1));
            }

            if (go == null)
            {
                Debug.LogWarning("No encontrado: BG_Bloque" + (i + 1));
                continue;
            }

            // Load sprite
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePaths[i]);
            if (sprite == null)
            {
                Debug.LogWarning("Sprite no encontrado: " + spritePaths[i]);
                continue;
            }

            // Assign
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) sr = go.AddComponent<SpriteRenderer>();

            sr.sprite = sprite;
            sr.sortingOrder = -10;

            // Scale to fill screen
            if (sprite != null)
            {
                float spriteWidth  = sprite.bounds.size.x;
                float spriteHeight = sprite.bounds.size.y;

                float scaleX = camWidth  / spriteWidth;
                float scaleY = camHeight / spriteHeight;
                float scale  = Mathf.Max(scaleX, scaleY); // cover mode

                go.transform.localScale = new Vector3(scale, scale, 1f);
            }

            EditorUtility.SetDirty(go);
            assigned++;
        }

        Debug.Log($"Backgrounds asignados: {assigned}/7");
        if (assigned > 0)
            EditorUtility.DisplayDialog("Listo", $"{assigned} fondos asignados y escalados.", "OK");
        else
            EditorUtility.DisplayDialog("Error", "No se encontraron los GameObjects BG_BloqueX ni los sprites.\nVerifica que la escena tiene la estructura creada y que los fondos están en Assets/Sprites/UI/Tutorial/", "OK");
    }
}
