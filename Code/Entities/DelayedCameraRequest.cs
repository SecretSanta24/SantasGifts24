using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    public class DelayedCameraRequest : Entity
    {
        public Player player;
        public bool wipe;
        public float fade;
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
                (Scene as Level).DoScreenWipe(wipeIn: true);
                Add(new Coroutine(Fadeout()));
                (Scene as Level).Camera.Position = player.CameraTarget;
                player.StateMachine.State = 0;

            }

        }
        public IEnumerator Fadeout()
        {
            fade = 1;
            yield return null;
            if ((Scene as Level).Wipe != null)
            {
                (Scene as Level).Wipe.Cancel();
            }
            for (float i = 1; i > 0; i -= 0.1f * (Engine.DeltaTime * 60))
            {
                fade = Ease.SineOut(i);
                yield return null;
            }
            RemoveSelf();
        }

        public override void Render()
        {
            base.Render();
            Draw.Rect(Vector2.Zero, 1921, 1081, Color.Black * fade);
        }
    }
}