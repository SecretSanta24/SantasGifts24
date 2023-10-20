using Celeste.Mod.Entities;
using Celeste.Mod.SantasGifts24.Code.Helper;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
	[Tracked]
	[CustomEntity("SS2024/MinecartTrack")]
	public class MinecartTrack : Entity
	{
		public BakedCurve Curve;
		public StaticMover Mover;
		public float Length;

		public bool StartOpen;
		public bool EndOpen;

		private Vector2 shakeOffset;

		public MinecartTrack(EntityData data, Vector2 offset) : base(data.Position + offset)
		{
			StartOpen = data.Bool("startOpen");
			EndOpen = data.Bool("endOpen");

			Depth = Depths.SolidsBelow + 10;

			Add(Mover = new StaticMover
			{
				JumpThruChecker = CheckJumpThru,
				SolidChecker = CheckSolid,

				OnMove = OnMove,
				OnShake = (shake) => shakeOffset = shake
			});

			var absoluteNodes = data.NodesWithPosition(offset);
			var relativeNodes = new Vector2[absoluteNodes.Length];

			for (var i = 0; i < absoluteNodes.Length; i++)
				relativeNodes[i] = absoluteNodes[i] - Position;

			Curve = new BakedCurve(relativeNodes, data.Enum<CurveType>("curve"), 24);
			Length = Curve.Length;
		}

		private void OnMove(Vector2 move)
		{
			Position += move;

			foreach (Minecart minecart in Scene.Tracker.GetEntities<Minecart>())
			{
				if (minecart.Track == this)
				{
					minecart.SnapToTrack();
				}
			}
		}

		private bool CheckJumpThru(JumpThru jumpThru)
		{
			var curveSegments = (int)Math.Ceiling(Curve.Length / 6f);

			for (var i = 0; i < curveSegments - 1; i++)
			{
				GetAll(Math.Min(i * 6f, Curve.Length), out var point, out var derivative);

				if (Math.Abs(derivative.Y) > 0.05f)
					continue;

				var lineFrom = point;
				var lineTo = point + Calc.Perpendicular(derivative);

				if (Collide.CheckLine(jumpThru, lineFrom, lineTo))
					return true;
			}

			return false;
		}

		private bool CheckSolid(Solid solid)
		{
			var curveSegments = (int)Math.Ceiling(Curve.Length / 6f);

			for (var i = 0; i < curveSegments - 1; i++)
			{
				GetAll(Math.Min(i * 6f, Curve.Length), out var point, out var derivative);

				var perp = Calc.Perpendicular(derivative);

				var lineFrom = point - perp;
				var lineTo = point + perp;

				if (Collide.CheckLine(solid, lineFrom, lineTo))
					return true;
			}

			return false;
		}

		public Vector2 GetPoint(float distance)
		{
			return Position + Curve.GetPointByDistance(distance);
		}

		public Vector2 GetDerivative(float distance)
		{
			return Curve.GetDerivativeByDistance(distance).SafeNormalize();
		}

		public void GetAll(float distance, out Vector2 point, out Vector2 derivative)
		{
			Curve.GetAllByDistance(distance, out point, out derivative);

			point += Position;
			derivative.Normalize();
		}

		public bool GetIntersect(Vector2 lineFrom, Vector2 lineTo, out Vector2 intersect, out float distance, out Vector2 derivative)
		{
			var curveSegments = (int)Math.Ceiling(Length / 6f);

			for (var i = 0; i < curveSegments - 1; i++)
			{
				var startDist = i * 6f;
				var endDist = Math.Min((i + 1) * 6f, Length);

				var start = GetPoint(startDist);
				var end = GetPoint(endDist + 6f);

				if (Collide.LineCheck(lineFrom, lineTo, start, end, out intersect))
				{
					distance = startDist + Vector2.Distance(start, intersect);

					var intersectDist = Math.Min(distance, Length);
					derivative = GetDerivative(intersectDist);

					return true;
				}
			}

			intersect = Vector2.Zero;
			distance = 0f;
			derivative = Vector2.Zero;

			return false;
		}

		public override void Render()
		{
			var lineSegments = (int)Math.Ceiling(Length / 6f);

			var plankDiv = Length / 24f;

			var plankSegments = (int)Math.Floor(plankDiv);
			var plankOffset = (plankDiv - plankSegments) * 12f;

			var plank = GFX.Game["objects/ss2024/minecart/trackPlank"];

			GetAll(0f, out var firstPoint, out var firstDerivative);
			GetAll(Length, out var lastPoint, out var lastDerivative);

			plank.DrawOutlineJustified(firstPoint + shakeOffset, new Vector2(1f, 0.5f), Color.Black, 1f, firstDerivative.Angle());
			plank.DrawOutlineJustified(lastPoint + shakeOffset, new Vector2(0f, 0.5f), Color.Black, 1f, lastDerivative.Angle());

			for (var i = 0; i < lineSegments - 1; i++)
			{
				var startDist = i * 6f;
				var endDist = Math.Min((i + 1) * 6f, Length);

				var start = shakeOffset + GetPoint(startDist);
				var end = shakeOffset + GetPoint(endDist + 6f);

				Draw.Line(start, end, Color.Black, 3f);
			}

			for (var i = 0; i < plankSegments; i++)
			{
				GetAll((i * 24f) + plankOffset + 12f, out var point, out var derivative);

				plank.DrawOutlineJustified(point + shakeOffset, new Vector2(0.5f, 0.5f), Color.Black, 1f, derivative.Angle());
			}

			for (var i = 0; i < lineSegments - 1; i++)
			{
				var startDist = i * 6f;
				var endDist = Math.Min((i + 1) * 6f, Length);

				var start = shakeOffset + GetPoint(startDist);
				var end = shakeOffset + GetPoint(endDist + 6f);

				Draw.Line(start, end, Color.Gray, 1f);
			}

			for (var i = 0; i < plankSegments; i++)
			{
				GetAll((i * 24f) + plankOffset + 12f, out var point, out var derivative);

				plank.DrawJustified(point + shakeOffset, new Vector2(0.5f, 0.5f), Color.White, 1f, derivative.Angle());
			}

			plank.DrawJustified(firstPoint + shakeOffset, new Vector2(1f, 0.5f), Color.White, 1f, firstDerivative.Angle());
			plank.DrawJustified(lastPoint + shakeOffset, new Vector2(0f, 0.5f), Color.White, 1f, lastDerivative.Angle());

			base.Render();
		}
	}
}