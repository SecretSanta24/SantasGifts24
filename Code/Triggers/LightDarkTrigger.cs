using Celeste.Mod.Entities;
using Celeste.Mod.SantasGifts24.Code.Mechanics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Triggers
{
    [CustomEntity("SS2024/LightDarkTrigger")]
    internal class LightDarkTrigger : Trigger
    {
        private LightDarkMode mode;
        private bool persistent;
        private bool removeSelf;
        private bool onlyOnce;
        private EntityID id;

        public LightDarkTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            if (!Enum.TryParse(data.Attr("mode", "Normal"), out mode)) mode = LightDarkMode.Normal;
            persistent = data.Bool("persistent", false);
            removeSelf = data.Bool("removeSelf", false);
            onlyOnce = data.Bool("onlyOnce", false);
            id = new EntityID(data.Level.Name, data.ID);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (Scene is not Level level) return;
            level.LightDarkSet(mode, persistent);
            if (removeSelf || onlyOnce) RemoveSelf();
            if (onlyOnce) level.Session.DoNotLoad.Add(id);
        }
    }
}
