using Celeste.Mod.Backdrops;
using Celeste.Mod.SantasGifts24.Code.Entities;
using Monocle;
using System.Runtime.InteropServices.Expando;

namespace Celeste.Mod.SantasGifts24.Code.Effects
{
    //legit the only fast way to get the shockwaves to render on top
    [CustomBackdrop("SS2024/ShockwaveRenderer")]
    public class ShockwaveRenderer : Backdrop
    {
        public ShockwaveRenderer(BinaryPacker.Element child)
        {

        }

        public override void Render(Scene scene)
        {
            base.Render(scene);
            foreach (EllipticalShockwave shockwave in scene.Tracker.GetEntities<EllipticalShockwave>())
            {
                shockwave.RenderWave();
                //this makes the debug hitbox more visible in debug
                if (Engine.Commands.Open)
                {
                    shockwave.DebugRenderWave(((Level)scene).Camera);
                }
            }
            
        }
    }
}
