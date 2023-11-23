using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Triggers
{
    [Tracked]
    [CustomEntity("SS2024/NoCrashLand")]
    public class NoCrashLand : Trigger
    {
        public bool ifDash;
        public NoCrashLand(EntityData data, Vector2 offset) : base(data, offset)
        {
            ifDash = data.Bool("onlyIfDash");
        }
    }
}
