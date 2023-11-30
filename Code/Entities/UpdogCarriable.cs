﻿using System;
using System.Collections;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    [CustomEntity("SS2024/UpdogCarriable")]
    [Tracked(false)]
    public class UpdogCarriable : Actor
    {
        public static ParticleType P_Impact = new ParticleType(TheoCrystal.P_Impact);

        public Vector2 Speed;

        public bool OnPedestal;

        public Holdable Hold;

        private Sprite sprite;

        private bool dead;

        private Level Level;

        private Collision onCollideH;

        private Collision onCollideV;

        private float noGravityTimer;

        private Vector2 prevLiftSpeed;

        private Vector2 previousPosition;

        private HoldableCollider hitSeeker;

        private float swatTimer;

        private bool shattering;

        private float hardVerticalHitSoundCooldown;

        private BirdTutorialGui tutorialGui;

        private float tutorialTimer;

        public UpdogCarriable(Vector2 position)
            : base(position)
        {
            previousPosition = position;
            base.Depth = 100;
            base.Collider = new Hitbox(8f, 10f, -4f, -10f);
            Add(sprite = GFX.SpriteBank.Create("theo_crystal"));
            sprite.Scale.X = -1f;
            Add(Hold = new Holdable());
            Hold.PickupCollider = new Hitbox(16f, 22f, -8f, -16f);
            Hold.SlowFall = false;
            Hold.SlowRun = true;
            Hold.OnPickup = OnPickup;
            Hold.OnRelease = OnRelease;
            Hold.DangerousCheck = Dangerous;
            Hold.OnHitSeeker = HitSeeker;
            Hold.OnSwat = Swat;
            Hold.OnHitSpring = HitSpring;
            Hold.OnHitSpinner = HitSpinner;
            Hold.SpeedGetter = () => Speed;
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            LiftSpeedGraceTime = 0.1f;
            Add(new VertexLight(base.Collider.Center, Color.White, 1f, 32, 64));
            base.Tag = Tags.TransitionUpdate;
            Add(new MirrorReflection());
        }

        public UpdogCarriable(EntityData e, Vector2 offset)
            : this(e.Position + offset)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level = SceneAs<Level>();
            foreach (UpdogCarriable entity in Level.Tracker.GetEntities<UpdogCarriable>())
            {
                if (entity != this && entity.Hold.IsHeld)
                {
                    RemoveSelf();
                }
            }
            if (Level.Session.Level == "e-00")
            {
                tutorialGui = new BirdTutorialGui(this, new Vector2(0f, -24f), Dialog.Clean("tutorial_carry"), Dialog.Clean("tutorial_hold"), BirdTutorialGui.ButtonPrompt.Grab);
                tutorialGui.Open = false;
                base.Scene.Add(tutorialGui);
            }
        }

        public override void Update()
        {
            base.Update();
            if (shattering || dead)
            {
                return;
            }
            if (swatTimer > 0f)
            {
                swatTimer -= Engine.DeltaTime;
            }
            hardVerticalHitSoundCooldown -= Engine.DeltaTime;
            if (OnPedestal)
            {
                base.Depth = 8999;
                return;
            }
            base.Depth = 100;
            if (Hold.IsHeld)
            {
                prevLiftSpeed = Vector2.Zero;
            }
            else
            {
                if (OnGround())
                {
                    float target = ((!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f)));
                    Speed.X = Calc.Approach(Speed.X, target, 800f * Engine.DeltaTime);
                    Vector2 liftSpeed = base.LiftSpeed;
                    if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
                    {
                        Speed = prevLiftSpeed;
                        prevLiftSpeed = Vector2.Zero;
                        Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
                        if (Speed.X != 0f && Speed.Y == 0f)
                        {
                            Speed.Y = -60f;
                        }
                        if (Speed.Y < 0f)
                        {
                            noGravityTimer = 0.15f;
                        }
                    }
                    else
                    {
                        prevLiftSpeed = liftSpeed;
                        if (liftSpeed.Y < 0f && Speed.Y < 0f)
                        {
                            Speed.Y = 0f;
                        }
                    }
                }
                else if (Hold.ShouldHaveGravity)
                {
                    float num = 800f;
                    if (Math.Abs(Speed.Y) <= 30f)
                    {
                        num *= 0.5f;
                    }
                    float num2 = 350f;
                    if (Speed.Y < 0f)
                    {
                        num2 *= 0.5f;
                    }
                    Speed.X = Calc.Approach(Speed.X, 0f, num2 * Engine.DeltaTime);
                    if (noGravityTimer > 0f)
                    {
                        noGravityTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        Speed.Y = Calc.Approach(Speed.Y, 200f, num * Engine.DeltaTime);
                    }
                }
                previousPosition = base.ExactPosition;
                MoveH(Speed.X * Engine.DeltaTime, onCollideH);
                MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
                if (base.Center.X > (float)Level.Bounds.Right)
                {
                    MoveH(32f * Engine.DeltaTime);
                    if (base.Left - 8f > (float)Level.Bounds.Right)
                    {
                        RemoveSelf();
                    }
                }
                else if (base.Left < (float)Level.Bounds.Left)
                {
                    base.Left = Level.Bounds.Left;
                    Speed.X *= -0.4f;
                }
                else if (base.Top < (float)(Level.Bounds.Top - 4))
                {
                    base.Top = Level.Bounds.Top + 4;
                    Speed.Y = 0f;
                }
                else if (base.Bottom > (float)Level.Bounds.Bottom && SaveData.Instance.Assists.Invincible)
                {
                    base.Bottom = Level.Bounds.Bottom;
                    Speed.Y = -300f;
                    Audio.Play("event:/game/general/assist_screenbottom", Position);
                }
                else if (base.Top > (float)Level.Bounds.Bottom)
                {
                    Die();
                }
                if (base.X < (float)(Level.Bounds.Left + 10))
                {
                    MoveH(32f * Engine.DeltaTime);
                }
                Player entity = base.Scene.Tracker.GetEntity<Player>();
                TempleGate templeGate = CollideFirst<TempleGate>();
                if (templeGate != null && entity != null)
                {
                    templeGate.Collidable = false;
                    MoveH((float)(Math.Sign(entity.X - base.X) * 32) * Engine.DeltaTime);
                    templeGate.Collidable = true;
                }
            }
            if (!dead)
            {
                Hold.CheckAgainstColliders();
            }
            if (hitSeeker != null && swatTimer <= 0f && !hitSeeker.Check(Hold))
            {
                hitSeeker = null;
            }
            if (tutorialGui != null)
            {
                if (!OnPedestal && !Hold.IsHeld && OnGround() && Level.Session.GetFlag("foundTheoInCrystal"))
                {
                    tutorialTimer += Engine.DeltaTime;
                }
                else
                {
                    tutorialTimer = 0f;
                }
                tutorialGui.Open = tutorialTimer > 0.25f;
            }
        }

        public IEnumerator Shatter()
        {
            shattering = true;
            BloomPoint bloom = new BloomPoint(0f, 32f);
            VertexLight light = new VertexLight(Color.AliceBlue, 0f, 64, 200);
            Add(bloom);
            Add(light);
            for (float p = 0f; p < 1f; p += Engine.DeltaTime)
            {
                Position += Speed * (1f - p) * Engine.DeltaTime;
                Level.ZoomFocusPoint = base.TopCenter - Level.Camera.Position;
                light.Alpha = p;
                bloom.Alpha = p;
                yield return null;
            }
            yield return 0.5f;
            Level.Shake();
            sprite.Play("shatter");
            yield return 1f;
            Level.Shake();
        }

        public void ExplodeLaunch(Vector2 from)
        {
            if (!Hold.IsHeld)
            {
                Speed = (base.Center - from).SafeNormalize(120f);
                SlashFx.Burst(base.Center, Speed.Angle());
            }
        }

        public void Swat(HoldableCollider hc, int dir)
        {
            if (Hold.IsHeld && hitSeeker == null)
            {
                swatTimer = 0.1f;
                hitSeeker = hc;
                Hold.Holder.Swat(dir);
            }
        }

        public bool Dangerous(HoldableCollider holdableCollider)
        {
            if (!Hold.IsHeld && Speed != Vector2.Zero)
            {
                return hitSeeker != holdableCollider;
            }
            return false;
        }

        public void HitSeeker(Seeker seeker)
        {
            if (!Hold.IsHeld)
            {
                Speed = (base.Center - seeker.Center).SafeNormalize(120f);
            }
            Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
        }

        public void HitSpinner(Entity spinner)
        {
            if (!Hold.IsHeld && Speed.Length() < 0.01f && base.LiftSpeed.Length() < 0.01f && (previousPosition - base.ExactPosition).Length() < 0.01f && OnGround())
            {
                int num = Math.Sign(base.X - spinner.X);
                if (num == 0)
                {
                    num = 1;
                }
                Speed.X = (float)num * 120f;
                Speed.Y = -30f;
            }
        }

        public bool HitSpring(Spring spring)
        {
            if (!Hold.IsHeld)
            {
                if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
                {
                    Speed.X *= 0.5f;
                    Speed.Y = -160f;
                    noGravityTimer = 0.15f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = 220f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = -220f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    return true;
                }
            }
            return false;
        }

        private void OnCollideH(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
            if (Math.Abs(Speed.X) > 100f)
            {
                ImpactParticles(data.Direction);
            }
            Speed.X *= -0.4f;
        }

        private void OnCollideV(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
            }
            if (Speed.Y > 0f)
            {
                if (hardVerticalHitSoundCooldown <= 0f)
                {
                    Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", Calc.ClampedMap(Speed.Y, 0f, 200f));
                    hardVerticalHitSoundCooldown = 0.5f;
                }
                else
                {
                    Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", 0f);
                }
            }
            if (Speed.Y > 160f)
            {
                ImpactParticles(data.Direction);
            }
            if (Speed.Y > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch))
            {
                Speed.Y *= -0.6f;
            }
            else
            {
                Speed.Y = 0f;
            }
        }

        private void ImpactParticles(Vector2 dir)
        {
            float direction;
            Vector2 position;
            Vector2 positionRange;
            if (dir.X > 0f)
            {
                direction = (float)Math.PI;
                position = new Vector2(base.Right, base.Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            }
            else if (dir.X < 0f)
            {
                direction = 0f;
                position = new Vector2(base.Left, base.Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            }
            else if (dir.Y > 0f)
            {
                direction = -(float)Math.PI / 2f;
                position = new Vector2(base.X, base.Bottom);
                positionRange = Vector2.UnitX * 6f;
            }
            else
            {
                direction = (float)Math.PI / 2f;
                position = new Vector2(base.X, base.Top);
                positionRange = Vector2.UnitX * 6f;
            }
            Level.Particles.Emit(P_Impact, 12, position, positionRange, direction);
        }

        public override bool IsRiding(Solid solid)
        {
            if (Speed.Y == 0f)
            {
                return base.IsRiding(solid);
            }
            return false;
        }

        public override void OnSquish(CollisionData data)
        {
            if (!TrySquishWiggle(data) && !SaveData.Instance.Assists.Invincible)
            {
                Die();
            }
        }

        private void OnPickup()
        {
            Speed = Vector2.Zero;
            AddTag(Tags.Persistent);
        }

        private void OnRelease(Vector2 force)
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            RemoveTag(Tags.Persistent);
            if (player != null) {
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

            Speed = force * 200f;
            //Logger.Log(LogLevel.Info, "SantasGifts24", "theo y force: " + force.ToString());
            if (Speed != Vector2.Zero)
            {
                noGravityTimer = 0.1f;
            }
        }

        public void Die()
        {
            if (!dead)
            {
                dead = true;
                Player entity = Level.Tracker.GetEntity<Player>();
                entity?.Die(-Vector2.UnitX * (float)entity.Facing);
                Audio.Play("event:/char/madeline/death", Position);
                Add(new DeathEffect(Color.ForestGreen, base.Center - Position));
                sprite.Visible = false;
                base.Depth = -1000000;
                AllowPushing = false;
            }
        }

        public static void Load()
        {
            IL.Celeste.Player.Throw += PlayerThrowNoKB;
        }

        public static void Unload()
        {
            IL.Celeste.Player.Throw -= PlayerThrowNoKB;
        }

        private static void PlayerThrowNoKB(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdindR4()))
            {
                while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(80f)))
                {
                    Logger.Log(LogLevel.Info, "SS2024/UpdogCarriable", $"doing the things at {cursor.Index} in CIL code for {cursor.Method.FullName}");

                    cursor.Emit(OpCodes.Ldarg_0);

                    cursor.EmitDelegate<Func<Player, float>>(determineDashLengthFactor);
                    cursor.Emit(OpCodes.Mul);
                }

            }
        }
        private static float determineDashLengthFactor(Player player)
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
