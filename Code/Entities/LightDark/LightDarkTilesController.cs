using Celeste.Mod.Entities;
using Celeste.Mod.SantasGifts24.Code.Components;
using Celeste.Mod.SantasGifts24.Code.Mechanics;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    [Tracked]
    [CustomEntity("SS2024/LightDarkTilesController")]
    internal class LightDarkTilesController : Entity
    {
        private Dictionary<char, char> tileChanges = new Dictionary<char, char>();
        private VirtualMap<char> oldFgData;
        private VirtualMap<MTexture> oldFgTexes;
        private VirtualMap<char> newFgData;
        private VirtualMap<MTexture> newFgTexes;
        private LightDarkMode currentMode = LightDarkMode.Normal;

        public LightDarkTilesController(EntityData data, Vector2 offset) : base()
        {
            Add(new LightDarkListener(OnModeChange));
            TransitionListener listener = new TransitionListener();
            string changesStr = data.Attr("tileChanges", "")
                .Replace(" ", "")
                .Replace(",", "");
            if (string.IsNullOrEmpty(changesStr) || changesStr.Length % 2 != 0)
            {
                Logger.Log(LogLevel.Error, "SecretSanta2024", $"LightDarkTilesController has invalid changes string: {changesStr}");
            }
            for (int i = 0; i < changesStr.Length; i += 2)
            {
                tileChanges.Add(changesStr[i], changesStr[i + 1]);
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (scene is Level level)
            {
                GenerateTiles(level);
                OnModeChange(level.LightDarkGet());
            }
        }

        private void GetData(Level level, out int tw, out int th, out int ox, out int oy, out VirtualMap<char> fgData, out VirtualMap<MTexture> fgTexes)
        {
            tw = (int)Math.Ceiling(level.Bounds.Width / 8f);
            th = (int)Math.Ceiling(level.Bounds.Height / 8f);
            ox = (int)Math.Round((double)level.LevelSolidOffset.X);
            oy = (int)Math.Round((double)level.LevelSolidOffset.Y);
            fgData = level.SolidsData;
            fgTexes = level.SolidTiles.Tiles.Tiles;
        }

        private void GenerateTiles(Level level)
        {
            // A lot of this code is closely modeled after Pandoras Box Tile Glitcher code
            GetData(level, out int tw, out int th, out int ox, out int oy, out VirtualMap<char> fgData, out VirtualMap<MTexture> fgTexes);

            oldFgData = new VirtualMap<char>(tw, th, '0');
            oldFgTexes = new VirtualMap<MTexture>(tw, th, null);
            newFgData = new VirtualMap<char>(tw, th, '0');
            newFgTexes = new VirtualMap<MTexture>(tw, th, null);

            for (int x = 0; x < tw; x++)
            {
                for (int y = 0; y < th; y++)
                {
                    char tile = fgData[x + ox, y + oy];
                    oldFgData[x, y] = tile;
                    if (tileChanges.ContainsKey(tile))
                    {
                        newFgData[x, y] = tileChanges[tile];
                    }
                    else
                    {
                        newFgData[x, y] = tile;
                        newFgTexes[x, y] = fgTexes[x + ox, y + oy];
                    }
                }
            }

            Autotiler.Generated newFgTiles = GFX.FGAutotiler.GenerateMap(newFgData, true);

            for (int x = 0; x < tw; x++)
            {
                for (int y = 0; y < th; y++)
                {
                    oldFgTexes[x, y] = fgTexes[x + ox, y + oy];
                    if (newFgTexes[x, y] == null) newFgTexes[x, y] = newFgTiles.TileGrid.Tiles[x, y];
                }
            }
        }

        private void OnModeChange(LightDarkMode newMode)
        {
            if (newMode == currentMode) return;
            currentMode = newMode;
            if (newMode == LightDarkMode.Dark) ApplyAlteredTiles(SceneAs<Level>());
            else RestoreOriginalTiles(SceneAs<Level>());
        }

        private void ApplyAlteredTiles(Level level)
        {
            GetData(level, out int tw, out int th, out int ox, out int oy, out VirtualMap<char> fgData, out VirtualMap<MTexture> fgTexes);
            for (int x = 0; x < tw; x++)
            {
                for (int y = 0; y < th; y++)
                {
                    fgData[x + ox, y + oy] = newFgData[x, y];
                    fgTexes[x + ox, y + oy] = newFgTexes[x, y];
                }
            }
        }

        private void RestoreOriginalTiles(Level level)
        {
            GetData(level, out int tw, out int th, out int ox, out int oy, out VirtualMap<char> fgData, out VirtualMap<MTexture> fgTexes);
            for (int x = 0; x < tw; x++)
            {
                for (int y = 0; y < th; y++)
                {
                    fgData[x + ox, y + oy] = oldFgData[x, y];
                    fgTexes[x + ox, y + oy] = oldFgTexes[x, y];
                }
            }
        }

    }
}
