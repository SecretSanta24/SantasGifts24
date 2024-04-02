using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    [CustomEntity("SantasGifts24/ObeliskOpening")]
    public class ObeliskOpening : Entity
    {
        public class Orb : Entity
        {
            public Orb(Vector2 position, string imagestr) : base (position)
            {
                Image image;
                base.Add(image = new Image(GFX.Game[imagestr]));
                image.CenterOrigin();
            }
        }

        Vector2? target;
        Coroutine coroutine;
        Level level;
        Session session;
        Orb logic;
        Orb reason;
        Orb rationale;
        Sprite obiliskAnimation;
        bool teleport = false;

        public ObeliskOpening(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            target = (data.FirstNodeNullable(offset) ?? data.Position + offset);
            Depth = Depths.BGDecals-1;
            base.Collider = new Hitbox(data.Width, data.Height);
            base.Add(new PlayerCollider(onPlayer, null, null));
        }


        int wait = 200;

        public void onPlayer(Player player)
        {

            if (session == null) session = (Engine.Scene as Level).Session;

            if (session.GetFlag("logic_obtained")
             && session.GetFlag("reason_obtained")
             && session.GetFlag("rationale_obtained")
             && session.GetFlag("obelisk_method"))
            {
                if(Center.X-6 < player.X && player.X < Center.X+6)
                {
                    base.Add(coroutine = new Coroutine(Animation(), true));
                    this.Collidable = false;
                }
            } else
            {
                wait--;
                if(wait < 0)
                {
                    base.Scene.Add(new MiniTextbox("SecretSanta2024_auroraaquir_overworld_incomplete"));
                    this.Collidable = false;
                }
            }

        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            level = SceneAs<Level>();

            if (level == null || target == null)
            {
                RemoveSelf();
            }
            session = level.Session;
        }

        public override void Update()
        {
            base.Update();

            if (teleport) level.OnEndOfFrame += delegate ()
            {
                level.TeleportTo(level.Tracker.GetEntity<Player>(), "the_fight", Player.IntroTypes.Respawn);
            };

        }
        private IEnumerator Animation()
        {
            SoundSource sfx;
            Vector2 center = new(1560, -177);
            base.Add(sfx = new SoundSource());
            sfx.Play("event:/music/lvl5/mirror_cutscene", null, 0f);
            sfx.Position = center;
            Player player = level.Tracker.GetEntity<Player>();
            player.StateMachine.State = Player.StDummy;
            player.DummyGravity = false;

            session.Audio.Music.Event = SFX.EventnameByHandle("event:/aurora_aquir_gun");
            session.Audio.Apply(false);
            level.Add(logic = new Orb(player.Position, "decals/SS2024/auroraaquir/logic"));
            level.Add(reason = new Orb(player.Position, "decals/SS2024/auroraaquir/reason"));
            level.Add(rationale = new Orb(player.Position, "decals/SS2024/auroraaquir/rationale"));

            Vector2 goalLogic = new(1513, -204);
            Vector2 goalReason = new(1606, -204);
            Vector2 goalRationale = new(1560, -123);
            while(logic.Position != goalLogic || reason.Position != goalReason || rationale.Position != goalRationale) { 
                logic.Position = Calc.Approach(logic.Position, goalLogic, 1f);
                reason.Position = Calc.Approach(reason.Position, goalReason, 1f);
                rationale.Position = Calc.Approach(rationale.Position, goalRationale, 0.25f);
                yield return 0;
            }

            yield return 0.5f;

            //Vector2 distance = new(0, -19);

            goalLogic = new(1560, -196);
            goalReason = new(1576, -168);
            goalRationale = new(1544, -168);
            
            float goalAngleLogic = 0;
            float goalAngleReason = 120f;
            float goalAngleRationale = 240f;

            float angleLogic = 300f;
            float angleReason = 60f;
            float angleRationale = 180f;
            Vector2 currDistance = new(0, -54);

            int difference = 6;
            for (int i = 0; i < 180; i++)
            {
                if(i == 60) base.Add(new Coroutine(level.ZoomTo(new Vector2(160, 70), 3f, 12f), true));
                if (i < 120) player.Position = Calc.Approach(player.Position, center, 0.5f);
                else player.Position = Calc.Approach(player.Position, center+new Vector2(0, 6f), 1f);

                logic.Position = currDistance.Rotate(angleLogic.ToRad()) + center;
                reason.Position = currDistance.Rotate(angleReason.ToRad()) + center;
                rationale.Position = currDistance.Rotate(angleRationale.ToRad()) + center;

                if (i > 120) difference = 12;
                angleLogic += difference;
                angleReason += difference;
                angleRationale += difference;
                if (i < 60) yield return 0.03; 
                    
                yield return 0;
            }
            player.Position = center + new Vector2(0, 6f);

            angleLogic = 300f;
            angleReason = 60f;
            angleRationale = 180f;

            for (int i = 0; i < 60; i++)
            {
                logic.Position = currDistance.Rotate(angleLogic.ToRad()) + center;
                reason.Position = currDistance.Rotate(angleReason.ToRad()) + center;
                rationale.Position = currDistance.Rotate(angleRationale.ToRad()) + center;

                angleLogic += 12;
                angleReason += 12;
                angleRationale += 12;
                yield return 0;
            }
            for (int i = -54; i < -18; i++)
            {
                logic.Position = currDistance.Rotate(angleLogic.ToRad()) + center;
                reason.Position = currDistance.Rotate(angleReason.ToRad()) + center;
                rationale.Position = currDistance.Rotate(angleRationale.ToRad()) + center;

                currDistance = new(0, i);
                angleLogic += 12;
                angleReason += 12;
                angleRationale += 12;
                yield return 0;
            }
            logic.Position = currDistance.Rotate(goalAngleLogic.ToRad()) + center;
            reason.Position = currDistance.Rotate(goalAngleReason.ToRad()) + center;
            rationale.Position = currDistance.Rotate(goalAngleRationale.ToRad()) + center;

            yield return 1f;
            base.Add(this.obiliskAnimation = new Sprite(GFX.Game, "decals/SS2024/auroraaquir/obeliskactivation"));
            this.obiliskAnimation.Add("animate", "", 0.45f);
            bool playing = true;
            this.obiliskAnimation.OnFinish = delegate (string anim)
            {
                playing = false;
                this.obiliskAnimation.Visible = false;
            };
            obiliskAnimation.Position = (center) - Position + new Vector2(1, 0);
            obiliskAnimation.CenterOrigin();
            obiliskAnimation.Play("animate");
              
            while (playing) yield return 0;

            player.StateMachine.State = Player.StNormal;
            teleport = true;
            yield break;
        }


    }
}