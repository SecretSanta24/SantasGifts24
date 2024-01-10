using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    // by aurora
    [CustomEntity("SS2024/CrystalCollectable")]
    public class CrystalCollectable : Entity
    {

        [Tracked]
        public class CompleteCrystal : Entity
        {

            public Image crystalImage;
            public Image[] crystalPieces = new Image[9];
            public Vector2[] crystalOffsets =
            {
                new Vector2(0, -26),
                new Vector2(-8,-12),
                new Vector2(10,-8),
                new Vector2(-6,-3),
                new Vector2(-8,10),
                new Vector2(10,2),
                new Vector2(12,10),
                new Vector2(-8,24),
                new Vector2(8,27),
            };
            public Sprite flash;
            public bool flashPlaying = false;
            VertexLight vx;
            public CompleteCrystal(Vector2 pos) : base(pos)
            {
                Tag |= Tags.HUD;
                for (int i = 0; i < 9; i++)
                {
                    base.Add(crystalPieces[i] = new Image(GFX.Game["objects/ss2024/crystalCollectible/Crystal_Piece_" + (i + 1)]));
                    crystalPieces[i].CenterOrigin();
                    crystalPieces[i].Position = crystalOffsets[i]*6;
                    crystalPieces[i].Visible = false;
                }
                base.Add(crystalImage = new Image(GFX.Game["objects/ss2024/crystalCollectible/Big_Crystal_Complete"]));
                crystalImage.CenterOrigin();
                crystalImage.Visible = false;

                base.Add(new BloomPoint(0.8f, this.crystalImage.Width));
                base.Add(vx = new VertexLight(Color.Cyan, 1f, 16, 48));

                base.Add(this.flash = new Sprite(GFX.Game, "objects/ss2024/crystalCollectible/Big_Crystal_Complete_flash"));
                this.flash.Add("flash", "", 0.05f);
                this.flash.OnFinish = delegate (string anim)
                {
                    this.flash.Visible = false;
                    flashPlaying = false;
                };
                this.flash.CenterOrigin();
            }
        }

        public Image crystal;
        public int CrystalsCollected;
        private Wiggler wiggler;
        private BloomPoint bloom;
        private VertexLight light;
        private SineWave sine;
        private EntityID GID;
        private int nr;
        public CrystalCollectable(EntityData data, Vector2 offset, EntityID ID) : base(data.Position + offset) 
        {
            GID = ID;
            CrystalsCollected = SantasGiftsModule.Instance.Session.CrystalsCollected;

            nr = data.Int("number", 1);
            base.Add(crystal = new Image(GFX.Game["objects/ss2024/crystalCollectible/Small_Crystal"]));
            crystal.CenterOrigin();
            if (NrCollected(nr)) crystal.Color = Color.White * 0.5f;

            base.Add(this.wiggler = Wiggler.Create(0.5f, 4f, delegate (float v)
            {
                this.crystal.Scale = Vector2.One * (1f + v * 0.3f);
            }, false, false));
            //base.Add(new MirrorReflection());
            base.Add(this.bloom = new BloomPoint(0.8f, this.crystal.Width));
            base.Add(this.light = new VertexLight(Color.Cyan, 1f, 16, 48));
            base.Add(this.sine = new SineWave(0.6f, 0f));
            this.sine.Randomize();
            this.UpdateY();
            base.Collider = new Hitbox(this.crystal.Width, this.crystal.Height, -this.crystal.Width/2, -this.crystal.Height/2);
            base.Add(new PlayerCollider(new Action<Player>(this.OnPlayer), null, null));
        }

        private void OnPlayer(Player player)
        {
            Level level = base.Scene as Level;
            //player.PointBounce(base.Center);
            this.wiggler.Start();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            base.Add(new Coroutine(this.SmashRoutine(player, level), true));
        }

        private Vector2 HudPos(Vector2 pos, Camera camera)
        {
            return (pos-camera.Position)*6;
        }
        private Vector2 worldPos(Vector2 pos, Camera camera)
        {
            return pos/6 + camera.Position;
        }
        private IEnumerator SmashRoutine(Player player, Level level)
        {
            CrystalsCollected = SantasGiftsModule.Instance.Session.CrystalsCollected;
            SantasGiftsModule.Instance.Session.CrystalsCollected = (CrystalsCollected = CrystalsCollected | (1 << (nr-1)));
            this.Visible = false;
            this.Collidable = false;
            player.Stamina = 110f;
            SoundEmitter.Play("event:/game/07_summit/gem_get", this, null);
            Session session = level.Session;
            CompleteCrystal completeCrystal;
            Camera camera = level.Camera;
            level.Add(completeCrystal = new CompleteCrystal(new Vector2(1920/2, 1080/3)));
            Vector2 cPos = worldPos(completeCrystal.Position, camera);
            for (int i = 1; i <= 9; i++)
            {
                if(NrCollected(i))
                {
                    completeCrystal.crystalPieces[i - 1].Visible = true;
                }
            }

            completeCrystal.crystalImage.Color = Color.White * 0;
            session.DoNotLoad.Add(this.GID);
            session.SetFlag("SS24/CC/" + nr);
            level.Shake(0.3f);
            Celeste.Freeze(0.1f);
            SummitGem.P_Shatter.Color = Calc.HexToColor("0fc0ff");
            float num = player.Speed.Angle();
            level.ParticlesFG.Emit(SummitGem.P_Shatter, 5, this.Position, Vector2.One * 4f, num - 1.5707964f);
            level.ParticlesFG.Emit(SummitGem.P_Shatter, 5, this.Position, Vector2.One * 4f, num + 1.5707964f);
            SlashFx.Burst(this.Position, num);
            List<AbsorbOrb> orbs = new List<AbsorbOrb>();
            for (int i = 0; i < 10; i++)
            {
                AbsorbOrb orb = new AbsorbOrb(this.Position, null, cPos);
                orbs.Add(orb);
                base.Scene.Add(orb);
            }

            level.Flash(Color.White, true);
            base.Scene.Add(new SummitGem.BgFlash());
            Engine.TimeRate = 0.5f;
            while (Engine.TimeRate < 1f)
            {
                Engine.TimeRate += Engine.RawDeltaTime * 0.5f;
                //completeCrystal.crystalImage.Color = Color.White * (Engine.TimeRate-0.5f);
                for (int i = 1; i <= 9; i++)
                {
                    completeCrystal.crystalPieces[i - 1].Color = Color.White * (Engine.TimeRate-0.5f);
                }
                yield return null;
            }
            
            while(orbs.Any((orb) => orb.percent < 1))
            {
                foreach(AbsorbOrb orb in orbs)
                {
                    orb.AbsorbTarget = worldPos(completeCrystal.Position, camera);
                }
                yield return null;
            }
            yield return 1f;

            if (CrystalsCollected == 511)
            {
                completeCrystal.crystalImage.Visible = true;
                session.SetFlag("SS24/CC/all");
                float timer = 0.5f;
                while(timer > 0f)
                {
                    timer -= Engine.RawDeltaTime;
                    completeCrystal.crystalImage.Color = Color.White * (1f-(timer*2));
                    yield return null;
                }

                completeCrystal.flash.Visible = true;
                completeCrystal.flashPlaying = true;
                completeCrystal.flash.Play("flash", true, false);

                while(completeCrystal.flashPlaying)
                {
                    yield return null;
                }

                timer = 0.5f;
                while (timer > 0f)
                {
                    timer -= Engine.RawDeltaTime;
                    yield return null;
                }

                for (int i = 1; i <= 9; i++)
                {
                    completeCrystal.crystalPieces[i - 1].Visible = false;
                }

                timer = 1f;
                while (timer > 0f)
                {
                    timer -= Engine.RawDeltaTime;
                    completeCrystal.crystalImage.Scale = Vector2.One*timer;
                    Vector2 ppos = HudPos(player.Position, camera);
                    completeCrystal.Position = new SimpleCurve(completeCrystal.Position, ppos - new Vector2(0, 16f*6), ppos - new Vector2(0, 32f*6)).GetPoint(Ease.CubeIn(1f -timer));
                    yield return null;
                }

                completeCrystal.RemoveSelf();
            } else
            {
                float timer = 0.5f;
                while (timer > 0f)
                {
                    timer -= Engine.RawDeltaTime;
                    //completeCrystal.crystalImage.Color = Color.White * (timer);
                    for (int i = 1; i <= 9; i++)
                    {
                        completeCrystal.crystalPieces[i - 1].Color = Color.White * (timer);
                    }
                    yield return null;
                }
                
            }

            completeCrystal?.RemoveSelf();
            base.RemoveSelf();
            yield break;
        }
        private void UpdateY()
        {
            this.crystal.Y = this.sine.Value * 2f;
        }
        private bool NrCollected(int nr)
        {
            return 0 < (CrystalsCollected & (1 << (nr-1)));
        }

        public override void Update()
        {
            base.Update();
            UpdateY();
        }



    }

}
