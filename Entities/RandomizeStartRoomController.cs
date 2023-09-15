using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SantasGifts24.Entities
{
    // this just exists so we can get the entitydata from it
    [CustomEntity("SS2024/RandomizeStartRoomController")]
    public class RandomizeStartRoomController : Entity
    {
        public static string RoomNames;
        public RandomizeStartRoomController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
        }

        public override void Awake(Scene scene)
        {
            RemoveSelf();
        }
    }
}
