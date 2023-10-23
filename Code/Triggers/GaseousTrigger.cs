using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.NeutronHelper
{
    [Tracked]
    [CustomEntity("SS2024/GaseousTrigger")]
    public class GaseousTrigger : Trigger
    {
        public GaseousTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
        }
    }
}
