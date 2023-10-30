using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;

namespace Celeste.Mod.NeutronHelper
{
    // Celeste.PufferCollider


    [Tracked(false)]
    public class Cardiotangibility : Component
    {
        public Action<Thalassocardiologist> OnCollide;

        public Collider Collider;

        public Cardiotangibility(Action<Thalassocardiologist> onCollide, Collider collider = null)
            : base(active: false, visible: false)
        {
            OnCollide = onCollide;
            Collider = null;
        }

        public void Check(Thalassocardiologist puffer)
        {
            if (OnCollide != null)
            {
                Collider collider = base.Entity.Collider;
                if (Collider != null)
                {
                    base.Entity.Collider = Collider;
                }
                if (puffer.CollideCheck(base.Entity))
                {
                    OnCollide(puffer);
                }
                base.Entity.Collider = collider;
            }
        }
    }

}
