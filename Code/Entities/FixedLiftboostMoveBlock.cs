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
            xLiftboost = data.Float("xLiftboost", 40);
            yLiftboost = data.Float("yLiftboost", 40);

        }

        public override void Update()
        {
            base.Update();
            if (yLiftboost != 0 && this.direction != Directions.Up)
            {
                if (!(Scene.Tracker.GetEntity<Player>()?.IsRiding(this) == true) || Input.MoveY >= 0)
                {
                    this.LiftSpeed.Y = 0;
                }
            }
            if (xLiftboost != 0 && (this.direction == Directions.Up || direction == Directions.Down) )
            {
                if (!(Scene.Tracker.GetEntity<Player>()?.IsRiding(this) == true) || Input.MoveX != 0)
                {
                    this.LiftSpeed.X = 0;
                }
            }
        }
        public override void MoveHExact(int move)
        {
            if (move != 0) this.LiftSpeed.X = xLiftboost * (direction == Directions.Left ? -1 : 1);
            base.MoveHExact(move);
        }

        public override void MoveVExact(int move)
        {
            if (move < 0) this.LiftSpeed.Y = -yLiftboost;
            base.MoveVExact(move);
        }

    }
}
