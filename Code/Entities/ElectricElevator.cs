using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{

    [Tracked]
    [CustomEntity("SS2024/ElectricElevator")]
    class ElectricElevator : Solid
    {
        private class Rope : Entity
        {
            public ElectricElevator elevator;

            public Rope()
            {
                Depth = 1800;
            }

            public override void Render()
            {

                for (int i = 0; i < 2; i++)
                {
                    Vector2 vector6 = Vector2.UnitY * i;
                    Vector2 startPos = elevator.Position + vector6 + new Vector2(0f, -22f);
                    Vector2 endPos = elevator.Destination + vector6 + new Vector2(0f, -33f);

                    Draw.Line(startPos, endPos, Calc.HexToColor("423734"), 2f);
                }
            }
        }

        private Image front;
        private Entity back;
        private Image backImg;
        private Level level;
        private bool active;
        private float gondolaXSpeed;
        private float gondolaYSpeed;
        private bool canStartMove;
        private bool isMoving;
        public bool isShaking;
        private float randomTime;

        public Vector2 Start, Destination;
        private Vector2 switch1Pos, switch2Pos;

        private SoundSource moveLoopSfx;
        private Coroutine moveCoroutine;
        private Solid topCollider;
        private Vector2 shakeValue;
        public static ParticleType P_Dusticles = new ParticleType
        {
            Source = GFX.Game["particles/SS2024/ricky06/dustyMist"],
            Color = Calc.HexToColor("423734"),
            Color2 = Calc.HexToColor("2b2827"),
            ColorMode = ParticleType.ColorModes.Choose,
            FadeMode = ParticleType.FadeModes.Late,
            RotationMode = ParticleType.RotationModes.Random,
            Size = 0.3f,
            SizeRange = 0.2f,
            LifeMin = 0.8f,
            LifeMax = 1.2f,
            SpeedMin = 40f,
            SpeedMax = 60f,
            Direction = -3*(float)Math.PI/2 + 0.2f,
            DirectionRange = (float)Math.PI/12,  
        };

        public static ParticleType P_Electricity = new ParticleType
        {
            Source = GFX.Game["particles/rect"],
            Color = Calc.HexToColor("dade71"),
            Color2 = Calc.HexToColor("81eef0"),
            ColorMode = ParticleType.ColorModes.Blink,
            FadeMode = ParticleType.FadeModes.Late,
            Size = 0.5f,
            SizeRange = 0.2f,
            RotationMode = ParticleType.RotationModes.Random,
            LifeMin = 0.02f,
            LifeMax = 0.05f,
            SpeedMin = 50f,
            SpeedMax = 70f,
            DirectionRange = (float)Math.PI,
        };

        public Vector2 RenderPosition => Position + shakeValue - new Vector2(0f, 0f);


        public ElectricElevator(EntityData data, Vector2 offset)
            : base(data.Position + offset, 28f, 44f, safe: true)
        {
            active = true;
            EnableAssistModeChecks = false;


            Add(front = new Image(GFX.Game["objects/ss2024/elevator/activeFG"]));
            front.CenterOrigin();
            Depth = -1;
            SurfaceSoundIndex = 7;

            if (!active)
            {
                Collider = new Hitbox(Width, Height, -Width / 2f + 1f, -Height / 2f + 2f);
            }
            Add(moveLoopSfx = new SoundSource());

            Start = Position;
            Destination = data.Nodes[0] + offset;

            switch1Pos = data.Nodes[1] + offset;
            switch2Pos = data.Nodes[2] + offset;

            moveCoroutine = new Coroutine
            {
                RemoveOnComplete = false
            };
            Add(moveCoroutine);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (StaticMover component in scene.Tracker.GetComponents<StaticMover>())
            {
                if (component.IsRiding(this) && component.Platform == null)
                {
                    staticMovers.Add(component);
                    component.Platform = this;
                    component.OnAttach?.Invoke(this);
                }
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();

            scene.Add(back = new Entity(Position));
            back.Depth = 1000;


            if (active)
            {

                scene.Add(new Rope
                {
                    elevator = this
                });
                backImg = new Image(GFX.Game["objects/elevator/activeBG"]);
                backImg.CenterOrigin();
                back.Add(backImg);

                Collider = new Hitbox(Width, 8f, -Width / 2f, Height / 2f - 5f);
                topCollider = new Solid(Position + new Vector2(0f, -18f), Width, Height, true);
                topCollider.Collider = new Hitbox(Width, 8f, -Width / 2f, -Height / 2f + 2f);
                topCollider.SurfaceSoundIndex = 7;
                topCollider.EnableAssistModeChecks = false;

                scene.Add(topCollider);

                scene.Add(new ElevatorSwitch(switch1Pos, this, true));
                scene.Add(new ElevatorSwitch(switch2Pos, this, false));
            }

            canStartMove = true;
            isShaking = false;

            float gondolaSpeed = 1f;
            float totalDistance = Vector2.Distance(Start, Destination);
            gondolaXSpeed = gondolaSpeed * Math.Abs((Start.X - Destination.X) / totalDistance);
            gondolaYSpeed = gondolaSpeed * Math.Abs((Start.Y - Destination.Y) / totalDistance);
            randomTime = (Calc.Random.NextFloat() + 3f) * 0.5f;
        }


        public override void Update()
        {
            if(!level.Session.GetFlag("SS2024_level_electricity_flag"))
            {
                active = true;
            }
            else
            {
                active = false;
            }
            if (shakeTimer > 0f)
            {
                shakeTimer -= Engine.DeltaTime;
                if (shakeTimer <= 0f)
                {
                    shakeValue = Vector2.Zero;
                }
                else if (base.Scene.OnInterval(0.01f))
                {
                    shakeValue = Calc.Random.ShakeVector() * 0.7f;
                }
                level.Particles.Emit(P_Dusticles, topCollider.TopLeft);
                level.Particles.Emit(P_Dusticles, topCollider.TopRight, -3 * (float)Math.PI / 2 - 0.2f);
            }
            if (moveLoopSfx != null)
            {
                moveLoopSfx.Position = Position;
            }
            if (active)
            {
                if(level.OnInterval(randomTime))
                {
                    level.Particles.Emit(P_Electricity, topCollider.BottomCenter + new Vector2((Calc.Random.NextFloat() - 0.5f)*28f, 4f));
                    Audio.Play("event:/ricky06/SS2024/spark", Position);
                    randomTime = (Calc.Random.NextFloat() + 3f) * 0.5f;
                }
                if (HasPlayerOnTop() && !isMoving && canStartMove && !isShaking)
                {
                    if (Position == Start)
                    {
                        startMoving(true);
                    }
                    else
                    {
                        startMoving(false);
                    }
                }
                else if (!HasPlayerOnTop() && !isMoving)
                {
                    canStartMove = true;
                }
            }
            UpdatePositions();
            base.Update();
        }

        private void UpdatePositions()
        {
            back.Position = Position;
        }

        public void startMoving(bool moveToDest)
        {
            if (gondolaXSpeed == 0 && gondolaYSpeed == 0)
            {
                return;
            }
            canStartMove = false;
            moveCoroutine.Replace(MoveCoroutine(moveToDest));
        }

        private bool InView()
        {
            Camera camera = (base.Scene as Level).Camera;
            return base.Y > camera.Y - 40f && base.Y < camera.Y + 180f + 40f;
        }

        private IEnumerator shake()
        {
            isShaking = true;
            shakeTimer = 0.5f;
            yield return 0.7f;
        }

        private IEnumerator MoveCoroutine(bool moveToDest)
        {
            level.Shake();
            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
            if (!isMoving)
            {
                yield return shake();
            }
            moveLoopSfx.Play("event:/game/04_cliffside/gondola_movement_loop");
            isMoving = true;
            isShaking = false;
            Vector2 endNode = moveToDest ? Destination : Start;
            while (Position != endNode)
            {
                float currXSpeed = gondolaXSpeed;
                float currYSpeed = gondolaYSpeed;
                if (!InView())
                {
                    currXSpeed *= 4;
                    currYSpeed *= 4;
                }
                MoveTowardsX(endNode.X, currXSpeed);
                MoveTowardsY(endNode.Y, currYSpeed);
                topCollider.MoveTowardsX(endNode.X, currXSpeed);
                topCollider.MoveTowardsY(endNode.Y - 18f, currYSpeed);


                yield return null;
            }
            isMoving = false;
            moveLoopSfx.Stop();
        }
        public override void Render()
        {
            base.Render();
            backImg.RenderPosition = RenderPosition;
            front.RenderPosition = RenderPosition;
        }
    }
}
