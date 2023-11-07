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
    [CustomEntity("SS2024/GloriousPassage")]
    public class GloriousPassage : Entity
    {
        public string flag;
        public int lastinput;
        public GloriousPassage(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
            Add(new PlayerCollider(onPlayer, Collider));

            flag = data.Attr("flag", "door_check");
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            (Scene as Level).Session.SetFlag(flag, false);
        }

        public void onPlayer(Player player)
        {
            if (player.onGround)
            {
                if (Input.MoveY.Value == -1 && lastinput != -1)
                {
                    (Scene as Level).Session.SetFlag(flag, true);
                    Add(new Coroutine(Routine(player)));
                }
            }
            lastinput = Input.MoveY.Value;
        }

        public IEnumerator Routine(Player player)
        {
            yield return null;
            (Scene as Level).Session.SetFlag("bino_transition_assist", false);
        }

        public override void Render()
        {
            base.Render();
            Draw.Rect(Position, Width, Height, (Scene as Level).Session.GetFlag(flag) ? Color.Green : Color.Red);
        }
    }
}
