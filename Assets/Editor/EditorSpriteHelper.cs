using UnityEngine;
using UnityEditor;
using System.IO;

namespace CODEX.Editor
{
    /// <summary>
    /// Sprite blanco 1x1 guardado en el proyecto, usado por todos los setup scripts.
    /// </summary>
    public static class EditorSpriteHelper
    {
        private const string SpritePath = "Assets/Editor/WhiteSquare.png";

        public static Sprite GetWhiteSprite()
        {
            // Intentar cargar el existente
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            if (existing != null) return existing;

            // Crear la textura y guardarla como PNG
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();

            File.WriteAllBytes(Path.GetFullPath(SpritePath), tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(SpritePath);

            // Configurar como sprite
            var importer = AssetImporter.GetAtPath(SpritePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType         = TextureImporterType.Sprite;
                importer.spriteImportMode    = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 4f;
                importer.filterMode          = FilterMode.Point;
                importer.mipmapEnabled       = false;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        }
    }
}
