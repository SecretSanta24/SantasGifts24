using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    // by aurora
    [CustomEntity("SS2024/DownTransitionFlagController")]
    [Tracked]
    public class DownTransitionFlagController : Entity
    {
        private Level lvl;
        private string flag;
        public DownTransitionFlagController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            flag = data.Attr("flag", "");
        }

        public override void Added(Scene scene)
        {
            List<Entity> controllers = scene.Tracker.GetEntities<DownTransitionFlagController>();

            if (controllers != null && controllers.Count > 0)
            {
                foreach (var entity in controllers)
                {
                    entity.RemoveSelf();
                }
            }

            base.Added(scene);
        }

        public override void Update()
        {
            base.Update();

            if (lvl == null) lvl = (Engine.Scene as Level);

            if (lvl?.Session == null) return;

            lvl.Session.LevelData.DisableDownTransition = lvl.Session.GetFlag(flag);
        }
    }

}
