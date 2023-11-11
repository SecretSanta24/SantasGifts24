using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    [CustomEntity("SS2024/FixedLiftboostMoveBlock")]
    public class FixedLiftboostMoveBlock : MoveBlock
    {
        private float xLiftboost;
        private float yLiftboost;
        public FixedLiftboostMoveBlock(EntityData data, Vector2 offset) : base(data, offset) 
        {
            xLiftboost = data.Float("xLiftboost", 0);
            yLiftboost = data.Float("yLiftboost", 0);

        }
        public override void MoveHExact(int move)
        {
            this.LiftSpeed.X = -xLiftboost;
            base.MoveHExact(move);
        }

        public override void MoveVExact(int move)
        {
            if (move < 0) this.LiftSpeed.Y = -yLiftboost;
            else this.LiftSpeed.Y = 0;
            base.MoveVExact(move);
        }

    }
}
