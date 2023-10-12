using Celeste.Mod.SantasGifts24.Code.Entities;
using MonoMod.Utils;

namespace Celeste.Mod.SantasGifts24.Code.States
{
	public static class StMinecart
	{
		public static int Id;

		public static int MinecartUpdate(this Player player)
		{
			if (player.CanUnDuck)
				player.Ducking = false;

			if (player.Holding == null && player.CanDash)
			{
				player.Speed += player.LiftSpeed;

				if (DynamicData.For(player).TryGet<Minecart>("ss2024RidingMinecart", out var minecart))
				{
					minecart?.StopCarrying();
				}

				return player.StartDash();
			}

			if (player.Holding != null && !Input.GrabCheck)
			{
				player.Throw();
			}

			if (Input.Jump.Pressed)
			{
				player.Jump(false, true);

				if (DynamicData.For(player).TryGet<Minecart>("ss2024RidingMinecart", out var minecart))
				{
					minecart?.StopCarrying();
				}

				return Player.StNormal;
			}

			return Id;
		}
	}
}