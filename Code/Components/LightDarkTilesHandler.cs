using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SantasGifts24.Code.Components {
	[Tracked]
	internal class LightDarkTilesHandler : Component {

		public LightDarkTilesHandler() : base(true, false) { }

		public void SetTiles(char? type = null) {
			type ??= GetOriginalTileType();
			if (type == null) return;
			SwapTiles(GetBlendin(), type.Value);
		}

		internal bool GetBlendin() {
			if (Entity is DashBlock db) return db.blendIn;
			return false;
		}

		internal char? GetOriginalTileType() {
			if (Entity is DashBlock db) return db.tileType;
			return null;
		}

		private void SwapTiles(bool blend, char tileType) {
			Entity e = Entity;
			if (e == null) return;
			TileGrid oldGrid = e.Get<TileGrid>();
			if (oldGrid == null) return;
			oldGrid.RemoveSelf();
			e.Get<TileInterceptor>()?.RemoveSelf();
			TileGrid tileGrid;
			if (blend) {
				Level level = SceneAs<Level>();
				Rectangle tileBounds = level.Session.MapData.TileBounds;
				VirtualMap<char> solidsData = level.SolidsData;
				int x = (int)(e.X / 8f) - tileBounds.Left;
				int y = (int)(e.Y / 8f) - tileBounds.Top;
				int tilesX = (int)e.Width / 8;
				int tilesY = (int)e.Height / 8;
				tileGrid = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, tilesX, tilesY, solidsData).TileGrid;
			}
			else {
				tileGrid = GFX.FGAutotiler.GenerateBox(tileType, (int)e.Width / 8, (int)e.Height / 8).TileGrid;
			}
			e.Add(tileGrid);
			e.Add(new TileInterceptor(tileGrid, highPriority: true));
		}
	}
}
