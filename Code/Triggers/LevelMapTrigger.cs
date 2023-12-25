using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.SantasGifts24.Code.Cutscenes;


namespace Celeste.Mod.SantasGifts24.Code.Triggers
{
    [CustomEntity("SS2024/LevelMapTrigger")]
    public class LevelMapTrigger : Trigger
    {
        private Level level;
        public LevelMapTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {

        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            level = SceneAs<Level>();
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if(!level.Session.GetFlag("SS2024_ricky06_map_obtained"))
            {
                Scene.Add(new CS_MapCollect());
            }
        }
    }
}
