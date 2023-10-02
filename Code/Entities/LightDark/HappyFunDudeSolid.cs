using Celeste.Mod.SantasGifts24.Code.Components;
using Celeste.Mod.SantasGifts24.Code.Mechanics;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Entities.LightDark {
	public class HappyFunDudeSolid : Solid {

		private Sprite _normalSprite;
		private Sprite _darkSprite;

		public LightDarkMode CurrentMode { get; private set; } = LightDarkMode.Normal;

		protected Sprite NormalSprite {
			get { return _normalSprite; }
			set {
				_normalSprite = value;
				UpdateSprite();
			}
		}

		protected Sprite DarkSprite {
			get { return _darkSprite; }
			set {
				_darkSprite = value;
				UpdateSprite();
			}
		}

		private Sprite currentSprite;

		public bool SpriteVisible {
			get {
				return currentSprite?.Visible ?? false;
			}
			set {
				NormalSprite.Visible = value;
				DarkSprite.Visible = value;
			}
		}

		public HappyFunDudeSolid(Vector2 pos, int width, int height)
			: base(pos, width, height, false)
		{
			Add(new LightDarkListener(SetDarknessMode));
		}

		public HappyFunDudeSolid(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height)
		{ }

		public override void Added(Scene scene) {
			base.Added(scene);
			if (scene is Level level) {
				SetDarknessMode(level.LightDarkGet());
			}
		}

		public virtual void SetDarknessMode(LightDarkMode newMode) {
			if (newMode == CurrentMode) return;
			CurrentMode = newMode;
			UpdateSprite();
		}

		private void UpdateSprite() {
			string anim = "";
			int frame = 0;
			if (currentSprite != null) {
				anim = currentSprite.CurrentAnimationID;
				frame = currentSprite.CurrentAnimationFrame;
				currentSprite.RemoveSelf();
			}
			if (CurrentMode == LightDarkMode.Normal && NormalSprite != null) Add(currentSprite = NormalSprite);
			if (CurrentMode == LightDarkMode.Dark && DarkSprite != null) Add(currentSprite = DarkSprite);
			if (!string.IsNullOrEmpty(anim)) {
				currentSprite.Play(anim);
				currentSprite.SetAnimationFrame(frame);
			}
		}

		new public bool MoveHCollideSolids(float moveH, bool thruDashBlocks, Action<Vector2, Vector2, Platform> onCollide = null) {
			if (Engine.DeltaTime == 0f) {
				LiftSpeed.X = 0f;
			}
			else {
				LiftSpeed.X = moveH / Engine.DeltaTime;
			}

			movementCounter.X += moveH;
			int num = (int)Math.Round(movementCounter.X);
			if (num != 0) {
				movementCounter.X -= num;
				return MoveHExactCollideSolids(num, thruDashBlocks, onCollide);
			}
			return false;
		}

		new public bool MoveVCollideSolids(float moveV, bool thruDashBlocks, Action<Vector2, Vector2, Platform> onCollide = null) {
			if (Engine.DeltaTime == 0f) {
				LiftSpeed.Y = 0f;
			}
			else {
				LiftSpeed.Y = moveV / Engine.DeltaTime;
			}

			movementCounter.Y += moveV;
			int num = (int)Math.Round(movementCounter.Y);
			if (num != 0) {
				movementCounter.Y -= num;
				return MoveVExactCollideSolids(num, thruDashBlocks, onCollide);
			}
			return false;
		}

		new public bool MoveHExactCollideSolids(int moveH, bool thruDashBlocks, Action<Vector2, Vector2, Platform> onCollide = null) {
			float x = X;
			int moveStep = Math.Sign(moveH);
			int moveDistance = 0;
			Solid solid = null;
			Collider orig_collider = Collider;
			if (orig_collider != null) {
				Collider new_collider = new Hitbox(orig_collider.Width, orig_collider.Height - 2, orig_collider.Left, orig_collider.Top + 1);
				Collider = new_collider;
			}
			while (moveH != 0) {
				if (thruDashBlocks) {
					foreach (DashBlock entity in Scene.Tracker.GetEntities<DashBlock>()) {
						if (CollideCheck(entity, Position + Vector2.UnitX * moveStep)) {
							entity.Break(Center, Vector2.UnitX * moveStep, true, true);
							SceneAs<Level>().Shake(0.2f);
							Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
						}
					}
				}

				var p = CollideAll<Solid>(Position + Vector2.UnitX * moveStep);
				solid = p.FirstOrDefault(e => !(e is HappyFunDudeSolid)) as Solid;
				if (solid != null) {
					break;
				}
				moveDistance += moveStep;
				moveH -= moveStep;
				X += moveStep;
			}
			X = x;
			Collider = orig_collider;
			MoveHExact(moveDistance);
			if (solid != null) {
				onCollide?.Invoke(Vector2.UnitX * moveStep, Vector2.UnitX * moveDistance, solid);
			}
			return solid != null;
		}

		new public bool MoveVExactCollideSolids(int moveV, bool thruDashBlocks, Action<Vector2, Vector2, Platform> onCollide = null) {
			float y = Y;
			int num = Math.Sign(moveV);
			int num2 = 0;
			Platform platform = null;
			Collider orig_collider = Collider;
			if (orig_collider != null) {
				Collider new_collider = new Hitbox(orig_collider.Width - 2, orig_collider.Height, orig_collider.Left + 1, orig_collider.Top);
				Collider = new_collider;
			}
			while (moveV != 0) {
				if (thruDashBlocks) {
					foreach (DashBlock entity in Scene.Tracker.GetEntities<DashBlock>()) {
						if (CollideCheck(entity, Position + Vector2.UnitY * num)) {
							entity.Break(Center, Vector2.UnitY * num, true, true);
							SceneAs<Level>().Shake(0.2f);
							Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
						}
					}
				}
				var p = CollideAll<Solid>(Position + Vector2.UnitY * num);
				platform = p.FirstOrDefault(e => !(e is HappyFunDudeSolid)) as Solid;
				if (platform != null) {
					break;
				}
				num2 += num;
				moveV -= num;
				Y += num;
			}
			Y = y;
			Collider = orig_collider;
			MoveVExact(num2);
			if (platform != null) {
				onCollide?.Invoke(Vector2.UnitY * num, Vector2.UnitY * num2, platform);
			}
			return platform != null;
		}

	}
}
