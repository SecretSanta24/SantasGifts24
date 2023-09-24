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
	public class LightDarkProjectile : HappyFunDudeSolid {
		private static readonly float FlySpeed = 100f;
		private static readonly float FallAcceleration = 400f;

		private bool booped = false;
		private float fallSpeed = 0f;
		private bool goLeft = false;
		private Circle explodeEffectZone;
		private PlayerCollider playerColliderTop;
		private PlayerCollider playerColliderLeft;
		private PlayerCollider playerColliderRight;

		public LightDarkProjectile(Vector2 pos, bool left) : base(pos, 20, 16) {
			goLeft = left;
			NormalSprite = GFX.SpriteBank.Create("corkr900SS24LightDarkProjectileNormal");
			DarkSprite = GFX.SpriteBank.Create("corkr900SS24LightDarkProjectileDark");
			NormalSprite.Position = new Vector2(8f, 8f);
			DarkSprite.Position = new Vector2(8f, 8f);
			if (goLeft) {
				NormalSprite.FlipX = true;
				DarkSprite.FlipX = true;
			}
			explodeEffectZone = new Circle(40f);
			Hitbox topCollider = new Hitbox(Width, 3, 0, -3);
			Hitbox leftCollider = new Hitbox(3, Height - 2, -3, 1);
			Hitbox rightCollider = new Hitbox(3, Height - 2, 20, 1);
			playerColliderTop = new PlayerCollider(OnPlayerTop, topCollider);
			playerColliderLeft = new PlayerCollider(p => OnPlayerSide(p, true), leftCollider);
			playerColliderRight = new PlayerCollider(p => OnPlayerSide(p, false), rightCollider);
			Add(playerColliderTop);
			Add(playerColliderLeft);
			Add(playerColliderRight);
		}

		public override void Update() {
			base.Update();
			if (Scene is not Level level) return;

			float flySpeed = goLeft ? -FlySpeed : FlySpeed;
			if (booped) {
				fallSpeed += FallAcceleration * Engine.DeltaTime;
				MoveHCollideSolids(flySpeed * Engine.DeltaTime, true, OnFallCollide);
				MoveVCollideSolids(fallSpeed * Engine.DeltaTime, true, OnFallCollide);
				if (!level.IsInBounds(this)) {
					RemoveSelf();
				}
			}
			else {
				MoveHCollideSolids(flySpeed * Engine.DeltaTime, true, OnFlyCollide);
				if (!level.IsInBounds(this)) {
					RemoveSelf();
				}
			}
			foreach (Spring spring in level.Entities.FindAll<Spring>().Cast<Spring>()) {
				if (!CollideCheck(spring)) continue;
				booped = true;
				DynamicData dd = DynamicData.For(spring);
				dd.Invoke("BounceAnimate");
				switch (spring.Orientation) {
					case Spring.Orientations.Floor:
						fallSpeed = -160f;
						break;
					case Spring.Orientations.WallLeft:
						goLeft = false;
						NormalSprite.FlipX = false;
						DarkSprite.FlipX = false;
						fallSpeed = -72;
						break;
					case Spring.Orientations.WallRight:
						goLeft = true;
						NormalSprite.FlipX = true;
						DarkSprite.FlipX = true;
						fallSpeed = -72;
						break;

				}
			}
		}

		private void OnFlyCollide(Vector2 arg1, Vector2 arg2, Platform arg3) {
			Explode();
		}

		private void OnFallCollide(Vector2 arg1, Vector2 arg2, Platform arg3) {
			Explode();
		}

		private void OnPlayerTop(Player player) {
			if (booped || CurrentMode == LightDarkMode.Normal) return;
			Audio.Play("event:/game/general/thing_booped", Position);
			booped = true;
			player.Bounce(Position.Y);
		}

		private void OnPlayerSide(Player player, bool isLeft) {
			if (booped) return;
			if (isLeft == goLeft) {
				Explode();
			}
			else {
				player.Die(isLeft ? Vector2.UnitX : -Vector2.UnitX);
			}
		}

		private void Explode() {
			Audio.Play("event:/new_content/game/10_farewell/puffer_splode", Position)
				.setVolume(0.5f);

			Entity explodeFXEntity = new Entity();
			explodeFXEntity.Position = Position;
			Scene.Add(explodeFXEntity);
			Sprite sprite = GFX.SpriteBank.Create("seekerShockWave");
			explodeFXEntity.Add(sprite);
			sprite.OnLastFrame = delegate {
				explodeFXEntity.RemoveSelf();
			};
			sprite.Play("shockwave", restart: true);
			sprite.SetAnimationFrame(3);
			SceneAs<Level>().Shake(0.15f);

			Collider = explodeEffectZone;
			Player player = CollideFirst<Player>();
			if (player != null && !player.Dead) {
				player.ExplodeLaunch(Position, false, false);
				DynamicData dd = DynamicData.For(player);
				dd.Set("dashCooldownTimer", 0.02f);
			}
			Collider = null;

			RemoveSelf();
		}
	}
}
