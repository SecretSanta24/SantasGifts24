using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        private float breakoutSpeed;
        private float thickness = 3;

        private MTexture shockwave;
        private Vector2[] ellipsePoints;
        private VertexPositionColor[] verteces;

        private void UpdateShockwave()
        {
            expand += expandRate * Engine.DeltaTime;
            if (verteces == null) verteces = new VertexPositionColor[NumVerteces];
            for (int i = 0; i < ellipsePoints.Length; i++)
            {
                Vector2 v1 = ellipsePoints[(i + 0) % ellipsePoints.Length];
                Vector2 v2 = ellipsePoints[(i + 1) % ellipsePoints.Length];
                Vector2 v3 = ellipsePoints[(i + 2) % ellipsePoints.Length];
                float outerRingSize = expand;
                float innerRingSize = Math.Max(expand - thickness, 0);
                if (i % 2 == 1)
                {
                    v1 *= outerRingSize;
                    v2 *= innerRingSize;
                    v3 *= outerRingSize;
                } 
                else
                {
                    v1 *= innerRingSize;
                    v2 *= outerRingSize;
                    v3 *= innerRingSize;
                }

                verteces[3 * i + 0] = new VertexPositionColor(new Vector3(v1 + Position - SceneAs<Level>().Camera.Position, 0), Color.White * 0.5F);
                verteces[3 * i + 1] = new VertexPositionColor(new Vector3(v2 + Position - SceneAs<Level>().Camera.Position, 0), Color.White * 0.5F);
                verteces[3 * i + 2] = new VertexPositionColor(new Vector3(v3 + Position - SceneAs<Level>().Camera.Position, 0), Color.White * 0.5F);
            }
            
        }
        private int numPoints = 1000;
        private bool killPlayer;

        public int NumVerteces
        {
            get { return numPoints * 3 ; }
        }

        public EllipticalShockwave(Vector2 Position, float a, float b, float initialSize, float expandRate, float shockwaveThickness, float breakoutSpeed) : base(Position) {
            this.b = b;
            this.a = a;
            this.expand = initialSize;
            this.expandRate = expandRate;
            this.breakoutSpeed = breakoutSpeed;
            thickness = shockwaveThickness;
            Depth = Depths.Below;
            shockwave = GFX.Game["objects/ss2024/ellipticalShockwave/shockwave"];

            ellipsePoints = new Vector2[numPoints];
            for (int i = 0; i < numPoints; i++)
            {
                double angle = (float)(i / (float) numPoints * 2 * Math.PI);
                ellipsePoints[i] = new Vector2((float)(b * Math.Cos(angle)), (float)(a * Math.Sin(angle)));
            }
        }

        public override void Render()
        {
            base.Render();

            if (verteces != null)
            {
                GFX.DrawVertices(Matrix.Identity, verteces, NumVerteces);
            }
        }

        public override void DebugRender(Camera camera)
        {
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
            Collider = null;

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
            if (killPlayer)
            {
                player.Die(new Vector2(1, 0));
            }
            UpdateShockwave();
            if (CheckPlayerMovingInShockwaveDirection(player))
            {
                return;
            }


            Vector2 playerActualPosition = player.Position;
            Collider playerActualHitbox = player.Collider;
            float increment = (player.Speed.Length() * Engine.DeltaTime);
            if (player.Speed != Vector2.Zero)
            {
                for (float i = 0; i <= increment; i+=0.25F)
                {
                    player.Position = player.Position + i * player.Speed.SafeNormalize();
                    if (CheckPlayerPos(player))
                    {
                        killPlayer = true;
                        player.Position = playerActualPosition;
                        break;
                    }
                    player.Position = playerActualPosition;
                }

            }
            else
            {
                if (CheckPlayerPos(player)) killPlayer = true;

            }

            player.Collider = playerActualHitbox;
            player.Position = playerActualPosition;
        }


        public bool CheckPlayerPos(Player player)
        {
           
            bool toReturn = false;

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
            if (this.CollideCheck(player))
            {
                //check if it's inside the smaller ellipse
                if (expand - thickness > 0)
                {
                    Collider = new Circle(expand - thickness);
                    if (this.CollideCheck(player))
                    {


                    }
                    else
                    {

                        player.Collider = playerActualHitbox;
                        player.Position = playerActualPosition;

                        Position = shockwaveActualPos;
                        Collidable = false;
                        Collider = null;
                        toReturn = true;
                    }
                }
                else
                {

                    player.Collider = playerActualHitbox;
                    player.Position = playerActualPosition;

                    Position = shockwaveActualPos;
                    Collidable = false;
                    Collider = null;
                    toReturn = true;
                }
            }

            player.Collider = playerActualHitbox;
            player.Position = playerActualPosition;

            Position = shockwaveActualPos;
            Collidable = false;
            Collider = null;

            return toReturn;
        }

        private bool CheckPlayerMovingInShockwaveDirection(Player play)
        {
            if (play.Position == Position) return false;
            if (play.Speed.Length() <= breakoutSpeed) return false;
            Vector2 deltaPos = (play.Position - Position);
            deltaPos = new Vector2(deltaPos.X / b, deltaPos.Y / a);
            deltaPos.Normalize();   
            Vector2 playerSpeed = play.Speed;
            playerSpeed.Normalize();
            return Math.Acos(Vector2.Dot(deltaPos, playerSpeed)) < Math.PI * 0.5F;
        }
    }
}
