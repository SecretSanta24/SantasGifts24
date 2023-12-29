using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
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
        private Orientations orientation;
        private MTexture[] doorTextures;
        private Sprite doorLock;
        public bool despawning;
        private bool renderChain = true;

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
            orientation = ori;
            doorTextures = new MTexture[] {
                GFX.Game["objects/ss2024/smwDoor/chainTexture1" + (ori == Orientations.Horizontal ? "h" : "")],
                GFX.Game["objects/ss2024/smwDoor/chainTexture2" + (ori == Orientations.Horizontal ? "h" : "")]
            };
            Add(doorLock = GFX.SpriteBank.Create("smwDoorLock"));
            doorLock.CenterOrigin();
            doorLock.Position = new Vector2(Width / 2, Height / 2);
            Add(new ClimbBlocker(false));
            AllowStaticMovers = true;
        }

        public SMWDoor(EntityData data, Vector2 offset) : this(data, offset, Orientations.Vertical)
        {
        }

        public void Open()
        {
            despawning = true; //add this here to ensure 1 key = 1 door openned
            Add(new Coroutine(OpenRoutine()));
        }

        private IEnumerator OpenRoutine()
        {
            doorLock.Play("open");
            foreach (var sm in staticMovers)
            {
                sm.Entity.Collidable = false;
            }
            Collidable = false;
            yield return doorLock.CurrentAnimationTotalFrames * doorLock.currentAnimation.Delay;

            foreach (var sm in staticMovers)
            {
                Scene.Remove(sm.Entity);
            }
            renderChain = false;
            yield return doorLock.CurrentAnimationTotalFrames * doorLock.currentAnimation.Delay;
            RemoveSelf();
            yield break;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);

        }

        public override void Render()
        {
            if (renderChain) for (int i = 0; i < (int)(orientation == Orientations.Vertical ? Height : Width) / 8; i++)
            {
                doorTextures[i % 2].Draw(Position + new Vector2(orientation == Orientations.Horizontal ? i * 8 : 0, orientation == Orientations.Vertical ? i * 8 : 0));
            }
            base.Render();
        }
    }
}
