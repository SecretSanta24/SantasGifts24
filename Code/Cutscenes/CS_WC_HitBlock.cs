using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Celeste.Mod.NeutronHelper;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SantasGifts24.Code.Cutscenes
{
    [CustomEvent("SS2024/WC_HitBlock")]
    public class CS_WC_HitBlock : CutsceneEntity
    {
        public CS_WC_HitBlock(EventTrigger trigger, Player player, string eventID) : base(true, false)
        {
            this.trigger = trigger;
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            monos = Scene.Tracker.GetEntities<Monopticon>();
            if (player.Speed.Y == -160f && !player.DashAttacking) // wallbouncing, i hope!
            {
                Add(new Coroutine(Cutscene(level), true));
            }
            else
            {
                didntHappen = true;
                EndCutscene(level, true);
            }
        }

        private IEnumerator Cutscene(Level level)
        {
            level.Session.SetFlag("06_cs", true);

            var distance = player.X - trigger.X;
            if (distance < 16)
            {
                distance = 16;
                level.Session.SetFlag("06_csblock1", true);
            }
            else if (distance < 24)
            {
                distance = 24;
                level.Session.SetFlag("06_csblock2", true);
            }
            else
            {
                distance = 32;
                level.Session.SetFlag("06_csblock3", true);
            }

            Engine.TimeRate = 0.1f;
            Tween musicFade = Tween.Create(Tween.TweenMode.Oneshot, Ease.Linear, 0.1f, true);
            musicFade.OnUpdate = (Tween t) =>
            {
                Audio.SetMusicParam("fade", 1 - t.Eased);
            };
            Add(musicFade);
            Audio.Play("event:/game/03_resort/suite_bad_mirrorbreak");
            Add(new Coroutine(level.ZoomTo(new Vector2(trigger.X + distance - level.Camera.Left, 64), 2f, 0.08f)));
            float timer = 0.1f; // equal to 1 accounting for timescale
            while (player.Speed.Y < 0f)
            {
                timer -= Engine.DeltaTime;
                yield return null;
            }

            Monopticon mono1 = monos.First() as Monopticon;
            mono1.RemoveSelf();
            yield return timer;
            level.SnapColorGrade("SS2024/WatchtowerContest/WizardEyes/spirit");
            level.Lighting.Alpha = 0.02f;

            player.StateMachine.State = Player.StDummy;
            player.ForceCameraUpdate = true;
            player.DummyGravity = false;
            playerFalling = true;
            level.Session.SetFlag("06_gp", false);
            Engine.TimeRate = 1f;
            Add(new Coroutine(level.ZoomBack(0.5f)));
            yield return 0.01f; // slight delay so that the music fade can be reset

            Audio.SetMusic("event:/vitellary/wizardeyes_beat");
            Audio.SetMusicParam("fade", 1);
            level.Session.Audio.Music.Event = "event:/vitellary/wizardeyes_beat";

            float startX = player.X;
            float moveDist = (744f + level.Bounds.Left) - startX;
            Tween movemaddy = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 1f, true);
            movemaddy.OnUpdate = (Tween t) =>
            {
                player.X = startX + (moveDist * t.Eased);
                level.CameraOffset.Y = 64 * t.Eased;
            };
            Add(movemaddy);
            yield return 1f;

            player.Facing = Facings.Right;
            yield return 0.2f;

            level.NextColorGrade("SS2024/WatchtowerContest/WizardEyes/spiritDown", 0.5f);
            while (!player.onGround)
            {
                yield return null;
            }
            playerFalling = false;
            Engine.TimeRate = 0.65f;
            player.DummyAutoAnimate = false;
            player.Sprite.Play("roll");
            player.Play("event:/char/madeline/mirrortemple_big_landing");
            player.Speed = new Vector2(60f, 0f);
            player.DummyFriction = false;
            while (player.Speed.X > 0)
            {
                player.Speed.X = Calc.Approach(player.Speed.X, 0f, 90f * Engine.DeltaTime);
                if (player.Speed.X != 0f && Scene.OnInterval(0.1f))
                {
                    Dust.BurstFG(player.Position, -1.5707964f, 2);
                }
                yield return null;
            }
            player.Speed.X = 0f;
            player.DummyFriction = true;
            player.DummyGravity = true;
            yield return 0.25f;
            while (Engine.TimeRate < 1f)
            {
                Engine.TimeRate = Calc.Approach(Engine.TimeRate, 1f, 4f * Engine.DeltaTime);
                yield return null;
            }
            yield return 0.6f;
            player.Sprite.Play("rollGetUp");
            yield return 1.5f;
            yield return player.Sprite.PlayRoutine("wakeUpQuick");
            EndCutscene(level, true);
            yield break;
        }

        public override void Update()
        {
            base.Update();
            if (playerFalling)
            {
                player.Speed.Y = Calc.Approach(player.Speed.Y, 240f, 1000f * Engine.DeltaTime);
            }
        }

        public override void OnEnd(Level level)
        {
            if (!didntHappen)
            {
                if (WasSkipped)
                {
                    player.X = 4763f;
                    player.Y = 816f;
                    level.Session.SetFlag("06_gp", false);
                    level.Lighting.Alpha = 0.02f;
                    level.SnapColorGrade("SS2024/WatchtowerContest/WizardEyes/spiritDown");
                    level.CameraOffset.Y = 64f;
                    level.Camera.Position = player.CameraTarget;

                    if (monos.Count > 1)
                    {
                        monos[0].RemoveSelf();
                    }
                }
                level.ZoomSnap(new Vector2(160, 90), 1f);
                level.Session.SetFlag("06_cs", false);
                level.Session.SetFlag("06_gp2", true);
                player.StateMachine.State = Player.StNormal;
                player.ForceCameraUpdate = false;
                // player.Facing = Facings.Left;
            }
        }

        private Player player;
        private Trigger trigger;
        private List<Entity> monos;
        private bool didntHappen;
        private bool playerFalling;

        private void Log(string str)
        {
            Logger.Log(LogLevel.Warn, str, "WizardEyes");
        }
    }
}
