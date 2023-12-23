using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.SantasGifts24.Code.Cutscenes;


namespace Celeste.Mod.SantasGifts24.Code.Entities
{

    [CustomEntity("SS2024/BackupGenerator")]
    public class BackupGenerator : Entity
    {
        private Sprite sprite;
        private TalkComponent talker;
        public static ParticleType P_ElectricityBig = new ParticleType
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
            SpeedMin = 80f,
            SpeedMax = 120f,
            DirectionRange = (float)Math.PI
        };
        private float randomTime;



        public BackupGenerator(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Depth = 100;
            Add(sprite = GFX.SpriteBank.Create("backupGenerator"));
            
            Vector2 drawAt = new Vector2(0f, 50f);
 
            Add(talker = new TalkComponent(new Rectangle(-10, 60, 20, 8), drawAt, OnTalk));

            randomTime = (Calc.Random.NextFloat() + 1f) * 0.5f;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if(isTurnedOff())
            {
                sprite.Play("off");
            }
            else
            {
                sprite.Play("on");
            }
        }

        private void OnTalk(Player player)
        {
            Scene.Add(new CS_BackupGeneratorOff(this));
        }

        public override void Update()
        {
            base.Update();
            if(isTurnedOff())
            {
                talker.Enabled = false;
                sprite.Play("off");
            }
            else
            {
                talker.Enabled = true;
                sprite.Play("on");
            }
        }

        public override void Render()
        {
            sprite.DrawSimpleOutline();
            base.Render();
            if (base.Scene.OnInterval(randomTime) && !isTurnedOff())
            {
                Level level = SceneAs<Level>();
                for (int i = 0; i < 2; ++i)
                {
                    float num = Calc.Random.NextAngle();
                    float length = 30;
                    level.Particles.Emit(P_ElectricityBig, 3, base.Center + Calc.AngleToVector(num, length) + new Vector2(0f, 20f), Vector2.One * 2f, num);
                }
                Audio.Play("event:/ricky06/SS2024/spark", Position);
                randomTime = (Calc.Random.NextFloat() + 0.5f) * 0.5f;
            }
        }
        
        public void TurnOff()
        {
            talker.Enabled = false;
            sprite.Play("off");
            SceneAs<Level>().Session.SetFlag("SS2024_level_electricity_backup_flag", true);
        }

        private bool isTurnedOff()
        {
            Level level = SceneAs<Level>();
            if (level == null)
            {
                return false;
            }
            string flagName = "SS2024_level_electricity_backup_flag";
            return level.Session.GetFlag(flagName);
        }
    }
}
