using System;
using System.Collections;
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    [CustomEntity("SS2024/FlagLightning")]
    public class FlagLightning : Lightning
    {
        private bool previousFlagState;
        public FlagLightning(Vector2 position, int width, int height, Vector2? node, float moveTime)
            : base(position, width, height, node, moveTime)
        {
            VisualWidth = width;
            VisualHeight = height;
            base.Collider = new Hitbox(width - 2, height - 2, 1f, 1f);
            Add(new PlayerCollider(OnPlayer));
            if (node.HasValue)
            {
                Add(new Coroutine(MoveRoutine(position, node.Value, moveTime)));
            }
            toggleOffset = Calc.Random.NextFloat();
        }

        public FlagLightning(EntityData data, Vector2 levelOffset)
            : this(data.Position + levelOffset, data.Width, data.Height, data.FirstNodeNullable(levelOffset), data.Float("moveTime"))
        {
        }

        public override void Update()
        {
            base.Update();
            // flag on = electric off 
            bool currentFlagState = SceneAs<Level>().Session.GetFlag("SS2024_level_electricity_flag");

            if (currentFlagState)
            {
                Collidable = false;
                Visible = false;
                if (currentFlagState != previousFlagState)
                {
                    Scene.Tracker.GetEntity<LightningRenderer>().Untrack(this);
                }
            }
            else
            {
                Collidable = true;
                Visible = true;
                if (currentFlagState != previousFlagState)
                {
                    Scene.Tracker.GetEntity<LightningRenderer>().Track(this);
                }
            }
                previousFlagState = currentFlagState;
        }
    }

}
