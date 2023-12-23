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

    public class HappyFunDude : Entity
    {
        private Sprite _normalSprite;
        private Sprite _darkSprite;

        public LightDarkMode CurrentMode { get; private set; } = LightDarkMode.Normal;

        protected Sprite NormalSprite
        {
            get { return _normalSprite; }
            set
            {
                _normalSprite = value;
                UpdateSprite();
            }
        }

        protected Sprite DarkSprite
        {
            get { return _darkSprite; }
            set
            {
                _darkSprite = value;
                UpdateSprite();
            }
        }

        private Sprite currentSprite;

        public bool SpriteVisible
        {
            get
            {
                return currentSprite?.Visible ?? false;
            }
            set
            {
                NormalSprite.Visible = value;
                DarkSprite.Visible = value;
            }
        }

        public HappyFunDude(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(new LightDarkListener(SetDarknessMode));
        }

		public void PlayAnimation(string anim) {
			NormalSprite.Play(anim);
			DarkSprite.Play(anim);
		}

		public override void Added(Scene scene)
        {
            base.Added(scene);
            if (scene is Level level)
            {
                SetDarknessMode(level.LightDarkGet());
            }
        }

        public virtual void SetDarknessMode(LightDarkMode newMode)
        {
            if (newMode == CurrentMode) return;
            CurrentMode = newMode;
            UpdateSprite();
        }

        private void UpdateSprite()
        {
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

    }
}
