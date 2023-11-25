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
    [CustomEntity("SS2024/WaterDropletPoint")]
    public class WaterDropletPoint : Entity
    {
        private Color tint = Calc.HexToColor("a5a7b9") * 0.6f;
        private Sprite sprite;

        private float timeUntilDrop;

        public WaterDropletPoint(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Add(sprite = GFX.SpriteBank.Create("leak"));
            sprite.Color = tint;
        }

        public override void Update()
        {
            base.Update();
            if(timeUntilDrop > 0)
            {
                timeUntilDrop -= Engine.DeltaTime;
            }
            else
            {
                Add(new Coroutine(GenerateWaterDroplet()));
                timeUntilDrop = (Calc.Random.NextFloat()*4f + 3f) * 0.5f;
            }
        }

        private IEnumerator GenerateWaterDroplet()
        {
            sprite.Play("leak");
            yield return 0.16f;
            Scene.Add(new WaterDroplet(Position + new Vector2(0f, 1f), 1f, tint));
        }
    }
}
