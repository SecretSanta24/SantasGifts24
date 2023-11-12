using Celeste.Mod.Entities;
using Celeste.Mod.NeutronHelper;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    [CustomEntity("SS2024/EchoChamberManager")]
    public class EchoChamberManager : Entity
    {
        public Player player;
        public Monopticon mono;
        public bool gotMonopticon;

        public EchoChamberManager() : base(Vector2.Zero)
        {
            gotMonopticon = false;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = Scene.Tracker.GetEntity<Player>();
            if(player != null)
            {
                mono = Scene.Tracker.GetNearestEntity<Monopticon>(player.Position);
                gotMonopticon = true;
            }
            

            List<Entity> t = new List<Entity>();
            t = Scene.Tracker.GetEntities<TouchSwitch>();
            for (int i = 0; i < t.Count; i++)
            {
                TouchSwitch e = t[i] as TouchSwitch;
                if (e != null)
                {
                    e.Depth = -12000;
                }
            }
        }

        public override void Update()
        {


            base.Update();

            if(player != null)
            {

                if (!gotMonopticon)
                {
                    mono = Scene.Tracker.GetNearestEntity<Monopticon>(player.Position);
                    gotMonopticon = true;
                }

                if (player.Center.Y >= (Scene as Level).Bounds.Bottom + 16)
                {
                    player.StateMachine.state = 0; // this is idle state
                }
                mono.Depth = player.depth - 1;
                if(player.StateMachine.state != 13)
                {
                    player.Depth = 0;
                    mono.Depth = -8500;
                }
                if(player.StateMachine.state == 14 || player.StateMachine.state == 13)
                {
                    player.ForceCameraUpdate = true;
                } 
                else
                {
                    player.ForceCameraUpdate = false;
                }
            }
        }
    }
}
