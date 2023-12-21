using Celeste.Mod.Entities;
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
            }
            Engine.TimeRate = 1f;
            level.PauseLock = true;
            level.Session.SetFlag("SS2024_SunsetQuasar_endmeplease");
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
