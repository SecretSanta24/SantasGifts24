using System;
using System.Collections;
using System.Reflection;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using On.Celeste;
using Celeste.Mod.Entities;



namespace Celeste.Mod.SantasGifts24.Code.Entities
{
	[CustomEntity("SS2024/ElectricZipline")]
	public class ElectricZipLine : Entity
	{
		public static int ZipLineState;

		public static Player playerInstance;

		private static ElectricZipLine currentGrabbed;

		private static ElectricZipLine lastGrabbed;

		public Vector2 left;

		public Vector2 right;

		private float height;

		private float speed;

		private bool grabbed;

		private static float ziplineBuffer;

		private Sprite sprite;

		public SoundSource moveSfx;

		private bool bright;

        public static ParticleType P_ZiplineFriction = new ParticleType
        {
            Color = Calc.HexToColor("756e65"),
            ColorMode = ParticleType.ColorModes.Blink,
            FadeMode = ParticleType.FadeModes.Late,
            Size = 0.5f,
            SizeRange = 0.2f,
            RotationMode = ParticleType.RotationModes.SameAsDirection,
            LifeMin = 0.05f,
            LifeMax = 0.15f,
            SpeedMin = 100f,
            SpeedMax = 120f,
            DirectionRange = (float)Math.PI / 3
        };

        public static bool GrabbingCoroutine
		{
			get
			{
				if (currentGrabbed != null)
				{
					return !currentGrabbed.grabbed;
				}
				return false;
			}
		}

		private static void MoveEntityTo(Actor ent, Vector2 position)
		{
			ent.MoveToX(position.X);
			ent.MoveToY(position.Y);
		}

		private bool CanGrabZip(ElectricZipLine line)
		{
			if (!SceneAs<Level>().Session.GetFlag("SS2024_ricky06_haveZipline"))
            {
				return false;
            }
			if (lastGrabbed != line)
			{
				return true;
			}
			return ziplineBuffer <= 0f;
		}

		public static void Load()
        {
			On.Celeste.Player.Update += OnPlayerUpdate;
			On.Celeste.Player.UpdateSprite += UpdatePlayerVisuals;
			Everest.Events.Player.OnSpawn += Player_OnSpawn;
		}

		public static void Unload()
        {
			On.Celeste.Player.Update -= OnPlayerUpdate;
			On.Celeste.Player.UpdateSprite -= UpdatePlayerVisuals;
			Everest.Events.Player.OnSpawn -= Player_OnSpawn;
		}

		private static void Player_OnSpawn(Player obj)
		{
			playerInstance = obj;
		}

		public static void OnPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
		{
			orig.Invoke(self);
			ziplineBuffer = Calc.Approach(ziplineBuffer, 0f, Engine.DeltaTime);
			if (!Input.GrabCheck)
			{
				ziplineBuffer = 0f;
			}
		}

		private static void UpdatePlayerVisuals(On.Celeste.Player.orig_UpdateSprite orig, Player self)
		{
			if (ZipLineState != 0 && (int)self.StateMachine == ZipLineState)
			{
				self.Sprite.Scale.X = Calc.Approach(self.Sprite.Scale.X, 1f, 1.75f * Engine.DeltaTime);
				self.Sprite.Scale.Y = Calc.Approach(self.Sprite.Scale.Y, 1f, 1.75f * Engine.DeltaTime);
				if (!GrabbingCoroutine)
				{
					self.Sprite.PlayOffset("fallSlow_carry", 0.5f);
					self.Sprite.Rate = 0f;
				}
			}
			else
			{
				orig.Invoke(self);
			}
		}

