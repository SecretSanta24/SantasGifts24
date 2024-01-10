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


namespace Celeste.Mod.SantasGifts24.Code.Cutscenes
{
    public class CS_TreeBegin : CutsceneEntity
    {
        private Level level;
        private Player player;
        private MysteriousTree tree;
        private Image titleImage, titleImage2;
        private float alpha, alpha2;

        public CS_TreeBegin(MysteriousTree mt)
        {
            tree = mt;
            Tag = Tags.HUD;

            alpha = 0f;
            Add(titleImage = new Image(GFX.Gui["ss2024/tree/introFont"]));
            titleImage.Color = Color.White * alpha;

            alpha2 = 0f;
            Add(titleImage2 = new Image(GFX.Gui["ss2024/tree/introFont2"]));
            titleImage2.Color = Color.White * alpha2;
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

            yield return 1f;
            Add(new Coroutine(fadeTitleImage()));
            yield return 1.5f;
            Add(new Coroutine(fadeTitleImage2()));
            tree.PlayBegin();
            Add(new Coroutine(screamEffects()));
            yield return 2f;
            EndCutscene(level);
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
                player.ForceCameraUpdate = false;
            }
            tree.BeginFight = true;
            titleImage.Color = Color.White * 0f;
            titleImage2.Color = Color.White * 0f;
            Distort.Anxiety = 0f;
            level.Session.SetFlag(tree.beginImmediatelyFlag, true);
        }

        private IEnumerator fadeTitleImage()
        {
            while(alpha < 1f)
            {
                alpha += 0.05f;
                titleImage.Color = Color.White * alpha;
                yield return null;
            }
            yield return 3f;
            while (alpha > 0f)
            {
                alpha -= 0.1f;
                titleImage.Color = Color.White * alpha;
                yield return null;
            }
        }

        private IEnumerator fadeTitleImage2()
        {
            while (alpha2 < 1f)
            {
                alpha2 += 0.05f;
                titleImage2.Color = Color.White * alpha2;
                yield return null;
            }
            yield return 1.5f;
            while (alpha2 > 0f)
            {
                alpha2 -= 0.1f;
                titleImage2.Color = Color.White * alpha2;
                yield return null;
            }
        }

        private IEnumerator screamEffects()
        {
            for(int i = 0; i < 10; ++i)
            {
                level.Displacement.AddBurst(tree.ShotOrigin, 0.4f, 8f, 300f, 0.5f);
                yield return 0.2f;
            }
        }
    }
}
