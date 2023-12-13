using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    [CustomEntity("SS2024/RetainFlagStatesOnRespawnController")]
    public class RetainFlagOnRespawnController(EntityData data, Vector2 offset) : Entity(data.Position + offset)
    {
        private static SantasGiftsSession session => SantasGiftsModule.Instance.Session;


        public static void Load()
        {
            On.Celeste.Player.Die += Player_Die;
        }
        public static void Unload()
        {
            On.Celeste.Player.Die -= Player_Die;
        }

        private static PlayerDeadBody Player_Die(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {

            foreach (KeyValuePair<string, bool> kvp in session.respawnFlagMonitor)
                self.SceneAs<Level>().Session.SetFlag(kvp.Key, kvp.Value);
            return orig.Invoke(self, direction, evenIfInvincible, registerDeathInStats);
        }


    }
}
