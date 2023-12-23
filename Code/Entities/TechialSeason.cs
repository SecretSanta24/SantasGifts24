using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.TrackSpinner;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{

    [CustomEntity("SS2024/TechialSeason")]

    internal class TechialSeason : Spring
    {
        bool keepPlayerState;
        bool oneUse;
        bool freeze;

        public TechialSeason(EntityData data, Vector2 offset) : base(data, offset, data.Enum<Orientations>("orientation", 0))
        {
            keepPlayerState = data.Bool("keepPlayerState", false);
            oneUse = data.Bool("oneUse", true);
            freeze = data.Bool("freeze", false);
            Remove(sprite);
            Add(sprite = new Sprite(GFX.Game, data.Attr("texture", "objects/spring/")));
            sprite.Add("idle", "", 0f, default(int));
            sprite.Add("bounce", "", 0.07f, "idle", 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 4, 5);
            sprite.Add("disabled", "white", 0.07f);
            sprite.Play("idle");
            sprite.Origin.X = sprite.Width / 2f;
            sprite.Origin.Y = sprite.Height;

            switch (Orientation)
            {
                case Orientations.WallLeft:

                    sprite.Rotation = (float)Math.PI / 2f;
                    break;
                case Orientations.WallRight:

                    sprite.Rotation = -(float)Math.PI / 2f;
                    break;
            }
        }

        public static void Load()
        {
            On.Celeste.Spring.OnCollide += TechialOnCollideHook;
            On.Celeste.Spring.BounceAnimate += TechialBounceAnimateHook;
        }

        public static void Unload()
        {
            On.Celeste.Spring.OnCollide -= TechialOnCollideHook;
            On.Celeste.Spring.BounceAnimate -= TechialBounceAnimateHook;
        }

        public static void TechialOnCollideHook(On.Celeste.Spring.orig_OnCollide orig, Spring self, Player player)
        {
            TechialSeason s = self as TechialSeason;
            if (s != null)
            {
                if (s.keepPlayerState)
                {
                    if (player.StateMachine.State == 9 || !self.playerCanUse)
                    {
                        return;
                    }
                    if (self.Orientation == Orientations.Floor)
                    {
                        if (player.Speed.Y >= 0f)
                        {
                            s.BounceAnimate();
                            PlayerSuperBounce(player, self.Top);
                        }
                        return;
                    }
                    if (self.Orientation == Orientations.WallLeft)
                    {
                        if (PlayerSideBounce(player, 1, self.Right, self.CenterY))
                        {
                            s.BounceAnimate();
                        }
                        return;
                    }
                    if (self.Orientation == Orientations.WallRight)
                    {
                        if (PlayerSideBounce(player, -1, self.Left, self.CenterY))
                        {
                            s.BounceAnimate();
                        }
                        return;
                    }
                    throw new Exception("Orientation not supported!");
                } else orig(self, player);
            } 
            else orig(self, player);

        }

        public static void TechialBounceAnimateHook(On.Celeste.Spring.orig_BounceAnimate orig, Spring self)
        {
            TechialSeason s = self as TechialSeason;

            orig(self);
            if (s != null)
            {
                if (s.freeze) Celeste.Freeze(0.07f);
                if (s.oneUse)
                {
                    self.Add(new Coroutine(s.BreakSoon()));
                } 
            }
        }

        public IEnumerator BreakSoon()
        { 
            yield return 0.5f;
            Audio.Play("event:/new_content/game/10_farewell/quake_rockbreak", Position);
            RemoveSelf();
        }

        public static void PlayerSuperBounce(Player p, float fromY)
        {
            Collider collider = p.Collider;
            p.Collider = p.normalHitbox;
            p.MoveV(fromY - p.Bottom);
            if (!p.Inventory.NoRefills)
            {
                p.RefillDash();
            }
            p.RefillStamina();
            //p.Speed.X = 0f;
            p.varJumpTimer = 0.2f;
            p.varJumpSpeed = (p.Speed.Y = -185f);
            p.level.DirectionalShake(-Vector2.UnitY, 0.1f);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            p.Sprite.Scale = new Vector2(0.5f, 1.5f);
            p.Collider = collider;
        }

        public static bool PlayerSideBounce(Player p, int dir, float fromX, float fromY)
        {
            if (Math.Abs(p.Speed.X) > 240f && Math.Sign(p.Speed.X) == dir)
            {
                return false;
            }
            Collider collider = p.Collider;
            p.Collider = p.normalHitbox;
            p.MoveV(Calc.Clamp(fromY - p.Bottom, -4f, 4f));
            if (dir > 0)
            {
                p.MoveH(fromX - p.Left);
            }
            else if (dir < 0)
            {
                p.MoveH(fromX - p.Right);
            }
            if (!p.Inventory.NoRefills)
            {
                p.RefillDash();
            }
            p.RefillStamina();
            p.varJumpTimer = 0.2f;
            p.forceMoveX = dir;
            p.forceMoveXTimer = 0.3f;
            p.Speed.X = 240f * (float)dir;
            p.varJumpSpeed = (p.Speed.Y = -140f);
            p.level.DirectionalShake(Vector2.UnitX * dir, 0.1f);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            p.Sprite.Scale = new Vector2(1.5f, 0.5f);
            p.Collider = collider;
            return true;
        }

    }
}
