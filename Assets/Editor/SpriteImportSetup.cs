using UnityEngine;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using System.IO;
using System.Collections.Generic;

namespace CODEX.Editor
{
    /// <summary>
    /// Configura los import settings de todos los sprites del juego.
    /// Menú: CODEX > Configurar Sprites
    /// </summary>
    public static class SpriteImportSetup
    {
        [MenuItem("CODEX/Configurar Sprites")]
        public static void SetupAll()
        {
            // ── Fondos del tutorial (Single, sin filtro) ──────────────
            for (int i = 1; i <= 7; i++)
                ConfigureSingle($"Assets/Sprites/UI/Tutorial/fondo{i}.png", 100, FilterMode.Bilinear);

            // ── CODIGO-7 spritesheet (2048x512 → celdas 256x256) ──────
            ConfigureSheet("Assets/Sprites/UI/AssetsPersonajes/CODIGO-7/CODIGO-7.png",
                cellW: 256, cellH: 256, ppu: 32);

            // ── Enemies x1 (64x192 → 4 cols × 4 rows, 16x48 por celda) ──
            // Fila 0=Idle, 1=Run, 2=Shoot, 3=Death (4 frames cada una)
            ConfigureSheet("Assets/Sprites/UI/AssetsPersonajes/Enemies/enemies x1.png",
                cellW: 16, cellH: 48, ppu: 16);
            ConfigureSheet("Assets/Sprites/UI/AssetsPersonajes/Enemies/enemies x2.png",
                cellW: 32, cellH: 32, ppu: 32);
            ConfigureSheet("Assets/Sprites/UI/AssetsPersonajes/Enemies/enemies x3.png",
                cellW: 48, cellH: 16, ppu: 48);

            // ── Players spritesheets ──────────────────────────────────
            string[] playerColors = { "blue", "green", "grey", "red" };
            string[] scales       = { "x1", "x2", "x3" };
            int[]    ppuValues    = { 32, 64, 96 };
            int[,]   cellSizes    = { { 40, 32 }, { 80, 64 }, { 120, 96 } };

            foreach (string color in playerColors)
            {
                for (int s = 0; s < scales.Length; s++)
                {
                    ConfigureSheet(
                        $"Assets/Sprites/UI/AssetsPersonajes/Players/players {color} {scales[s]}.png",
                        cellSizes[s, 0], cellSizes[s, 1], ppuValues[s]);

                    ConfigureSheet(
                        $"Assets/Sprites/UI/AssetsPersonajes/Players/No Outlines/players {color} {scales[s]}.png",
                        cellSizes[s, 0], cellSizes[s, 1], ppuValues[s]);
                }
            }

            // ── Enemy (frames individuales 72x72) ─────────────────────
            string enemyBase = "Assets/Sprites/UI/AssetsPersonajes/Enemy";
            ConfigureAnimFrame($"{enemyBase}/Idle.png",    72, 72, 32);
            ConfigureAnimFrame($"{enemyBase}/Walk.png",    72, 72, 32);
            ConfigureAnimFrame($"{enemyBase}/Attack1.png", 72, 72, 32);
            ConfigureAnimFrame($"{enemyBase}/Attack2.png", 72, 72, 32);
            ConfigureAnimFrame($"{enemyBase}/Attack3.png", 72, 72, 32);
            ConfigureAnimFrame($"{enemyBase}/Attack4.png", 72, 72, 32);
            ConfigureAnimFrame($"{enemyBase}/Death.png",   72, 72, 32);
            ConfigureAnimFrame($"{enemyBase}/Hurt.png",    72, 72, 32);
            ConfigureAnimFrame($"{enemyBase}/Special.png", 72, 72, 32);

            // ── EnemyFly ──────────────────────────────────────────────
            string flyBase  = "Assets/Sprites/UI/AssetsPersonajes/EnemyFly";
            string[] flyAnims = { "Idle", "Walk", "Attack1", "Attack2", "Attack3",
                                  "Death", "Hurt", "Special", "BOOM_death" };
            foreach (string anim in flyAnims)
                ConfigureAnimFrame($"{flyBase}/{anim}.png", 72, 72, 32);

            // ── BossLevel1 ────────────────────────────────────────────
            string bossBase  = "Assets/Sprites/UI/AssetsPersonajes/BossLevel1";
            string[] bossAnims = { "Idle", "Walk", "Attack1", "Attack2", "Attack3", "Attack4",
                                   "Death", "Hurt", "Special",
                                   "Death1","Death2","Death3","Death4","Death5","Death6" };
            foreach (string anim in bossAnims)
                ConfigureAnimFrame($"{bossBase}/{anim}.png", 96, 96, 32);

            // ── Proyectiles ───────────────────────────────────────────
            for (int s = 1; s <= 3; s++)
                ConfigureSheet(
                    $"Assets/Sprites/UI/AssetsPersonajes/Projectiles/projectiles x{s}.png",
                    cellW: 32 * s, cellH: 32 * s, ppu: 32 * s);

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Sprites configurados",
                "Import settings aplicados correctamente.\n\n" +
                "Los fondos son sprites simples (Single).\n" +
                "CODIGO-7, Players y Enemies son spritesheets cortados en grid.\n\n" +
                "Ahora ejecuta:\n" +
                "CODEX > Crear Prefabs del Jugador\n" +
                "para actualizar el prefab con el sprite real.",
                "OK");
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private static void ConfigureSingle(string path, int ppu, FilterMode filter)
        {
            if (!File.Exists(Path.GetFullPath(path))) return;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            importer.spriteImportMode    = SpriteImportMode.Single;
            importer.spritePivot         = new Vector2(0.5f, 0.5f);
            importer.spritePixelsPerUnit = ppu;
            importer.filterMode          = filter;
            importer.textureCompression  = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled       = false;
            importer.SaveAndReimport();
        }

