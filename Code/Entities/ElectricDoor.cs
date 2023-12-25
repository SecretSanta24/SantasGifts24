using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.Helpers;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using MonoMod;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    [CustomEntity(new string[] { "SS2024/ElectricDoor" })]
    [Tracked]
    class ElectricDoor : Solid
    {
        private Sprite sprite;
        private Sprite spriteBG;
        private bool right;

        public ElectricDoor(EntityData data, Vector2 offset)
            : base(data.Position + offset, 4, 48, true)
        {
            Depth = 100;
            Collider.Position.Y -= 24;

            Add(spriteBG = GFX.SpriteBank.Create("electricDoorBG"));

            right = data.Bool("right");

        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Entity holder = new Entity(Position);
            holder.Depth = -10000;
            holder.Add(sprite = GFX.SpriteBank.Create("electricDoorFG"));

            Scene.Add(holder);

            if (isElectricityOff())
            {
                Collidable = false;
                sprite.Play("open");
                spriteBG.Play("open");
            }
            else
            {
                Collidable = true;
                sprite.Play("close");
                spriteBG.Play("close");
            }

            if (right)
            {
                sprite.FlipX = true;
                spriteBG.FlipX = true;
                Collider.Position.X -= 1;
            }
            else
            {
                sprite.FlipX = false;
                spriteBG.FlipX = false;
                Collider.Position.X -= 3;
            }
        }

        public override void Update()
        {
            base.Update();
            if (isElectricityOff())
            {
                Collidable = false;
                sprite.Play("open");
                spriteBG.Play("open");
            }
            else
            {
                Collidable = true;
                sprite.Play("close");
                spriteBG.Play("close");
            }
        }

        private bool isElectricityOff()
        {
            Level level = SceneAs<Level>();
            if (level == null)
            {
                return false;
            }
            string flagName = "SS2024_level_electricity_flag";
            return level.Session.GetFlag(flagName);
        }
    }
}
