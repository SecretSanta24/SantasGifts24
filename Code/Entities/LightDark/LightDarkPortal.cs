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

namespace Celeste.Mod.SantasGifts24.Code.Entities.LightDark {
    [CustomEntity("SS2024/LightDarkPortal")]
    public class LightDarkPortal : Entity
    {
        private const float radius = 10f;

        private Vector2 nodePos;
        private Circle primaryCollider;
        private Circle secondaryCollider;
        private bool cooldownPrimary = false;
        private bool cooldownSecondary = false;
        private Sprite spritePrimary;
        private Sprite spriteSecondary;

        private LightDarkMode currentMode;

        public bool CanUsePrimary {
            get {
                return SceneAs<Level>()?.LightDarkGet() == LightDarkMode.Normal;
            }
		}
		public bool CanUseSecondary {
			get {
				return SceneAs<Level>()?.LightDarkGet() == LightDarkMode.Dark;
			}
		}

		public LightDarkPortal(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = Depths.Player + 5;
            nodePos = data.Nodes.Length > 0 ? data.Nodes[0] + offset : Position;
            Vector2 secondaryPos = nodePos - Position;
            primaryCollider = new Circle(radius);
            secondaryCollider = new Circle(radius, secondaryPos.X, secondaryPos.Y);
            Add(new PlayerCollider(OnPlayerPrimary, primaryCollider));
            Add(new PlayerCollider(OnPlayerSecondary, secondaryCollider));
            Add(spritePrimary = GFX.SpriteBank.Create("corkr900SS24LightDarkPortalNormal"));
			Add(spriteSecondary = GFX.SpriteBank.Create("corkr900SS24LightDarkPortalDark"));
			spriteSecondary.Position = secondaryPos;
            Add(new LightDarkListener(OnModeChange));
		}

		public override void Added(Scene scene) {
			base.Added(scene);
            OnModeChange((scene as Level)?.LightDarkGet() ?? LightDarkMode.Normal);
		}

		public override void Update()
        {
            base.Update();

            if (cooldownPrimary)
            {
                Collider = primaryCollider;
                if (!CollideCheck<Player>()) cooldownPrimary = false;
            }
            if (cooldownSecondary)
            {
                Collider = secondaryCollider;
                if (!CollideCheck<Player>()) cooldownSecondary = false;
            }
        }

        private void OnPlayerPrimary(Player player)
        {
            if (cooldownPrimary || currentMode == LightDarkMode.Dark) return;
            OnPlayer(player, secondaryCollider, LightDarkMode.Dark);
        }

        private void OnPlayerSecondary(Player player)
        {
            if (cooldownSecondary || currentMode == LightDarkMode.Normal) return;
            OnPlayer(player, primaryCollider, LightDarkMode.Normal);
        }

        private void OnPlayer(Player player, Circle to, LightDarkMode mode)
        {
            Audio.Play("event:/game/06_reflection/badeline_freakout_1", player.Position);
            cooldownPrimary = true;
            cooldownSecondary = true;
            player.Position = to.Center + Position;
            if (player.CollideCheck<Solid>())
            {
                player.Die(Vector2.Zero);
            }
            if (Scene is Level level)
            {
                level.LightDarkSet(mode);
            }
        }

		private void OnModeChange(LightDarkMode ldm) {
            currentMode = ldm;
            if (ldm == LightDarkMode.Normal) {
                spritePrimary.Play("reenable");
				spriteSecondary.Play("disable");
				// TMP
				spritePrimary.SetColor(Color.White);
				spriteSecondary.SetColor(Color.White * 0.3f);
			}
            else {
				spritePrimary.Play("disable");
				spriteSecondary.Play("reenable");
                // TMP
                spritePrimary.SetColor(Color.White * 0.3f);
				spriteSecondary.SetColor(Color.White);
			}
		}

	}
}
