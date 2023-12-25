using Celeste.Mod.Entities;
using Celeste.Mod.SantasGifts24.Code.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Entities
{
    [CustomEntity("SS2024/MortisDummy")]
    public class MortisDummy : Entity
    {
        public class AuxiliaryLightEntity : Entity
        {
            public VertexLight light;
            public BloomPoint bloom;
            public AuxiliaryLightEntity(Vector2 pos) : base(pos) 
            {
                Add(light = new VertexLight(Color.LimeGreen, 0f, 20, 50));
                Add(bloom = new BloomPoint(0f, 32f));
            }
        }

        public Sprite portal;
        public float mortisPercent;
        public Vector2 mortisPosition;
        public float pausefade;
        public MTexture rickmortis;
        public MTexture whitemortis;
        public AuxiliaryLightEntity aux;

        public MortisDummy(EntityData data, Vector2 offset) : this(data.Position + offset) 
        {
            Tag = Tags.HUD | Tags.PauseUpdate;
        }
        public MortisDummy(Vector2 position) : base(position) 
        {
            Add(portal = GFX.SpriteBank.Create("SS2024_SunsetQuasar_portal"));
            portal.Play("idle");
            portal.Rotation = 135f * Calc.DegToRad;
            portal.Scale = Vector2.Zero;
            rickmortis = GFX.Game["objects/SS2024/SunsetQuasar/portal/rickmortis"];
            whitemortis = GFX.Game["objects/SS2024/SunsetQuasar/portal/whitemortis"];
            mortisPosition = new Vector2(0f, 0f);

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(spawnPortal()));
            Scene.Add(aux = new AuxiliaryLightEntity(Position));
        }
        public override void Update()
        {
            if (!(Scene as Level).Paused)
            {
                base.Update();
            }
            pausefade = Calc.Approach(pausefade, (Scene as Level).Paused ? 1f : 0f, 8f * Engine.RawDeltaTime);
        }
        public override void Render()
        {
            Color col = Color.Lerp(Color.White, Color.Black, pausefade * 0.7f);
            Position -= (Scene as Level).Camera.Position;
            Position *= 6;
            portal.SetColor(col);
            base.Render();
            rickmortis.Draw(Position + mortisPosition, new Vector2(68f, 87f), col, mortisPercent);
            whitemortis.Draw(Position + mortisPosition, new Vector2(68f, 87f), col * (1-mortisPercent), mortisPercent);
            Position /= 6;
            Position += (Scene as Level).Camera.Position;
        }

        public IEnumerator spawnPortal()
        {
            yield return 1f;
            for (float i = 0; i < 1f; i += Engine.DeltaTime / 1f)
            {
                portal.Scale = Vector2.One * Ease.BackOut(i);
                aux.light.Alpha = aux.bloom.Alpha = Ease.SineOut(Math.Min(i * 3, 1));
                aux.light.startRadius = Ease.SineOut(i) * 20;
                aux.light.endRadius = aux.light.startRadius + (Ease.SineOut(i) * 30);
                aux.light.HandleGraphicsReset();
                yield return null;
            }
            Add(new Coroutine(mortisAppear(new Vector2(-108f, 20f))));
        }

        public IEnumerator mortisAppear(Vector2 offset)
        {
            for (float i = 0; i < 1f; i += Engine.DeltaTime / 1f)
            {
                mortisPercent = Ease.SineInOut(i);
                mortisPosition.X = Ease.SineInOut(i) * offset.X;
                mortisPosition.Y = Ease.ExpoIn(i) * offset.Y;
                yield return null;
            }
        }
    }

    public class DoctorDummy : Entity
    {
        public Sprite doctorSprite;
        public DoctorDummy(Vector2 pos) : base(pos)
        {
            Add(doctorSprite = new Sprite(GFX.Game, "characters/SS2024/SunsetQuasar/74shiro/oshiro"));
            Add(new VertexLight(Color.White, 1, 24, 40));
            doctorSprite.AddLoop("idle", "", 0.09f);
            doctorSprite.Play("idle");
            doctorSprite.CenterOrigin();
        }

        public void Flip()
        {
            doctorSprite.FlipX = !doctorSprite.FlipX;
        }
    }
    [CustomEntity("SS2024/QueerEventTrigger")]
    public class QueerEventTrigger : Trigger
    {
        string ev;
        public QueerEventTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            ev = data.Attr("event", "");
        }
        public override void OnEnter(Player player)
        {
            switch (ev)
            {
                case "endmeplease":
                    base.Scene.Add(new CS01_Ending(player));
                    break;
                case "thehellfire":
                    if ((Scene as Level).Session.GetFlag("SS2024_TheLargeArtifact0") && (Scene as Level).Session.GetFlag("SS2024_TheLargeArtifact1"))
                    {
                        base.Scene.Add(new CS01_OwieExplode(player));
                        RemoveSelf();
                    }
                    break;
                default:
                    RemoveSelf();
                    break;
            }
        }
    }

    public class CS01_Ending : CutsceneEntity
    {
        private Player player;

        private PrologueEndingTextButGroceries endingText;

        public CS01_Ending(Player player)
            : base(fadeInOnSkip: false, endingChapterAfter: true)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            level.RegisterAreaComplete();
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            player.Dashes = 1;
            yield return 0.7f;
            yield return player.DummyWalkTo(level.Bounds.Center.X);
            yield return 0.4f;
            player.DummyAutoAnimate = false;
            player.Sprite.Play("sleep");
            yield return 1.5f;
            float ease = 0f;
            endingText = new PrologueEndingTextButGroceries(instant: false);
            base.Scene.Add(endingText);
            level.Add(level.HiresSnow = new HiresSnow());
            level.HiresSnow.Alpha = 0f;
            while (ease < 1f)
            {
                ease += Engine.DeltaTime * 0.25f;
                float num = Ease.CubeInOut(ease);
                level.HiresSnow.Alpha = Calc.Approach(level.HiresSnow.Alpha, 1f, Engine.DeltaTime * 0.5f);
                endingText.Position = new Vector2(960f, 540f - 1080f * (1f - num));
                level.Camera.Y = (float)level.Bounds.Top - 2900f * num;
                yield return null;
            }
            level.PauseLock = true;
            yield return 2.5f;
            player.Add(new Coroutine(fadestuff(level, level.HiresSnow, endingText)));
            yield return 0.5f;
            EndCutscene(level);
        }

        public IEnumerator fadestuff(Level level, HiresSnow hr, PrologueEndingTextButGroceries pl)
        {
            float ease = 0f;
            while (ease < 1f)
            {
                ease += Engine.DeltaTime * 1f;
                float num = 1 - Ease.CubeInOut(ease);
                hr.Alpha = num;
                pl.alpha = num;
                yield return null;
            }
        }

        public IEnumerator waitthenfade(Level level, HiresSnow hr, PrologueEndingTextButGroceries pl)
        {
            yield return 2.5f;
            player.Add(new Coroutine(fadestuff(level, level.HiresSnow, endingText)));
            yield return 0.5f;
            level.Session.SetFlag("SS2024_SunsetQuasar_endmeplease");
        }

        public override void OnEnd(Level level)
        {
            if (WasSkipped)
            {

                if (player != null)
                {
                    player.Position = new Vector2(level.Bounds.Center.X, -328f);
                    player.StateMachine.State = 11;
                    player.DummyAutoAnimate = false;
                    player.Speed = Vector2.Zero;
                }
                if (level.HiresSnow == null)
                {
                    level.Add(level.HiresSnow = new HiresSnow());
                }
                level.HiresSnow.Alpha = 1f;
                if (endingText != null)
                {
                    level.Remove(endingText);
                }
                level.Add(endingText = new PrologueEndingTextButGroceries(instant: true));
                endingText.Position = new Vector2(960f, 540f);
                level.Camera.Y = level.Bounds.Top - 2900;
                if (player != null)
                {
                    player.Add(new Coroutine(waitthenfade(level, level.HiresSnow, endingText)));
                }
            } else
            {
                level.Session.SetFlag("SS2024_SunsetQuasar_endmeplease");
            }
            Engine.TimeRate = 1f;
            level.PauseLock = true;

        }
    }

    public class OrbEffect : Entity
    {
        public float percent = 1f;
        public float size = 1f;
        public MTexture texture;
        public OrbEffect(Vector2 pos, float size) : base(pos)
        {
            percent = 1f;
            this.size = size;
            texture = GFX.Game["objects/SS2024/SunsetQuasar/ball"];
            Depth = -21500;
        }
        public override void Update()
        {
            base.Update();
            if(percent > 0)
            {
                percent = (float)Math.Max(percent - Engine.DeltaTime * 1.25, 0f);
            }
        }

        public override void Render()
        {
            base.Render();
            Color col = Color.White * percent;
            col.A = 0;
            texture.DrawCentered(Position, col, size * Ease.CubeOut(percent));
        }
    }

    public class CS01_OwieExplode : CutsceneEntity
    {
        private Player player;

        private PrologueEndingTextButGroceries endingText;

        public SoundSource sound;

        public float red;

        public float dist;

        public float angle;

        public bool ohshit = false;

        public float angleSpeed = 0f;

        public CS01_OwieExplode(Player player)
            : base(fadeInOnSkip: false, endingChapterAfter: true)
        {
            this.player = player;
            Depth = -22000;
            red = 0;
            dist = 24;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        public override void Update()
        {
            base.Update();
            angle += angleSpeed * Engine.DeltaTime;
        }

        private IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            player.Dashes = 1;
            player.Drop();
            player.ForceCameraUpdate = true;
            Vector2 madpos = player.Position;
            yield return 1.5f;

            UpdogCarriable updog = Scene.Tracker.GetEntity<TheoCrystal>() as UpdogCarriable;
            if(updog != null)
            {
                Vector2 origpos = updog.Position;
                float percent = 0;
                updog.Collidable = false;
                updog.fg = true;
                updog.noGravityTimer = 999999999; // i am seriously so done with this collab i do not care anymore
                while (percent < 1)
                {
                    percent += Engine.DeltaTime / 1.25f;
                    yield return null;
                    updog.X = Calc.LerpClamp(origpos.X, 1759, Ease.CubeIn(percent));
                    updog.Y = Calc.LerpClamp(origpos.Y, 170, Ease.CubeIn(percent));
                }
                Audio.Play("event:/sunset_secretsanta/snap", updog.Position);
                Celeste.Freeze(0.08f);
                level.Shake();
                Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);

            }
            else
            {
                EndCutscene(level);
            }
            yield return 1f;
            TheLargeArtifact art0 = new TheLargeArtifact(updog.Position + new Vector2(24, -10), 0, true);
            TheLargeArtifact art1 = new TheLargeArtifact(updog.Position + new Vector2(-24, -10), 1, true);
            Scene.Add(art1);
            Scene.Add(art0);
            Scene.Add(new OrbEffect(art0.Center, 0.2f));
            Scene.Add(new OrbEffect(art1.Center, 0.2f));
            Audio.Play("event:/sunset_secretsanta/crystal", updog.Position);
            yield return 0.5f;
            Add(new Coroutine(spinnnn(updog, art0, art1)));
            yield return 1.5f;
            Add(sound = new SoundSource());
            sound.Play("event:/sunset_secretsanta/impending");
            Add(new Coroutine(redflash()));
            yield return 6f;
            sound.Param("death", 1f);
            ohshit = true;
            (Scene as Level).NextColorGrade("SS2024/SunsetQuasar/white", 0.5f);
            while (true)
            {
                dist = Math.Max(dist - Engine.DeltaTime * 10, 0);
                if (dist == 0) break;
                yield return null;
            }
            yield return 0.5f;

            EndCutscene(level);
        }

        public IEnumerator redflash()
        {
            float num = 0;
            while (true)
            {
                num += Engine.DeltaTime * 1.7f;
                red = (float)Math.Sin(num) * (float)Math.Sin(num);
                yield return null;
                
                if(Distort.Anxiety < 0.5f)
                {
                    Distort.Anxiety = Math.Min(Distort.Anxiety + Engine.DeltaTime / 10, 0.5f);
                }
            }
        }

        public IEnumerator spinnnn(UpdogCarriable c, TheLargeArtifact a0, TheLargeArtifact a1)
        {
            while (true)
            {
                angleSpeed = Calc.Approach(angleSpeed, ohshit ? 100 : 13, Engine.DeltaTime * (ohshit ? 8 : 5));

                a0.Position = c.Position + Calc.AngleToVector(angle, 24 * Ease.CubeInOut(dist / 24)) - (Vector2.UnitY * 10);
                a1.Position = c.Position + Calc.AngleToVector(angle, -24 * Ease.CubeInOut(dist / 24)) - (Vector2.UnitY * 10);
                yield return null;
            }
        }

        public override void OnEnd(Level level)
        {
            if (WasSkipped)
            {


            }
            UpdogCarriable updog = Scene.Tracker.GetEntity<TheoCrystal>() as UpdogCarriable;
            if (updog != null)
            {
                updog.Collidable = false;
                updog.fg = true;
                updog.noGravityTimer = 999999999;
                updog.Position = new Vector2(1759, 170);
            }
            player.ForceCameraUpdate = false;
            player.StateMachine.State = 0;
            level.Session.SetFlag("SS2024_SunsetQuasar_thehellfire");
        }

        public override void Render()
        {
            base.Render();
            Level level = Scene as Level;
            Color col = Color.Red * red * 0.25f;
            col.A = 0;
            Draw.Rect(level.Camera.Position, 320, 180, col);
        }
    }

    public class CS01_Conflict1 : CutsceneEntity
    {
        private Player player;

        private PrologueEndingTextButGroceries endingText;

        public CS01_Conflict1(Player player)
            : base(fadeInOnSkip: false, endingChapterAfter: true)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            player.Dashes = 1;
            yield return 1.5f;

            EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            if (WasSkipped)
            {


            }
            level.SnapColorGrade("SS2024/SunsetQuasar/white");
            player.StateMachine.State = 11;
            level.Session.SetFlag("SS2024_SunsetQuasar_thehellfire");
        }
    }

    [Tracked]
    public class PrologueEndingTextButGroceries : Entity
    {
        private FancyText.Text text;

        public float alpha;

        public PrologueEndingTextButGroceries(bool instant)
        {
            base.Tag = Tags.HUD;
            text = FancyText.Parse(Dialog.Clean("SecretSanta2024_2_Medium_sunsetquasar_end"), 960, 4, 0f);
            Add(new Coroutine(Routine(instant)));
            alpha = 1f;
        }

        private IEnumerator Routine(bool instant)
        {
            if (!instant)
            {
                yield return 4f;
            }
            for (int i = 0; i < text.Count; i++)
            {
                if (text[i] is FancyText.Char c)
                {
                    while ((c.Fade += Engine.DeltaTime * 20f) < 1f)
                    {
                        yield return null;
                    }
                    c.Fade = 1f;
                }
            }
        }

        public override void Render()
        {
            text.Draw(Position, new Vector2(0.5f, 0.5f), Vector2.One, alpha);
        }
    }
}
