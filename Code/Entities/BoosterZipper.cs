using Celeste.Mod.Entities;
using FMOD;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Threading;

namespace Celeste.Mod.SantasGifts24.Entities
{
    // by Aurora Aquir
    [CustomEntity("SS2024/BoosterZipper")]
    [Tracked]
    public class BoosterZipper : Booster
    {
        private float percent;
        private SoundSource sfx = new SoundSource();
        private BoosterZipper.ZipMoverPathRenderer pathRenderer;
        private Vector2 target;
        public bool atEndPoint = false;
        public bool explodeNextUpdate = false;

        private float respawnTime = 1f;
        private float zipperMoveTime = 0.5f;

        public BoosterZipper(EntityData data, Vector2 offset) : base(data.Position + offset, false)
        {
            respawnTime = data.Float("boosterRespawnTime", 1f);
            zipperMoveTime = data.Float("zipperMoveTime", 0.5f);
            if (zipperMoveTime < 0.017f) zipperMoveTime = 0.017f;

            GFX.SpriteBank.CreateOn(sprite, "boosterZipper");
            base.Add(new Coroutine(this.Sequence(), true));
            this.sfx.Position = new Vector2(base.Width, base.Height) / 2f;
            base.Add(this.sfx);
            if(data.Nodes.Length >= 1)
            {
                target = data.NodesWithPosition(offset)[1];
            }
        }

        public static void Load()
        {
            On.Celeste.Player.BoostCoroutine += Player_BoostCoroutine;
            On.Celeste.Player.BoostUpdate += Player_BoostUpdate;
            On.Celeste.Booster.PlayerReleased += Booster_PlayerReleased;
        }
        public static void Unload()
        {
            On.Celeste.Player.BoostCoroutine -= Player_BoostCoroutine;
            On.Celeste.Player.BoostUpdate -= Player_BoostUpdate;
            On.Celeste.Booster.PlayerReleased -= Booster_PlayerReleased;
        }


        private static int Player_BoostUpdate(On.Celeste.Player.orig_BoostUpdate orig, Player self)
        {
            int result = orig(self);
            if(result == 2 && self.CurrentBooster is BoosterZipper bz && bz.atEndPoint)
            {
                bz.explodeNextUpdate = true;
            }
            return result;
        }

        private static IEnumerator Player_BoostCoroutine(On.Celeste.Player.orig_BoostCoroutine orig, Player self)
        {
            if (self.CurrentBooster is BoosterZipper)
            {
                yield break;
            }

            var orig_enum = orig(self);
            while (orig_enum.MoveNext())
                yield return orig_enum.Current;
        }