		public static void ZipLineBegin()
		{
			if (playerInstance == null)
            {
				return;
            }
			playerInstance.Ducking = false;
			float speedMagnitude = playerInstance.Speed.Length();
			Vector2 direction = Vector2.Zero;
			if (Math.Sign(playerInstance.Speed.X) > 0)
			{
				direction = (currentGrabbed.right - currentGrabbed.left).SafeNormalize();
			}
			else if (Math.Sign(playerInstance.Speed.X) < 0)
			{
				direction = (currentGrabbed.left - currentGrabbed.right).SafeNormalize();
			}
			playerInstance.Speed = direction*speedMagnitude;
			currentGrabbed.moveSfx.Play("event:/ricky06/SS2024/zipline", "fade", 0);
		}

		public static void ZipLineEnd()
		{
			currentGrabbed.moveSfx.Stop();
			currentGrabbed.grabbed = false;
			currentGrabbed = null;
			ziplineBuffer = 0.35f;
		}

		public static int ZipLineUpdate()
		{
			if (playerInstance == null)
			{
				return 0;
			}
			if (currentGrabbed == null)
			{
				return 0;
			}
			if (!currentGrabbed.grabbed)
			{
				return ZipLineState;
			}
			currentGrabbed.speed = playerInstance.Speed.X;
            if (Math.Abs(playerInstance.LiftSpeed.X) <= Math.Abs(playerInstance.Speed.X))
            {
                playerInstance.LiftSpeed = new Vector2(playerInstance.Speed.X, 0f);
                playerInstance.LiftSpeedGraceTime = 0.15f;
            }

            Vector2 direction;
			if (Math.Sign(Input.Aim.Value.X) > 0)
			{
				direction = (currentGrabbed.right - currentGrabbed.left).SafeNormalize();
			}
			else
            {
				direction = (currentGrabbed.left - currentGrabbed.right).SafeNormalize();
			}
			playerInstance.Speed.X = Calc.Approach(playerInstance.Speed.X, Input.Aim.Value.X * 120f * Math.Abs(direction.X), 250f * Engine.DeltaTime * Math.Abs(direction.X));
			playerInstance.Speed.Y = Calc.Approach(playerInstance.Speed.Y, Input.Aim.Value.X * 120f * Math.Abs(direction.X), 250f * Engine.DeltaTime * Math.Abs(direction.Y));
			currentGrabbed.moveSfx.Param("fade", Calc.Clamp(playerInstance.Speed.Length() * 0.008f, 0, 1));

			if (!Input.GrabCheck || playerInstance.Stamina <= 0f)
			{
				return 0;
			}
			if (Input.Jump.Pressed)
			{
				Input.Jump.ConsumePress();
				playerInstance.Stamina -= 13.75f;
				playerInstance.Speed.X *= 0.1f;
				playerInstance.Jump(particles: false);
				playerInstance.LiftSpeed *= 0.4f;
				currentGrabbed.speed = Calc.Approach(currentGrabbed.speed, 0f, 20f);
				return 0;
			}
			if (playerInstance.CanDash)
			{
				playerInstance.StartDash();
				return 2;
			}
			playerInstance.Stamina -= 5f * Engine.DeltaTime;
			return ZipLineState;
		}

		public static IEnumerator ZipLineCoroutine()
		{
			playerInstance.Sprite.Play("pickup");
			playerInstance.Play("event:/char/madeline/crystaltheo_lift");
			Vector2 newPosition = new Vector2(playerInstance.X, getPlayerGrabYPosition());
			MoveEntityTo(playerInstance, newPosition);

            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.Linear, 0.07f, start: true);
            while (tween.Active)
            {
                tween.Update();
                MoveEntityTo(playerInstance, Vector2.Lerp(playerInstance.Position, newPosition, tween.Percent));
                yield return null;
            }
            currentGrabbed.grabbed = true;
            MoveEntityTo(playerInstance, newPosition);
			if (currentGrabbed.ElectricityIsOn())
			{
				playerInstance.Die((playerInstance.Position - currentGrabbed.Position).SafeNormalize());
				currentGrabbed.Visible = false;
			}
		}

