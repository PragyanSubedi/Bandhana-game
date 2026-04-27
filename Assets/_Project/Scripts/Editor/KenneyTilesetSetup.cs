#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Bandhana.EditorTools
{
    // One-shot setup for the Kenney roguelike-indoors sheet:
    //   1. Configures the texture importer (Multi sprite, point filter, 16 PPU, no compression).
    //   2. Slices the sheet on a 27x18 grid of 16x16 tiles with 1px padding.
    //   3. Generates one Tile asset per sliced sprite under Tiles/.
    //   4. Builds (or refreshes) a Tile Palette prefab and stamps every tile into it
    //      in its original 27x18 layout.
    public static class KenneyTilesetSetup
    {
        const string SheetPath     = "Assets/_Project/Art/Tilesets/Kenney/roguelikeIndoor_transparent.png";
        const string TilesFolder   = "Assets/_Project/Art/Tilesets/Kenney/Tiles";
        const string PaletteFolder = "Assets/_Project/Art/Tilesets/Kenney/Palette";
        const string PaletteName   = "KenneyRoguelikeIndoor";

        const int TileSize = 16;
        const int Padding  = 1;
        const int Cols     = 27;
        const int Rows     = 18;

        [MenuItem("Bandhana/Tilesets/Setup Kenney Roguelike Indoor")]
        public static void Setup()
        {
            if (!ConfigureImporterAndSlice()) return;
            int created = GenerateTileAssets();
            int painted = BuildPalette();
            EditorUtility.DisplayDialog(
                "Kenney tileset",
                $"Sliced {Cols * Rows} sprites, wrote/updated {created} Tile assets, and painted {painted} cells into the palette at:\n{PaletteFolder}/{PaletteName}.prefab\n\n" +
                "Next: Window > 2D > Tile Palette, pick \"" + PaletteName + "\" from the dropdown, then create a Tilemap (GameObject > 2D Object > Tilemap > Rectangular) and start painting.",
                "OK");
        }

        [MenuItem("Bandhana/Tilesets/Rebuild Kenney Palette Only")]
        public static void RebuildPaletteOnly()
        {
            int painted = BuildPalette();
            EditorUtility.DisplayDialog("Kenney palette",
                $"Painted {painted} cells into {PaletteName}.prefab.", "OK");
        }

        [MenuItem("Bandhana/Tilesets/Diagnose Palette API")]
        public static void DiagnosePaletteAPI()
        {
            var asm = typeof(GridPalette).Assembly;
            Debug.Log($"[Diag] GridPalette assembly: {asm.FullName}");

            var type = asm.GetType("UnityEditor.Tilemaps.GridPaletteUtility");
            if (type == null)
            {
                Debug.LogError("[Diag] UnityEditor.Tilemaps.GridPaletteUtility not found in this assembly. Searching all loaded assemblies...");
                foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    var t = a.GetType("UnityEditor.Tilemaps.GridPaletteUtility");
                    if (t != null) { type = t; Debug.Log($"[Diag] Found GridPaletteUtility in {a.FullName}"); break; }
                }
            }
            if (type == null) { Debug.LogError("[Diag] GridPaletteUtility not found anywhere."); return; }

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var m in methods)
            {
                var ps = m.GetParameters();
                var sig = string.Join(", ", System.Array.ConvertAll(ps, p => $"{p.ParameterType.Name} {p.Name}"));
                Debug.Log($"[Diag] {m.ReturnType.Name} {m.Name}({sig})");
            }
        }

        static bool ConfigureImporterAndSlice()
        {
            var importer = AssetImporter.GetAtPath(SheetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"[KenneyTilesetSetup] Texture not found at {SheetPath}");
                return false;
            }

            importer.textureType        = TextureImporterType.Sprite;
            importer.spriteImportMode   = SpriteImportMode.Multiple;
            importer.filterMode         = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = TileSize;
            importer.mipmapEnabled      = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode           = TextureWrapMode.Clamp;
            importer.isReadable         = false;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType                    = SpriteMeshType.FullRect;
            settings.spriteExtrude                     = 0;
            settings.spriteGenerateFallbackPhysicsShape = false;
            settings.spriteAlignment                   = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);

            // Unity sprite rects are measured from the bottom-left of the texture,
            // but Kenney's grid is row 0 = top, so we flip the row index here.
            var metas = new List<SpriteMetaData>(Cols * Rows);
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    metas.Add(new SpriteMetaData
                    {
                        name = $"roguelikeIndoor_{c:00}_{r:00}",
                        rect = new Rect(
                            c * (TileSize + Padding),
                            (Rows - 1 - r) * (TileSize + Padding),
                            TileSize,
                            TileSize),
                        alignment = (int)SpriteAlignment.Center,
                        pivot     = new Vector2(0.5f, 0.5f),
                        border    = Vector4.zero
                    });
                }
            }
            importer.spritesheet = metas.ToArray();
            importer.SaveAndReimport();
            return true;
        }

        static int GenerateTileAssets()
        {
            if (!AssetDatabase.IsValidFolder(TilesFolder))
            {
                Directory.CreateDirectory(TilesFolder);
                AssetDatabase.Refresh();
            }

            int touched = 0;
            var assets = AssetDatabase.LoadAllAssetsAtPath(SheetPath);
            foreach (var a in assets)
            {
                if (a is not Sprite sprite) continue;
                var path = $"{TilesFolder}/{sprite.name}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<Tile>(path);
                if (existing == null)
                {
                    var tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = sprite;
                    AssetDatabase.CreateAsset(tile, path);
                    touched++;
                }
                else if (existing.sprite != sprite)
                {
                    existing.sprite = sprite;
                    EditorUtility.SetDirty(existing);
                    touched++;
                }
            }
            AssetDatabase.SaveAssets();
            return touched;
        }

        // Builds (or rebuilds) a palette prefab at PaletteFolder/PaletteName.prefab.
        // Uses Unity's internal GridPaletteUtility.CreateNewPaletteAtPath via reflection
        // to create the prefab + GridPalette sub-asset (the same code path the Tile Palette
        // window's "Create New Palette" button uses), then opens the prefab in edit mode
        // and paints every Tile asset onto its Tilemap in the original 27x18 layout.
        static int BuildPalette()
        {
            if (!AssetDatabase.IsValidFolder(PaletteFolder))
            {
                Directory.CreateDirectory(PaletteFolder);
                AssetDatabase.Refresh();
            }

            var tileGuids = AssetDatabase.FindAssets("t:Tile", new[] { TilesFolder });
            if (tileGuids.Length == 0)
            {
                Debug.LogError($"[KenneyTilesetSetup] No Tile assets found under {TilesFolder}. Run Setup first.");
                return 0;
            }

            var byPos = new Dictionary<Vector2Int, Tile>(tileGuids.Length);
            foreach (var guid in tileGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
                if (tile == null) continue;
                var nm = Path.GetFileNameWithoutExtension(path);
                var parts = nm.Split('_');
                if (parts.Length < 3) continue;
                if (!int.TryParse(parts[parts.Length - 2], out int col)) continue;
                if (!int.TryParse(parts[parts.Length - 1], out int row)) continue;
                byPos[new Vector2Int(col, row)] = tile;
            }

            // Wipe any previous attempt for a clean slate.
            var prefabPath = $"{PaletteFolder}/{PaletteName}.prefab";
            if (AssetDatabase.LoadAssetAtPath<Object>(prefabPath) != null)
                AssetDatabase.DeleteAsset(prefabPath);

            // Create the palette via Unity's own internal routine (guarantees a valid
            // GridPalette sub-asset, which is what the Tile Palette window scans for).
            var prefab = CreatePaletteViaInternalAPI(PaletteFolder, PaletteName);
            if (prefab == null)
            {
                Debug.LogError("[KenneyTilesetSetup] Could not invoke GridPaletteUtility.CreateNewPaletteAtPath via reflection.");
                return 0;
            }

            // Open the prefab in edit mode, ensure a Tilemap child exists, paint, save.
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var tilemap = root.GetComponentInChildren<Tilemap>();
                if (tilemap == null)
                {
                    var tilemapGO = new GameObject("Layer1");
                    tilemapGO.transform.SetParent(root.transform, false);
                    tilemap = tilemapGO.AddComponent<Tilemap>();
                    tilemapGO.AddComponent<TilemapRenderer>();
                }

                tilemap.ClearAllTiles();
                int painted = 0;
                foreach (var kv in byPos)
                {
                    // y = -row so row 0 (top of the sheet) ends up at the top of the palette.
                    tilemap.SetTile(new Vector3Int(kv.Key.x, -kv.Key.y, 0), kv.Value);
                    painted++;
                }
                tilemap.CompressBounds();

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                AssetDatabase.ImportAsset(prefabPath, ImportAssetOptions.ForceUpdate);
                return painted;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // Reflection wrapper around UnityEditor.Tilemaps.GridPaletteUtility.CreateNewPalette
        // (older Unity versions: CreateNewPaletteAtPath). The type isn't always in the
        // same assembly as GridPalette (it can be in Unity.2D.Tilemap.Editor), so we
        // search every loaded assembly.
        static GameObject CreatePaletteViaInternalAPI(string folder, string name)
        {
            System.Type type = null;
            foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType("UnityEditor.Tilemaps.GridPaletteUtility");
                if (type != null) break;
            }
            if (type == null)
            {
                Debug.LogError("[KenneyTilesetSetup] GridPaletteUtility type not found in any loaded assembly.");
                return null;
            }

            // Multiple overloads exist; pick the (string, string, CellLayout, CellSizing,
            // Vector3, CellSwizzle) one explicitly. Fall back to looser matching if the
            // exact signature isn't present in this Unity version.
            const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var sixArgTypes = new[]
            {
                typeof(string), typeof(string),
                typeof(GridLayout.CellLayout),
                typeof(GridPalette.CellSizing),
                typeof(Vector3),
                typeof(GridLayout.CellSwizzle),
            };
            MethodInfo method = null;
            foreach (var methodName in new[] { "CreateNewPalette", "CreateNewPaletteAtPath" })
            {
                method = type.GetMethod(methodName, Flags, null, sixArgTypes, null);
                if (method != null) break;

                // Fallback: any overload by name with a supported arg count.
                foreach (var candidate in type.GetMethods(Flags))
                {
                    if (candidate.Name != methodName) continue;
                    var n = candidate.GetParameters().Length;
                    if (n == 5 || n == 6 || n == 8) { method = candidate; break; }
                }
                if (method != null) break;
            }
            if (method == null)
            {
                Debug.LogError("[KenneyTilesetSetup] No suitable CreateNewPalette / CreateNewPaletteAtPath overload on GridPaletteUtility.");
                return null;
            }

            var ps = method.GetParameters();
            object[] args = ps.Length switch
            {
                6 => new object[]
                {
                    folder, name,
                    GridLayout.CellLayout.Rectangle,
                    GridPalette.CellSizing.Automatic,
                    Vector3.one,
                    GridLayout.CellSwizzle.XYZ,
                },
                5 => new object[]
                {
                    folder, name,
                    GridLayout.CellLayout.Rectangle,
                    GridPalette.CellSizing.Automatic,
                    Vector3.one,
                },
                8 => new object[]
                {
                    folder, name,
                    GridLayout.CellLayout.Rectangle,
                    GridPalette.CellSizing.Automatic,
                    Vector3.one,
                    GridLayout.CellSwizzle.XYZ,
                    TransparencySortMode.Default,
                    new Vector3(0f, 0f, 1f),
                },
                _ => null,
            };
            if (args == null)
            {
                Debug.LogError($"[KenneyTilesetSetup] Unexpected {method.Name} signature ({ps.Length} params).");
                return null;
            }

            try
            {
                return method.Invoke(null, args) as GameObject;
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                Debug.LogError($"[KenneyTilesetSetup] {method.Name} threw: {ex.InnerException}");
                return null;
            }
        }
    }
}
#endif