        private static void Booster_PlayerReleased(On.Celeste.Booster.orig_PlayerReleased orig, Booster self)
        {
            orig(self);
            if(self is BoosterZipper bz)
            {
                self.respawnTimer = bz.respawnTime;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(this.pathRenderer = new BoosterZipper.ZipMoverPathRenderer(this, Center, target));
            Image image = this.outline.Components.Get<Image>();
            if(image != null)
            {
                string path = GFX.SpriteBank?.SpriteData["boosterZipper"]?.Sources?.First<SpriteDataSource>()?.XML?.Attr("path");
                if (path == null || path.Length == 0) return;
                image.Texture = GFX.Game[ path + "outline"];
            }
        }

        public override void Update()
        {
            base.Update();
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && explodeNextUpdate)
            {
                explodeNextUpdate = false;
                Vector2 dashDir = player.CorrectDashPrecision(player.lastAim);
                if (dashDir.X != 0) dashDir.Y = 0;
                player.ExplodeLaunch(Center - dashDir, true, false);
                player.dashCooldownTimer = 0;
            }
            if (player != null && player.CurrentBooster == this)
            {
                BoostingPlayer = true;
                player.boostTarget = Center;
                var targetPos = Center - player.Collider.Center + (Input.Aim.Value * 3f);
                player.MoveToX(targetPos.X);
                player.MoveToY(targetPos.Y);
            }
        }
        private IEnumerator Sequence()
        {
            var player = Scene.Tracker.GetEntity<Player>();
            Vector2 start = this.Position;

            float movementSpeed = 1f/zipperMoveTime;
            for (; ; )
            {
                if (BoostingPlayer)
                {
                    this.sfx.Play("event:/game/01_forsaken_city/zip_mover", null, 0f);
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                    yield return 0.1f;
                    float at = 0f;
                    while (at < 1f)
                    {
                        yield return null;
                        at = Calc.Approach(at, 1f, movementSpeed * Engine.DeltaTime);
                        this.percent = Ease.SineIn(at);
                        Vector2 vector = Vector2.Lerp(start, this.target, this.percent);
                        //this.ScrapeParticlesCheck(vector);
                        if (base.Scene.OnInterval(0.1f))
                        {
                            this.pathRenderer.CreateSparks();
                        }
                        //base.MoveTo(vector);
                        if(at > 1f-0.017f)
                        {
                            //(vector - Position) / Engine.DeltaTime;
                            atEndPoint = true;
                            sprite.Color = Color.Yellow;
                        }
                        Position = vector;
                        outline.Position = Position;
                    }
                    //base.StartShaking(0.2f);
                    Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                    base.SceneAs<Level>().Shake(0.3f);
                    //this.StopPlayerRunIntoAnimation = true;
                    yield return 0.5f;

                    sprite.Color = Color.White;
                    atEndPoint = false;

                    //this.StopPlayerRunIntoAnimation = false;
                    //this.streetlight.SetAnimationFrame(2);
                    at = 0f;
                    while (at < 1f)
                    {
                        yield return null;
                        at = Calc.Approach(at, 1f, movementSpeed/4 * Engine.DeltaTime);
                        this.percent = 1f - Ease.SineIn(at);
                        Vector2 position = Vector2.Lerp(this.target, start, Ease.SineIn(at));
                        //base.MoveTo(position);
                        Position = position;
                        outline.Position = Position;
                    }
                    //this.StopPlayerRunIntoAnimation = true;
                    //base.StartShaking(0.2f);
                    //this.streetlight.SetAnimationFrame(1);
                    yield return 0.5f;
                    this.sfx.Stop();
                }
                else
                {
                    yield return null;
                }
            }
            yield break;
        }
        private class ZipMoverPathRenderer : Entity
        {
            public BoosterZipper Booster;
            private MTexture cog;
            private Vector2 from;
            private Vector2 to;
            private Vector2 sparkAdd;
            private float sparkDirFromA;
            private float sparkDirFromB;
            private float sparkDirToA;
            private float sparkDirToB;
            public ZipMoverPathRenderer(BoosterZipper Booster, Vector2 start, Vector2 target)
            {
                base.Depth = 5000;
                this.Booster = Booster;
                this.from = start;
                this.to = target;
                this.sparkAdd = (this.from - this.to).SafeNormalize(5f).Perpendicular();
                float num = (this.from - this.to).Angle();
                this.sparkDirFromA = num + 0.3926991f;
                this.sparkDirFromB = num - 0.3926991f;
                this.sparkDirToA = num + 3.1415927f - 0.3926991f;
                this.sparkDirToB = num + 3.1415927f + 0.3926991f;
                this.cog = GFX.Game["objects/zipmover/cog"];
            }

            // Token: 0x06002E23 RID: 11811 RVA: 0x00123270 File Offset: 0x00121470
            public void CreateSparks()
            {
                base.SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, this.from + this.sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), this.sparkDirFromA);
                base.SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, this.from - this.sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), this.sparkDirFromB);
                base.SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, this.to + this.sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), this.sparkDirToA);
                base.SceneAs<Level>().ParticlesBG.Emit(ZipMover.P_Sparks, this.to - this.sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), this.sparkDirToB);
            }

            // Token: 0x06002E24 RID: 11812 RVA: 0x001233A8 File Offset: 0x001215A8
            public override void Render()
            {
                this.DrawCogs(Vector2.UnitY, new Color?(Color.Black));
                this.DrawCogs(Vector2.Zero, Calc.HexToColor("45bee5"));
                
            }

            // Token: 0x06002E25 RID: 11813 RVA: 0x0012345C File Offset: 0x0012165C
            private void DrawCogs(Vector2 offset, Color? colorOverride = null)
            {
                Vector2 vector = (this.to - this.from).SafeNormalize();
                Vector2 value = vector.Perpendicular() * 3f;
                Vector2 value2 = -vector.Perpendicular() * 4f;
                float rotation = this.Booster.percent * 3.1415927f * 2f;
                Draw.Line(this.from + value + offset, this.to + value + offset, (colorOverride != null) ? colorOverride.Value : ZipMover.ropeColor);
                Draw.Line(this.from + value2 + offset, this.to + value2 + offset, (colorOverride != null) ? colorOverride.Value : ZipMover.ropeColor);
                for (float num = 4f - this.Booster.percent * 3.1415927f * 8f % 4f; num < (this.to - this.from).Length(); num += 4f)
                {
                    Vector2 value3 = this.from + value + vector.Perpendicular() + vector * num;
                    Vector2 value4 = this.to + value2 - vector * num;
                    Draw.Line(value3 + offset, value3 + vector * 2f + offset, (colorOverride != null) ? colorOverride.Value : ZipMover.ropeLightColor);
                    Draw.Line(value4 + offset, value4 - vector * 2f + offset, (colorOverride != null) ? colorOverride.Value : ZipMover.ropeLightColor);
                }
                this.cog.DrawCentered(this.from + offset, (colorOverride != null) ? colorOverride.Value : Color.White, 1f, rotation);
                this.cog.DrawCentered(this.to + offset, (colorOverride != null) ? colorOverride.Value : Color.White, 1f, rotation);
            }
        }
    }

}
