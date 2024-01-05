using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;

namespace Celeste.Mod.SantasGifts24.Entities
{
    [CustomEntity("SS2024/GlitchlessAnyPercent")]
    public class GlitchlessAnyPercent : Entity
    {
        private class Fader : Entity
        {
            public float Fade;

            public Fader()
            {
                base.Tag = Tags.HUD;
            }

            public override void Render()
            {
                Draw.Rect(-10f, -10f, Engine.Width + 20, Engine.Height + 20, Color.White * Fade);
            }
        }

        private Sprite sprite;

        private Image pupil;

        private bool triggered;

        private Vector2 pupilTarget;

        private float pupilDelay;

        private Wiggler bounceWiggler;

        private Wiggler pupilWiggler;

        private float shockwaveTimer;

        private bool shockwaveFlag;

        private float pupilSpeed = 40f;

        private bool bursting;

        public bool playerHasMoved;

        public GlitchlessAnyPercent(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Add(sprite = GFX.SpriteBank.Create("temple_eyeball"));
            Add(pupil = new Image(GFX.Game["danger/templeeye/pupil"]));
            pupil.CenterOrigin();
            base.Collider = new Hitbox(48f, 64f, -24f, -32f);
            Add(new PlayerCollider(OnPlayer));
            Add(new HoldableCollider(OnHoldable));
            Add(bounceWiggler = Wiggler.Create(0.5f, 3f));
            Add(pupilWiggler = Wiggler.Create(0.5f, 3f));
            shockwaveTimer = 2.05f;
            shockwaveFlag = true;
        }

        private void OnPlayer(Player player)
        {
            if (!triggered)
            {
                Audio.Play("event:/game/05_mirror_temple/eyewall_bounce", player.Position);
                player.ExplodeLaunch(player.Center + Vector2.UnitX * 20f, true, false);
                player.Swat(-1);
                bounceWiggler.Start();
            }
        }

        private void OnHoldable(Holdable h)
        {
            if (!(h.Entity is TheoCrystal))
            {
                return;
            }
            TheoCrystal theoCrystal = h.Entity as TheoCrystal;
            if (!triggered && theoCrystal.Speed.X > 32f && !theoCrystal.Hold.IsHeld)
            {
                theoCrystal.Speed.X = -50f;
                theoCrystal.Speed.Y = -10f;
                triggered = true;
                bounceWiggler.Start();
                Collidable = false;
                Audio.SetAmbience(null);
                Audio.Play("event:/game/05_mirror_temple/eyewall_destroy", Position);
                Alarm.Set(this, 1.3f, delegate
                {
                    Audio.SetMusic(null);
                });
                Add(new Coroutine(Burst()));
            }
        }

        private IEnumerator Burst()
        {
            bursting = true;
            Level level = base.Scene as Level;
            level.StartCutscene(OnSkip, fadeInOnSkip: false, endingChapterAfterCutscene: true);
            level.RegisterAreaComplete();
            global::Celeste.Celeste.Freeze(0.1f);
            yield return null;
            float start = Glitch.Value;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, null, 0.5f, start: true);
            tween.OnUpdate = delegate (Tween t)
            {
                Glitch.Value = MathHelper.Lerp(start, 0f, t.Eased);
            };
            Add(tween);
            Player player = base.Scene.Tracker.GetEntity<Player>();
            TheoCrystal entity = base.Scene.Tracker.GetEntity<TheoCrystal>();
            if (player != null)
            {
                player.StateMachine.State = 11;
                player.StateMachine.Locked = true;
                if (player.OnGround())
                {
                    player.DummyAutoAnimate = false;
                    player.Sprite.Play("shaking");
                }
            }
            Add(new Coroutine(level.ZoomTo(entity.TopCenter - level.Camera.Position, 2f, 0.5f)));
            Add(new Coroutine(entity.Shatter()));
            foreach (TempleEye item in base.Scene.Entities.FindAll<TempleEye>())
            {
                item.Burst();
            }
            sprite.Play("burst");
            pupil.Visible = false;
            level.Shake(0.4f);
            yield return 2f;
            if (player != null && player.OnGround())
            {
                player.DummyAutoAnimate = false;
                player.Sprite.Play("shaking");
            }
            Visible = false;
            Fader fade = new Fader();
            level.Add(fade);
            while ((fade.Fade += Engine.DeltaTime) < 1f)
            {
                yield return null;
            }
            yield return 1f;
            level.EndCutscene();
        }

        private void OnSkip(Level level)
        {
        }

        public override void Update()
        {
            
            base.Update();
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                Audio.SetMusicParam("eye_distance", Calc.ClampedMap(entity.X, (base.Scene as Level).Bounds.Left, base.X));
            }
            if (!playerHasMoved && entity != null && entity.Speed != Vector2.Zero && entity.StateMachine.state != Player.StIntroRespawn)
            {
                playerHasMoved = true;
            }

            if (!triggered && shockwaveTimer > 0f)
            {
                if(playerHasMoved) shockwaveTimer -= Engine.DeltaTime;
                if (shockwaveTimer <= 0f)
                {
                    if (entity != null)
                    {
                        shockwaveTimer = Calc.ClampedMap(Math.Abs(base.X - entity.X), 100f, 500f, 2f, 3f);
                        shockwaveFlag = !shockwaveFlag;
                        if (shockwaveFlag)
                        {
                            shockwaveTimer = 1f;
                        }
                    }
                    base.Scene.Add(Engine.Pooler.Create<TempleBigEyeballShockwave>().Init(base.Center + new Vector2(50f, 0f)));
                    pupilWiggler.Start();
                    pupilTarget = new Vector2(-1f, 0f);
                    pupilSpeed = 120f;
                    pupilDelay = Math.Max(0.5f, pupilDelay);
                }
            }
            pupil.Position = Calc.Approach(pupil.Position, pupilTarget * 12f, Engine.DeltaTime * pupilSpeed);
            pupilSpeed = Calc.Approach(pupilSpeed, 40f, Engine.DeltaTime * 400f);
            TheoCrystal entity2 = base.Scene.Tracker.GetEntity<TheoCrystal>();
            if (entity2 != null && Math.Abs(base.X - entity2.X) < 64f && Math.Abs(base.Y - entity2.Y) < 64f)
            {
                pupilTarget = (entity2.Center - Position).SafeNormalize();
            }
            else if (pupilDelay < 0f)
            {
                pupilTarget = Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 1f);
                pupilDelay = Calc.Random.Choose(0.2f, 1f, 2f);
            }
            else
            {
                pupilDelay -= Engine.DeltaTime;
            }
            if (entity != null)
            {
                Level level = base.Scene as Level;
                Audio.SetMusicParam("eye_distance", Calc.ClampedMap(entity.X, level.Bounds.Left + 32, base.X - 32f, 1f, 0f));
            }
        }

        public override void Render()
        {
            sprite.Scale.X = 1f + 0.15f * bounceWiggler.Value;
            pupil.Scale = Vector2.One * (1f + pupilWiggler.Value * 0.15f);
            base.Render();
        }
    }

}
