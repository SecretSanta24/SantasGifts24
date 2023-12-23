using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.SantasGifts24.Code.Cutscenes;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{

    [CustomEntity("SS2024/ZiplineCollect")]
    public class ZiplineCollect : Entity
    {
        private TalkComponent talker;
        private Image image;
        public ZiplineCollect(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            Add(talker = new TalkComponent(new Rectangle(-10, 0, 20, 8), Vector2.Zero, OnTalk));
            Add(image = new Image(GFX.Game["objects/ss2024/zipline/handle"]));
            image.CenterOrigin();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!SceneAs<Level>().Session.GetFlag("SS2024_ricky06_haveZipline"))
            {
                talker.Enabled = true;
                image.Visible = true;
            }
            else
            {
                talker.Enabled = false;
                image.Visible = false;
            }
        }

        public override void Update()
        {
            base.Update();
            if (!SceneAs<Level>().Session.GetFlag("SS2024_ricky06_haveZipline"))
            {
                talker.Enabled = true;
                image.Visible = true;
            }
            else
            {
                talker.Enabled = false;
                image.Visible = false;
            }
        }

        public void OnTalk(Player player)
        {
            SceneAs<Level>().Session.SetFlag("SS2024_ricky06_haveZipline", true);
            Scene.Add(new CS_ZiplineCollect());
        }
    }
}
