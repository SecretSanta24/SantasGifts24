using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;



namespace Celeste.Mod.SantasGifts24.Code.Entities
{
	[CustomEntity("SS2024/WaterLightning")]
	[Tracked]
	public class WaterLightning : Entity
	{
		public static ParticleType P_Shatter = Lightning.P_Shatter;

		public const string Flag = "disable_lightning";

		public float Fade;

		private bool disappearing;

		private float toggleOffset;

		public int VisualWidth;

		public int VisualHeight;

		public WaterLightning(Vector2 position, int width, int height, Vector2? node, float moveTime)
			: base(position)
		{
			VisualWidth = width;
			VisualHeight = height;
			base.Collider = new Hitbox(width - 2, height - 2, 1f, 1f);
			Add(new PlayerCollider(OnPlayer));
			if (node.HasValue)
			{
				Add(new Coroutine(MoveRoutine(position, node.Value, moveTime)));
			}
			toggleOffset = Calc.Random.NextFloat();
		}

		public WaterLightning(EntityData data, Vector2 levelOffset)
			: this(data.Position + levelOffset, data.Width, data.Height, data.FirstNodeNullable(levelOffset), data.Float("moveTime"))
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			scene.Tracker.GetEntity<WaterLightningRenderer>().Track(this);
		}

		public override void Removed(Scene scene)
		{
			base.Removed(scene);
			scene.Tracker.GetEntity<WaterLightningRenderer>().Untrack(this);
		}

		public override void Update()
		{
			if (SceneAs<Level>().Session.GetFlag("SS2024_level_electricity_flag"))
			{
				Visible = false;
				SceneAs<Level>().Tracker.GetEntity<WaterLightningRenderer>().Visible = false;
			}
			else
            {
				Visible = true;
				SceneAs<Level>().Tracker.GetEntity<WaterLightningRenderer>().Visible = true;
			}
			if (Collidable && base.Scene.OnInterval(0.25f, toggleOffset))
			{
				ToggleCheck();
			}
			if (!Collidable && base.Scene.OnInterval(0.05f, toggleOffset))
			{
				ToggleCheck();
			}
			base.Update();
		}

		public void ToggleCheck()
		{
			Collidable = (Visible = InView());
		}

		private bool InView()
		{
			Camera camera = (base.Scene as Level).Camera;
			if (base.X + base.Width > camera.X - 16f && base.Y + base.Height > camera.Y - 16f && base.X < camera.X + 320f + 16f)
			{
				return base.Y < camera.Y + 180f + 16f;
			}
			return false;
		}

		private void OnPlayer(Player player)
		{
			if(SceneAs<Level>().Session.GetFlag("SS2024_level_electricity_flag"))
            {
				return;
            }
			if (!disappearing && !SaveData.Instance.Assists.Invincible)
			{
				int num = Math.Sign(player.X - base.X);
				if (num == 0)
				{
					num = -1;
				}
				player.Die(Vector2.UnitX * num);
			}
		}

		private IEnumerator MoveRoutine(Vector2 start, Vector2 end, float moveTime)
		{
			while (true)
			{
				yield return Move(start, end, moveTime);
				yield return Move(end, start, moveTime);
			}
		}

		private IEnumerator Move(Vector2 start, Vector2 end, float moveTime)
		{
			float at = 0f;
			while (true)
			{
				Position = Vector2.Lerp(start, end, Ease.SineInOut(at));
				if (at >= 1f)
				{
					break;
				}
				yield return null;
				at = MathHelper.Clamp(at + Engine.DeltaTime / moveTime, 0f, 1f);
			}
		}

		private void Shatter()
		{
			if (base.Scene == null)
			{
				return;
			}
			for (int i = 4; (float)i < base.Width; i += 8)
			{
				for (int j = 4; (float)j < base.Height; j += 8)
				{
					SceneAs<Level>().ParticlesFG.Emit(P_Shatter, 1, base.TopLeft + new Vector2(i, j), Vector2.One * 3f);
				}
			}
		}

		public static IEnumerator PulseRoutine(Level level)
		{
			for (float t2 = 0f; t2 < 1f; t2 += Engine.DeltaTime * 8f)
			{
				SetPulseValue(level, t2);
				yield return null;
			}
			for (float t2 = 1f; t2 > 0f; t2 -= Engine.DeltaTime * 8f)
			{
				SetPulseValue(level, t2);
				yield return null;
			}
			SetPulseValue(level, 0f);
		}

		private static void SetPulseValue(Level level, float t)
		{
			BloomRenderer bloom = level.Bloom;
			WaterLightningRenderer entity = level.Tracker.GetEntity<WaterLightningRenderer>();
			Glitch.Value = MathHelper.Lerp(0f, 0.075f, t);
			bloom.Strength = MathHelper.Lerp(1f, 1.2f, t);
			entity.Fade = t * 0.2f;
		}

		private static void SetBreakValue(Level level, float t)
		{
			BloomRenderer bloom = level.Bloom;
			WaterLightningRenderer entity = level.Tracker.GetEntity<WaterLightningRenderer>();
			Glitch.Value = MathHelper.Lerp(0f, 0.15f, t);
			bloom.Strength = MathHelper.Lerp(1f, 1.5f, t);
			entity.Fade = t * 0.6f;
		}

		public static IEnumerator RemoveRoutine(Level level, Action onComplete = null)
		{
			List<WaterLightning> blocks = level.Entities.FindAll<WaterLightning>();
			foreach (WaterLightning item in new List<WaterLightning>(blocks))
			{
				item.disappearing = true;
				if (item.Right < level.Camera.Left || item.Bottom < level.Camera.Top || item.Left > level.Camera.Right || item.Top > level.Camera.Bottom)
				{
					blocks.Remove(item);
					item.RemoveSelf();
				}
			}
			WaterLightningRenderer entity = level.Tracker.GetEntity<WaterLightningRenderer>();
			entity.StopAmbience();
			entity.UpdateSeeds = false;
			for (float t2 = 0f; t2 < 1f; t2 += Engine.DeltaTime * 4f)
			{
				SetBreakValue(level, t2);
				yield return null;
			}
			SetBreakValue(level, 1f);
			level.Shake();
			for (int num = blocks.Count - 1; num >= 0; num--)
			{
				blocks[num].Shatter();
			}
			for (float t2 = 0f; t2 < 1f; t2 += Engine.DeltaTime * 8f)
			{
				SetBreakValue(level, 1f - t2);
				yield return null;
			}
			SetBreakValue(level, 0f);
			foreach (WaterLightning item2 in blocks)
			{
				item2.RemoveSelf();
			}
			FlingBird flingBird = level.Entities.FindFirst<FlingBird>();
			if (flingBird != null)
			{
				flingBird.LightningRemoved = true;
			}
			onComplete?.Invoke();
		}
	}
}