        private static void ConfigureSheet(string path, int cellW, int cellH, int ppu)
        {
            if (!File.Exists(Path.GetFullPath(path))) return;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            importer.spriteImportMode    = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = ppu;
            importer.filterMode          = FilterMode.Point;
            importer.textureCompression  = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled       = false;
            importer.SaveAndReimport();

            // Leer dimensiones reales del PNG (no las de Unity que pueden estar paddeadas)
            if (!ReadPNGSize(Path.GetFullPath(path), out int texW, out int texH)) return;

            int cols = texW / cellW;
            int rows = texH / cellH;
            if (cols <= 0 || rows <= 0) return;

            var factory      = new SpriteDataProviderFactories();
            factory.Init();
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            dataProvider.InitSpriteEditorDataProvider();

            var rects = new List<SpriteRect>();
            int idx   = 0;
            for (int row = rows - 1; row >= 0; row--)
            {
                for (int col = 0; col < cols; col++)
                {
                    float x = col * cellW;
                    float y = row * cellH;
                    if (x + cellW > texW || y + cellH > texH) continue;

                    rects.Add(new SpriteRect
                    {
                        name      = $"{Path.GetFileNameWithoutExtension(path)}_{idx}",
                        rect      = new Rect(x, y, cellW, cellH),
                        pivot     = new Vector2(0.5f, 0.5f),
                        alignment = SpriteAlignment.Center,
                        spriteID  = GUID.Generate()
                    });
                    idx++;
                }
            }

            if (rects.Count == 0) return;
            dataProvider.SetSpriteRects(rects.ToArray());
            dataProvider.Apply();
            (dataProvider.targetObject as AssetImporter)?.SaveAndReimport();
        }

        private static void ConfigureAnimFrame(string path, int frameW, int frameH, int ppu)
        {
            if (!File.Exists(Path.GetFullPath(path))) return;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            // Usar dimensiones reales del PNG para evitar el error de rects fuera de textura
            if (!ReadPNGSize(Path.GetFullPath(path), out int texW, out int texH))
                return;

            int frameCount = (frameW > 0 && texW >= frameW) ? texW / frameW : 0;
            int usedH      = Mathf.Min(frameH, texH);

            importer.spriteImportMode    = frameCount <= 1 ? SpriteImportMode.Single : SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = ppu;
            importer.filterMode          = FilterMode.Point;
            importer.textureCompression  = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled       = false;
            importer.SaveAndReimport();

            if (frameCount <= 1) return;

            var factory      = new SpriteDataProviderFactories();
            factory.Init();
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            dataProvider.InitSpriteEditorDataProvider();

            var rects = new List<SpriteRect>();
            for (int i = 0; i < frameCount; i++)
            {
                float x = i * frameW;
                if (x + frameW > texW) break;   // rect fuera de la textura real

                rects.Add(new SpriteRect
                {
                    name      = $"{Path.GetFileNameWithoutExtension(path)}_{i}",
                    rect      = new Rect(x, 0, frameW, usedH),
                    pivot     = new Vector2(0.5f, 0.5f),
                    alignment = SpriteAlignment.Center,
                    spriteID  = GUID.Generate()
                });
            }

            if (rects.Count == 0) return;
            dataProvider.SetSpriteRects(rects.ToArray());
            dataProvider.Apply();
            (dataProvider.targetObject as AssetImporter)?.SaveAndReimport();
        }

        /// Lee ancho y alto directamente del header del PNG (bytes 16-23 del archivo).
        /// Esto es más fiable que GetSourceTextureWidthAndHeight, que puede devolver
        /// dimensiones paddeadas a potencia de 2.
        private static bool ReadPNGSize(string fullPath, out int width, out int height)
        {
            width = height = 0;
            try
            {
                using var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                var buf = new byte[24];
                if (fs.Read(buf, 0, 24) < 24) return false;

                // Firma PNG: 89 50 4E 47 0D 0A 1A 0A
                if (buf[0] != 0x89 || buf[1] != 0x50 || buf[2] != 0x4E || buf[3] != 0x47)
                    return false;

                // IHDR: bytes 16-19 = width, bytes 20-23 = height (big-endian)
                width  = (buf[16] << 24) | (buf[17] << 16) | (buf[18] << 8) | buf[19];
                height = (buf[20] << 24) | (buf[21] << 16) | (buf[22] << 8) | buf[23];
                return width > 0 && height > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
