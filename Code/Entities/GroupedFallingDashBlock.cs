using System;
using System.Collections;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
	[CustomEntity("SS2024/GroupedFallingDashBlock")]
	[Tracked]
	class GroupedFallingDashBlock : Solid
	{
		public static ParticleType P_FallDustA = FallingBlock.P_FallDustA;

		public static ParticleType P_FallDustB = FallingBlock.P_FallDustB;

		public static ParticleType P_LandDust = FallingBlock.P_LandDust;

		private TileGrid _tiles;

		private readonly char _tileType;

		private GroupedFallingDashBlock _master;

		private bool _awake;

		private TileGrid _highlight;

		private bool _climbFall;

		public List<GroupedFallingDashBlock> Group;

		public List<JumpThru> Jumpthrus;

		public Point GroupBoundsMin;

		public Point GroupBoundsMax;

		public bool Triggered;

		public float FallDelay;

		public bool HasStartedFalling { get; private set; }

		public bool HasGroup { get; private set; }

		public bool MasterOfGroup { get; private set; }

		public Vector2 GroupPosition => new Vector2(GroupBoundsMin.X, GroupBoundsMin.Y);


		public GroupedFallingDashBlock(Vector2 position, float width, float height, char tileType, bool climbFall)
			: base(position, width, height, safe: true)
		{
			_climbFall = climbFall;
			_tileType = tileType;
			base.Depth = -9000;
			Add(new LightOcclude());
			SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
			Add(new TileInterceptor(_tiles, highPriority: false));
			OnDashCollide = OnDashed;
		}

		public GroupedFallingDashBlock(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.Char("tiletype", '3'), data.Bool("climbFall", defaultValue: true))
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			_awake = true;
			if (!HasGroup)
			{
				MasterOfGroup = true;
				Group = new List<GroupedFallingDashBlock>();
				Jumpthrus = new List<JumpThru>();
				GroupBoundsMin = new Point((int)base.X, (int)base.Y);
				GroupBoundsMax = new Point((int)base.Right, (int)base.Bottom);
				AddToGroupAndFindChildren(this);
				Rectangle rectangle = new Rectangle(GroupBoundsMin.X / 8, GroupBoundsMin.Y / 8, (GroupBoundsMax.X - GroupBoundsMin.X) / 8 + 1, (GroupBoundsMax.Y - GroupBoundsMin.Y) / 8 + 1);
				VirtualMap<char> virtualMap = new VirtualMap<char>(rectangle.Width, rectangle.Height, '0');
				foreach (GroupedFallingDashBlock item in Group)
				{
					int num = (int)(item.X / 8f) - rectangle.X;
					int num2 = (int)(item.Y / 8f) - rectangle.Y;
					int num3 = (int)(item.Width / 8f);
					int num4 = (int)(item.Height / 8f);
					for (int i = num; i < num + num3; i++)
					{
						for (int j = num2; j < num2 + num4; j++)
						{
							virtualMap[i, j] = _tileType;
						}
					}
				}
				_tiles = GFX.FGAutotiler.GenerateMap(virtualMap, new Autotiler.Behaviour
				{
					EdgesExtend = false,
					EdgesIgnoreOutOfLevel = false,
					PaddingIgnoreOutOfLevel = false
				}).TileGrid;
				_tiles.Position = new Vector2((float)GroupBoundsMin.X - base.X, (float)GroupBoundsMin.Y - base.Y);
				Add(_tiles);
			}
			if (MasterOfGroup)
			{
				Add(new Coroutine(Sequence()));
				Group.Sort((GroupedFallingDashBlock block, GroupedFallingDashBlock otherBlock) => otherBlock.Bottom.CompareTo(block.Bottom));
			}
		}

		public void Trigger()
		{
			if (MasterOfGroup)
			{
				Triggered = true;
			}
			else
			{
				_master.Triggered = true;
			}
		}

		private void AddToGroupAndFindChildren(GroupedFallingDashBlock from)
		{
			if (from.X < (float)GroupBoundsMin.X)
			{
				GroupBoundsMin.X = (int)from.X;
			}
			if (from.Y < (float)GroupBoundsMin.Y)
			{
				GroupBoundsMin.Y = (int)from.Y;
			}
			if (from.Right > (float)GroupBoundsMax.X)
			{
				GroupBoundsMax.X = (int)from.Right;
			}
			if (from.Bottom > (float)GroupBoundsMax.Y)
			{
				GroupBoundsMax.Y = (int)from.Bottom;
			}
			from.HasGroup = true;
			Group.Add(from);
			if (from != this)
			{
				from._master = this;
			}
			foreach (JumpThru item in base.Scene.CollideAll<JumpThru>(new Rectangle((int)from.X - 1, (int)from.Y, (int)from.Width + 2, (int)from.Height)))
			{
				if (!Jumpthrus.Contains(item))
				{
					AddJumpThru(item);
				}
			}
			foreach (JumpThru item2 in base.Scene.CollideAll<JumpThru>(new Rectangle((int)from.X, (int)from.Y - 1, (int)from.Width, (int)from.Height + 2)))
			{
				if (!Jumpthrus.Contains(item2))
				{
					AddJumpThru(item2);
				}
			}
			foreach (GroupedFallingDashBlock entity in base.Scene.Tracker.GetEntities<GroupedFallingDashBlock>())
			{
				if (!entity.HasGroup && entity._tileType == _tileType && (base.Scene.CollideCheck(new Rectangle((int)from.X - 1, (int)from.Y, (int)from.Width + 2, (int)from.Height), entity) || base.Scene.CollideCheck(new Rectangle((int)from.X, (int)from.Y - 1, (int)from.Width, (int)from.Height + 2), entity)))
				{
					AddToGroupAndFindChildren(entity);
				}
			}
		}

		private void AddJumpThru(JumpThru jp)
		{
			Jumpthrus.Add(jp);
			foreach (GroupedFallingDashBlock entity in base.Scene.Tracker.GetEntities<GroupedFallingDashBlock>())
			{
				if (!entity.HasGroup && entity._tileType == _tileType && base.Scene.CollideCheck(new Rectangle((int)jp.X - 1, (int)jp.Y, (int)jp.Width + 2, (int)jp.Height), entity))
				{
					AddToGroupAndFindChildren(entity);
				}
			}
		}

		public override void OnStaticMoverTrigger(StaticMover sm)
		{
			if (MasterOfGroup)
			{
				Triggered = true;
			}
			else
			{
				_master.Triggered = true;
			}
		}

		public override void OnShake(Vector2 amount)
		{
			if (!MasterOfGroup)
			{
				return;
			}
			base.OnShake(amount);
			_tiles.Position += amount;
			foreach (JumpThru jumpthru in Jumpthrus)
			{
				foreach (Component component in jumpthru.Components)
				{
					if (component is Image image)
					{
						image.Position += amount;
					}
				}
			}
		}

		private IEnumerator Sequence()
		{
			while (!Triggered && !PlayerFallCheck())
			{
				yield return null;
			}
			while (FallDelay > 0f)
			{
				FallDelay -= Engine.DeltaTime;
				yield return null;
			}
			HasStartedFalling = true;
			while (true)
			{
				ShakeSfx();
				StartShaking();
				Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
				yield return 0.2f;
				float timer = 0.4f;
				while (timer > 0f && PlayerWaitCheck())
				{
					yield return null;
					timer -= Engine.DeltaTime;
				}
				StopShaking();
				foreach (GroupedFallingDashBlock block2 in Group)
				{
					for (int i = 2; (float)i < base.Width; i += 4)
					{
						if (block2.CollideCheck<Solid>(block2.TopLeft + new Vector2(i, -2f)))
						{
							SceneAs<Level>().Particles.Emit(P_FallDustA, 2, new Vector2(block2.X + (float)i, block2.Y), Vector2.One * 4f, (float)Math.PI / 2f);
						}
						SceneAs<Level>().Particles.Emit(P_FallDustB, 2, new Vector2(block2.X + (float)i, block2.Y), Vector2.One * 4f);
					}
				}
				float speed = 0f;
				float maxSpeed = 160f;
				List<GroupedFallingDashBlock> hitters;
				while (true)
				{
					Level level = SceneAs<Level>();
					speed = Calc.Approach(speed, maxSpeed, 500f * Engine.DeltaTime);
					bool breakCollideCheck = false;
					hitters = new List<GroupedFallingDashBlock>();
					foreach (GroupedFallingDashBlock block6 in Group)
					{
						if (block6.MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true))
						{
							breakCollideCheck = true;
							hitters.Add(block6);
						}
					}
					foreach (JumpThru jp3 in Jumpthrus)
					{
						jp3.MoveV(speed * Engine.DeltaTime);
					}
					if (breakCollideCheck)
					{
						break;
					}
					if ((float)GroupBoundsMin.Y > (float)(level.Bounds.Bottom + 16) || ((float)GroupBoundsMin.Y > (float)(level.Bounds.Bottom - 1) && CollideCheck<Solid>(GroupPosition + new Vector2(0f, 1f))))
					{
						Collidable = (Visible = false);
						yield return 0.2f;
						if (level.Session.MapData.CanTransitionTo(level, new Vector2(base.Center.X, base.Bottom + 12f)))
						{
							yield return 0.2f;
							SceneAs<Level>().Shake();
							Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
						}
						foreach (GroupedFallingDashBlock block in Group)
						{
							block.RemoveSelf();
							block.DestroyStaticMovers();
						}
						foreach (JumpThru jp in Jumpthrus)
						{
							jp.RemoveSelf();
							jp.DestroyStaticMovers();
						}
						yield break;
					}
					yield return null;
				}
				foreach (GroupedFallingDashBlock block5 in Group)
				{
					if (!hitters.Contains(block5))
					{
						block5.MoveV((0f - speed) * Engine.DeltaTime);
					}
				}
				foreach (JumpThru jp2 in Jumpthrus)
				{
					jp2.MoveV((0f - speed) * Engine.DeltaTime);
				}
				ImpactSfx();
				Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
				SceneAs<Level>().DirectionalShake(Vector2.UnitY);
				StartShaking();
				LandParticles();
				yield return 0.2f;
				StopShaking();
				bool collideSolidTiles = false;
				foreach (GroupedFallingDashBlock block4 in Group)
				{
					if (block4.CollideCheck<SolidTiles>(block4.Position + new Vector2(0f, 1f)))
					{
						collideSolidTiles = true;
						break;
					}
				}
				if (collideSolidTiles)
				{
					break;
				}
				bool collidePlatforms;
				do
				{
					collidePlatforms = false;
					foreach (GroupedFallingDashBlock block3 in Group)
					{
						foreach (Platform platform in base.Scene.Tracker.GetEntities<Platform>())
						{
							if ((platform is GroupedFallingDashBlock && Group.Contains(platform as GroupedFallingDashBlock)) || !block3.CollideCheck(platform, block3.Position + new Vector2(0f, 1f)))
							{
								continue;
							}
							collidePlatforms = true;
							break;
						}
						if (collidePlatforms)
						{
							break;
						}
					}
					if (collidePlatforms)
					{
						yield return 0.1f;
					}
				}
				while (collidePlatforms);
			}
			Safe = true;
		}
		private DashCollisionResults OnDashed(Player player, Vector2 direction)
		{
			if(MasterOfGroup) { 
				Break(player.Center, direction);
			}
			else
            {
				_master.Break(player.Center, direction);
            }
			return DashCollisionResults.Rebound;
		}


		public void Break(Vector2 from, Vector2 direction, bool playSound = true, bool playDebrisSound = true)
		{
			if (playSound)
			{
				if (_tileType == '1')
				{
					Audio.Play("event:/game/general/wall_break_dirt", Position);
				}
				else if (_tileType == '3')
				{
					Audio.Play("event:/game/general/wall_break_ice", Position);
				}
				else if (_tileType == '9')
				{
					Audio.Play("event:/game/general/wall_break_wood", Position);
				}
				else
				{
					Audio.Play("event:/game/general/wall_break_stone", Position);
				}
			}

			Collidable = false;
			foreach (GroupedFallingDashBlock blockToBreak in Group)
			{
				for (int i = 0; (float)i < blockToBreak.Width / 8f; i++)
				{
					for (int j = 0; (float)j < blockToBreak.Height / 8f; j++)
					{
						base.Scene.Add(Engine.Pooler.Create<Debris>().Init(blockToBreak.Position + new Vector2(4 + i * 8, 4 + j * 8), _tileType, playDebrisSound).BlastFrom(from));
					}
				}
				blockToBreak.RemoveSelf();
			}
		}

		private bool PlayerFallCheck()
		{
			foreach (GroupedFallingDashBlock item in Group)
			{
				if (item._climbFall)
				{
					if (item.HasPlayerRider())
					{
						return true;
					}
				}
				else if (item.HasPlayerOnTop())
				{
					return true;
				}
			}
			foreach (JumpThru jumpthru in Jumpthrus)
			{
				if (jumpthru.HasPlayerRider())
				{
					return true;
				}
			}
			return false;
		}

		private bool PlayerWaitCheck()
		{
			if (Triggered)
			{
				return true;
			}
			if (PlayerFallCheck())
			{
				return true;
			}
			if (_climbFall)
			{
				foreach (GroupedFallingDashBlock item in Group)
				{
					if (!item.CollideCheck<Player>(Position - Vector2.UnitX))
					{
						if (CollideCheck<Player>(Position + Vector2.UnitX))
						{
							return true;
						}
						continue;
					}
					return true;
				}
			}
			return false;
		}

		private void LandParticles()
		{
			foreach (GroupedFallingDashBlock item in Group)
			{
				for (int i = 2; (float)i <= item.Width; i += 4)
				{
					foreach (Solid entity in base.Scene.Tracker.GetEntities<Solid>())
					{
						if (!(entity is GroupedFallingDashBlock) && item.CollideCheck(entity, item.Position + new Vector2(i, 3f)))
						{
							SceneAs<Level>().ParticlesFG.Emit(P_FallDustA, 1, new Vector2(item.X + (float)i, item.Bottom), Vector2.One * 4f, -(float)Math.PI / 2f);
							float direction = ((!((float)i < item.Width / 2f)) ? 0f : ((float)Math.PI));
							SceneAs<Level>().ParticlesFG.Emit(P_LandDust, 1, new Vector2(item.X + (float)i, item.Bottom), Vector2.One * 4f, direction);
						}
					}
				}
			}
		}

		private void ShakeSfx()
		{
			Vector2 groupCenter = GetGroupCenter();
			if (_tileType == '3')
			{
				Audio.Play("event:/game/01_forsaken_city/fallblock_ice_shake", groupCenter);
			}
			else if (_tileType == '9')
			{
				Audio.Play("event:/game/03_resort/fallblock_wood_shake", groupCenter);
			}
			else if (_tileType == 'g')
			{
				Audio.Play("event:/game/06_reflection/fallblock_boss_shake", groupCenter);
			}
			else
			{
				Audio.Play("event:/game/general/fallblock_shake", groupCenter);
			}
		}

		private void ImpactSfx()
		{
			Vector2 groupBottomCenter = GetGroupBottomCenter();
			if (_tileType == '3')
			{
				Audio.Play("event:/game/01_forsaken_city/fallblock_ice_impact", groupBottomCenter);
			}
			else if (_tileType == '9')
			{
				Audio.Play("event:/game/03_resort/fallblock_wood_impact", groupBottomCenter);
			}
			else if (_tileType == 'g')
			{
				Audio.Play("event:/game/06_reflection/fallblock_boss_impact", groupBottomCenter);
			}
			else
			{
				Audio.Play("event:/game/general/fallblock_impact", groupBottomCenter);
			}
		}

		private Vector2 GetGroupCenter()
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			foreach (GroupedFallingDashBlock item in Group)
			{
				float num4 = item.Width * item.Height;
				float num5 = item.CenterX - base.CenterX;
				float num6 = item.CenterY - base.CenterY;
				num += num4;
				num2 += num5 * num4;
				num3 += num6 * num4;
			}
			return base.Center + new Vector2(num2 / num, num3 / num);
		}

		private Vector2 GetGroupBottomCenter()
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = float.MinValue;
			foreach (GroupedFallingDashBlock item in Group)
			{
				float num4 = item.Width * item.Height;
				float num5 = item.CenterX - base.CenterX;
				num += num4;
				num2 += num5 * num4;
				if (item.Bottom > num3)
				{
					num3 = item.Bottom;
				}
			}
			return new Vector2(base.CenterX + num2 / num, num3);
		}
	}
}
