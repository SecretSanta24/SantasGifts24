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
        private Sprite[] chainSprites;

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
            chainSprites = new Sprite[(int)Math.Max(Width / 8, Height / 8)];
            string string0 = ori == Orientations.Horizontal ? "smwDoorChainHBot" : "smwDoorChainBot";
            string string1 = ori == Orientations.Horizontal ? "smwDoorChainHTop" : "smwDoorChainTop";
            for (int i = 0; i < chainSprites.Length; i++) 
            {
                Add(chainSprites[i] = GFX.SpriteBank.Create((i % 2 == 0) ? string1 : string0));
                chainSprites[i].Position = new Vector2(orientation == Orientations.Horizontal ? i * 8 + 4: Width / 2, orientation == Orientations.Vertical ? i * 8 + 4 : Height / 2);
            }
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
            int i = 0;
            foreach (Sprite sprite in chainSprites)
            {
                Remove(sprite);
                SceneAs<Level>().ParticlesBG.Emit(FinalBoss.P_Burst, 3, Position + sprite.Position, new Vector2(4, 4), Calc.HexToColor("ebaa77"),
                    (float)(orientation == Orientations.Horizontal ? Math.PI / 2 + i++ % 2 * Math.PI : i++ % 2 * Math.PI));
            }
            Collidable = false;
            yield return doorLock.CurrentAnimationTotalFrames * doorLock.currentAnimation.Delay;

            foreach (var sm in staticMovers)
            {
                Scene.Remove(sm.Entity);
            }
            yield return doorLock.CurrentAnimationTotalFrames * doorLock.currentAnimation.Delay;
            RemoveSelf();
            yield break;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);

        }

    }
}
