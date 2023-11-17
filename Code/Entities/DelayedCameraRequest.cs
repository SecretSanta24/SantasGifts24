using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Drawing;

namespace Celeste.Mod.SantasGifts24.Entities
{
    public class DelayedCameraRequest : Entity
    {
        public Player player;
        public bool wipe;
        public float fade;
        public bool timecard;
        public DelayedCameraRequest(Player player, bool wipe) : base(Vector2.Zero) { 
            this.player = player;
            base.Tag = Tags.HUD;
            this.wipe = wipe;
            if(wipe) fade = 1;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            if (wipe)
            {
                (Scene as Level).DoScreenWipe(wipeIn: true);
                (Scene as Level).Camera.Position = player.CameraTarget;
                player.StateMachine.State = 0;
                RemoveSelf();
            }
            else
            {
                Add(new Coroutine(Fadeout()));
                (Scene as Level).Camera.Position = player.CameraTarget;
                player.StateMachine.State = 0;

            }

        }
        public IEnumerator Fadeout()
        {
            yield return null;
            RemoveSelf();
        }

        public override void Render()
        {
            base.Render();
            if (!wipe)
            {
                GFX.Game["objects/ss2024/timecard/six"].Draw(Vector2.Zero, Vector2.Zero);
            }
        }
    }
}