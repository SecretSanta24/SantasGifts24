using Celeste.Mod.Entities;
using Celeste.Mod.NeutronHelper;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Cutscenes
{
    [CustomEvent("SS2024/WC_Cleanup1")]
    public class CS_WC_Cleanup1 : Entity
    {
        public CS_WC_Cleanup1() : base() { }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            var monos = scene.Tracker.GetEntities<Monopticon>();
            if (monos.Count > 1)
            {
                monos[0].RemoveSelf();
            }
        }
    }
}
