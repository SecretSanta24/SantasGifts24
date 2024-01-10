using Celeste.Mod.Entities;
using Celeste.Mod.SantasGifts24.Triggers;
using IL.Celeste;
using IL.MonoMod;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Monocle;
using On.Celeste;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using static Celeste.MoonGlitchBackgroundTrigger;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    // by Aurora Aquir
    [CustomEntity("SS2024/ObeliskFightController")]
    public class ObeliskFightController : Entity
    {
        public static bool trueEvil = false;

        //straight from my unreleased bullet helper lmao
        [Pooled]
        [Tracked(false)]
        public class Bullet : Entity
        {
            public Color OutlineColor = Color.Black;
            public Sprite sprite;
            public PlayerCollider pc;
            private bool dead;
            private float appearTimer;
            private Vector2 anchor; // curr pos
            private Vector2 target;

            private Vector2 speed;
            private Level level;

            // if it was in camera and leaves again, remove
            private bool hasBeenInCamera;

            // This is for sine wavy movement of the bullets.. not really useful for us
            //private Vector2 perp;
            private float particleDir;
            public Bullet() : base(Vector2.Zero)
            {
                base.Add(this.sprite = GFX.SpriteBank.Create("badeline_projectile"));
                base.Collider = new Hitbox(4f, 4f, -2f, -2f);
                base.Add(pc = new PlayerCollider(new Action<Player>(this.OnPlayer), null, null));
                base.Depth = -1000000;
                //base.Add(this.sine = new SineWave(1.4f, 0f));
            }

            // Token: 0x060015BE RID: 5566 RVA: 0x00059288 File Offset: 0x00057488
            public Bullet Init(Vector2 from, Vector2 target)
            {
                this.anchor = (this.Position = from);
                this.target = target;
                //this.angleOffset = angleOffset;
                this.dead = (this.hasBeenInCamera = false);
                this.appearTimer = 0.1f;
                //this.sine.Reset();
                //this.sineMult = 0f;
                this.sprite.Play("charge", true, false);
                this.InitSpeed();
                return this;
            }
            private void InitSpeed()
            {
                this.speed = (this.target - base.Center).SafeNormalize(100f);
                this.particleDir = (-this.speed).Angle();
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                this.level = base.SceneAs<Level>();
            }

            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                this.level = null;
            }

            public override void Update()
            {
                base.Update();
                if (this.appearTimer > 0f)
                {
                    this.Position = this.anchor;
                    this.appearTimer -= Engine.DeltaTime;
                    return;
                }
                this.anchor += this.speed * Engine.DeltaTime;
                this.Position = this.anchor; //+ this.perp * this.sineMult * this.sine.Value * 3f;
                                             //this.sineMult = Calc.Approach(this.sineMult, 1f, 2f * Engine.DeltaTime);
                if (!this.dead)
                {
                    bool isInCamera = this.level.IsInCamera(this.Position, 8f);

                    if (isInCamera && !this.hasBeenInCamera)
                    {
                        this.hasBeenInCamera = true;
                    }
                    else if (!isInCamera && this.hasBeenInCamera || !this.level.IsInBounds(this))
                    {
                        this.Destroy();
                    }
                    // Consider for removal (or option) if it clutters screen.
                    if (base.Scene.OnInterval(0.04f))
                    {
                        this.level.ParticlesFG.Emit(FinalBossShot.P_Trail, 1, base.Center, Vector2.One * 2f, this.particleDir);
                    }
                }
            }

            public override void Render()
            {
                Color color = this.sprite.Color;
                Vector2 position = this.sprite.Position;
                this.sprite.Color = OutlineColor;
                this.sprite.Position = position + new Vector2(-1f, 0f);
                this.sprite.Render();
                this.sprite.Position = position + new Vector2(1f, 0f);
                this.sprite.Render();
                this.sprite.Position = position + new Vector2(0f, -1f);
                this.sprite.Render();
                this.sprite.Position = position + new Vector2(0f, 1f);
                this.sprite.Render();
                this.sprite.Color = color;
                this.sprite.Position = position;
                base.Render();
            }

            public void Destroy()
            {
                this.dead = true;
                base.RemoveSelf();
            }

            //this.level.IsInCamera(this.Position, 8f);
            private void OnPlayer(Player player)
            {
                if (!this.dead)
                {
                    player.Die((player.Center - this.Position).SafeNormalize(), false, true);
                }
            }
        }

        [Pooled]
        public class Shot : Bullet
        {

            public Shot() : base()
            {
                base.Remove(base.pc);
                base.Collider = new Circle(6f);
            }

            public new Shot Init(Vector2 from, Vector2 to)
            {
                base.Init(from, to);
                sprite.Color = Color.DarkCyan;
                base.OutlineColor = Color.White;

                return this;
            }

            public override void Update()
            {
                base.Update();
                DustStaticSpinner dss = base.CollideFirst<DustStaticSpinner>();
                if(dss != null)
                {
                    if(trueEvil)
                    {
                        Level level = Engine.Scene as Level;
                        if(level != null)
                        {
                            level.Add(new DustStaticSpinner(this.Position, false));
                        }
                    }
                    RemoveSelf();
                    return;
                }
                Boss boss = base.CollideFirst<Boss>();
                if (boss != null)
                {
                    boss.Hit();
                    RemoveSelf();
                }
            }
        }

        public class Strike : Entity
        {
            private SoundSource laserSfx;
            Sprite shot;
            bool finishedShot = false;
            int linger;
            Player player;
            float targetingSpeed;
            bool doNotMove = false;
            public Strike() : base(Vector2.Zero)
            {

            }
           
            public Strike Init(Player player, float duration, int lingerFrames, float targetingSpeed, Vector2? forcePosition = null)
            {
                if (player == null) return null;
                Position = player.Center;
                base.Add(this.laserSfx = new SoundSource());
                if (forcePosition != null)
                {
                    doNotMove = true;
                    Position = (Vector2)forcePosition;
                }
                base.Add(shot = new Sprite(GFX.Game, "decals/ssc24auroraaquir/obeliskactivation"));
                shot.Add("animate", "", duration / 18f);
                this.shot.OnFinish = delegate (string anim)
                {
                    finishedShot = true;
                    Collidable = true;
                    this.laserSfx.Stop(true);
                    Audio.Play("event:/char/badeline/boss_laser_fire", this.Position);
                    shot.Color = Color.Red;
                };
                shot.CenterOrigin();
                shot.Play("animate");
                shot.Visible = true;
                Depth = Depths.FGDecals;

                Collider = new Circle(26, 0f, 0f);
                Collidable = false;
                this.player = player;
                this.linger = lingerFrames;
                this.targetingSpeed = targetingSpeed;
                base.Add(new PlayerCollider(OnPlayer));
                this.laserSfx.Play("event:/char/badeline/boss_laser_charge", null, 0f);
                return this;
            }

            public void OnPlayer(Player player)
            {
                player.Die(player.Position - this.Position);
            }

            public override void Update()
            {
                base.Update();
                if(!finishedShot)
                {
                    if(!doNotMove) this.Position = Calc.Approach(this.Position, player.Center, targetingSpeed);
                } else
                {
                    if (linger <= 0) RemoveSelf();
                    else linger--;
                }
            }
        }
        [Tracked]
        public class Boss : Entity
        {
            public int health = 5;
            Image image;
            public Boss(Vector2 position, string imagestr) : base(position)
            {
                base.Add(image = new Image(GFX.Game[imagestr]));
                image.CenterOrigin();
                Collider = new Circle(image.Width/2);
                base.Add(new PlayerCollider(OnPlayer));
            }

            public void OnPlayer(Player player)
            {
                player.Die(player.Position-this.Position);
            }

            int hitShowCount = 0;
            public bool disabled = false;
            public override void Update()
            {
                base.Update();
                if(hitShowCount > 0)
                {
                    image.Color = Color.Red;
                    hitShowCount--;
                }
                if(hitShowCount == 0)
                {
                    hitShowCount = -1;
                    if (disabled) image.Color = Color.Gray;
                    else image.Color = Color.White;
                }
            }

            public void Hit()
            {
                bool hit = true;
                if (hitShowCount > 0) hit = false;
                hitShowCount = 11;
                if (health <= 0)
                {
                    Level level = Engine.Scene as Level;
                    if (level == null) return;
                    Player player = level.Tracker.GetEntity<Player>();
                    if (player == null) return;
                    level.Add(Engine.Pooler.Create<Strike>().Init(player, 1f, 11, 0.3F));
                    return;
                }
                if(hit || !trueEvil) health--;
                Audio.Play("event:/char/badeline/boss_idle_air", this.Position);
                if(health <= 0)
                {
                    Disable();
                }
            }

            public void Disable()
            {
                disabled = true;
            }

            public void Enable()
            {
                disabled = false;
                health = 5;
            }
        }

        Level level;
        static bool alreadySeenTheIntro = false;
        public ObeliskFightController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            trueEvil = data.Bool("trueEvil", false);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            level = (scene as Level);
            base.Add(new Coroutine(Fight()));
        }

        int cooldown = 11;
        bool canShoot = false;
        public override void Update()
        {
            base.Update();
            if(cooldown > 0) cooldown--;
            if(canShoot && Input.Dash.Pressed && cooldown <= 0)
            {
                Input.Dash.ConsumePress();
                cooldown = (trueEvil ? 0 : 11);
                Player player = level.Tracker.GetEntity<Player>();
                if (player == null) return;
                Vector2 dir = new Vector2(player.Facing == Facings.Left ? -1 : 1, 0);
                level.Add(Engine.Pooler.Create<Shot>().Init(player.Center + dir * 10, player.Center + dir * 11));
            }
        }

        public IEnumerator Fight()
        {
            yield return 0;
            float rotation = 0;
            Vector2 distance = new(0, 0); // max ~ 80?
            Player player = level.Tracker.GetEntity<Player>();
            Vector2 center = (trueEvil ? player.Center : new(1576, -732));

            Boss logic;
            Boss reason;
            Boss rationale;
            level.Add(logic = new Boss(player.Position, "decals/ssc24auroraaquir/logic"));
            level.Add(reason = new Boss(player.Position, "decals/ssc24auroraaquir/reason"));
            level.Add(rationale = new Boss(player.Position, "decals/ssc24auroraaquir/rationale"));

            List<DustStaticSpinner> spinners = new List<DustStaticSpinner>();

            void SetPositionBasedOnRotation(float rotation)
            {
                logic.Position = center + distance.Rotate(rotation.ToRad());
                reason.Position = center + distance.Rotate((120f + rotation).ToRad());
                rationale.Position = center + distance.Rotate((240f + rotation).ToRad());
            }
            void shootStrike(float targetingSpeed = 0.5f, float duration = 1f, int lingerFrames = 11, Vector2? forcePosition = null)
            {
                level.Add(Engine.Pooler.Create<Strike>().Init(player, duration, lingerFrames, targetingSpeed, forcePosition));
            }

            void shootBulletFrom(Vector2 from)
            {
                level.Add(Engine.Pooler.Create<Bullet>().Init(from, player.Center));
            }


            if (!alreadySeenTheIntro)
            {
                player.StateMachine.State = Player.StDummy;
                player.DummyGravity = false;
                player.DummyAutoAnimate = false;
                player.Speed = Vector2.Zero;

                logic.Collidable = false;
                reason.Collidable = false;
                rationale.Collidable = false;


                for (int i = 1; i <= 90; i++)
                {
                    SetPositionBasedOnRotation(rotation);
                    distance = new Vector2(0, -1 * i);
                    yield return 0;
                }
                MiniTextbox tb = null;
                for (int i = 1; i <= 360; i++)
                {
                    rotation = (rotation + 2) % 360;
                    if (rotation % 10 == 0 && i <= 60)
                    {
                        DustStaticSpinner ds;
                        level.Add(ds = new DustStaticSpinner(logic.Position, false));
                        spinners.Add(ds);
                        level.Add(ds = new DustStaticSpinner(reason.Position, false));
                        spinners.Add(ds);
                        level.Add(ds = new DustStaticSpinner(rationale.Position, false));
                        spinners.Add(ds);
                    }
                    else if (rotation % 5 == 0 && i > 60 && distance.Y < -75)
                    {
                        distance.Y += 1;
                    }
                    if (i == 55) level.Add(tb = new MiniTextbox("SecretSanta2024_auroraaquir_overworld_fight"));
                    SetPositionBasedOnRotation(rotation);
                    yield return 0;
                }
                tb?.RemoveSelf();
                logic.Collidable = true;
                reason.Collidable = true;
                rationale.Collidable = true;

                player.StateMachine.State = Player.StNormal;
            } else
            {

                distance = new Vector2(0, -90);
                for (int i = 0; i <= 12; i++)
                {
                    SetPositionBasedOnRotation(rotation);
                    rotation += 10;
                    DustStaticSpinner ds;
                    level.Add(ds = new DustStaticSpinner(logic.Position, false));
                    spinners.Add(ds);
                    level.Add(ds = new DustStaticSpinner(reason.Position, false));
                    spinners.Add(ds);
                    level.Add(ds = new DustStaticSpinner(rationale.Position, false));
                    spinners.Add(ds);
                }
                distance = new Vector2(0, -75);
                rotation = 0;
                SetPositionBasedOnRotation(rotation);
            }
            alreadySeenTheIntro = true;
            canShoot = true;
            int logicAttackCounter = 250;
            int reasonAttackCounter = 150;
            int rationaleAttackCounter = 50;

            int logicAttackNr = 300;
            int reasonAttackNr = 300;
            int rationaleAttackNr = 300;
            while (logic.health + reason.health + rationale.health > 10)
            {
                rotation = (rotation + 2) % 360;
                SetPositionBasedOnRotation(rotation);

                if (logicAttackCounter >= logicAttackNr)
                {
                    logicAttackCounter = 0;
                    shootBulletFrom(logic.Position);
                }
                if (reasonAttackCounter >= reasonAttackNr)
                {
                    reasonAttackCounter = 0;
                    shootBulletFrom(reason.Position);
                }
                if (rationaleAttackCounter >= rationaleAttackNr)
                {
                    rationaleAttackCounter = 0;
                    shootBulletFrom(rationale.Position);
                }
                logicAttackCounter++;
                reasonAttackCounter++;
                rationaleAttackCounter++;
                yield return 0;
            }


            logicAttackCounter = 133;
            reasonAttackCounter = 66;
            rationaleAttackCounter = 0;
             
            shootStrike(0.5f, 2f);
            logicAttackNr = 200;
            reasonAttackNr = 200;
            rationaleAttackNr = 200;
            while (logic.health + reason.health + rationale.health > 5)
            {
                rotation = (rotation + 2) % 360;
                SetPositionBasedOnRotation(rotation);

                if (logicAttackCounter >= logicAttackNr)
                {
                    logicAttackCounter = 0;
                    if(trueEvil || !logic.disabled) shootBulletFrom(logic.Position);
                }
                if (reasonAttackCounter >= reasonAttackNr)
                {
                    reasonAttackCounter = 0;
                    if (trueEvil || !reason.disabled) shootBulletFrom(reason.Position);
                }
                if (rationaleAttackCounter >= rationaleAttackNr)
                {
                    rationaleAttackCounter = 0;
                    if (trueEvil || !rationale.disabled) shootBulletFrom(rationale.Position);
                }
                logicAttackCounter++;
                reasonAttackCounter++;
                rationaleAttackCounter++;
                yield return 0;
            }

            logicAttackCounter = 66;
            reasonAttackCounter = 33;
            rationaleAttackCounter = 0;
            int strikeCounter = 400;
            int strikeAttackNr = 400;
            logicAttackNr = 100;
            reasonAttackNr = 100;
            rationaleAttackNr = 100;
            while (logic.health + reason.health + rationale.health > 0)
            {
                rotation = (rotation + 2) % 360;
                SetPositionBasedOnRotation(rotation);

                if (strikeCounter >= strikeAttackNr)
                {
                    strikeCounter = 0;
                    shootStrike(0.5f, 2f);
                }
                if (logicAttackCounter >= logicAttackNr)
                {
                    logicAttackCounter = 0;
                    if (trueEvil || !logic.disabled) shootBulletFrom(logic.Position);
                }
                if (reasonAttackCounter >= reasonAttackNr)
                {
                    reasonAttackCounter = 0;
                    if (trueEvil || !reason.disabled) shootBulletFrom(reason.Position);
                }
                if (rationaleAttackCounter >= rationaleAttackNr)
                {
                    rationaleAttackCounter = 0;
                    if (trueEvil || !rationale.disabled) shootBulletFrom(rationale.Position);
                }
                logicAttackCounter++;
                reasonAttackCounter++;
                rationaleAttackCounter++;
                strikeCounter++;
                yield return 0;
            }
            for (int i = 0; i < 60; i++)
            {
                rotation = (rotation + 3) % 360;
                SetPositionBasedOnRotation(rotation);
                reason.RemoveSelf();

                if (i == 0) shootStrike(0.5f, 1f, 50);
                if (i == 10) shootStrike(0.5f, 1f, 40);
                if (i == 20) shootStrike(0.5f, 1f, 30);
                if (i == 30) shootStrike(0.5f, 1f, 20);
                if (i == 40) shootStrike(0.5f, 1f, 10);
                yield return 0.02f;
            }

            for (int i = 0; i < 60; i++)
            {
                rotation = (rotation + 2) % 360;
                SetPositionBasedOnRotation(rotation);
                logic.RemoveSelf();

                if (i == 0) shootStrike(0.8f, 1f, 20, center);
                if (i == 30) {
                    shootStrike(0.8f, 1f, 20, center + new Vector2(40, 0));
                    shootStrike(0.8f, 1f, 20, center + new Vector2(-40, 0));
                    shootStrike(0.8f, 1f, 20, center + new Vector2(0, 40));
                    shootStrike(0.8f, 1f, 20, center + new Vector2(0, -40));
                }
                yield return 0.02f;
            }

            for (int i = 0; i < 60; i++)
            {
                rotation = (rotation + 2) % 360;
                SetPositionBasedOnRotation(rotation);
                rationale.RemoveSelf();

                if (i == 5) shootStrike(0.8f, 0.5f, 10, center + new Vector2(-40, -40));
                if (i == 10) shootStrike(0.8f, 0.5f, 10, center + new Vector2(0, -40));
                if (i == 15) shootStrike(0.8f, 0.5f, 10, center + new Vector2(40, -40));
                if (i == 20) shootStrike(0.8f, 0.5f, 10, center + new Vector2(-40, 0));
                if (i == 25) shootStrike(0.8f, 0.5f, 10, center + new Vector2(0, 0));
                if (i == 30) shootStrike(0.8f, 0.5f, 10, center + new Vector2(40, 0));
                if (i == 35) shootStrike(0.8f, 0.5f, 10, center + new Vector2(-40, 40));
                if (i == 40) shootStrike(0.8f, 0.5f, 10, center + new Vector2(0, 40));
                if (i == 45) shootStrike(0.8f, 0.5f, 10, center + new Vector2(40, 40));
                yield return 0.02f;
            }

            yield return 0.5f;
            for (int i = 0; i < 60; i++)
            {
                shootStrike(0.1f, 2f, 10, player.Center);
                yield return 0;
            }

            for (int i = 0; i <=12; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    if (spinners.Count <= 0) continue;
                    DustStaticSpinner ds = spinners.First();
                    shootBulletFrom(ds.Position);
                    ds.RemoveSelf();
                    spinners.RemoveAt(0);
                }
                if (trueEvil) shootStrike(0.3f, 2f, 10);
                if (i < 6) yield return 1f;
                else yield return 0.5f;
            }

            yield return 0.5f;
            shootStrike(500f, 3.018f, 10);
            yield return 3f; 
            level.OnEndOfFrame += delegate ()
            {
                level.TeleportTo(level.Tracker.GetEntity<Player>(), "the_end", Player.IntroTypes.Respawn);
            };
            yield break;
        }
    }

}
