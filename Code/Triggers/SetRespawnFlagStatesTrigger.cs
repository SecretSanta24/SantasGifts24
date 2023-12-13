using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Triggers
{
    [CustomEntity("SS2024/SetRespawnFlagStatesTrigger")]
    public class SetRespawnFlagStatesTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
    {
        private List<string> flagWatch = [.. data.Attr("flags", "").Split(',')];
        private static SantasGiftsSession session => SantasGiftsModule.Instance.Session;

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            session.respawnFlagMonitor.Clear();
            foreach (var flag in flagWatch)
            {
                session.respawnFlagMonitor[flag] = SceneAs<Level>().Session.GetFlag(flag);
            }
        }
    }
}