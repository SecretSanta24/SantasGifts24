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
    [Tracked]
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
            Tag = Tags.HUD | Tags.PauseUpdate;

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

        public IEnumerator end(Vector2 offset)
        {
            for (float i = 0; i < 1f; i += Engine.DeltaTime / 1f)
            {
                mortisPercent = Ease.SineInOut(1-i);
                mortisPosition.X = Ease.SineInOut(1-i) * offset.X;
                mortisPosition.Y = Ease.ExpoIn(1-i) * offset.Y;
                portal.Scale = Vector2.One * Ease.BackOut(1 - i);
                aux.light.Alpha = aux.bloom.Alpha = Ease.SineOut(Math.Min((1 - i) * 3, 1));
                aux.light.startRadius = Ease.SineOut(1 - i) * 20;
                aux.light.endRadius = aux.light.startRadius + (Ease.SineOut(1 - i) * 30);
                aux.light.HandleGraphicsReset();
                yield return null;
            }
            RemoveSelf();
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
    [Tracked]
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
                        Collidable = false;
                    }
                    break;
                case "thatonee":
                    if (!(Scene as Level).Session.GetFlag("SS2024_Sunsetquasar_updog")) Scene.Add(new CS01_Conflict1(player));
                    Collidable = false;
                    break;
                default:
                    Collidable = false;
                    break;
            }
        }

        public static void Load()
        {
            On.Celeste.Level.Shake += ShakeHook;
        }

        public static void Unload()
        {
            On.Celeste.Level.Shake -= ShakeHook;
        }

        public static void ShakeHook(On.Celeste.Level.orig_Shake orig, Level self, float time)
        {
            QueerEventTrigger q = self.Tracker.GetEntity<QueerEventTrigger>();
            if(q == null)
            {
                orig(self, time);
                return;
            }
            if (self.InCutscene)
            {
                self.shakeDirection = Vector2.Zero;
                self.shakeTimer = Math.Max(self.shakeTimer, time);
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
            Audio.Play("event:/char/madeline/campfire_sit", player.Position);
            yield return 1.5f;
            float ease = 0f;
            endingText = new PrologueEndingTextButGroceries(instant: false);
            base.Scene.Add(endingText);
            level.Add(level.HiresSnow = new HiresSnow());
            level.HiresSnow.Alpha = 0f;
            player.ForceCameraUpdate = false;
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

        public SoundSource sound;

        public float red;

        public float dist;

        public float angle;

        public bool ohshit = false;

        public float angleSpeed = 0f;

        public CS01_OwieExplode(Player player)
            : base(fadeInOnSkip: false, endingChapterAfter: false)
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
            Add(new Coroutine(fademus()));
            player.StateMachine.State = 11;
            player.Dashes = 1;
            player.Drop();
            player.ForceCameraUpdate = true;
            Vector2 madpos = player.Position;
            Add(new Coroutine(player.DummyWalkTo(1760, false, 1, false)));
            yield return Textbox.Say("SecretSanta2024_2_Medium_sunsetquasar_d05");
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
            level.Shake();
            sound.Play("event:/sunset_secretsanta/impending");
            Add(new Coroutine(redflash()));
            yield return 1f;
            yield return player.DummyRunTo(player.X + 16, false);
            yield return 0.5f;
            yield return Textbox.Say("SecretSanta2024_2_Medium_sunsetquasar_d03" + (player.Y < 160 ? "_down" : "_up"), Walk2);
            
            yield return 2f;
            Add(new Coroutine(Textbox.Say("SecretSanta2024_2_Medium_sunsetquasar_d04" + (player.Y < 160 ? "_down" : "_up"))));
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

        public IEnumerator fademus()
        {
            for(float i = 0; i < 1f; i += Engine.DeltaTime)
            {
                Audio.SetMusicParam("fade", 1 - i);
                yield return null;
            }
            Level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/none");
            Level.Session.Audio.Apply();
        }

        public IEnumerator Walk2()
        {
            yield return player.DummyRunTo(player.X - 32, false);
            yield return 0.5f;
            player.Sprite.Play("tired");
            yield return 0.5f;
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
                Level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/none");
                Level.Session.Audio.Apply();
            }
            level.SnapColorGrade("SS2024/SunsetQuasar/white");
            Distort.Anxiety = 0;
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
            player.Facing = Facings.Right;
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

        public float boomAlpha;

        public DoctorDummy doc;
        public MortisDummy rickmortis;

        public float beamPercent;

        public CS01_Conflict1(Player player)
            : base(fadeInOnSkip: false, endingChapterAfter: false)
        {
            this.player = player;
            Depth = -99999;

        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        public override void Update()
        {
            base.Update();
            if (beamPercent > 0) 
            {
                beamPercent = Math.Max(beamPercent - Engine.DeltaTime / 1.4f, 0);
            }
        }

        private IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            player.Dashes = 1;
            player.DummyAutoAnimate = true;
            player.ForceCameraUpdate = false;
            yield return 0.5f;
            yield return player.DummyWalkTo(1472f);
            yield return 1f;
            Audio.Play("event:/sunset_secretsanta/doctorcrash");
            yield return 1.12f;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            level.Shake(0.5f);
            boomAlpha = 1;
            player.Facing = Facings.Right;
            player.Jump();
            Add(new Coroutine(boom()));

            yield return Textbox.Say("SecretSanta2024_2_Medium_sunsetquasar_d06", go, yeah2, yea3, yeahh4, stopitfuck);

            EndCutscene(level);
        }

        public IEnumerator go()
        {
            Level level = (Scene as Level);
            yield return 0.6f;
            player.ForceCameraUpdate = true;
            Add(new Coroutine(Level.ZoomTo(new Vector2(230f, 116f), 2f, 1.2f)));

            Scene.Add(doc = new DoctorDummy(new Vector2(1688f, 554f)));
            doc.Flip();

            yield return player.DummyRunTo(player.X + 104);
            yield return 0.5f;
        }

        public IEnumerator yeah2()
        {
            Add(new Coroutine(player.DummyRunTo(player.X + 80)));
            Add(new Coroutine((Scene as Level).ZoomBack(1f)));
            yield return 2f;
        }

        public IEnumerator yea3()
        {
            yield return 0.5f;
            Scene.Add(rickmortis = new MortisDummy(new Vector2(1736, 552)));
            yield return 0.5f;
            Audio.Play("event:/sunset_secretsanta/portal", rickmortis.Position);
            doc.Flip();
            yield return 2.5f;
        }

        public IEnumerator yeahh4()
        {
            yield return 0.5f;
            float yy = doc.Y;
            float target = doc.Y - 56;
            for(float p = 0; p < 1; p += Engine.DeltaTime / 2f)
            {
                doc.Y = Calc.LerpClamp(yy, target, Ease.SineInOut(p));
                yield return null;
            }
            yield return 0.5f;
            UpdogCarriable updog = new UpdogCarriable(doc.Position + new Vector2(0, 12));
            Scene.Add(updog);
            if (!Settings.Instance.DisableFlashes) Scene.Add(new OrbEffect(doc.Position, 4));
            else Scene.Add(new OrbEffect(doc.Position, 0.22f));
            beamPercent = 1f;
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            Level.Shake();
            Audio.Play("event:/sunset_secretsanta/crystal", doc.Position);
            updog.noGravityTimer = 1f;
            doc.Visible = false;
            yield return 1.5f;
        }

        public IEnumerator stopitfuck()
        {
            yield return 0.4f;
            rickmortis.Add(new Coroutine(rickmortis.end(new Vector2(-108f, 20f))));
            yield return 1.8f;
        }

        public IEnumerator boom()
        {
            while(boomAlpha > 0)
            {
                boomAlpha = Math.Max(boomAlpha - Engine.DeltaTime, 0);
                yield return null;
            }
        }

        public override void OnEnd(Level level)
        {
            if (WasSkipped)
            {
                player.Position = new Vector2(1658, 568);
                Scene.Add(new UpdogCarriable(new Vector2(1688, 568)));
                level.Camera.Position = new Vector2(1546, 432);
                //level.Session.Audio.Ambience.Event = SFX.EventnameByHandle("event:/sunset_secretsanta/amb");
            }
            if(rickmortis != null) rickmortis.RemoveSelf();
            if(doc != null) doc.RemoveSelf();
            player.StateMachine.State = 0;
            level.Session.SetFlag("SS2024_Sunsetquasar_updog");
        }

        public override void Render()
        {
            base.Render();

            float beam2 = Ease.SineIn(beamPercent);

            GFX.Game["objects/SS2024/SunsetQuasar/boom"].Draw((Scene as Level).Camera.Position, Vector2.Zero, new Color(boomAlpha, boomAlpha, boomAlpha, 0));
            if(doc != null)
            {
                if(rickmortis != null)
                {
                    Draw.Line(doc.Position, rickmortis.Position - (Vector2.UnitX * 13), new Color(beam2, beam2, beam2, 0), 5 * beam2);
                }
            }
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
