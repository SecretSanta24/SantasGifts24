using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SantasGifts24.Code.States
{
    internal class RocketFly
    {
        public class RocketRenderer : Entity{

            public Sprite sprite;
            private Player player;

            public RocketRenderer(Player player) : base(player.Position)
            {
                this.player = player;
                base.Add(sprite = GFX.SpriteBank.Create("madelineRocket"));
                sprite.Play("stopped");
                sprite.CenterOrigin();
                sprite.Position -= new Vector2(0, player.Height) / 2;
            }

            public override void Update()
            {
                base.Update();
                if (player != null) this.Position = player.Position;
            }

        }

        public static int StateNumber;
        private static Player player;

        public static float maxSpeed = 250;
        public static float timeToMaxSpeed = 0.25f;
        public static float timeToStop = 0.25f;

        public static float slowDown = 1000;
        public static float speedUp = 1000;
        private static RocketRenderer rocket;
        public static int Update()
        {
            if (player == null) return Player.StNormal;

            Vector2 dir = player.CorrectDashPrecision(player.lastAim);
            if(Input.Aim.Value == Vector2.Zero)
            {
                player.Speed.X = Calc.Approach(player.Speed.X, 0f, slowDown * Engine.DeltaTime);
                player.Speed.Y = Calc.Approach(player.Speed.Y, 0f, slowDown * Engine.DeltaTime);
            } else 
            {
                Vector2 target = new Vector2(maxSpeed, maxSpeed) * dir;
                player.Speed.X = Calc.Approach(player.Speed.X, target.X, speedUp * Engine.DeltaTime);
                player.Speed.Y = Calc.Approach(player.Speed.Y, target.Y, speedUp * Engine.DeltaTime);
            }

            if (player.Speed == Vector2.Zero) rocket.sprite.Play("stopped");
            else rocket.sprite.Play("loop");
            if(Input.Aim.Value != Vector2.Zero) rocket.sprite.Rotation = dir.Angle() + (float)Math.PI/2;

            return StateNumber;
        }

        public static IEnumerator Coroutine()
        {
            yield break;
        }

        public static void Begin()
        {
            Level level = (Engine.Scene as Level);
            player = level?.Tracker?.GetEntity<Player>();
            if (player == null) return;
            player.Sprite.Visible = false;
            player.Hair.Visible = false;

            slowDown = maxSpeed / timeToStop;
            speedUp = maxSpeed / timeToMaxSpeed;
            level.Add(rocket = new RocketRenderer(player));
        }

        public static void End()
        {
            player.Sprite.Visible = true;
            player.Hair.Visible = true;

            rocket?.RemoveSelf();
        }
    }
}
