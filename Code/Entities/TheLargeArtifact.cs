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
    [CustomEntity("SS2024/TheLargeArtifact")]
    public class TheLargeArtifact : Entity
    {
        private class BgFlash : Entity
        {
            private float alpha = 1f;

            public BgFlash()
            {
                base.Depth = 10100;
                base.Tag = Tags.Persistent;
            }

            public override void Update()
            {
                base.Update();
                alpha = Calc.Approach(alpha, 0f, Engine.DeltaTime * 0.5f);
                if (alpha <= 0f)
                {
                    RemoveSelf();
                }
            }

            public override void Render()
            {
                Vector2 position = (base.Scene as Level).Camera.Position;
                Draw.Rect(position.X - 10f, position.Y - 10f, 340f, 200f, Color.Black * alpha);
            }
        }

        public static ParticleType P_Shatter = new ParticleType(SummitGem.P_Shatter);

        public static readonly Color[] GemColors = new Color[6]
        {
        Calc.HexToColor("72cca2"),
        Calc.HexToColor("cfbe5d"),
        Calc.HexToColor("90ff2d"),
        Calc.HexToColor("ffd300"),
        Calc.HexToColor("ff609d"),
        Calc.HexToColor("c5e1ba")
        };

        public int GemID;

        public EntityID GID;

        private Sprite sprite;

        private Wiggler scaleWiggler;

        private Vector2 moveWiggleDir;

        private Wiggler moveWiggler;

        private float bounceSfxDelay;

        public float timer;

        public bool lockin;

        public TheLargeArtifact(Vector2 Position, int gemid, bool lockin) : base(Position)
        {
            this.lockin = lockin;
            
            GemID = gemid;
            base.Collider = new Hitbox(12f, 12f, -6f, -6f);
            Add(sprite = new Sprite(GFX.Game, "collectables/SS2024/SunsetQuasar/" + GemID + "/gem"));
            Add(new VertexLight(GemColors[GemID], 1, 24, 40));
            //sprite.AddLoop("idle", "", 0.05f, [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7]);
            sprite.Play("idle");
            sprite.CenterOrigin();
            Add(scaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
            {
                sprite.Scale = Vector2.One * (1f + f * 0.3f);
            }));
            moveWiggler = Wiggler.Create(0.8f, 2f);
            moveWiggler.StartZero = true;
            Add(moveWiggler);
            Add(new PlayerCollider(OnPlayer));
            Add(new HoldableCollider(OnHoldable));
            timer = 0;
            if (lockin)
            {
                timer = Calc.Random.NextFloat(1000);
                Depth = -21000;
            }
        }

        public TheLargeArtifact(EntityData data, Vector2 position, EntityID gid)
            : base(data.Position + position)
        {
            GID = gid;
            lockin = false;
            GemID = data.Int("gem", 0);
            base.Collider = new Hitbox(12f, 12f, -6f, -6f);
            Add(sprite = new Sprite(GFX.Game, "collectables/SS2024/SunsetQuasar/" + GemID + "/gem"));
            Add(new VertexLight(GemColors[GemID], 1, 24, 40));
            //sprite.AddLoop("idle", "", 0.05f, [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7]);
            sprite.Play("idle");
            sprite.CenterOrigin();
            Add(scaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
            {
                sprite.Scale = Vector2.One * (1f + f * 0.3f);
            }));
            moveWiggler = Wiggler.Create(0.8f, 2f);
            moveWiggler.StartZero = true;
            Add(moveWiggler);
            Add(new PlayerCollider(OnPlayer));
            Add(new HoldableCollider(OnHoldable));
            timer = 0;
        }

        private void OnPlayer(Player player)
        {
            if (lockin) return;
            Level level = base.Scene as Level;
            if (player.DashAttacking)
            {
                Add(new Coroutine(SmashRoutine(player, level, Vector2.Zero)));
                return;
            }
            player.PointBounce(base.Center);
            moveWiggler.Start();
            scaleWiggler.Start();
            moveWiggleDir = (base.Center - player.Center).SafeNormalize(Vector2.UnitY);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            if (bounceSfxDelay <= 0f)
            {
                Audio.Play("event:/game/general/crystalheart_bounce", Position);
                bounceSfxDelay = 0.1f;
            }
        }
        
        public void OnHoldable(Holdable h)
        {
            if (lockin) return;
            Level level = base.Scene as Level;
            Player p = level.Tracker.GetNearestEntity<Player>(h.Entity.Position);
            if(p != null && !h.IsHeld) Add(new Coroutine(SmashRoutine(p, level, h.SpeedGetter())));
        }

        private IEnumerator SmashRoutine(Player player, Level level, Vector2 otherspeed)
        {
            Visible = false;
            Collidable = false;
            player.Stamina = 110f;
            SoundEmitter.Play("event:/game/07_summit/gem_get", this);
            Session session = (base.Scene as Level).Session;
            session.DoNotLoad.Add(GID);
            session.SetFlag("SS2024_TheLargeArtifact" + GemID, true);
            level.Shake();
            Celeste.Freeze(0.1f);
            P_Shatter.Color = GemColors[GemID];
            float num = player.Speed.Angle();
            if(otherspeed != Vector2.Zero)
            {
                num = otherspeed.Angle();
            }
            level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, num - (float)Math.PI / 2f);
            level.ParticlesFG.Emit(P_Shatter, 5, Position, Vector2.One * 4f, num + (float)Math.PI / 2f);
            SlashFx.Burst(Position, num);
            for (int i = 0; i < 10; i++)
            {
                base.Scene.Add(new AbsorbOrb(Position, player));
            }
            level.Flash(Color.White, drawPlayerOver: true);
            base.Scene.Add(new BgFlash());
            Engine.TimeRate = 0.5f;
            while (Engine.TimeRate < 1f)
            {
                Engine.TimeRate += Engine.RawDeltaTime * 0.5f;
                yield return null;
            }
            RemoveSelf();
        }

        public override void Update()
        {
            base.Update();
            bounceSfxDelay -= Engine.DeltaTime;
            timer += Engine.DeltaTime;
            sprite.Position = (Vector2.UnitY * (float)Math.Sin(timer) * 2) + moveWiggleDir * moveWiggler.Value * -8f;
            sprite.Rotation = (float)Math.Sin((timer * 1.1) + 1.2) * 0.3f;
        }

        public override void Render()
        {
            sprite.DrawSimpleOutline();
            base.Render();
        }
    }

}
