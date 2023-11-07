using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Cutscenes
{
    [CustomEvent("SS2024/WC_Broken")]
    public class CS_WC_Broken : CutsceneEntity
    {
        public CS_WC_Broken(EventTrigger trigger, Player player, string eventID) : base(true, false)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level), true));
        }

        private IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = Player.StDummy;
            player.DummyAutoAnimate = false;
            player.DummyFriction = false;
            player.ForceCameraUpdate = true;
            Audio.Play("event:/char/madeline/theo_collapse", player.Position);
            player.Sprite.Play("roll");
            player.Speed = new Vector2(-60f, 60f); // downwards to cause ground collision sfx
            while (player.Speed.X < 0)
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
            yield return 1f;
            player.Sprite.Play("rollGetUp");
            yield return 1f;
            yield return Textbox.Say("SS2024_WatchtowerContest_WizardEyes_broken", new Func<IEnumerator>[0]);
            yield return 1f;
            yield return player.Sprite.PlayRoutine("wakeUpQuick");
            EndCutscene(level);
            yield break;
        }

        public override void OnEnd(Level level)
        {
            player.StateMachine.State = Player.StNormal;
            player.ForceCameraUpdate = false;
            player.DummyAutoAnimate = true;
            level.Session.SetFlag("slowWalk", true);
        }

        private Player player;
    }
}
