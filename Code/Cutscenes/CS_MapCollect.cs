using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod;
using System.Collections;
using System.Reflection;
using Celeste.Mod.Entities;
using MonoMod.Utils;
using Celeste.Mod.SantasGifts24.Code.Entities;
using Celeste.Mod.SantasGifts24;


namespace Celeste.Mod.SantasGifts24.Code.Cutscenes
{
    public class CS_MapCollect : CutsceneEntity
    {
        private class CollectableUI : Entity
        {
            private Image ZiplineImage;
            private float alpha;
            private string tutorialBig;
            private string tutorial;
            private string tutorial2;
            private MTexture controlGUI;

            public CollectableUI()
            {
                Tag = Tags.HUD;
                alpha = 0;
                controlGUI = Input.GuiButton(SantasGiftsModule.Settings.MiniMapShow.Button,"controls/keyboard/oemquestion");
                tutorial = "It hardly resembles the current state of the area...but it may still prove useful.";
                tutorial2 = "to view the map. You can change bindings in Mod Options."; // TODO: change L2 to be dynamic
                tutorialBig = "Area Blueprint";
                Add(ZiplineImage = new Image(GFX.Gui["notes/SS2024/ricky06/blueprint_small"]));
                ZiplineImage.Position = new Vector2(710f, 120f);
                ZiplineImage.Color = Color.White * alpha;
            }

            public IEnumerator FadeIn()
            {
                alpha = 0f;
                for (int i = 1; i < 26; ++i)
                {
                    alpha = i * 0.04f;
                    ZiplineImage.Color = Color.White * alpha;
                    yield return null;
                }
            }

            public IEnumerator FadeOut()
            {
                alpha = 1f;
                for (int i = 25; i >= 0; --i)
                {
                    alpha = i * 0.04f;
                    ZiplineImage.Color = Color.White * alpha;
                    yield return null;
                }
            }

            public override void Render()
            {
                base.Render();
                ActiveFont.Draw(tutorialBig, new Vector2(710f, 600f), new Vector2(0f, 0f), new Vector2(1.5f, 1.5f), Color.White * alpha);
                ActiveFont.Draw(tutorial, new Vector2(240f, 720f), new Vector2(0f, 0f), new Vector2(0.8f, 0.8f), Color.Gray * alpha);
                ActiveFont.Draw(tutorial2, new Vector2(440f, 790f), new Vector2(0f, 0f), new Vector2(0.8f, 0.8f), Color.Gray * alpha);
                controlGUI.Draw(new Vector2(360f, 780f), Vector2.Zero, Color.White * alpha);
            }
        }
        private Level level;
        private Player player;
        private CollectableUI zip;

        public CS_MapCollect()
        {
            Tag = Tags.HUD;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene()));
        }

        private IEnumerator Cutscene()
        {
            while (player == null)
            {
                player = Scene.Tracker.GetEntity<Player>();
                if (player != null)
                {
                    break;
                }
                yield return null;
            }

            player.StateMachine.State = 11;
            player.DummyAutoAnimate = true;
            player.ForceCameraUpdate = true;

            while (!player.OnGround() || player.Speed.Y < 0f)
            {
                yield return null;
            }
            Add(new Coroutine(ScrollCamera()));
            yield return player.DummyWalkTo(1896f);
            yield return 1f;
            player.DummyAutoAnimate = false;
            player.Sprite.Play("duck");
            yield return 1f;

            Audio.Play("event:/game/06_reflection/hug_levelup_text_in");
            level.FormationBackdrop.Display = true;
            level.FormationBackdrop.Alpha = 1f;
            yield return 0.2f;
            zip = new CollectableUI();
            Scene.Add(zip);
            yield return zip.FadeIn();

            while (!Input.MenuConfirm.Pressed && !Input.MenuCancel.Pressed)
            {
                yield return null;
            }
            Audio.Play("event:/ui/main/button_lowkey");
            yield return zip.FadeOut();
            yield return level.ZoomBack(0.5f);
            EndCutscene(level);
        }

        private IEnumerator ScrollCamera()
        {
            yield return level.ZoomTo(new Vector2(160f, 124f), 2f, 2f);
        }

        public override void OnEnd(Level level)
        {
            if (player != null)
            {
                //player.StateMachine.Locked = false;
                player.StateMachine.State = 0;
                player.Speed.Y = 0f;
                while (player.CollideCheck<Solid>())
                {
                    player.Y--;
                }
                player.DummyAutoAnimate = true;
                player.DummyGravity = true;
                player.ForceCameraUpdate = false;
            }
            Scene.Remove(zip);
            level.ResetZoom();
            level.FormationBackdrop.Display = false;
            level.Session.SetFlag("SS2024_ricky06_map_obtained", true);
        }
    }
}
