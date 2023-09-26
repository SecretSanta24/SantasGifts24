using Celeste.Mod.Entities;
using Celeste.Mod.SantasGifts24.Code.Mechanics;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Entities.LightDark {
    [CustomEntity("SS2024/BouncyDude")]
    public class BouncyDude : HappyFunDude
    {
        private static readonly int stIdle = 0;
        private static readonly int stBounced = 1;
        private static readonly int stJumping = 2;
        private static readonly int stExplode = 3;
        private static readonly int stRespawn = 4;

        private Hitbox headHitbox;
        private Hitbox bodyHitbox;
        private Circle explodeEffectZone;

        private Vector2 startingPosition;
        private const float jumpDelay = 0.5f;
        private const float jumpHeight = 64f;
        private const float jumpTime = 0.5f;
        private const float effectRadius = 40f;

        private StateMachine stateMachine;
        private Player player;
        private float timeUntilBoom;
        private float timeSinceBoom;

        public BouncyDude(EntityData data, Vector2 offset) : base(data, offset)
        {
            startingPosition = Position;
            NormalSprite = GFX.SpriteBank.Create("corkr900SS24BouncyDudeNormal");
            NormalSprite.Position = new Vector2(0f, -8f);
            DarkSprite = GFX.SpriteBank.Create("corkr900SS24BouncyDudeDark");
            DarkSprite.Position = new Vector2(0f, -8f);
            headHitbox = new Hitbox(16f, 4f, -8f, -16f);
            bodyHitbox = new Hitbox(16f, 18f, -8f, -10f);
            explodeEffectZone = new Circle(effectRadius);
            Add(new PlayerCollider(OnPlayerHead, headHitbox));
            Add(new PlayerCollider(OnPlayerBody, bodyHitbox));

            stateMachine = new StateMachine(5);
            stateMachine.SetCallbacks(stIdle, null, null, BeginIdle);
            stateMachine.SetCallbacks(stBounced, BouncedUpdate, null, BouncedBegin);
            stateMachine.SetCallbacks(stJumping, JumpingUpdate);
            stateMachine.SetCallbacks(stExplode, ExplodeUpdate, ExplodeCoroutine);
            stateMachine.SetCallbacks(stRespawn, null, RespawnCoroutine);
            Add(stateMachine);
        }

        private void OnPlayerHead(Player player)
        {
            this.player = player;
            if (stateMachine.State == stIdle)
            {
                OnPlayerBounced();
            }
        }

        private void OnPlayerBody(Player player)
        {
            this.player = player;
            if (stateMachine.State == stIdle) {
				Audio.Play("event:/game/general/thing_booped", Position);
				player.ExplodeLaunch(Position, false, true);
				DynamicData dd = DynamicData.For(player);
				dd.Set("dashCooldownTimer", 0.02f);
				stateMachine.State = stBounced;
			}
        }

        private void OnPlayerBounced()
        {
            if (player == null) return;
            Celeste.Freeze(0.05f);
            Audio.Play("event:/game/general/thing_booped", Position);
            float speedX = player.Speed.X;
            player.SuperBounce(Position.Y + headHitbox.Top);
            player.Speed.X = speedX * 1.2f;
            stateMachine.State = stBounced;

        }

        private void BeginIdle()
        {
            SpriteVisible = true;
            Collidable = true;
            Position = startingPosition;
        }

        private void BouncedBegin()
        {
            timeUntilBoom = jumpDelay + jumpTime;
        }

        private int BouncedUpdate()
        {
            timeUntilBoom = Calc.Approach(timeUntilBoom, jumpTime, Engine.DeltaTime);
            return timeUntilBoom < jumpDelay + 0.001f ? stJumping : stBounced;
        }

        private int JumpingUpdate()
        {
            timeUntilBoom = Calc.Approach(timeUntilBoom, 0, Engine.DeltaTime);
            float a = -jumpHeight / (jumpTime * jumpTime);
            float currentHeight = a * timeUntilBoom * timeUntilBoom + jumpHeight;
            Position = startingPosition - Vector2.UnitY * currentHeight;
            if (timeUntilBoom < 0.001f)
            {
                timeSinceBoom = 0f;
                return stExplode;
            }
            return stJumping;
        }

        private IEnumerator ExplodeCoroutine()
        {
            // Explode Sound
            Audio.Play("event:/new_content/game/10_farewell/puffer_splode", Position);

            // Explode VFX
            Entity explodeFXEntity = new Entity();
            explodeFXEntity.Position = Position;
            Scene.Add(explodeFXEntity);
            Sprite sprite = GFX.SpriteBank.Create("seekerShockWave");
            explodeFXEntity.Add(sprite);
            sprite.OnLastFrame = delegate
            {
                explodeFXEntity.RemoveSelf();
            };
            sprite.Play("shockwave", restart: true);
            sprite.SetAnimationFrame(3);
            SceneAs<Level>().Shake(0.15f);

            // Explode physics stuff
            if (CurrentMode == LightDarkMode.Normal) {
				Collider = explodeEffectZone;
				Player player = CollideFirst<Player>();
				if (player != null && !Scene.CollideCheck<Solid>(Position, player.Center)) {
					player.ExplodeLaunch(Position, false, true);
					DynamicData dd = DynamicData.For(player);
					dd.Set("dashCooldownTimer", 0.02f);
				}
				Collider = null;
			}
            else {
                Scene.Add(new LightDarkProjectile(Position + Vector2.UnitX * 10, false));
				Scene.Add(new LightDarkProjectile(Position + Vector2.UnitX * -10, true));
			}

            Collidable = false;
            SpriteVisible = false;
            yield return 2f;
            stateMachine.State = stRespawn;
        }

        private int ExplodeUpdate()
        {
            timeSinceBoom += Engine.DeltaTime;
            return stExplode;
        }

        private IEnumerator RespawnCoroutine()
        {
            Position = startingPosition;
            SpriteVisible = true;
            yield return 0.2f;
            Collidable = true;
            stateMachine.State = stIdle;
        }

        public override void Render()
        {
            base.Render();

            if (stateMachine.State == stBounced || stateMachine.State == stJumping)
            {
                float lerp = 1 - Calc.Clamp(timeUntilBoom / (jumpDelay + jumpTime), 0f, 1f);
                Vector2 pos = startingPosition - Vector2.UnitY * jumpHeight;
                float radius = Ease.CubeOut(lerp) * effectRadius;
                Color color = CurrentMode == LightDarkMode.Normal ? Color.Pink : Color.Silver;
                color *= 0.5f * lerp;
                Draw.Circle(pos, radius, color, 2f, 6);
            }
            else if (stateMachine.State == stExplode)
            {
                Vector2 pos = startingPosition - Vector2.UnitY * jumpHeight;
                Color color = CurrentMode == LightDarkMode.Normal ? Color.Pink : Color.Silver;
                color *= Calc.ClampedMap(timeSinceBoom, 0, 0.5f, 0.5f, 0f);
                Draw.Circle(pos, effectRadius, color, 2f, 6);
            }
        }

    }
}
