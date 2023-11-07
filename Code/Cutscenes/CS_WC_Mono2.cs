using Celeste.Mod.Entities;
using Celeste.Mod.NeutronHelper;
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
    [CustomEvent("SS2024/WC_Mono2")]
    public class CS_WC_Mono2 : CutsceneEntity
    {
        public CS_WC_Mono2(EventTrigger trigger, Player player, string eventID) : base(true, false)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            mono = Scene.Tracker.GetEntity<Monopticon>();
            Add(new Coroutine(Cutscene(level), true));
        }

        private IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = Player.StDummy;
            player.ForceCameraUpdate = true;
            level.Session.SetFlag("slowWalk", false);
            if (player.X < 4424)
            {
                yield return player.DummyWalkTo(4427, false, 0.6f);
            }
            else
            {
                yield return player.DummyWalkTo(4421, false, 0.6f);
            }
            yield return 0.01f;
            player.Speed.X = 0f;
            player.X = 4424;
            yield return 0.5f;
            yield return level.ZoomTo(new Vector2(160, 84), 1.5f, 1.5f);
            yield return Textbox.Say("SS2024_WatchtowerContest_WizardEyes_power2", new Func<IEnumerator>[0]);
            yield return 1f;
            mono.Interact(player);
            level.Session.SetFlag("binoUpdate2", true);
            yield return 1f;
            // level.SnapColorGrade("feelingdown");
            level.Lighting.Alpha = 0.2f;
            Audio.SetMusic(null);
            level.Session.SetFlag("hasBino2", true);
            yield return 2f;
            yield return Textbox.Say("SS2024_WatchtowerContest_WizardEyes_power3", new Func<IEnumerator>[0]);
            Audio.SetMusic("event:/vitellary/wizardeyes2");
            Audio.SetMusicParam("layer1", 1f);
            level.Session.Audio.Music.Event = "event:/vitellary/wizardeyes2";
            level.Session.Audio.Music.Layer(1, true);
            yield return level.ZoomBack(1f);
            EndCutscene(level);
            yield break;
        }

        public override void OnEnd(Level level)
        {
            player.StateMachine.State = Player.StNormal;
            player.ForceCameraUpdate = false;
            player.X = 4424;
            level.Session.SetFlag("slowWalk", false);
            level.Session.SetFlag("binoUpdate2", true);
            level.Session.SetFlag("hasBino2", true);

            level.Session.SetFlag("spiritRealm", true);
        }

        private Player player;
        private Monopticon mono;
    }
}
