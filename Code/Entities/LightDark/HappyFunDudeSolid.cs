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
		private Sprite _normalSpriteSecondary;
		private Sprite _darkSpriteSecondary;
		private Entity _secondarySpritesEntity;

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

		protected Sprite NormalSpriteSecondary {
			get { return _normalSpriteSecondary; }
			set {
				_normalSpriteSecondary = value;
				UpdateSprite();
			}
		}

		protected Sprite DarkSpriteSecondary {
			get { return _darkSpriteSecondary; }
			set {
				_darkSpriteSecondary = value;
				UpdateSprite();
			}
		}

		private Sprite currentSprite;
		private Sprite currentSpriteSecondary;

		public bool SpriteVisible {
			get {
				return currentSprite?.Visible ?? false;
			}
			set {
				NormalSprite.Visible = value;
				DarkSprite.Visible = value;
				if (NormalSpriteSecondary != null) {
					NormalSpriteSecondary.Visible = value;
					DarkSpriteSecondary.Visible = value;
				}
			}
		}

		public void PlayAnimation(string anim) {
			NormalSprite.Play(anim);
			DarkSprite.Play(anim);
			NormalSpriteSecondary?.Play(anim);
			DarkSpriteSecondary?.Play(anim);
		}

		public void SetFlipX(bool v) {
			NormalSprite.FlipX = true;
			DarkSprite.FlipX = true;
			if (NormalSpriteSecondary != null) {
				NormalSpriteSecondary.FlipX = true;
				DarkSpriteSecondary.FlipX = true;
			}
		}

		public HappyFunDudeSolid(Vector2 pos, int width, int height)
			: base(pos, width, height, false)
		{
			Add(new LightDarkListener(SetDarknessMode));
			Depth = Depths.Solids;
		}

		public HappyFunDudeSolid(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height)
		{ }

		public override void Added(Scene scene) {
			base.Added(scene);
			if (scene is Level level) {
				SetDarknessMode(level.LightDarkGet());
			}
			CheckSecondarySpritesEntity(scene);
		}

		public override void Removed(Scene scene) {
			base.Removed(scene);
			_secondarySpritesEntity?.RemoveSelf();
		}

		public virtual void SetDarknessMode(LightDarkMode newMode) {
			if (newMode == CurrentMode) return;
			CurrentMode = newMode;
			UpdateSprite();
		}

		private void CheckSecondarySpritesEntity(Scene scene = null) {
			scene ??= Scene;
			if (_secondarySpritesEntity == null) {
				_secondarySpritesEntity = new Entity();
				_secondarySpritesEntity.Depth = Depths.Below;
				_secondarySpritesEntity.Position = Position;
			}
			if (scene != null && _secondarySpritesEntity.Scene == null) {
				scene.Add(_secondarySpritesEntity);
			}
		}

		private void UpdateSprite() {
			string anim = "";
			int frame = 0;
			string anim2 = "";
			int frame2 = 0;
			// Get current position in animations
			if (currentSprite != null) {
				anim = currentSprite.CurrentAnimationID;
				frame = currentSprite.CurrentAnimationFrame;
				currentSprite.RemoveSelf();
			}
			if (currentSpriteSecondary != null) {
				anim2 = currentSpriteSecondary.CurrentAnimationID;
				frame2 = currentSpriteSecondary.CurrentAnimationFrame;
				currentSpriteSecondary.RemoveSelf();
			}
			// Swap in the sprites
			if (CurrentMode == LightDarkMode.Normal && NormalSprite != null) {
				Add(currentSprite = NormalSprite);
				if (NormalSpriteSecondary != null) {
					CheckSecondarySpritesEntity();
					_secondarySpritesEntity.Add(currentSpriteSecondary = NormalSpriteSecondary);
				}
			}
			if (CurrentMode == LightDarkMode.Dark && DarkSprite != null) {
				Add(currentSprite = DarkSprite);
				if (DarkSpriteSecondary != null) {
					CheckSecondarySpritesEntity();
					_secondarySpritesEntity.Add(currentSpriteSecondary = DarkSpriteSecondary);
				}
			}
			// Play the same animation on the new sprite
			if (!string.IsNullOrEmpty(anim)) {
				currentSprite?.Play(anim);
				currentSprite?.SetAnimationFrame(frame);
			}
			if (!string.IsNullOrEmpty(anim2)) {
				currentSpriteSecondary?.Play(anim2);
				currentSpriteSecondary?.SetAnimationFrame(frame2);
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
				bool ret = MoveHExactCollideSolids(num, thruDashBlocks, onCollide);
				if (_secondarySpritesEntity != null) _secondarySpritesEntity.Position = Position;
				return ret;
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
				bool ret = MoveVExactCollideSolids(num, thruDashBlocks, onCollide);
				if (_secondarySpritesEntity != null) _secondarySpritesEntity.Position = Position;
				return ret;
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
