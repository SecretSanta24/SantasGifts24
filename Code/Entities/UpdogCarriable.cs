using System;
using System.Collections;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using static Celeste.GaussianBlur;
using static Celeste.Mod.SantasGifts24.SantasGiftsModule;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    [CustomEntity("SS2024/UpdogCarriable")]
    [TrackedAs(typeof(TheoCrystal))]
    public class UpdogCarriable : TheoCrystal
    {
        public float springTimer;

        public bool dontspawnflag;

        public bool fg;

        public UpdogCarriable(Vector2 position)
            : base(position)
        {
        }

        public UpdogCarriable(EntityData e, Vector2 offset)
            : this(e.Position + offset)
        {
            springTimer = 0f;
            dontspawnflag = e.Bool("dontspawnflag", false);
            fg = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (dontspawnflag && !(scene as Level).Session.GetFlag("SS2024_Sunsetquasar_updog"))
            {
                RemoveSelf();
            }
        }

        public override void Update()
        {
            base.Update();
            springTimer = Math.Max(springTimer - Engine.DeltaTime, 0f);
            if (fg)
            {
                (Scene as Level).OnEndOfFrame += delegate
                {
                    Depth = -20000;
                };
            }

            DeathEffect death = Components.Get<DeathEffect>();

            if (death != null)
            {
                death.Color = Calc.HexToColor("B864BE");
            }
        }


        public static void Load()
        {
            IL.Celeste.Player.Throw += PlayerThrowNoKB;
            On.Celeste.TheoCrystal.OnRelease += onReleaseHook;
            On.Celeste.TheoCrystal.OnCollideH += onCollideHook;
            On.Celeste.TheoCrystal.OnCollideV += onCollideVook;
            On.Celeste.TheoCrystal.HitSpring += hitSpringHook;
        }

        public static void Unload()
        {
            IL.Celeste.Player.Throw -= PlayerThrowNoKB;
            On.Celeste.TheoCrystal.OnRelease -= onReleaseHook;
            On.Celeste.TheoCrystal.OnCollideH -= onCollideHook;
            On.Celeste.TheoCrystal.OnCollideV -= onCollideVook;
            On.Celeste.TheoCrystal.HitSpring -= hitSpringHook;
        }
        public static bool hitSpringHook(On.Celeste.TheoCrystal.orig_HitSpring orig, TheoCrystal self, Spring spring)
        {
            UpdogCarriable c = self as UpdogCarriable;
            if (c != null)
            {
                if (!self.Hold.IsHeld && c.springTimer <= 0)
                {
                    if (spring.Orientation == Spring.Orientations.Floor && self.Speed.Y >= 0f)
                    {
                        self.Speed.X *= 0.5f;
                        self.Speed.Y = -160f;
                        self.noGravityTimer = 0.15f;
                        c.springTimer = 0.1f;
                        return true;
                    }
                    if (spring.Orientation == Spring.Orientations.WallLeft)
                    {
                        self.MoveTowardsY(spring.CenterY + 5f, 4f);
                        self.Speed.X = 220f;
                        self.Speed.Y = -80f;
                        self.noGravityTimer = 0.1f;
                        c.springTimer = 0.1f;
                        return true;
                    }
                    if (spring.Orientation == Spring.Orientations.WallRight)
                    {
                        self.MoveTowardsY(spring.CenterY + 5f, 4f);
                        self.Speed.X = -220f;
                        self.Speed.Y = -80f;
                        self.noGravityTimer = 0.1f;
                        c.springTimer = 0.1f;
                        return true;
                    }
                }
                return false;
            }
            else return orig(self, spring);
        }

        public static void onCollideHook(On.Celeste.TheoCrystal.orig_OnCollideH orig, TheoCrystal self, CollisionData data)
        {
            UpdogCarriable c = self as UpdogCarriable;
            if (c != null)
            {
                if (data.Hit is CrushBlock)
                {
                    if ((data.Hit as CrushBlock).CanActivate(-data.Direction))
                    {
                        if (Math.Abs(self.Speed.X) >= 120) (data.Hit as CrushBlock).Attack(-data.Direction);
                    }
                }

                if (data.Hit is DashBlock)
                {
                    if (Math.Abs(self.Speed.X) >= 120) (data.Hit as DashBlock).Break(self.Center, data.Direction, true, true);
                }

                if (data.Hit is DashSwitch)
                {
                    (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(self.Speed.X));
                }
                Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", self.Position);
                if (Math.Abs(self.Speed.X) > 100f)
                {
                    self.ImpactParticles(data.Direction);
                }
                self.Speed.X *= -0.4f;
            }
            else orig(self, data);
        }
        public static void onCollideVook(On.Celeste.TheoCrystal.orig_OnCollideV orig, TheoCrystal self, CollisionData data)
        {
            UpdogCarriable c = self as UpdogCarriable;
            if (c != null)
            {

                if (data.Hit is CrushBlock)
                {
                    if ((data.Hit as CrushBlock).CanActivate(-data.Direction))
                    {
                        if (Math.Abs(self.Speed.Y) >= 120) (data.Hit as CrushBlock).Attack(-data.Direction);
                    }
                }

                if (data.Hit is DashBlock)
                {
                    if (Math.Abs(self.Speed.Y) >= 120) (data.Hit as DashBlock).Break(self.Center, data.Direction, true, true);
                }

                if (data.Hit is DashSwitch)
                {
                    (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(self.Speed.Y));
                }
                if (self.Speed.Y > 0f)
                {
                    if (self.hardVerticalHitSoundCooldown <= 0f)
                    {
                        Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", self.Position, "crystal_velocity", Calc.ClampedMap(self.Speed.Y, 0f, 200f));
                        self.hardVerticalHitSoundCooldown = 0.5f;
                    }
                    else
                    {
                        Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", self.Position, "crystal_velocity", 0f);
                    }
                }
                if (self.Speed.Y > 160f)
                {
                    self.ImpactParticles(data.Direction);
                }
                if (self.Speed.Y > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch))
                {
                    self.Speed.Y *= -0.6f;
                }
                else
                {
                    self.Speed.Y = 0f;
                }
            }
            else orig(self, data);
        }

        public static void onReleaseHook(On.Celeste.TheoCrystal.orig_OnRelease orig, TheoCrystal self, Vector2 force)
        {
            UpdogCarriable c = self as UpdogCarriable;
            if (c != null)
            {
                Player player = self.Scene.Tracker.GetEntity<Player>();
                self.RemoveTag(Tags.Persistent);
                if (player != null)
                {
                    if (force.X != 0f && force.Y == 0f)
                    {
                        force.Y = -0.4f;
                        if (Input.Aim.Value.Y < 0f)
                        {
                            force.Y = -1f;
                            force.X = player.Speed.X / 200f;

                        }
                    }
                }

                self.Speed = force * 200f;
                //Logger.Log(LogLevel.Info, "SantasGifts24", "theo y force: " + force.ToString());
                if (self.Speed != Vector2.Zero)
                {
                    self.noGravityTimer = 0.1f;
                }
            }
            else orig(self, force);
        }

        private static void PlayerThrowNoKB(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdindR4()))
            {
                while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(80f)))
                {
                    //Logger.Log(LogLevel.Debug, "SS2024/UpdogCarriable", $"doing the things at {cursor.Index} in CIL code for {cursor.Method.FullName}");

                    cursor.Emit(OpCodes.Ldarg_0);

                    cursor.EmitDelegate<Func<Player, float>>(upthrowcheck);
                    cursor.Emit(OpCodes.Mul);
                }

            }
        }
        private static float upthrowcheck(Player player)
        {
            if (player != null)
            {
                if(player.Holding != null)
                {
                    if(player.Holding.Entity is UpdogCarriable && Input.Aim.Value.Y < 0f)
                    {
                        return 0f;
                    }
                }
            }
            return 1f;
        }
    }

}
