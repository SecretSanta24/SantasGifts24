using Celeste.Mod.Entities;
using Celeste.Mod.SantasGifts24.Code.Helper;
using Celeste.Mod.SantasGifts24.Code.Mechanics;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Entities.LightDark {
	[CustomEntity("SS2024/GunnerDude")]
	public class GunnerDude : HappyFunDudeSolid {
		private bool facingLeft;
		private float cooldown = 0f;

		public GunnerDude(EntityData data, Vector2 offset) : base(data.Position + offset - new Vector2(8, 12), 16, 28) {

			NormalSprite = GFX.SpriteBank.Create("corkr900SS24GunnerDudeNormalFront");
			DarkSprite = GFX.SpriteBank.Create("corkr900SS24GunnerDudeDarkFront");
			NormalSpriteSecondary = GFX.SpriteBank.Create("corkr900SS24GunnerDudeNormalBack");
			DarkSpriteSecondary = GFX.SpriteBank.Create("corkr900SS24GunnerDudeDarkBack");

			NormalSprite.Position = new Vector2(8f, 12f);
			DarkSprite.Position = new Vector2(8f, 12f);
			NormalSpriteSecondary.Position = new Vector2(8f, 12f);
			DarkSpriteSecondary.Position = new Vector2(8f, 12f);

			facingLeft = data.Bool("faceLeft", false);
			if (facingLeft) {
				SetFlipX(true);
			}
			OnDashCollide = OnDashed;
		}

		public override void Update() {
			base.Update();

			if (cooldown > 0) {
				cooldown -= Engine.DeltaTime;
			}
			if (CurrentMode == LightDarkMode.Dark && cooldown <= 0 && CheckLineOfSight()) {
				Fire();
			}
		}

		private DashCollisionResults OnDashed(Player player, Vector2 direction) {
			if (CurrentMode == LightDarkMode.Normal) {
				Fire();
				player.RefillDash();
				return direction.Equals(Vector2.UnitY) ? DashCollisionResults.NormalCollision : DashCollisionResults.Rebound;
			}
			else {
				return DashCollisionResults.NormalCollision;
			}
		}

		public void Fire() {
			if (cooldown > 0) return;
			PlayAnimation("shoot");
			cooldown = 1.0f;
			Scene.Add(new LightDarkProjectile(Position + new Vector2(facingLeft ? -10 : 10, 4), facingLeft));
		}

		public bool CheckLineOfSight() {
			if (Scene?.Tracker.GetEntity<Player>() is not Player player) return false;
			if (facingLeft == (player.CenterX > CenterX)) return false;
			float detectW = Math.Abs(CenterX - player.CenterX);
			if (player.Bottom < Top + 4 || player.Top > Top + 20) return false;
			float minY = Calc.Max(player.Top, Top);
			float maxY = Calc.Min(player.Bottom, Bottom);
			float endX = facingLeft ? CenterX - detectW : CenterX + detectW;
			for (int y = (int)Math.Round(minY); y <= (int)Math.Round(maxY); y++) {
				Vector2 start = new Vector2(CenterX, y);
				Vector2 end = new Vector2(endX, y);
				if (FancyCollide.DoRaycast<Solid, GunnerDude>(Scene, start, end) == null) return true;
			}
			return false;
		}

	}
}
