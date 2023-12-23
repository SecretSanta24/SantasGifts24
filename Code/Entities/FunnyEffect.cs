using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    [CustomEntity("SantasGifts24/FunnyEffect")]
    public class FunnyEffect : Entity
    {
        class LineData
        {
            public Vector2 start, end;
            public Color color;
            public LineData(Vector2 start, Vector2 end, Color color)
            {
                this.start = start;
                this.end = end;
                this.color = color;
            }
        }

        Vector2? target;
        Coroutine coroutine;
        Level level;
        List<LineData> lines;
        ParticleType greenParticle;
        ParticleType blueParticle;
        Sprite Blob;

        Session session;
        string startFlag;
        string endFlag;
        string sound;

        public FunnyEffect(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            startFlag = data.Attr("startFlag", "");
            endFlag = data.Attr("endFlag", "");
            sound = data.Attr("sound", "event:/santas_gifts_funny_effect");

            lines = new List<LineData>();
            target = data.Position+offset;
            Depth = Depths.Top;

            base.Add(Blob = GFX.SpriteBank.Create("SS2024centerBlob"));
            Blob.Visible = false;
            Blob.CenterOrigin();
            Blob.Position = (target ?? Vector2.Zero) - Position;
            Blob.Position += new Vector2(-2, -3);

            greenParticle = new ParticleType
            {
                Source = GFX.Game["particles/cloud"],
                Color = Color.Green,
                Color2 = Color.Red,
                ColorMode = ParticleType.ColorModes.Choose,
                FadeMode = ParticleType.FadeModes.Late,
                Acceleration = new Vector2(0f, 1.2f),
                Size = 0.5f,
                SizeRange = 0.33333334f,
                SpeedMin = 50f,
                SpeedMax = 50f,
                SpeedMultiplier = 0.98f,
                DirectionRange = 2.7925267f,
                RotationMode = ParticleType.RotationModes.Random,
                LifeMin = 0.5f,
                LifeMax = 0.5f
            };
            blueParticle = new ParticleType(greenParticle)
            {
                Color = Color.Blue
            };
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            level = SceneAs<Level>();

            if (level == null || target == null)
            {
                RemoveSelf();
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }

        public override void Update()
        {
            base.Update();

            if (session == null)
            {
                session = (Engine.Scene as Level)?.Session;
                session?.SetFlag(startFlag, false);
            }

            if(coroutine == null && (session?.GetFlag(startFlag) ?? false))
            {
                Add(coroutine = new Coroutine(Animation(), true));
            }
        }

        public override void Render()
        {

            void LineOutline(LineData line)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue;
                        Vector2 offset = new(x, y);
                        Draw.Line(line.start + offset, line.end + offset, line.color * 0.25f);
                    }
                }
            }

            foreach (LineData line in lines)
            {
                LineOutline(line);
                Draw.Line(line.start, line.end, Color.White * 0.5f);
            }

            base.Render();
        }
        private IEnumerator Animation()
        {
            Vector2 realTarget = (Vector2)target;
            List<IEnumerator> enumerators = new List<IEnumerator>
            {
                RotatingLine(realTarget, 0, Color.Green),
                RotatingLine(realTarget, 120, Color.Blue),
                RotatingLine(realTarget, 240, Color.Red)
            };

            void advanceAllEnumerators()
            {
                foreach (IEnumerator enumerator in enumerators.ToArray())
                {
                    if (!enumerator.MoveNext())
                    {
                        enumerators.Remove(enumerator);
                    }
                }
            }
            Audio.Play(sound);

            for (int i = 0; i < 50; i++)
            {
                advanceAllEnumerators();

                if (i == 18)
                {
                    Blob.Visible = true;
                    Blob.Play("boom");
                }
                if (i == 10) enumerators.Add(ShrinkingHexagon(realTarget, Color.Red, 25));
                if (i == 20) enumerators.Add(ShrinkingHexagon(realTarget, Color.Green, 25));
                if (i == 30) enumerators.Add(ShrinkingHexagon(realTarget, Color.Blue, 25));
                if (i == 47) enumerators.Add(ShrinkingHexagon(realTarget, Color.Red, 20));
                yield return 0.016f;
            }
            enumerators.Add(RotatingLine(realTarget, 90, Color.Green, 15));
            enumerators.Add(RotatingLine(realTarget, 210, Color.Blue, 15));
            enumerators.Add(RotatingLine(realTarget, 330, Color.Red, 15));

            for (int i = 0; i < 30; i++)
            {
                advanceAllEnumerators();

                if (i == 3) enumerators.Add(ShrinkingHexagon(realTarget, Color.Green, 20));
                if (i == 7) enumerators.Add(ShrinkingHexagon(realTarget, Color.Blue, 20));
                //if (i == 11) enumerators.Add(ShrinkingHexagon(realTarget, Color.Red, 20));
                //if (i == 13) enumerators.Add(ShrinkingHexagon(realTarget, Color.Green, 20));
                //if (i == 15) enumerators.Add(ShrinkingHexagon(realTarget, Color.Blue, 20));
                yield return 0.016f;
            }

            level.Displacement.AddBurst(realTarget, 0.4f, 12f, 36f, 0.5f, null, null);
            level.Displacement.AddBurst(realTarget, 0.4f, 24f, 48f, 0.5f, null, null);
            level.Displacement.AddBurst(realTarget, 0.4f, 36f, 60f, 0.5f, null, null);

            for(float i = 0; i < Math.PI*2; i += Calc.DegToRad*30)
            {
                level.ParticlesFG.Emit(greenParticle, realTarget, i);
                level.ParticlesFG.Emit(blueParticle, realTarget, i);
            }

            while (enumerators.Count > 0)
            {

                advanceAllEnumerators();

                yield return 0.016f;
            }

            //Blob.Visible = false;
            coroutine = null;
            (Engine.Scene as Level)?.Session?.SetFlag(endFlag, true);
            RemoveSelf();
            yield break;
        }

        private IEnumerator RotatingLine(Vector2 target, float angleOffset, Color color, int speed=30)
        {

            float length = 1;

            Vector2 outer = new Vector2(1, 0).Rotate(Calc.DegToRad * angleOffset);
            LineData line = new LineData(target, target+outer*100, color);
            lines.Add(line);

            for(int i = 0; i < speed; i++)
            {
                outer = outer.Rotate((float)Math.PI * 0.125f * (1f / speed));
                length = Calc.Approach(length, 100, 100f/ speed);

                line.start = target + outer * (100 - length);
                line.end = target+outer*100;
                yield return 0;
            }
            for (int i = 0; i < speed; i++)
            {
                outer = outer.Rotate((float)Math.PI * 0.125f * (1f/ speed));
                length = Calc.Approach(length, 0, 100f / speed);
                line.end = new Vector2(i, 0);

                line.start = target;
                line.end = target + outer * length;
                yield return 0;
            }

            lines.Remove(line);
            yield break;
        }

        private IEnumerator ShrinkingHexagon(Vector2 target, Color color, int speed=60)
        {
            float distanceFromCenter = 80;
            float alpha = 1f;
            Vector2 pointA = new(0, -1);
            Vector2 pointB = pointA.Rotate(Calc.DegToRad * 60);
            Vector2 pointC = pointB.Rotate(Calc.DegToRad * 60);
            Vector2 pointD = pointC.Rotate(Calc.DegToRad * 60);
            Vector2 pointE = pointD.Rotate(Calc.DegToRad * 60);
            Vector2 pointF = pointE.Rotate(Calc.DegToRad * 60);
            LineData line1 = new(pointA, pointB, color * alpha);
            LineData line2 = new(pointB, pointC, color * alpha);
            LineData line3 = new(pointC, pointD, color * alpha);
            LineData line4 = new(pointD, pointE, color * alpha);
            LineData line5 = new(pointE, pointF, color * alpha);
            LineData line6 = new(pointF, pointA, color * alpha);

            lines.Add(line1);
            lines.Add(line2);
            lines.Add(line3);
            lines.Add(line4);
            lines.Add(line5);
            lines.Add(line6);

            void setHexagonLines(float distance)
            {
                line1.start = pointA * distance + target;
                line1.end = pointB * distance + target;

                line2.start = pointB * distance + target;
                line2.end = pointC * distance + target;

                line3.start = pointC * distance + target;
                line3.end = pointD * distance + target;

                line4.start = pointD * distance + target;
                line4.end = pointE * distance + target;

                line5.start = pointE * distance + target;
                line5.end = pointF * distance + target;

                line6.start = pointF * distance + target;
                line6.end = pointA * distance + target;
            }

            for (int i = 0; i < speed; i++)
            {
                distanceFromCenter = Calc.Approach(distanceFromCenter, 0, 80f/speed);
                setHexagonLines(distanceFromCenter);

                yield return 0;
            }

            lines.Remove(line1);
            lines.Remove(line2);
            lines.Remove(line3);
            lines.Remove(line4);
            lines.Remove(line5);
            lines.Remove(line6);
            yield break;
        }
    }
}