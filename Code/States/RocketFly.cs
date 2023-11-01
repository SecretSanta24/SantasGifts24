using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SantasGifts24.Code.States
{
    internal class RocketFly
    {
        public class RocketRenderer : Entity{

            private Sprite sprite;
            private Sprite spriteDiag;
            private Player player;
            private static PlayerDeadBody pdb;

            public RocketRenderer(Player player) : base(player.Position)
            {
                this.player = player;
                RocketRenderer.pdb = null;
                base.Add(sprite = GFX.SpriteBank.Create("madelineRocket"));
                sprite.Play("stopped");
                sprite.CenterOrigin();
                sprite.Position -= new Vector2(0, player.Height) / 2;
                base.Add(spriteDiag = GFX.SpriteBank.Create("madelineRocketDiag"));
                spriteDiag.Play("stopped");
                spriteDiag.CenterOrigin();
                spriteDiag.Position -= new Vector2(0, player.Height) / 2;

                spriteDiag.Visible = false;
            }

            public static void Load()
            {
                On.Celeste.Player.Die += Player_Die;
            }

            public static void Unload()
            {
                On.Celeste.Player.Die -= Player_Die;
            }
            private static PlayerDeadBody Player_Die(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
            {
                PlayerDeadBody pdb = orig(self, direction, evenIfInvincible, registerDeathInStats);

                if(pdb != null)
                {
                    RocketRenderer.pdb = pdb;
                }

                return pdb;
            }


            public override void Update()
            {
                base.Update();
                if (player != null) this.Position = player.Position;
                if (pdb != null )
                {
                    this.sprite.Visible = false;
                    this.spriteDiag.Visible = true;
                    Play("stopped");

                    if (pdb.deathEffect != null) RemoveSelf();
                    this.Position = pdb.Position;
                    this.spriteDiag.Rotation += Calc.DegToRad * 33;
                }
            }

            private void Play(string id)
            {
                sprite.Play(id);
                spriteDiag.Play(id);
            }

            public void SetDIR(Vector2 dir)
            {

                if (dir == Vector2.Zero)
                {
                    if (!sprite.CurrentAnimationID.Contains("stop")) Play("stop");
                    return;
                } 
                else if (sprite.CurrentAnimationID.Contains("stop")) Play("start");

                // diagonal check
                if(dir.X != 0 && dir.Y != 0)
                {
                    spriteDiag.Visible = true;
                    sprite.Visible = false;
                } else
                {
                    spriteDiag.Visible = false;
                    sprite.Visible = true;
                }

                sprite.Rotation = dir.Angle() + (float)Math.PI / 2;
                spriteDiag.Rotation = dir.Angle() + (float)Math.PI / 2 - 45f.ToRad();

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

            rocket.SetDIR(Input.Aim.Value);

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
