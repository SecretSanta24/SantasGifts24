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
    [CustomEntity("SS2024/ElectricGenerator")]
    public class ElectricGenerator : Entity
    {
        private TalkComponent talker;
        private Sprite sprite;
        public ElectricGenerator(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Add(sprite = GFX.SpriteBank.Create("electricGenerator"));
            Vector2 drawAt = new Vector2(data.Width / 2, 0);

            Add(talker = new TalkComponent(new Rectangle(-0, -16, 20, 32), drawAt, OnTalk));
            talker.PlayerMustBeFacing = true;
        }

        private void OnTalk(Player player)
        {
            Level level = SceneAs<Level>();
            string flagName = "SS2024_level_electricity_flag";
            bool flag = level.Session.GetFlag(flagName);
            if (flag)
            {
                level.Session.SetFlag(flagName, false);
                sprite.Play("off");
            }
            else
            {
                level.Session.SetFlag(flagName, true);
                sprite.Play("on");
            }

            //TODO: some visual effects
        }
    }
}
