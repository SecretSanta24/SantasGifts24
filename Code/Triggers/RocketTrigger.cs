using Celeste.Mod.Entities;
using Celeste.Mod.SantasGifts24.Code.States;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.SantasGifts24.Code.Triggers
{
    // by Aurora Aquir
    [CustomEntity("SS2024/RocketTrigger")]
    [Tracked]
    public class RocketTrigger : Trigger
    {
        private readonly bool enable = true;

        private readonly float maxSpeed = 250;
        private readonly float timeToMaxSpeed = 0.25f;
        private readonly float timeToStop = 0.25f;
        public RocketTrigger(EntityData data, Vector2 offset) : base(data,  offset)
        {
            enable = data.Bool("enable", true);
            maxSpeed = data.Float("maxSpeed", 250);
            timeToMaxSpeed = data.Float("timeToMaxSpeed", 250);
            timeToStop = data.Float("timeToStop", 250);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if(enable)
            {
                RocketFly.maxSpeed = maxSpeed;
                RocketFly.timeToStop = timeToStop;
                RocketFly.timeToMaxSpeed = timeToMaxSpeed;
                player.StateMachine.State = RocketFly.StateNumber;
            } else if(player.StateMachine.state == RocketFly.StateNumber)
            {
                player.StateMachine.State = Player.StNormal;
            }
        }
    }
}
