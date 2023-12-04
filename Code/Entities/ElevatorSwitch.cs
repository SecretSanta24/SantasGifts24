using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    [Tracked]
    class ElevatorSwitch : Entity
    {
        private ElectricElevator elevator;
        private Sprite Lever;
        private bool atStart;
        public TalkComponent Talker;
        public ElevatorSwitch(Vector2 position, ElectricElevator gondola, bool start)
        : base(position)
        {
            this.elevator = gondola;
            atStart = start;

            Collider = new Hitbox(16f, 16f, -8f);

            Add(Lever = GFX.SpriteBank.Create("elevatorLever"));
            Lever.Play("idle");
            Add(Talker = new TalkComponent(new Rectangle(-16, 0, 32, 8), new Vector2(0f, -8f), OnTalk));
            Talker.PlayerMustBeFacing = false;
            Depth = -10500;
        }

        public void OnTalk(Player player)
        {
            Add(new Coroutine(pullReleaseLever()));
            if (atStart && elevator.Position != elevator.Start || !atStart && elevator.Position != elevator.Destination && !elevator.isShaking)
            {
                elevator.startMoving(!atStart);
            }
        }

        private IEnumerator pullReleaseLever()
        {
            Lever.Play("active");
            Audio.Play("event:/game/general/cassette_block_switch_1", Position);
            yield return 0.5f;
            Lever.Play("idle");
        }

        public override void Update()
        {
            base.Update();
            if (!SceneAs<Level>().Session.GetFlag("SS2024_level_electricity_flag"))
            {
                Talker.Enabled = true;
            }
            else
            {
                Talker.Enabled = false;
            }
        }
    }
}
