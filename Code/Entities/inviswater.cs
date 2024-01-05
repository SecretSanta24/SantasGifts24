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
    [CustomEntity("SS2024/WaterfallBlocker")]
    [TrackedAs(typeof(Water))]
    public class inviswater : Water
    {
        public inviswater(EntityData data, Vector2 offset) : base(data, offset)
        {
            Visible = false;
            Collidable = true;
            Components.RemoveAll<DisplacementRenderHook>();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Collidable = true;
            scene.OnEndOfFrame += delegate ()
            {
                Collidable = false;
            };
        }

        public override void Update() { }

        public override void Render() { }
    }
}
