using Celeste;
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
    [Tracked]
    [CustomEntity(new string[] { 
        "SS2024/SMWDoor = LoadVertical", 
        "SS2024/SMWDoorVertical = LoadVertical", 
        "SS2024/SMWDoorHorizontal = LoadHorizontal" })]
    public class SMWDoor : Solid
    {
        private MTexture doorTexture;
        private Orientations orientation;

        public static Entity LoadVertical(Level level, LevelData levelData, Vector2 offset, EntityData data) => new SMWDoor(data, offset, Orientations.Vertical);
        public static Entity LoadHorizontal(Level level, LevelData levelData, Vector2 offset, EntityData data) => new SMWDoor(data, offset, Orientations.Horizontal);

        public enum Orientations { Vertical,  Horizontal };

        public SMWDoor(Vector2 position, float width, float height, bool safe) :
            base(position, width, height, safe)
        {

        }

        public SMWDoor(EntityData data, Vector2 offset, Orientations ori) : 
            base(
                data.Position + offset, 
                ori == Orientations.Vertical ? 8 : data.Width, 
                ori == Orientations.Vertical ? data.Height : 8, 
                false)
        {
            doorTexture = GFX.Game["objects/ss2024/smwDoor/smwDoor"];
            orientation = ori;
        }

        public SMWDoor(EntityData data, Vector2 offset) : this(data, offset, Orientations.Vertical)
        {
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (StaticMover sm in staticMovers)
            {
                scene.Remove(sm.Entity);
            }
        }

        public override void Render()
        {
            base.Render();
            for (int i = 0; i< (int) (orientation == Orientations.Vertical ? Height : Width) / 8; i++)
            {
                doorTexture.Draw(Position + new Vector2(orientation == Orientations.Horizontal ? i * 8 : 0, orientation == Orientations.Vertical ? i * 8 : 0));
            }
        }
    }
}
