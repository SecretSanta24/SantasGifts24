using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    
    public class EllipticalShockwave : Entity
    {
        private float b;
        private float a;

        private float expand;

        private float expandRate = 0.1F;
        private float thickness = 3;

        private MTexture shockwave;
        private Sprite sprite;

        public EllipticalShockwave(Vector2 Position, float a, float b, float initialSize, float expandRate, float shockwaveThickness) : base(Position) {
            this.b = b;
            this.a = a;
            this.expand = initialSize;
            this.expandRate = expandRate;
            thickness = shockwaveThickness;
            Depth = Depths.Below;

            shockwave = GFX.Game["objects/ss2024/ellipticalShockwave/shockwave"];

            sprite = new Sprite(GFX.Game, "objects/ss2024/ellipticalShockwave/shockwave");
            Add(sprite);
            sprite.AddLoop("idle", 0.1F, new MTexture[] { GFX.Game["objects/ss2024/ellipticalShockwave/shockwave"] });
            sprite.Play("idle");
            sprite.Scale = new Vector2(b * expand / 100, a * expand / 100);
            sprite.CenterOrigin();
        }

        public override void Render()
        {
            base.Render();
            sprite.Scale = new Vector2(b * expand / 100, a * expand / 100);
            sprite.Color = Color.White;
            sprite.CenterOrigin();
            sprite.Render();
            if (expand - thickness > 0)
            {
                sprite.Scale = new Vector2(b * (expand - thickness) / 100, a * (expand - thickness) / 100);
                sprite.Color = Color.Blue;
                sprite.CenterOrigin();
                sprite.Render();
            }
        }

        public override void DebugRender(Camera camera)
        {
            /**
            base.DebugRender(camera);

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null)
            {
                return;
            }
            if (player.Dead)
            {
                return;
            }
            Vector2 playerPos = player.TopLeft;
            Vector2 playerSize = new Vector2(player.Width, player.Height);

            Vector2 playerTransformedPos = new Vector2((playerPos.X - Position.X) / b, (playerPos.Y - Position.Y) / a);
            Vector2 playerTransformedSize = new Vector2(playerSize.X / b, playerSize.Y / a);

            Hitbox transformedHitbox = new Hitbox(playerTransformedSize.X, playerTransformedSize.Y);
            
            Vector2 playerActualPosition = player.Position;
            Collider playerActualHitbox = player.Collider;

            player.Collider = transformedHitbox;
            player.Position = playerTransformedPos;

            Vector2 shockwaveTransformedPosition = new Vector2(0);
            Vector2 shockwaveActualPos = Position;
            Position = shockwaveTransformedPosition + new Vector2(180, 90);
            Collider = new Circle(expand);
            this.Collidable = true;
            collider.Render(camera);
            Collider = transformedHitbox;
            Position = playerTransformedPos + new Vector2(180, 90);

            Collider.Render(camera, Color.Blue);


            player.Collider = playerActualHitbox;
            player.Position = playerActualPosition;

            Position = shockwaveActualPos;
            Collidable = false;
            Collider = null;*/

        }

        public override void Update()
        {
            base.Update();
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null)
            {
                return;
            }
            if (player.Dead)
            {
                return;
            }
            expand += expandRate * Engine.DeltaTime;
            if (CheckPlayerMovingInShockwaveDirection(player))
            {
                return;
            }
            Vector2 playerPos = player.TopLeft;
            Vector2 playerSize = new Vector2(player.Width, player.Height);

            Vector2 playerTransformedPos = new Vector2((playerPos.X - Position.X) / b, (playerPos.Y - Position.Y) / a);
            Vector2 playerTransformedSize = new Vector2(playerSize.X / b, playerSize.Y / a);

            Hitbox transformedHitbox = new Hitbox(playerTransformedSize.X, playerTransformedSize.Y);

            Vector2 playerActualPosition = player.Position;
            Collider playerActualHitbox = player.Collider;

            player.Collider = transformedHitbox;
            player.Position = playerTransformedPos;

            Vector2 shockwaveTransformedPosition = new Vector2(0);
            Vector2 shockwaveActualPos = Position;

            Position = shockwaveTransformedPosition;
            Collider = new Circle(expand);
            this.Collidable = true;
            if(this.CollideCheck(player))
            {
                //check if it's inside the smaller ellipse
                if (expand - thickness > 0)
                {
                    Collider = new Circle(expand - thickness);
                    if (this.CollideCheck(player))
                    {


                    } else
                    {

                        player.Collider = playerActualHitbox;
                        player.Position = playerActualPosition;

                        Position = shockwaveActualPos;
                        Collidable = false;
                        Collider = null;
                        player.Die(Vector2.UnitY);
                    }
                }
                else
                {

                    player.Collider = playerActualHitbox;
                    player.Position = playerActualPosition;

                    Position = shockwaveActualPos;
                    Collidable = false;
                    Collider = null;
                    player.Die(Vector2.UnitY);
                }
            }

            player.Collider = playerActualHitbox;
            player.Position = playerActualPosition;

            Position = shockwaveActualPos;
            Collidable = false;
            Collider = null;
            


        }

        private bool CheckPlayerMovingInShockwaveDirection(Player play)
        {
            if (play.Position == Position) return false;

            Vector2 deltaPos = (play.Position - Position);
            deltaPos.Normalize();
            Vector2 playerSpeed = play.Speed;
            playerSpeed.Normalize();
            return Math.Acos(Vector2.Dot(deltaPos, playerSpeed)) < Math.PI * 0.5F;
        }
    }
}