		private static float getPlayerGrabYPosition()
        {
			float slope = (currentGrabbed.left.Y - currentGrabbed.right.Y) / (currentGrabbed.left.X - currentGrabbed.right.X);
			float intercept = currentGrabbed.left.Y - slope * currentGrabbed.left.X;

			return slope * playerInstance.X + intercept + 22;
		}

		public ElectricZipLine(EntityData _data, Vector2 offset)
			: base(_data.Position + offset)
		{
			left = Position;
			right = Position;
			Vector2[] nodes = _data.Nodes;
			for (int i = 0; i < nodes.Length; i++)
			{
				Vector2 vector = nodes[i];
				float nodePosX = vector.X + offset.X;
				if (nodePosX < left.X)
                {
					left = vector + offset;
                }
				else
                {
					right = vector + offset;
                }
			}

			height = (_data.Position + offset).Y;
			base.Collider = new Hitbox(20f, 16f, -10f, 1f);
			currentGrabbed = null;
			base.Depth = -110;
			sprite = GFX.SpriteBank.Create("zipline");
			sprite.Play("idle");
			sprite.JustifyOrigin(new Vector2(0.5f, 0.25f));
			Add(moveSfx = new SoundSource());
			Add(sprite);
			P_ZiplineFriction.Source = GFX.Game["particles/rect"];
			this.bright = _data.Bool("bright");
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			scene.Add(new ZipLineRender(this, bright));
		}

		public override void Update()
		{
			base.Update();
			Player playerInstance = SceneAs<Level>().Tracker.GetEntity<Player>();
			if (playerInstance == null || playerInstance.Dead)
			{
				return;
			}
			if (grabbed)
			{
				if (playerInstance.CenterX > right.X || playerInstance.CenterX < left.X)
				{
					playerInstance.Speed.X = 0f;
					playerInstance.Speed.Y = 0f;
				}
                playerInstance.CenterX = MathHelper.Clamp(playerInstance.CenterX, left.X, right.X);
				playerInstance.MoveToY(getPlayerGrabYPosition());
                Position.X = playerInstance.CenterX;
                Position.Y = playerInstance.Position.Y - 22f;
				if (playerInstance.Speed.Length() > 100f && Scene.OnInterval(0.05f))
				{
					Vector2 sparkDirection = -1f * playerInstance.Speed.SafeNormalize();
                    SceneAs<Level>().Particles.Emit(P_ZiplineFriction, playerInstance.Position - 22 * Vector2.UnitY, (float)Math.Atan2(sparkDirection.X, sparkDirection.Y) - (float)Math.PI / 2);
                }
				return;
			}
			if (currentGrabbed == null && playerInstance != null && !playerInstance.Dead && playerInstance.CanUnDuck && Input.GrabCheck && CanGrabZip(this))
			{
				PropertyInfo property = typeof(Player).GetProperty("IsTired", BindingFlags.Instance | BindingFlags.NonPublic);
				if (ziplineCollideCheck() && !(bool)property.GetValue(playerInstance))
				{
					currentGrabbed = this;
					lastGrabbed = currentGrabbed;
					playerInstance.StateMachine.State = ZipLineState;
				}
			}
		}

		public override void Render()
		{
			if (grabbed)
			{
				sprite.Visible = true;
				Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
				if (player != null)
				{
					sprite.Play((player.Facing == Facings.Left) ? "held_l" : "held_r");
				}
			}
			else
			{
				sprite.Visible = false;
			}
			base.Render();
		}

		private bool ziplineCollideCheck()
        {
			Player player = Scene.CollideFirst<Player>(left + Vector2.UnitY * 4, right + Vector2.UnitY * 4);
			return player != null;
        }

		public bool ElectricityIsOn()
        {
			return SceneAs<Level>().Session.GetFlag("SS2024_level_electricity_flag") && !SceneAs<Level>().Session.GetFlag("SS2024_level_electricity_backup_flag");
		}
	}
}
