﻿using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    [CustomEntity("SS2024/AutoReturnFallingBlock")]
    public class AutoReturnFallingBlock : FallingBlock
    {
        private float restartDelay;
        private string flagOnReset;
        private string flagOnFall;
        private string flagTrigger;
        private bool resetFlagState;
        private bool fallFlagState;
        private bool invertTriggerFlag;
        private float maxSpeed;
        private float fallingAcceleration;
        private bool climbFall;
        private char TileType;
        private float restartTimer;
        private bool goingUp;
        private Orientation orientation;
        private Vector2 initialPos;
        private string shakeSound;
        private string impactSound;
        private string returnSound;
        private float returnMaxSpeed;
        private float returnAcceleration;

        public AutoReturnFallingBlock(EntityData data, Vector2 offset) : base(data, offset)
        {
            restartDelay = data.Float("resetDelay", 1F);
            flagOnReset = data.Attr("flagOnReset", "");
            flagOnFall = data.Attr("flagOnFall", "");
            flagTrigger = data.Attr("flagTrigger", "");
            resetFlagState = data.Bool("resetFlagState");
            fallFlagState = data.Bool("fallFlagState");
            invertTriggerFlag = data.Bool("invertFlagTrigger");
            maxSpeed = data.Float("maxSpeed", 160);
            fallingAcceleration = data.Float("acceleration", 500);
            climbFall = data.Bool("climbFall", false);
            TileType = data.Char("tiletype", '3');
            shakeSound = data.Attr("shakeSound", "");
            impactSound = data.Attr("landingSound", "");
            returnSound = data.Attr("returnSound", "");
            returnMaxSpeed = data.Float("returnMaxSpeed", 0.0F);
            returnAcceleration = data.Float("returnAcceleration", 0.0F);
            if (returnMaxSpeed <= 0) returnMaxSpeed = maxSpeed;
            if (returnAcceleration <= 0) returnAcceleration = fallingAcceleration;
            string direction = data.Attr("direction", "Down");
            if (direction == "Down")
            {
                orientation = Orientation.Down;
            }
            else
            if (direction == "Up")
            {
                orientation = Orientation.Up;
            }
            else
            if (direction == "Left")
            {
                orientation = Orientation.Left;
            }
            else
            {
                orientation = Orientation.Right;
            }
            initialPos = Position;
            Remove(Get<Coroutine>());
            Add(new Coroutine(NewSequence()));
        }

        private enum Orientation { Up, Down, Left, Right }

        private IEnumerator NewSequence()
        {
            yield return null;
            while (true)
            {
                while (!Triggered && !PlayerFallCheck())
                {
                    if (flagTrigger != "" && SceneAs<Level>().Session.GetFlag(flagTrigger) ^ invertTriggerFlag)
                    {
                        Triggered = true;
                    }
                    yield return null;
                }

                while (FallDelay > 0f)
                {
                    FallDelay -= Engine.DeltaTime;
                    yield return null;
                }

                bool impact = false;
                while (true)
                {
                    ShakeSfx();
                    StartShaking();
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

                    yield return 0.2f;
                    float timer = 0.4f;

                    while (timer > 0f && PlayerWaitCheck())
                    {
                        yield return null;
                        timer -= Engine.DeltaTime;
                    }

                    StopShaking();
                    for (int i = 2; i < Width; i += 4)
                    {
                        if (Scene.CollideCheck<Solid>(TopLeft + new Vector2(i, -2f)))
                        {
                            SceneAs<Level>().Particles.Emit(P_FallDustA, 2, new Vector2(X + i, Y), Vector2.One * 4f, (float)Math.PI / 2f);
                        }

                        SceneAs<Level>().Particles.Emit(P_FallDustB, 2, new Vector2(X + i, Y), Vector2.One * 4f);
                    }

                    float speed = 0f;
                    float maxSpeed = this.maxSpeed;
                    while (true)
                    {
                        Level level = SceneAs<Level>();
                        speed = Calc.Approach(speed, maxSpeed, fallingAcceleration * Engine.DeltaTime);
                        switch (orientation)
                        {
                            case Orientation.Up:
                                if (impact = MoveVCollideSolids(-speed * Engine.DeltaTime, thruDashBlocks: true)) break;
                                break;
                            case Orientation.Down:
                                if (impact = MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true)) break;
                                break;
                            case Orientation.Left:
                                if (impact = MoveHCollideSolids(-speed * Engine.DeltaTime, thruDashBlocks: true)) break;
                                break;
                            case Orientation.Right:
                                if (impact = MoveHCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true)) break;
                                break;
                            default:
                                break;
                        }
                        if (impact) break;
                        if (Top > level.Bounds.Bottom + 16 || Top > level.Bounds.Bottom - 1 && CollideCheck<Solid>(Position + new Vector2(0f, 1f)))
                        {
                            Collidable = Visible = false;
                            yield return 0.2f;
                            if (level.Session.MapData.CanTransitionTo(level, new Vector2(Center.X, Bottom + 12f)))
                            {
                                yield return 0.2f;
                                SceneAs<Level>().Shake();
                                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                            }

                            RemoveSelf();
                            DestroyStaticMovers();
                            yield break;
                        }

                        yield return null;
                    }
                    SceneAs<Level>().Session.SetFlag(flagOnFall, fallFlagState); //fall flag
                    ImpactSfx();
                    Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                    SceneAs<Level>().DirectionalShake(Vector2.UnitY, 0.3f);
                    switch (orientation)
                    {
                        case Orientation.Up:
                        case Orientation.Down:
                            SceneAs<Level>().DirectionalShake(Vector2.UnitY, 0.3f);
                            break;
                        case Orientation.Left:
                        case Orientation.Right:
                            SceneAs<Level>().DirectionalShake(Vector2.UnitX, 0.3f);
                            break;
                    }
                    StartShaking();
                    LandParticles();
                    yield return 0.2f;
                    StopShaking();
                    break;
                }
                float impactTimer = restartDelay;

                Vector2 crashPosition = Position;

                while (impactTimer > 0f)
                {
                    yield return null;
                    impactTimer -= Engine.DeltaTime;
                }
                impact = false;
                //return code
                while (true)
                {
                    ShakeSfx();
                    StartShaking();
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

                    yield return 0.2f;
                    float timer = 0.4f;

                    while (timer > 0f)
                    {
                        yield return null;
                        timer -= Engine.DeltaTime;
                    }

                    StopShaking();
                    for (int i = 2; i < Width; i += 4)
                    {
                        if (Scene.CollideCheck<Solid>(TopLeft + new Vector2(i, -2f)))
                        {
                            SceneAs<Level>().Particles.Emit(P_FallDustA, 2, new Vector2(X + i, Y), Vector2.One * 4f, (float)Math.PI / 2f);
                        }

                        SceneAs<Level>().Particles.Emit(P_FallDustB, 2, new Vector2(X + i, Y), Vector2.One * 4f);
                    }

                    float speed = 0f;
                    float maxSpeed = this.maxSpeed;
                    while (true)
                    {
                        Level level = SceneAs<Level>();
                        float ease = 0;
                        switch (orientation)
                        {
                            case Orientation.Left:
                            case Orientation.Right:
                                if (crashPosition.X - initialPos.X != 0)
                                {
                                    ease = (Position.X - initialPos.X) / (crashPosition.X - initialPos.X);
                                }
                                else
                                {
                                    ease = 1;
                                }
                                break;
                            case Orientation.Up:
                            case Orientation.Down:
                                if (crashPosition.Y - initialPos.Y != 0)
                                {
                                    ease = (Position.Y - initialPos.Y) / (crashPosition.Y - initialPos.Y);
                                }
                                else
                                {
                                    ease = 1;
                                }
                                break;
                        }
                        speed = Calc.Approach(speed, returnMaxSpeed * Calc.Clamp(Ease.QuadOut(Math.Abs(ease)), 0.2F, 1F), returnAcceleration * Engine.DeltaTime);
                        if (speed > returnMaxSpeed * Calc.Clamp(Ease.QuadOut(Math.Abs(ease)), 0.2F, 1F)) speed = returnMaxSpeed * Calc.Clamp(Ease.QuadOut(Math.Abs(ease)), 0.2F, 1F);
                        bool stop = false;
                        switch (orientation)
                        {
                            case Orientation.Left:

                                stop = Position.X >= initialPos.X;
                                break;
                            case Orientation.Right:
                                stop = Position.X <= initialPos.X;
                                break;
                            case Orientation.Up:
                                stop = Position.Y >= initialPos.Y;
                                break;
                            case Orientation.Down:
                                stop = Position.Y <= initialPos.Y;

                                break;

                        }
                        bool flag2 = false;
                        switch (orientation)
                        {
                            case Orientation.Up:
                                if (flag2 = MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true)) break;
                                break;
                            case Orientation.Down:
                                if (flag2 = MoveVCollideSolids(-speed * Engine.DeltaTime, thruDashBlocks: true)) break;
                                break;
                            case Orientation.Left:
                                if (flag2 = MoveHCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true)) break;
                                break;
                            case Orientation.Right:
                                if (flag2 = MoveHCollideSolids(-speed * Engine.DeltaTime, thruDashBlocks: true)) break;
                                break;
                            default:
                                break;
                        }
                        if (flag2 || stop)
                        {
                            //collision happens here normally, let it impact
                            switch (orientation)
                            {
                                case Orientation.Left:
                                case Orientation.Right:
                                    Position.X = initialPos.X;
                                    break;
                                case Orientation.Up:
                                case Orientation.Down:
                                    Position.Y = initialPos.Y;
                                    break;

                            }
                            break;
                        }

                        if (Top > level.Bounds.Bottom + 16 || Top > level.Bounds.Bottom - 1 && CollideCheck<Solid>(Position + new Vector2(0f, 1f)))
                        {
                            Collidable = Visible = false;
                            yield return 0.2f;
                            if (level.Session.MapData.CanTransitionTo(level, new Vector2(Center.X, Bottom + 12f)))
                            {
                                yield return 0.2f;
                                SceneAs<Level>().Shake();
                                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                            }

                            RemoveSelf();
                            DestroyStaticMovers();
                            yield break;
                        }

                        yield return null;
                    }

                    SceneAs<Level>().Session.SetFlag(flagOnReset, resetFlagState);
                    ReturnSfx();
                    Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                    switch (orientation)
                    {
                        case Orientation.Up:
                        case Orientation.Down:
                            SceneAs<Level>().DirectionalShake(Vector2.UnitY, 0.3f);
                            break;
                        case Orientation.Left:
                        case Orientation.Right:
                            SceneAs<Level>().DirectionalShake(Vector2.UnitX, 0.3f);
                            break;
                    }
                    yield return 0.2f;
                    StopShaking();
                    break;
                }
                Safe = true;
                Triggered = false;
                yield return null;
            }
        }

        private bool CheckFlag(string flag, bool invert)
        {
            return flag != "" && SceneAs<Level>().Session.GetFlag(flag) ^ invert;
        }

        private void ShakeSfx()
        {
            if (shakeSound != "")
            {
                Audio.Play(shakeSound, Center);
            }
            else if (TileType == '3')
            {
                Audio.Play("event:/game/01_forsaken_city/fallblock_ice_shake", Center);
            }
            else if (TileType == '9')
            {
                Audio.Play("event:/game/03_resort/fallblock_wood_shake", Center);
            }
            else if (TileType == 'g')
            {
                Audio.Play("event:/game/06_reflection/fallblock_boss_shake", Center);
            }
            else
            {
                Audio.Play("event:/game/general/fallblock_shake", Center);
            }
        }
        //TODO: Add custom sounds, add slowdown, add return speed / acceleration
        private void ImpactSfx()
        {
            Vector2 soundPos;
            switch (orientation)
            {
                case Orientation.Left:
                    soundPos = CenterLeft;
                    break;
                case Orientation.Right:
                    soundPos = CenterRight;
                    break;
                case Orientation.Up:
                    soundPos = TopCenter;
                    break;
                case Orientation.Down:
                default:
                    soundPos = BottomCenter;
                    break;

            }
            if (impactSound != "")
            {
                Audio.Play(impactSound, soundPos);
            }
            else if (TileType == '3')
            {
                Audio.Play("event:/game/01_forsaken_city/fallblock_ice_impact", soundPos);
            }
            else if (TileType == '9')
            {
                Audio.Play("event:/game/03_resort/fallblock_wood_impact", soundPos);
            }
            else if (TileType == 'g')
            {
                Audio.Play("event:/game/06_reflection/fallblock_boss_impact", soundPos);
            }
            else
            {
                Audio.Play("event:/game/general/fallblock_impact", soundPos);
            }
        }

        private void ReturnSfx()
        {
            if (impactSound != "")
            {
                Audio.Play(impactSound, BottomCenter);
            }
        }

        private bool PlayerFallCheck()
        {
            if (climbFall)
            {
                return HasPlayerRider();
            }

            return HasPlayerOnTop();
        }

        private bool PlayerWaitCheck()
        {
            if (Triggered)
            {
                return true;
            }

            if (PlayerFallCheck())
            {
                return true;
            }

            if (climbFall)
            {
                if (!CollideCheck<Player>(Position - Vector2.UnitX))
                {
                    return CollideCheck<Player>(Position + Vector2.UnitX);
                }

                return true;
            }

            return false;
        }


        private void LandParticles()
        {
            Vector2 pos1;
            float pos2;
            switch (orientation)
            {
                case Orientation.Left:
                case Orientation.Right:
                    pos1 = orientation == Orientation.Left ? TopLeft : TopRight;
                    pos2 = orientation == Orientation.Left ? Left : Right;
                    for (int i = 2; i <= Height; i += 4)
                    {
                        if (Scene.CollideCheck<Solid>(pos1 + new Vector2(3f, i)))
                        {
                            SceneAs<Level>().ParticlesFG.Emit(P_FallDustA, 1, new Vector2(pos2, Y + i), Vector2.One * 4f, -(float)Math.PI / 2f);
                            float direction = !(i < Height / 2f) ? 0f : (float)Math.PI;
                            SceneAs<Level>().ParticlesFG.Emit(P_LandDust, 1, new Vector2(pos2, Y + i), Vector2.One * 4f, direction + (float)Math.PI / 2);
                        }
                    }
                    break;
                case Orientation.Up:
                case Orientation.Down:
                    pos1 = orientation == Orientation.Down ? BottomLeft : TopLeft;
                    pos2 = orientation == Orientation.Down ? Bottom : Top;
                    for (int i = 2; i <= Width; i += 4)
                    {
                        if (Scene.CollideCheck<Solid>(pos1 + new Vector2(i, 3f)))
                        {
                            SceneAs<Level>().ParticlesFG.Emit(P_FallDustA, 1, new Vector2(X + i, pos2), Vector2.One * 4f, -(float)Math.PI / 2f);
                            float direction = !(i < Width / 2f) ? 0f : (float)Math.PI;
                            SceneAs<Level>().ParticlesFG.Emit(P_LandDust, 1, new Vector2(X + i, pos2), Vector2.One * 4f, direction);
                        }
                    }
                    break;
            }
        }
    }
}
