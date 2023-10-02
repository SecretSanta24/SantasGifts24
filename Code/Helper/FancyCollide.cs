using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Helper {
	public class FancyCollide {

		public static Vector2? DoRaycast<T>(Scene scene, Vector2 start, Vector2 end) where T : Entity
			=> DoRaycast(scene.Tracker.GetEntities<T>().Select(s => s.Collider), start, end);

		public static Vector2? DoRaycast<T, TExclude>(Scene scene, Vector2 start, Vector2 end) where T : Entity
			=> DoRaycast(scene.Tracker.GetEntities<T>().Where(e => e is not TExclude).Select(s => s.Collider), start, end);

		public static Vector2? DoRaycast(IEnumerable<Collider> cols, Vector2 start, Vector2 end) {
			Vector2? curPoint = null;
			foreach (Collider c in cols) {
				if (!(DoRaycast(c, start, end) is Vector2 intersectionPoint)) continue;
				curPoint = intersectionPoint;
				end = intersectionPoint;
			}
			return curPoint;
		}

		public static Vector2? DoRaycast(Collider col, Vector2 start, Vector2 end) => col switch {
			Hitbox hbox => DoRaycast(hbox, start, end),
			Grid grid => DoRaycast(grid, start, end),
			ColliderList colList => DoRaycast(colList.colliders, start, end),
			_ => null //Unknown collider type
		};

		public static Vector2? DoRaycast(Hitbox hbox, Vector2 start, Vector2 end) {
			start -= hbox.AbsolutePosition;
			end -= hbox.AbsolutePosition;

			Vector2 dir = Vector2.Normalize(end - start);
			float tmin = float.NegativeInfinity, tmax = float.PositiveInfinity;

			if (dir.X != 0) {
				float tx1 = (hbox.Left - start.X) / dir.X, tx2 = (hbox.Right - start.X) / dir.X;
				tmin = Math.Max(tmin, Math.Min(tx1, tx2));
				tmax = Math.Min(tmax, Math.Max(tx1, tx2));
			}
			else if (start.X < hbox.Left || start.X > hbox.Right) return null;

			if (dir.Y != 0) {
				float ty1 = (hbox.Top - start.Y) / dir.Y, ty2 = (hbox.Bottom - start.Y) / dir.Y;
				tmin = Math.Max(tmin, Math.Min(ty1, ty2));
				tmax = Math.Min(tmax, Math.Max(ty1, ty2));
			}
			else if (start.Y < hbox.Top || start.Y > hbox.Bottom) return null;

			return (0 <= tmin && tmin <= tmax && tmin * tmin <= Vector2.DistanceSquared(start, end)) ? hbox.AbsolutePosition + start + tmin * dir : null;
		}

		public static Vector2? DoRaycast(Grid grid, Vector2 start, Vector2 end) {

			start = (start - grid.AbsolutePosition) / new Vector2(grid.CellWidth, grid.CellHeight);
			end = (end - grid.AbsolutePosition) / new Vector2(grid.CellWidth, grid.CellHeight);
			Vector2 dir = Vector2.Normalize(end - start);
			int xDir = Math.Sign(end.X - start.X), yDir = Math.Sign(end.Y - start.Y);
			if (xDir == 0 && yDir == 0) return null;
			int gridX = (int)start.X, gridY = (int)start.Y;
			float nextX = xDir < 0 ? (float)Math.Ceiling(start.X) - 1 : xDir > 0 ? (float)Math.Floor(start.X) + 1 : float.PositiveInfinity;
			float nextY = yDir < 0 ? (float)Math.Ceiling(start.Y) - 1 : yDir > 0 ? (float)Math.Floor(start.Y) + 1 : float.PositiveInfinity;
			while (Math.Sign(end.X - start.X) != -xDir || Math.Sign(end.Y - start.Y) != -yDir) {
				if (grid[gridX, gridY]) {
					return grid.AbsolutePosition + start * new Vector2(grid.CellWidth, grid.CellHeight);
				}
				if (Math.Abs((nextX - start.X) * dir.Y) < Math.Abs((nextY - start.Y) * dir.X)) {
					start.Y += Math.Abs((nextX - start.X) / dir.X) * dir.Y;
					start.X = nextX;
					nextX += xDir;
					gridX += xDir;
				}
				else {
					start.X += Math.Abs((nextY - start.Y) / dir.Y) * dir.X;
					start.Y = nextY;
					nextY += yDir;
					gridY += yDir;
				}
			}
			return null;
		}
	}
}
