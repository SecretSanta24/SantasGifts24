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
    [CustomEvent("SS2024/WC_Mono1")]
    public class CS_WC_Mono1 : CutsceneEntity
    {
        public CS_WC_Mono1(EventTrigger trigger, Player player, string eventID) : base(true, false)
        {
            this.trigger = trigger;
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
            if (player.X < -408)
            {
                yield return player.DummyWalkTo(-405, false, 0.8f);
            }
            else
            {
                yield return player.DummyWalkTo(-411, false, 0.8f);
            }
            yield return 0.01f;
            player.Speed.X = 0f;
            player.X = -408;
            yield return 0.5f;
            yield return level.ZoomTo(new Vector2(160, 68), 1.5f, 1.5f);
            mono.Interact(player);
            level.Session.SetFlag("binoUpdate", true);
            yield return 1f;
            Tween bloom = Tween.Create(Tween.TweenMode.YoyoOneshot, Ease.Linear, 0.25f, true);
            bloom.OnUpdate = (Tween t) =>
            {
                level.Bloom.Base = 0.5f * t.Eased;
            };
            Add(bloom);
            yield return 0.25f;
            Audio.Play("event:/game/05_mirror_temple/mainmirror_torch_lit_2");
            level.Session.SetFlag("hasBino", true);
            level.Session.Audio.Music.Event = "event:/vitellary/wizardeyes1";
            level.Session.Audio.Music.Layer(1, true); // idk why i have to specify this?
            level.Session.Audio.Music.Layer(2, true);
            level.Session.Audio.Apply();
            yield return 2f;
            yield return Textbox.Say("SS2024_WatchtowerContest_WizardEyes_power", new Func<IEnumerator>[0]);
            yield return level.ZoomBack(1f);
            EndCutscene(level);
            yield break;
        }

        public override void OnEnd(Level level)
        {
            if (WasSkipped && level.Session.Audio.Music.Event != "event:/vitellary/wizardeyes1")
            {
                level.Session.Audio.Music.Event = "event:/vitellary/wizardeyes1";
                level.Session.Audio.Music.Layer(1, true);
                level.Session.Audio.Music.Layer(2, true);
                level.Session.Audio.Apply();
            }
            player.X = -408;
            player.StateMachine.State = Player.StNormal;
            player.ForceCameraUpdate = false;
            level.Session.SetFlag("binoUpdate", true);
            level.Session.SetFlag("hasBino", true);
            level.Session.SetFlag("binoAppear", false);
        }

        private Player player;
        private Trigger trigger;
        private Monopticon mono;
    }
}
