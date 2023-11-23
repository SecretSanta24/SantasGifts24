using Celeste.Mod.Entities;
using Celeste.Mod.SantasGifts24.Code.Triggers;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    [Tracked]
    [CustomEntity("SS2024/CrashLanding")]
    public class CrashLanding : Entity
    {
        public CrashLanding(EntityData data, Vector2 offset) : base(data.Position + offset) { }

        public static void Load()
        {
            On.Celeste.Player.OnCollideH += Player_OnCollideH;
            On.Celeste.Player.OnCollideV += Player_OnCollideV;
            On.Celeste.Player.ClimbBegin += Player_ClimbBegin;
        }

        public static void Unload()
        {
            On.Celeste.Player.OnCollideH -= Player_OnCollideH;
            On.Celeste.Player.OnCollideV -= Player_OnCollideV;
        }

        public static bool ShouldCrash => (Engine.Scene as Level).Tracker.GetEntity<CrashLanding>() != null;

        private static void Player_OnCollideH(On.Celeste.Player.orig_OnCollideH orig, Player self, CollisionData data)
        {
            if (ShouldCrash)
            {
                NoCrashLand trigger = self.CollideFirst<NoCrashLand>();
                if (trigger == null || (trigger.ifDash && !self.DashAttacking))
                {
                    self.Die(-data.Direction);
                    return;
                }
            }
            orig(self, data);
        }

        private static void Player_OnCollideV(On.Celeste.Player.orig_OnCollideV orig, Player self, CollisionData data)
        {
            if (ShouldCrash)
            {
                NoCrashLand trigger = self.CollideFirst<NoCrashLand>();
                if (trigger == null || (trigger.ifDash && !self.DashAttacking))
                {
                    self.Die(-data.Direction);
                    return;
                }
            }
            orig(self, data);
        }

        private static void Player_ClimbBegin(On.Celeste.Player.orig_ClimbBegin orig, Player self)
        {
            if (ShouldCrash)
            {
                NoCrashLand trigger = self.CollideFirst<NoCrashLand>();
                if (trigger == null || (trigger.ifDash && !self.DashAttacking))
                {
                    self.Die(new Vector2(-(int)self.Facing, 0f));
                    return;
                }
            }
            orig(self);
        }
    }
}
