using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    [CustomEntity("SS2024/ElectricGenerator")]
    public class ElectricGenerator : Entity
    {
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
            LifeMax = 0.1f,
            SpeedMin = 50f,
            SpeedMax = 70f,
            DirectionRange = (float)Math.PI
        };
        private TalkComponent talker;
        private Sprite sprite;
        private float randomTime;
        private SoundSource sfx;

        public ElectricGenerator(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Depth = 100;
            Add(sprite = GFX.SpriteBank.Create("electricGenerator"));
            Vector2 drawAt = new Vector2(-20f, 0f);

            Add(talker = new TalkComponent(new Rectangle(-32, 8, 20, 8), drawAt, OnTalk));
            Add(sfx = new SoundSource());
            sfx.Position = Position;
            talker.PlayerMustBeFacing = false;
            randomTime = (Calc.Random.NextFloat() + 1f)*0.5f;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (isElectricityOff())
            {
                sprite.Play("off");
            }
            else
            {
                changeMode(true);
            }
        }

        private void OnTalk(Player player)
        {
            Add(new Coroutine(PlayerOnGeneratorRoutine(player)));

        }

        private IEnumerator PlayerOnGeneratorRoutine(Player player)
        {
            talker.Enabled = false;
            Level level = SceneAs<Level>();
            player.StateMachine.State = 11;
            yield return player.DummyWalkToExact((int)X-24, walkBackwards: false, 1f, cancelOnFall: true);
            player.Facing = Facings.Right;

            string flagName = "SS2024_level_electricity_flag";
            if (isElectricityOff())
            {
                level.Session.SetFlag(flagName, false);
                changeMode(true);
            }
            else
            {
                level.Session.SetFlag(flagName, true);
                changeMode(false);
            }
            yield return 0.2f;
            player.StateMachine.State = 0;
            yield return 1f;
            talker.Enabled = true;
        }


        private void changeMode(bool on)
        {
            if(on)
            {
                sprite.Play("on");
                sfx.Play("event:/ricky06/SS2024/generator_start");
            }
            else
            {
                sprite.Play("off");
                sfx.Stop();
                Audio.Play("event:/ricky06/SS2024/generator_end", Position);
            }
        }
        public override void Render()
        {
            base.Render();
            if (base.Scene.OnInterval(randomTime) && !isElectricityOff())
            {
                Level level = SceneAs<Level>();
                for (int i = 0; i < 2; ++i)
                {
                    float num = Calc.Random.NextAngle();
                    float length = 15;
                    level.Particles.Emit(P_Electricity, 3, base.Center + Calc.AngleToVector(num, length) + new Vector2(4f, 4f), Vector2.One * 2f, num);
                }
                Audio.Play("event:/ricky06/SS2024/spark", Position);
                randomTime = (Calc.Random.NextFloat() + 0.5f) * 0.5f;
            }
        }

        private bool isElectricityOff()
        {
            Level level = SceneAs<Level>();
            if(level == null)
            {
                return false;
            }
            string flagName = "SS2024_level_electricity_flag";
            return level.Session.GetFlag(flagName);
        }
    }
}
