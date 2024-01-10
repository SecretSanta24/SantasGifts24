using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    [CustomEntity("SS2024/MysteriousTreeManager")]
    public class MysteriousTreeManager : Entity
    {

        private string flag1, flag2, flag3, roomName, beginImmediatelyFlag;
        private Level level;

        public MysteriousTreeManager(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            flag1 = data.Attr("flag1");
            flag2 = data.Attr("flag2");
            flag3 = data.Attr("flag3");
            roomName = data.Attr("onDeathTeleportRoomName");
            beginImmediatelyFlag = data.Attr("beginImmediatelyFlag");
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            level = SceneAs<Level>();
            level.Add(new MysteriousTree(Position, flag1, flag2, flag3, roomName, beginImmediatelyFlag));
        }


    }
}
