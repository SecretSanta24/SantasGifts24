using Celeste.Mod.Entities;
using Celeste.Mod.SantasGifts24.Code.Entities;
using Celeste.Mod.SantasGifts24.Code.States;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.SantasGifts24.Code.Triggers
{
    // by Aurora Aquir
    [CustomEntity("SS2024/RewindBlockTrigger")]
    [Tracked]
    public class RewindBlockTrigger : Trigger
    {
        public RewindBlockTrigger(EntityData data, Vector2 offset) : base(data,  offset)
        {
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            RewindController.timeSinceReset = 0;
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            RewindController.states.Clear();
        }
    }
}
