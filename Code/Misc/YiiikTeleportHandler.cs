// need to provide the type `Celeste.Mod.QuickAndDirtyDebrisLimiter.QuickAndDirtyDebrisLimiterModule`
// since that's what's referenced in `Assets/SS2024/Gamation/f1Teleport.lua`

using Microsoft.Xna.Framework;
using Monocle;

// ReSharper disable once CheckNamespace
namespace Celeste.Mod.QuickAndDirtyDebrisLimiter;

// ReSharper disable once UnusedType.Global
public class QuickAndDirtyDebrisLimiterModule{
    
    // adapted from https://github.com/Cruor/LuaCutscenes/blob/ec4528808243e537f66d6c45318a8c3d26937510/Helpers/MethodWrappers.cs#L145C9-L204C10 under MIT
	// fixes breaking outback helper portals by adding the player at the correct time
	
	// ReSharper disable once UnusedMember.Global
	public static void InstantTeleportPlusPlus(Scene scene, Player player, string room, bool sameRelativePosition, float positionX, float positionY){
		if(scene is not Level level || player is not { Dead: false })
			return;

		if(string.IsNullOrEmpty(room)){
			Vector2 playerRelativeOffset = new Vector2(positionX, positionY) - player.Position;

			player.Position = new Vector2(positionX, positionY);
			level.Camera.Position += playerRelativeOffset;
			player.Hair.MoveHairBy(playerRelativeOffset);
		}else{
			level.OnEndOfFrame += delegate{
				Vector2 levelOffset = level.LevelOffset;
				Vector2 playerOffset = player.Position - level.LevelOffset;
				Vector2 cameraOffset = level.Camera.Position - level.LevelOffset;
				Facings facing = player.Facing;

				level.Remove(player);
				level.UnloadLevel();
				level.Session.Level = room;
				level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top));
				level.Session.FirstLevel = false;
				
				if(sameRelativePosition){
					level.Add(player);
					player.Position = level.LevelOffset + playerOffset;
					player.Facing = facing;
					player.Hair.MoveHairBy(level.LevelOffset - levelOffset);
				}else{
					Vector2 playerRelativeOffset = new Vector2(positionX, positionY) - level.LevelOffset - playerOffset;

					level.Add(player);
					player.Position = new Vector2(positionX, positionY);
					player.Facing = facing;
					player.Hair.MoveHairBy(level.LevelOffset - levelOffset + playerRelativeOffset);
				}
				
				level.LoadLevel(Player.IntroTypes.Transition);

				if(sameRelativePosition)
					level.Camera.Position = level.LevelOffset + cameraOffset;
				else{
					Vector2 playerRelativeOffset = new Vector2(positionX, positionY) - level.LevelOffset - playerOffset;
					level.Camera.Position = level.LevelOffset + cameraOffset + playerRelativeOffset;
				}
				level.Wipe?.Cancel();
			};
		}
	}
}