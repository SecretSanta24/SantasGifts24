using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.SantasGifts24.Code.Cutscenes;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    public class MysteriousTree : Entity
    {
        public class ConfettiRenderer : Entity
        {
            private struct Particle
            {
                public Vector2 Position;

                public Color Color;

                public Vector2 Speed;

                public float Timer;

                public float Percent;

                public float Duration;

                public float Alpha;

                public float Approach;
            }

            private static readonly Color[] confettiColors = new Color[2]
            {
            Calc.HexToColor("37db45"),
            Calc.HexToColor("43a633"),
            };

            private Particle[] particles = new Particle[30];

            public ConfettiRenderer(Vector2 position)
                : base(position)
            {
                base.Depth = -10010;
                for (int i = 0; i < particles.Length; i++)
                {
                    particles[i].Position = Position + new Vector2(Calc.Random.Range(-3, 3), Calc.Random.Range(-3, 3));
                    particles[i].Color = Calc.Random.Choose(confettiColors);
                    particles[i].Timer = Calc.Random.NextFloat();
                    particles[i].Duration = Calc.Random.Range(2, 4);
                    particles[i].Alpha = 1f;
                    float angleRadians = Calc.Random.Range(0f, (float)Math.PI * 2f);
                    int num = Calc.Random.Range(80, 120);
                    particles[i].Speed = Calc.AngleToVector(angleRadians, num);
                }
            }

            public override void Update()
            {
                for (int i = 0; i < particles.Length; i++)
                {
                    particles[i].Position += particles[i].Speed * Engine.DeltaTime;
                    particles[i].Speed.X = Calc.Approach(particles[i].Speed.X, 0f, 80f * Engine.DeltaTime);
                    particles[i].Speed.Y = Calc.Approach(particles[i].Speed.Y, 20f, 100f * Engine.DeltaTime);
                    particles[i].Timer += Engine.DeltaTime;
                    particles[i].Percent += Engine.DeltaTime / particles[i].Duration;
                    particles[i].Alpha = Calc.ClampedMap(particles[i].Percent, 0.9f, 1f, 1f, 0f);
                    if (particles[i].Speed.Y > 0f)
                    {
                        particles[i].Approach = Calc.Approach(particles[i].Approach, 5f, Engine.DeltaTime * 16f);
                    }
                }
            }

            public override void Render()
            {
                for (int i = 0; i < particles.Length; i++)
                {
                    float num = 0f;
                    Vector2 position = particles[i].Position;
                    if (particles[i].Speed.Y < 0f)
                    {
                        num = particles[i].Speed.Angle();
                    }
                    else
                    {
                        num = (float)Math.Sin(particles[i].Timer * 4f) * 1f;
                        position += Calc.AngleToVector((float)Math.PI / 2f + num, particles[i].Approach);
                    }
                    GFX.Game["particles/petal"].DrawCentered(position + Vector2.UnitY, Color.Black * (particles[i].Alpha * 0.5f), 1f, num);
                    GFX.Game["particles/petal"].DrawCentered(position, particles[i].Color * particles[i].Alpha, 1f, num);
                }
            }
        }

        private class TreeExplosion : Entity {

            private float stayTimer;
            public TreeExplosion(Vector2 Position)
                : base(Position)
            {
                Add(GFX.SpriteBank.Create("treeExplosion"));
                stayTimer = 0.6f;
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                Audio.Play("event:/Kataiser/sfx/ww2_ssc24_hk_death_explosion", Position);
            }

            public override void Update()
            {
                base.Update();
                if(stayTimer > 0f)
                {
                    stayTimer -= Engine.DeltaTime;
                }
                else
                {
                    RemoveSelf();
                }
            }


        }

        public Vector2 BeamOrigin, ShotOrigin;
        public string RoomName;
        public bool BeginFight;
        public string beginImmediatelyFlag;

        private Sprite sprite;
        private StateMachine stateMachine;
        private bool attacking, inCutscene;
        private SoundSource laserSfx, chargeSfx;
        private Level level;
        private Solid trunk;
        private TreeKeyBarrier barrier;
        private string flag1, flag2, flag3;


        private int hp = 3;
        
        public MysteriousTree(Vector2 position, string flag1, string flag2, string flag3, string roomName, string beginImmediatelyFlag)
            : base(position)
        {
            RoomName = roomName;
            this.flag1 = flag1;
            this.flag2 = flag2;
            this.flag3 = flag3;
            this.beginImmediatelyFlag = beginImmediatelyFlag;
            Depth = 100;
            Add(laserSfx = new SoundSource());
            Add(chargeSfx = new SoundSource());
            Add(sprite = GFX.SpriteBank.Create("mysteriousTree"));
            sprite.Position.X -= 32f;
            Add(stateMachine = new StateMachine());

            stateMachine.SetCallbacks(0, idleUpdate, begin:idleBegin);
            stateMachine.SetCallbacks(1, attackUpdate, attackCoroutine, attackBegin, attackEnd);
            stateMachine.SetCallbacks(2, hurtUpdate, hurtCoroutine, hurtBegin);
            stateMachine.SetCallbacks(3, deathUpdate, begin:deathBegin);
            BeamOrigin = Center + new Vector2(26f, 84f);
            ShotOrigin = Center + new Vector2(28f, 140f);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            level = SceneAs<Level>();
            scene.Add(trunk = new Solid(new Vector2(Position.X + 9f, Position.Y + 16f), 80f, 200f, false));
            trunk.Add(new ClimbBlocker(edge: true));
            scene.Add(barrier = new TreeKeyBarrier(new Vector2(Position.X + 8f, Position.Y + 60f), 48f, 44f, this));
        }

        // idle
        private void idleBegin()
        {
            selectIdleAnimation();
        }

        private int idleUpdate()
        {
            Player player = Scene.Tracker.GetEntity<Player>();

            if (BeginFight)
            {
                return 1;
            }

            if (level.Session.GetFlag(beginImmediatelyFlag))
            {
                if (player != null && player.Speed != Vector2.Zero)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            if (!inCutscene && Math.Abs(player.Position.X - Position.X) < 200f)
            {
                Scene.Add(new CS_TreeBegin(this));
                inCutscene = true;
            }

            return 0;
        }

        // attack
        private void attackBegin()
        {
            attacking = true;
            selectIdleAnimation();
        }

        private void attackEnd()
        {
            attacking = false;
            foreach (MysteriousTreeBeam mtb in level.Tracker.GetEntities<MysteriousTreeBeam>())
            {
                level.Remove(mtb);
                mtb.RemoveSelf();
            }
            foreach (MysteriousTreeShot mts in level.Tracker.GetEntities<MysteriousTreeShot>())
            {
                level.Remove(mts);
                mts.RemoveSelf();
            }
        }

        private int attackUpdate()
        {
            return 1;
        }

        private IEnumerator attackCoroutine()
        {
            while (attacking)
            {
                switch (hp)
                {
                    case 3:
                        yield return Beam();
                        yield return 1f;
                        break;
                    case 2:
                        Shoot();
                        yield return 2f;
                        break;
                    default:
                        Add(new Coroutine(Beam()));
                        Shoot();
                        yield return 2f;
                        Shoot();
                        yield return 2f;
                        break;
                }
            }
        }

        // hurt

        private void hurtBegin()
        {
            attacking = false;
        }

        private int hurtUpdate()
        {
            if(attacking)
            {
                return 1;
            }
            return 2;
        }

        private IEnumerator hurtCoroutine()
        {
            level.Shake();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            sprite.Play("damage");
            Audio.Play("event:/Kataiser/sfx/ww2_ssc24_hk_tree_hit", Position);
            level.Add(new ConfettiRenderer(trunk.TopLeft + new Vector2(0f, 16f)));
            level.Add(new ConfettiRenderer(trunk.TopCenter + new Vector2(0f, 16f)));
            yield return 0.2f;
            selectIdleAnimation();
            yield return 2f;
            attacking = true;
        }

        // death

        private void deathBegin()
        {
            Scene.Add(new CS_TreeEnd(this));
        }

        private int deathUpdate()
        {
            if(Scene.OnInterval(0.2f))
            {
                level.Add(new TreeExplosion(getRandomPosition()));
            }
            return 3;
        }

        public void PlayBegin()
        {
            sprite.Play("yellA");
            Audio.Play("event:/Kataiser/sfx/ww2_ssc24_hk_scream", Position);
            level.Session.Audio.Music.Param("NightProgression", 6);
            level.Session.Audio.Apply(forceSixteenthNoteHack: false);
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        }

        public void Die()
        {
            sprite.Play("death");
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            level.Add(new ConfettiRenderer(trunk.TopLeft + new Vector2(0f, 16f)));
            level.Add(new ConfettiRenderer(trunk.TopCenter + new Vector2(0f, 16f)));
            Audio.Play("event:/Kataiser/sfx/ww2_ssc24_hk_tree_hit", Position);
        }

        private Vector2 getRandomPosition()
        {
            Vector2 initialPos = new Vector2(Position.X - 16f, Position.Y + 32f);
            float randomX = (float)Calc.Random.NextDouble() * 100f;
            float randomY = (float)Calc.Random.NextDouble() * 200f;

            return new Vector2(initialPos.X + randomX, initialPos.Y + randomY);
        }

        private void selectIdleAnimation()
        {
            switch(hp)
            {
                case 3:
                    sprite.Play("idleA");
                    break;
                case 2:
                    sprite.Play("idleB");
                    break;
                default:
                    sprite.Play("idleC");
                    break;
            }
        }

        private IEnumerator Beam()
        {
            laserSfx.Play("event:/char/badeline/boss_laser_charge");
            yield return 0.1f;
            Player entity = level.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                level.Add(Engine.Pooler.Create<MysteriousTreeBeam>().Init(this, entity));
            }
            yield return 1.4f;
            laserSfx.Stop();
            Audio.Play("event:/char/badeline/boss_laser_fire", Position);
        }

        private void Shoot()
        {

            Audio.Play("event:/char/badeline/boss_bullet", "end", 1f);

            Player entity = level.Tracker.GetEntity<Player>();
            Vector2 at = ShotOrigin;
            level.Add(Engine.Pooler.Create<MysteriousTreeShot>().Init(this, at, entity));
            level.Add(Engine.Pooler.Create<MysteriousTreeShot>().Init(this, at + new Vector2(6f, 16f), entity));
            level.Add(Engine.Pooler.Create<MysteriousTreeShot>().Init(this, at + new Vector2(-6f, 16f), entity));
        }

        public void TakeDamage()
        {
            switch (hp)
            {
                case 3:
                    level.Session.SetFlag(flag1);
                    break;
                case 2:
                    level.Session.SetFlag(flag2);
                    break;
                case 1:
                    level.Session.SetFlag(flag3);
                    break;
            }
            hp -= 1;
            if (hp <= 0)
            {
                stateMachine.State = 3;
            }
            else
            {
                stateMachine.State = 2;
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Remove(trunk);
            scene.Remove(barrier);
        }

        public override void Render()
        {
            sprite.DrawSimpleOutline();
            base.Render();
        }
    }
}
