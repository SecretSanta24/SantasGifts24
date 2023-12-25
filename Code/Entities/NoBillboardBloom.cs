using Celeste.Mod.Entities;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    [Tracked]
    [CustomEntity("SS2024/NoBillboardBloom")]
    public class NoBillboardBloom : Entity
    {
        public NoBillboardBloom() : base()
        {

        }

        public static void Load()
        {
            On.Celeste.PlaybackBillboard.RenderBloom += NoBillboardBloomHook;
        }
        public static void Unload() 
        {
            On.Celeste.PlaybackBillboard.RenderBloom -= NoBillboardBloomHook;
        }

        public static void NoBillboardBloomHook(On.Celeste.PlaybackBillboard.orig_RenderBloom orig, PlaybackBillboard self)
        {
            NoBillboardBloom b = self.Scene.Tracker.GetEntity<NoBillboardBloom>();
            if (b == null) orig(self);
        }
    }
}
