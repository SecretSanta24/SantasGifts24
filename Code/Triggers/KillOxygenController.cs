using Celeste.Mod.Entities;
using Celeste.Mod.NeutronHelper;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Triggers;
[CustomEntity("SS2024/KillOxygenTrigger")]
public class KillOxygenController : Trigger
{
    public KillOxygenController(EntityData data, Vector2 offset) : base(data, offset)
    {

    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        Level level = (Scene as Level);
        if (player != null)
        {

            Distort.AnxietyOrigin = new Vector2((player.Center.X - level.Camera.X) / 320f, (player.Center.Y - level.Camera.Y) / 180f);
            Distort.Anxiety = 0;

            foreach (Backdrop item in (Scene as Level).Background.GetEach<Backdrop>("o2_in_tag"))
            {
                item.ForceVisible = true;
                item.FadeAlphaMultiplier = 0;
            }
            foreach (Backdrop item in (Scene as Level).Background.GetEach<Backdrop>("o2_out_tag"))
            {
                item.ForceVisible = true;
                item.FadeAlphaMultiplier = 1;
            }
            foreach (Backdrop item in (Scene as Level).Foreground.GetEach<Backdrop>("o2_in_tag"))
            {
                item.ForceVisible = true;
                item.FadeAlphaMultiplier = 0;
            }
            foreach (Backdrop item in (Scene as Level).Foreground.GetEach<Backdrop>("o2_out_tag"))
            {
                item.ForceVisible = true;
                item.FadeAlphaMultiplier = 1;
            }

            level.Session.SetFlag("o2_flag_hcd", false);
        }
    }
}
