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
    public class CS_BackupGeneratorOff : CutsceneEntity
    {
        private Level level;
        private Player player;
        private BackupGenerator bg;
        private bool shake;
        private SoundSource shakeSfx;

        public CS_BackupGeneratorOff(BackupGenerator bg)
        {
            this.bg = bg;
            Add(shakeSfx = new SoundSource());
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
            while (!player.OnGround() || player.Speed.Y < 0f)
            {
                yield return null;
            }

            shake = true;
            Add(new Coroutine(ShakeLevel()));
            shakeSfx.Play("event:/game/00_prologue/bridge_rumble_loop");
            Audio.Play("event:/ricky06/SS2024/shutdown");
            yield return 0.5f;
            shake = false;
            shakeSfx.Stop();
            yield return 0.2f;

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
                player.DummyAutoAnimate = true;
                player.DummyGravity = true;
            }
            bg.TurnOff();
        }

        private IEnumerator ShakeLevel()
        {
            List<int> directionsX = new List<int> { 1, 0, -1, 2, -1, -1, 0, 2, -2, 1, -1 };
            List<int> directionsY = new List<int> { 0, 3, -1, 2, -2, -1, -1, -1, -2, 2, 1 };

            int i = 0;

            while (shake)
            {
                level.Camera.X += directionsX[i % 11]*0.5f;
                level.Camera.Y += directionsY[i % 11]*0.5f;
                ++i;
                yield return null;
            }
        }
    }
}
