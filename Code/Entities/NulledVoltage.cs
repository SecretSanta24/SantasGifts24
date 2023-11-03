using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Entities
{
    [CustomEntity("SS2024/NulledVoltage")]
    public class NulledVoltage : Lightning
    {
        public NulledVoltage(EntityData data, Vector2 offset) : base(data, offset) 
        {

        }

        public static void Load()
        {
            On.Celeste.Lightning.OnPlayer += NulledVoltageHook;
        }

        public static void Unload()
        {
            On.Celeste.Lightning.OnPlayer -= NulledVoltageHook;
        }

        public static void NulledVoltageHook(On.Celeste.Lightning.orig_OnPlayer orig, Lightning self, Player player)
        {
            NulledVoltage nv = self as NulledVoltage;
            if (nv == null)
            {
                orig(self, player);
            }
        }
    }
}
