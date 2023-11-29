using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Triggers
{
    [TrackedAs(typeof(LookoutBlocker))]
    [Tracked(false)]
    [CustomEntity("SS2024/MonoBlocker")]
    public class MonoBlocker : LookoutBlocker
    {
        public string Flag;
        public MonoBlocker(EntityData data, Vector2 offset) : base(data, offset)
        {
            Flag = data.Attr("flag", "");
        }
    }
}
