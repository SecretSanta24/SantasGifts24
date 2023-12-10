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
        public RefillClutterDoor(EntityData data, Vector2 offset) : base(data, offset, GetSession())
        {
            this.OnDashCollide = new DashCollision(this.OnDashed);
            this.doubleDash = data.Bool("doubleDash", false);
            this.sprite.Color = (this.doubleDash ? data.HexColor("TwoDashColor",Player.TwoDashesHairColor) : data.HexColor("OneDashColor", Player.NormalHairColor));
        }

        private static Session GetSession()
        {
            Level lvl = Engine.Scene switch
            {
                Level level => level,
                LevelLoader loader => loader.Level,
                AssetReloadHelper => (Level)AssetReloadHelper.ReturnToScene,
                _ => throw new Exception("RefillClutterDoor: GetSession called outside of a level... how did you manage that?")
            };

            return lvl.Session;
        }
        private new DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            this.wiggler.Start();
            Audio.Play("event:/game/03_resort/forcefield_bump", this.Position);
            if(player.UseRefill(this.doubleDash))
            {
                Audio.Play(this.doubleDash ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", this.Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            }
            return DashCollisionResults.Bounce;
        }
    }
}
