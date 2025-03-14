﻿using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;

namespace Celeste.Mod.SantasGifts24.Code.Entities {
	[TrackedAs(typeof(Spring))]
	[CustomEntity("SS2024/RedirectSpringUp", "SS2024/RedirectSpringLeft", "SS2024/RedirectSpringRight")]
	public class RedirectSpring : Spring {

		private static Orientations GetOrientation(EntityData data) {
			switch (data.Name) {
				default:
				case "SS2024/RedirectSpringUp":
					return Orientations.Floor;
				case "SS2024/RedirectSpringLeft":
					return Orientations.WallLeft;
				case "SS2024/RedirectSpringRight":
					return Orientations.WallRight;
			}
		}

		private float cooldown = 0;
		private Vector2 prevPlayerVelocity;
		private Player player;

		public RedirectSpring(EntityData data, Vector2 offset) : base(data, offset, GetOrientation(data)) {
			Get<PlayerCollider>().OnCollide = OnPlayerCollide;
		}

		public override void Update() {
			base.Update();
			if (cooldown > 0) cooldown -= Engine.DeltaTime;
			if (player == null) {
				player = Scene.Tracker.GetEntity<Player>();
			}
			else {
				prevPlayerVelocity = player.Speed;
			}
		}

		private void OnPlayerCollide(Player player) {
			if (cooldown > 0) return;
			cooldown = 0.05f;
			Vector2 playerSpeed = prevPlayerVelocity;
			if (Orientation == Orientations.Floor) {
				BounceAnimate();
				player.SuperBounce(Top);
				player.Speed.X = playerSpeed.X;
				player.Speed.Y = Calc.Min(player.Speed.Y, -Math.Abs(playerSpeed.Y));
				return;
			}
			if (Orientation == Orientations.WallLeft) {
				player.Speed.X = 0;
				player.SideBounce(1, Right, CenterY);
				BounceAnimate();
				player.Speed.X = Calc.Max(player.Speed.X, Math.Abs(playerSpeed.X) * 1.2f);
				return;
			}
			if (Orientation == Orientations.WallRight) {
				player.Speed.X = 0;
				player.SideBounce(-1, Left, CenterY);
				BounceAnimate();
				player.Speed.X = Calc.Min(player.Speed.X, -Math.Abs(playerSpeed.X) * 1.2f);
				return;
			}
		}

	}
}
