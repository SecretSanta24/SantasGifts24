using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;
using System.Reflection;
using Celeste.Mod.Entities;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    [Tracked (false)]
    public class WaterDroplet : Entity
    {
        private Image droplet;
        private Sprite splashSprite;
        private float speed;
        private float maxSpeed = 3f;
        private bool stopFalling;

        public WaterDroplet(Vector2 position, float initialSpeed, Color color)
            : base(position)
        {
            speed = initialSpeed;
            Add(droplet = new Image(GFX.Game["objects/ss2024/waterSplash/droplet00"]));
            droplet.CenterOrigin();
            droplet.Color = color;
            droplet.FlipX = Calc.Random.NextDouble() > 0.5f;

            Add(splashSprite = GFX.SpriteBank.Create("waterSplash"));
            splashSprite.Color = color * 0.5f;
            Collider = new Hitbox(4, 4, -2f, -2f);

            
        }

        public override void Update()
        {
            base.Update();

            if(stopFalling)
            {
                return;
            }

            speed = Calc.Approach(speed, maxSpeed, 10f * Engine.DeltaTime);
            Position.Y += speed;
            
            foreach (WaterLightning e in Scene.Tracker.GetEntities<WaterLightning>())
            {
                if (e.CollideCheck(this))
                {
                    Position.Y = e.Top - 1;
                    Add(new Coroutine(SplashRoutine()));
                }
            }
        }

        private IEnumerator SplashRoutine()
        {
            droplet.Visible = false;
            splashSprite.Play("splash");
            stopFalling = true;
            yield return 1f;
            RemoveSelf();
        }
    }
}
