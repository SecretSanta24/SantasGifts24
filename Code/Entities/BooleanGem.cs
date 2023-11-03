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
    [CustomEntity("SS2024/BooleanGem")]
    public class BooleanGem : Entity
    {
        public static ParticleType P_Shatter;

        public static ParticleType P_Regen;

        public static ParticleType P_Glow;

        public static ParticleType P_ShatterTwo;

        public static ParticleType P_RegenTwo;

        public static ParticleType P_GlowTwo;

        private Sprite sprite;

        private Sprite flash;

        private Image outline;

        private Wiggler wiggler;

        private BloomPoint bloom;

        private VertexLight light;

        private Level level;

        private SineWave sine;

        private bool twoDashes;

        private bool oneUse;

        private ParticleType p_shatter;

        private ParticleType p_regen;

        private ParticleType p_glow;

        private float respawnTimer;

        public string flag;

        public bool stopMomentum;

        public BooleanGem(Vector2 position, bool twoDashes, bool oneUse, string path, string path2)
            : base(position)
        {
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(OnPlayer));
            this.twoDashes = twoDashes;
            this.oneUse = oneUse;
            string text;
            if (twoDashes)
            {
                text = path;
                p_shatter = new ParticleType(Refill.P_ShatterTwo);
                p_regen = new ParticleType(Refill.P_RegenTwo);
                p_glow = new ParticleType(Refill.P_GlowTwo);

                p_shatter.Color = Calc.HexToColor("d3edff");
                p_shatter.Color2 = Calc.HexToColor("94a5ef");
                p_regen.Color = Calc.HexToColor("a5c3ff");
                p_regen.Color2 = Calc.HexToColor("6c74dd");
                p_glow.Color = Calc.HexToColor("a5c3ff");
                p_glow.Color2 = Calc.HexToColor("6c74dd");
            }
            else
            {
                text = path2;
                p_shatter = new ParticleType(Refill.P_Shatter);
                p_regen = new ParticleType(Refill.P_Regen);
                p_glow = new ParticleType(Refill.P_Glow);
            }
            Add(outline = new Image(GFX.Game[text + "outline"]));
            outline.CenterOrigin();
            outline.Visible = false;
            Add(sprite = new Sprite(GFX.Game, text + "idle"));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            Add(flash = new Sprite(GFX.Game, text + "flash"));
            flash.Add("flash", "", 0.05f);
            flash.OnFinish = delegate
            {
                flash.Visible = false;
            };
            flash.CenterOrigin();
            Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v)
            {
                sprite.Scale = flash.Scale = Vector2.One * (1f + v * 0.2f);
            }));
            Add(new MirrorReflection());
            Add(bloom = new BloomPoint(0.8f, 16f));
            Add(light = new VertexLight(Color.White, 1f, 16, 48));
            Add(sine = new SineWave(0.6f));
            sine.Randomize();
            UpdateY();
            Depth = -100;

        }

        public BooleanGem(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("twoDash"), data.Bool("oneUse"), data.Attr("path", "objects/refill/"), data.Attr("path2", "objects/refillTwo/"))
        {
            P_Shatter = new ParticleType(Refill.P_Shatter);
            P_Regen = new ParticleType(Refill.P_Regen);
            P_Glow = new ParticleType(Refill.P_Glow);
            P_ShatterTwo = new ParticleType(Refill.P_ShatterTwo);
            P_RegenTwo = new ParticleType(Refill.P_RegenTwo);
            P_GlowTwo = new ParticleType(Refill.P_GlowTwo);
            flag = data.Attr("flag", "yeah");
            stopMomentum = data.Bool("stopMomentum", true);
            
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            (scene as Level).Session.SetFlag("flag", false);
        }

        public override void Update()
        {
            base.Update();
            if (respawnTimer > 0f)
            {
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f)
                {
                    Respawn();
                }
            }
            else if (Scene.OnInterval(0.1f))
            {
                level.ParticlesFG.Emit(p_glow, 1, Position, Vector2.One * 5f);
            }
            UpdateY();
            light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
            bloom.Alpha = light.Alpha * 0.8f;
            if (Scene.OnInterval(2f) && sprite.Visible)
            {
                flash.Play("flash", restart: true);
                flash.Visible = true;
            }
        }

        private void Respawn()
        {
            if (!Collidable)
            {
                Collidable = true;
                sprite.Visible = true;
                outline.Visible = false;
                Depth = -100;
                wiggler.Start();
                Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_return" : "event:/game/general/diamond_return", Position);
                level.ParticlesFG.Emit(p_regen, 16, Position, Vector2.One * 2f);
            }
        }

        private void UpdateY()
        {
            Sprite obj = flash;
            Sprite obj2 = sprite;
            float num2 = bloom.Y = sine.Value * 2f;
            float num5 = obj.Y = obj2.Y = num2;
        }

        public override void Render()
        {
            if (sprite.Visible)
            {
                sprite.DrawOutline();
            }
            base.Render();
        }

        private void OnPlayer(Player player)
        {
            if (UseRefill2(player, twoDashes))
            {
                Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Collidable = false;
                Add(new Coroutine(RefillRoutine(player)));
                respawnTimer = 2.5f;
            }
        }

        private IEnumerator RefillRoutine(Player player)
        {
            Celeste.Freeze(0.05f);
            yield return null;
            level.Shake();
            sprite.Visible = flash.Visible = false;
            if (!oneUse)
            {
                outline.Visible = true;
            }
            Depth = 8999;
            yield return 0.05f;
            float num = player.Speed.Angle();
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num - (float)Math.PI / 2f);
            level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num + (float)Math.PI / 2f);
            SlashFx.Burst(Position, num);
            if (oneUse)
            {
                RemoveSelf();
            }
        }
        public bool UseRefill2(Player player, bool twoDashes)
        {
            if(stopMomentum) player.Speed = Vector2.Zero;
            Add(new Coroutine(FlagRoutine(flag)));
            return true;
        }
        public IEnumerator FlagRoutine(string flag)
        {
            (Scene as Level).Session.SetFlag(flag, true);
            yield return null;
            (Scene as Level).Session.SetFlag(flag, false);
        }
    }

}
