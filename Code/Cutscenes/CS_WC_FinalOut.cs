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
    [CustomEvent("SS2024/WC_FinalOut")]
    public class CS_WC_FinalOut: CutsceneEntity
    {
        public CS_WC_FinalOut(EventTrigger trigger, Player player, string eventID) : base(true, false)
        {
            this.trigger = trigger;
            this.player = player;
        }
        public override void OnBegin(Level level)
        {
            if (player.StateMachine.State == Player.StRedDash)
            {
                Add(new Coroutine(Cutscene(level), true));
            }
            else
            {
                didntHappen = true;
                EndCutscene(level);
            }
        }

        private IEnumerator Cutscene(Level level)
        {
            while (player.StateMachine.State == Player.StRedDash)
            {
                yield return null;
            }
            Tween musicFade = Tween.Create(Tween.TweenMode.Oneshot, Ease.Linear, 0.1f, true);
            musicFade.OnUpdate = (Tween t) =>
            {
                Audio.SetMusicParam("fade", 1 - t.Eased);
            };
            Add(musicFade);
            yield return 3f;
            level.Session.SetFlag("cs_fadeout", true);
            yield return 3f;
            level.EndCutscene();
            new FadeWipe(level, false, delegate ()
            {
                EndCutscene(level, true);
            });
            yield break;
        }

        public override void OnEnd(Level level)
        {
            if (!didntHappen)
            {
                level.Remove(player);
                level.UnloadLevel();
                level.Session.Dreaming = false;
                level.Session.Level = "end";
                level.Session.RespawnPoint = new Vector2?(level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Bottom)));
                level.LoadLevel(Player.IntroTypes.None, false);
                level.Session.SetFlag("end_fade", true);
                level.Session.SetFlag("cs_fadeout", false);
                if (WasSkipped)
                {
                    level.SnapColorGrade("none");
                    level.Lighting.Alpha = 1f;
                }
            }
        }

        private void Log(string str)
        {
            Logger.Log(LogLevel.Warn, str, "WizardEyes");
        }

        private Player player;
        private Trigger trigger;
        private bool didntHappen;
    }
}
