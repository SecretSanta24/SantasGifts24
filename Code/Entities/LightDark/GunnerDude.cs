using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Entities.LightDark {
	[CustomEntity("SS2024/GunnerDude")]
	public class GunnerDude : HappyFunDudeSolid {
		private bool facingLeft = false;
		private float shootInterval = 2.0f;
		
		public GunnerDude(EntityData data, Vector2 offset) : base(data.Position + offset - new Vector2(8, 12), 16, 28) {
			NormalSprite = GFX.SpriteBank.Create("corkr900SS24GunnerDudeNormal");
			DarkSprite = GFX.SpriteBank.Create("corkr900SS24GunnerDudeDark");
			NormalSprite.Position = new Vector2(8f, 12f);
			DarkSprite.Position = new Vector2(8f, 12f);
			OnDashCollide = OnDashed;
		}

		public override void Update() {
			base.Update();

			if (CurrentMode == Mechanics.LightDarkMode.Dark && (Scene?.OnInterval(shootInterval) ?? false)) {
				Fire();
			}
		}

		private DashCollisionResults OnDashed(Player player, Vector2 direction) {
			if (CurrentMode == Mechanics.LightDarkMode.Normal) {
				Fire();
			}
			player.RefillDash();
			return direction.Equals(Vector2.UnitY) ? DashCollisionResults.NormalCollision : DashCollisionResults.Rebound;
		}

		public void Fire() {
			Scene.Add(new LightDarkProjectile(Position + new Vector2(facingLeft ? -10 : 10, 4), facingLeft));
		}
	}
}
