using System;
using System.Collections;
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using MonoMod.Utils;


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

        public override void Added(Scene scene)
        {
            base.Added(scene);
            bool currentFlagState = SceneAs<Level>().Session.GetFlag("SS2024_level_electricity_flag");

            if (currentFlagState)
            {
                Collidable = false;
                Visible = false;
                UntrackLightning();                
            }
            else
            {
                Collidable = true;
                Visible = true;
                TrackLightning();
            }
        }

        public override void Removed(Scene scene)
        {
            //UntrackLightning();
            base.Removed(scene);
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
                    UntrackLightning();
                }
            }
            else
            {
                Collidable = true;
                Visible = true;
                if (currentFlagState != previousFlagState)
                {
                    TrackLightning();
                }
            }
            previousFlagState = currentFlagState;
        }

        private void TrackLightning()
        {
            LightningRenderer lr = Scene.Tracker.GetEntity<LightningRenderer>();
            DynData<LightningRenderer> lrData = new DynData<LightningRenderer>(lr);
            List<Lightning> lightningList = lrData.Get<List<Lightning>>("list");
            if(lightningList.Contains(this))
            {
                return;
            }
            lr.Visible = true;
            lr.Track(this);
        }

        private void UntrackLightning()
        {
            LightningRenderer lr = Scene.Tracker.GetEntity<LightningRenderer>();
            DynData<LightningRenderer> lrData = new DynData<LightningRenderer>(lr);
            List<Lightning> lightningList = lrData.Get<List<Lightning>>("list");
            if (lightningList.Contains(this))
            {
                lr.Visible = false;
                lr.Untrack(this);
            }
        }
    }

}
