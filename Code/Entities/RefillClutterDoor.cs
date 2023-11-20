using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using On.Celeste;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{


    [CustomEntity("SS2024/RefillClutterDoor")]
    [TrackedAs(typeof(ClutterDoor))]
    public class RefillClutterDoor : ClutterDoor
    {
        private bool doubleDash = false;
        public RefillClutterDoor(EntityData data, Vector2 offset) : base(data, offset, (Engine.Scene as LevelLoader).session)
        {
            this.OnDashCollide = new DashCollision(this.OnDashed);
            this.doubleDash = data.Bool("doubleDash", false);
            this.sprite.Color = (this.doubleDash ? data.HexColor("TwoDashColor",Player.TwoDashesHairColor) : data.HexColor("OneDashColor", Player.NormalHairColor));
        }

        private new DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            this.wiggler.Start();
            Audio.Play("event:/game/03_resort/forcefield_bump", this.Position);
            player.UseRefill(this.doubleDash);
            return DashCollisionResults.Bounce;
        }
    }
}
