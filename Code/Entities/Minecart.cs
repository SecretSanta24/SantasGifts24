using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Collections.Generic;
using System;
using Celeste.Mod.SantasGifts24.Code.States;
using Mono.Cecil.Cil;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
	[Tracked]
	[CustomEntity("SS2024/Minecart")]
	public class Minecart : Actor
	{
		public Entity Carrying;
		public MinecartTrack Track;

		public Image Body;
		public Vector2 Speed;

		private float targetSpeed;
		private float approachSpeed;
		private float reentryCooldown;
		private Facings facing;
		private bool oneUse;

		private Outline outline;
		private WheelPart leftWheel;
		private WheelPart rightWheel;
		private Facings trackFacing;
		private float rotationAdd = 0f;
		private float trackDistance = 0f;
		private float trackSpeed = 0f;
		private float attachDelay = 0f;
		private float reentryTimer = 0f;
		private bool activated = false;
		private bool stopped = false;
		private int lastDepth;
		private int? lastHoldingDepth;
		private Dictionary<Entity, Vector2> lastCarryablePositions = new();

		private float _rotation;
		public float Rotation
		{
			get => _rotation;
			set
			{
				var wrappedValue = Calc.WrapAngle(value);

				if (_rotation == wrappedValue)
					return;

				_rotation = wrappedValue;

				Body.Rotation = _rotation;

				UpdateWheels();
				UpdateCarrying();
			}
		}

		public Minecart(EntityData data, Vector2 offset) : base(data.Position + offset)
		{
			targetSpeed = data.Float("speed", 100f);
			approachSpeed = data.Float("approachSpeed", targetSpeed);
			facing = data.Enum("direction", Facings.Right);
			oneUse = data.Bool("oneUse", true);
			reentryCooldown = data.Float("reentryCooldown", 0.5f);

			Depth = Depths.SolidsBelow + 2;

			Collider = new Hitbox(32f, 20f, -16f, -20f);

			Add(Body = new Image(GFX.Game["objects/ss2024/minecart/body"]));
			Body.JustifyOrigin(0.5f, 1f);

			Add(new PostUpdateHook(PostUpdateEntryCheck));
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);

			outline = new Outline(this, Position);
			leftWheel = new WheelPart(this, Position + new Vector2(-10.5f, -3.5f));
			rightWheel = new WheelPart(this, Position + new Vector2(10.5f, -3.5f));

			scene.Add(outline);
			scene.Add(leftWheel);
			scene.Add(rightWheel);

			UpdateWheels();
		}

		public override void Removed(Scene scene)
		{
			base.Removed(scene);

			outline?.RemoveSelf();
			leftWheel?.RemoveSelf();
			rightWheel?.RemoveSelf();

			outline = null;
			leftWheel = null;
			rightWheel = null;
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);

			TryAttach(Position - (Vector2.UnitY * 2f), Position + (Vector2.UnitY * 2f));

			UpdateLastCarryablePositions();
		}

		public override bool IsRiding(JumpThru jumpThru)
		{
			return Track == null && base.IsRiding(jumpThru);
		}

		public override bool IsRiding(Solid solid)
		{
			return Track == null && base.IsRiding(solid);
		}

		private bool TryAttach(Vector2 lineFrom, Vector2 lineTo)
		{
			foreach (MinecartTrack track in Scene.Tracker.GetEntities<MinecartTrack>())
			{
				if (track.GetIntersect(lineFrom, lineTo, out var intersect, out var distance, out var derivative))
				{
					Position = Calc.Floor(intersect);

					Track = track;
					trackDistance = distance;

					if (derivative.X < 0f)
					{
						trackFacing = Facings.Left;
						rotationAdd = (float)Math.PI;
					}
					else if (derivative.X > 0f)
					{
						trackFacing = Facings.Right;
						rotationAdd = 0f;
					}

					Rotation = rotationAdd + derivative.Angle();

					UpdateWheels();

					if (activated)
						Track.Mover.TriggerPlatform();

					return true;
				}
			}

			return false;
		}

		private void UpdateWheels()
		{
			if (leftWheel == null || rightWheel == null)
				return;

			var angleX = Rotation;
			var angleY = Rotation + ((float)Math.PI / 2f);

			var wheelOffsetX = Calc.AngleToVector(angleX, 10.5f);
			var wheelOffsetY = Calc.AngleToVector(angleY, -3.5f);

			leftWheel.Position = Position + (wheelOffsetY - wheelOffsetX);
			rightWheel.Position = Position + (wheelOffsetY + wheelOffsetX);
		}

		private void UpdateLastPositionFor(Entity entity)
		{
			if (entity is Player)
				lastCarryablePositions[entity] = entity.BottomCenter;
			else
				lastCarryablePositions[entity] = entity.Position;
		}

		private void UpdateLastCarryablePositions()
		{
			foreach (Holdable holdable in Scene.Tracker.GetComponents<Holdable>())
			{
				UpdateLastPositionFor(holdable.Entity);
			}

			foreach (Player player in Scene.Tracker.GetEntities<Player>())
			{
				UpdateLastPositionFor(player);
			}
		}

		private bool IsEntering(Entity entity)
		{
			if (!lastCarryablePositions.TryGetValue(entity, out var lastPosition))
				return entity.Bottom < Bottom && CollideCheck(entity);

			if (entity is Player)
				return lastPosition.Y < (Top + 2f) && entity.BottomCenter.Y >= (Top + 2f) && entity.CenterX < Right && entity.CenterX >= Left && CollideCheck(entity);

			var holdable = entity.Get<Holdable>();

			if (holdable != null && holdable.IsHeld)
				return false;

			return entity.Bottom < Bottom && CollideCheck(entity) && !entity.CollideCheck(this, lastPosition);
		}

		public void StartCarrying(Entity entity)
		{
			if (Carrying != null)
				StopCarrying();

			Carrying = entity;

			lastDepth = entity.Depth;

			if (entity is Player player && player.Holding != null)
				lastHoldingDepth = player.Holding.Entity.Depth;

			UpdateCarrying();

			DynamicData.For(entity).Set("ss2024RidingMinecart", this);

			SceneAs<Level>().OnEndOfFrame += () =>
			{
				SceneAs<Level>().Entities.UpdateLists();
			};
		}

		public void StopCarrying()
		{
			if (Carrying == null)
				return;

			Carrying.Depth = lastDepth;

			if (Carrying is Player player)
			{
				if (player.StateMachine.State == StMinecart.Id)
					player.StateMachine.State = Player.StNormal;

				if (player.Holding != null && lastHoldingDepth.HasValue)
					player.Holding.Entity.Depth = lastHoldingDepth.Value;

				var realPosition = player.Position;
				player.Position = Calc.Floor(realPosition);

				player.MoveToX(realPosition.X);
				player.MoveToY(realPosition.Y);

			}
			else
			{
				Carrying.Active = true;

				if (Carrying is Actor actor)
				{
					var realPosition = actor.Position;
					actor.Position = Calc.Floor(realPosition);

					actor.MoveToX(realPosition.X);
					actor.MoveToY(realPosition.Y);
				}
			}

			DynamicData.For(Carrying).Set("ss2024RidingMinecart", null);

			Carrying = null;

			lastHoldingDepth = null;
		}

		private void UpdateCarrying()
		{
			if (Carrying == null)
				return;

			Carrying.Depth = Depth + 1;

			if (Carrying is Player player)
			{
				player.StateMachine.State = StMinecart.Id;
				player.Facing = facing;

				if (player.Holding != null)
					player.Holding.Entity.Depth = Depth + 2;
			}
			else
			{
				Carrying.Active = false;
			}

			var carryAngle = Rotation - (Math.PI / 2f);
			var carryPosition = Position + (11f * new Vector2((float)Math.Cos(carryAngle), (float)Math.Sin(carryAngle)));

			Carrying.BottomCenter = carryPosition;
		}

		private void CheckCarryableEntry()
		{
			foreach (Holdable holdable in Scene.Tracker.GetComponents<Holdable>())
			{
				if (IsEntering(holdable.Entity))
				{
					StartCarrying(holdable.Entity);
					return;
				}
			}

			foreach (Player player in Scene.Tracker.GetEntities<Player>())
			{
				if (IsEntering(player))
				{
					StartCarrying(player);
					return;
				}
			}
		}

		public void PostUpdateEntryCheck()
		{
			if (Carrying == null && !stopped && reentryTimer <= 0f)
				CheckCarryableEntry();

			UpdateLastCarryablePositions();
		}

		private void LaunchCarrying(Vector2 speed)
		{
			if (speed.Y <= 50f)
				speed.Y = Math.Min(-150f, speed.Y);

			var dynData = DynamicData.For(Carrying);

			StopCarrying();

			//Audio.Play("event:/game/06_reflection/crushblock_impact", Center);

			reentryTimer = reentryCooldown;

			if (Carrying is Player player)
			{
				Celeste.Freeze(0.1f);
				player.Speed = speed;
				player.jumpGraceTimer = 0.1f;
			}
			else if (dynData.TryGet("Speed", out var speedVar) && speedVar is Vector2)
			{
				dynData.Set("Speed", speed);
			}
			else if (dynData.TryGet("speed", out var speedVar2) && speedVar2 is Vector2)
			{
				dynData.Set("speed", speed);
			}
		}

		public void SnapToTrack(float nextDistance)
		{
			Track.GetAll(nextDistance, out var point, out var derivative);

			trackDistance = nextDistance;
			Position = Calc.Floor(point);

			if (Carrying != null)
			{
				UpdateCarrying();

				var moveDirection = (int)facing * (int)trackFacing;
				var liftSpeed = derivative * trackSpeed * moveDirection;

				if (Carrying is Actor actor)
				{
					actor.LiftSpeed = liftSpeed;
				}
			}

			Rotation = rotationAdd + derivative.Angle();

			UpdateWheels();
		}

		public void SnapToTrack()
		{
			SnapToTrack(trackDistance);
		}

		public void Stop(bool flip = false)
		{
			stopped = oneUse;
			activated = false;

			if (flip)
			{
				facing = (Facings)(-(int)facing);
			}

			trackSpeed = 0f;

			Track = null;
			TryAttach(Position - (Vector2.UnitY * 2f), Position + (Vector2.UnitY * 2f));
		}

		public override void Update()
		{
			base.Update();

			if (!activated && Carrying != null)
			{
				activated = true;

				Track?.Mover.TriggerPlatform();
			}

			if (reentryTimer > 0f)
				reentryTimer = Calc.Approach(reentryTimer, 0f, Engine.DeltaTime);

			if (!activated)
				return;

			if (Track != null)
				UpdateTrack();
			else
				UpdateOffroad();
		}

		private void UpdateTrack()
		{
			trackSpeed = Calc.Approach(trackSpeed, targetSpeed, approachSpeed * Engine.DeltaTime);

			var moveDirection = (int)facing * (int)trackFacing;
			var endDistance = Calc.ClampedMap(moveDirection, -1f, 1f, 0f, Track.Length);

			var nextDistance = Calc.Approach(trackDistance, endDistance, trackSpeed * Engine.DeltaTime);

			if (nextDistance != endDistance)
			{
				SnapToTrack(nextDistance);
			}
			else
			{
				var point = Track.GetPoint(endDistance);
				var derivative = Track.GetDerivative(trackDistance);

				trackDistance = Track.Length;
				Position = point;

				var currentSpeed = derivative * trackSpeed * moveDirection;

				if ((moveDirection == -1 && Track.StartOpen) || (moveDirection == 1 && Track.EndOpen))
				{
					Track = null;
					Speed = currentSpeed;

					attachDelay = 0.1f;
					rotationAdd = 0f;

					UpdateCarrying();
					UpdateWheels();

					return;
				}

				if (Carrying != null)
				{
					UpdateCarrying();
					LaunchCarrying(currentSpeed);
				}

				Rotation = rotationAdd + derivative.Angle();

				UpdateWheels();

				Stop(true);
			}

			leftWheel.Rotation = Calc.WrapAngle(leftWheel.Rotation + (int)facing * ((float)Math.PI * (trackSpeed / 25f)) * Engine.DeltaTime);
			rightWheel.Rotation = Calc.WrapAngle(rightWheel.Rotation + (int)facing * ((float)Math.PI * (trackSpeed / 25f)) * Engine.DeltaTime);
		}

		private void UpdateOffroad()
		{
			var grounded = OnGround();

			trackSpeed = Calc.Approach(trackSpeed, targetSpeed, approachSpeed * Engine.DeltaTime);

			Speed.X = Calc.Approach(Speed.X, trackSpeed * (int)facing, approachSpeed * Engine.DeltaTime);

			if (grounded)
			{
				Speed.Y = 0f;
				Rotation = 0f;
			}
			else
			{
				var yAccel = 800f;

				if (Math.Abs(Speed.Y) <= 30f)
					yAccel *= 0.5f;

				Speed.Y = Calc.Approach(Speed.Y, 200f, yAccel * Engine.DeltaTime);
				Rotation = Calc.AngleApproach(Rotation, 0f, (float)Math.PI * 2f * Engine.DeltaTime);
			}

			var move = Speed * Engine.DeltaTime;

			if (move.X < 0)
				facing = Facings.Left;
			else if (move.X > 0)
				facing = Facings.Right;

			if (!grounded && attachDelay <= 0f && TryAttach(ExactPosition, ExactPosition + move))
				return;

			attachDelay = Calc.Approach(attachDelay, 0f, Engine.DeltaTime);

			var collidedX = MoveH(move.X);
			var collidedY = MoveV(move.Y);

			if (Carrying != null)
			{
				UpdateCarrying();

				if (Carrying is Actor actor)
				{
					actor.LiftSpeed = move;
				}
			}

			leftWheel.Rotation = Calc.WrapAngle(leftWheel.Rotation + (int)facing * ((float)Math.PI * (Speed.X / 25f)) * Engine.DeltaTime);
			rightWheel.Rotation = Calc.WrapAngle(rightWheel.Rotation + (int)facing * ((float)Math.PI * (Speed.X / 25f)) * Engine.DeltaTime);

			UpdateWheels();

			if (grounded && TryAttach(Position - (Vector2.UnitY * 2f), Position + (Vector2.UnitY * 2f)))
				return;

			if (collidedX)
			{
				if (Carrying != null)
				{
					LaunchCarrying(Speed);
				}
				Speed.X = 0f;
				if (grounded)
				{
					Stop(true);
				}
			}

			if (collidedY)
			{
				if (Speed.Y > 0 && Speed.X == 0f)
				{
					Stop(true);
				}
				Speed.Y = 0f;
			}
		}


		public static void Load()
		{
			IL.Celeste.Holdable.Pickup += Holdable_Pickup;
		}

		public static void Unload()
		{
			IL.Celeste.Holdable.Pickup -= Holdable_Pickup;
		}

		private static void Holdable_Pickup(ILContext il)
		{
			var cursor = new ILCursor(il);

			if (!cursor.TryGotoNext(MoveType.AfterLabel, instr => instr.MatchStfld<Holdable>("idleDepth")))
			{
				Logger.Log(LogLevel.Error, "SecretSanta2024", "Failed to IL hook Holdable.Pickup for Minecart");
				return;
			}

			cursor.Emit(OpCodes.Ldarg_0);
			cursor.EmitDelegate((int depth, Holdable holdable) =>
			{
				if (DynamicData.For(holdable.Entity).TryGet<Minecart>("ss2024RidingMinecart", out var minecart))
				{
					minecart?.StopCarrying();
					return holdable.Entity.Depth;
				}
				return depth;
			});
		}


		public class Outline : Entity
		{
			private Minecart minecart;
			private Image image;

			public Outline(Minecart minecart, Vector2 position) : base(position)
			{
				this.minecart = minecart;

				Add(image = new Image(GFX.Game["objects/ss2024/minecart/body"]));
				image.JustifyOrigin(0.5f, 1f);
				image.Visible = false;

				Depth = minecart.Depth + 7;
			}

			public override void Render()
			{
				Position = minecart.Position;
				image.Rotation = minecart.Body.Rotation;

				image.DrawOutline();

				base.Render();
			}
		}

		public class WheelPart : Entity
		{
			private Image image;

			public float Rotation
			{
				get => image.Rotation;
				set => image.Rotation = value;
			}

			public WheelPart(Minecart minecart, Vector2 position) : base(position)
			{
				Add(image = new Image(GFX.Game["objects/ss2024/minecart/wheel"]));
				image.JustifyOrigin(0.5f, 0.5f);

				Depth = minecart.Depth - 1;
			}

			public override void Render()
			{
				image.DrawOutline();

				base.Render();
			}
		}
	}
}