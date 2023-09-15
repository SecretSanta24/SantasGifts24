using System;
using System.Linq;
using Monocle;

namespace Celeste.Mod.SantasGifts24;

public class AuroraAquir {

	public AuroraAquir() {

	}

	public static void Load() {
        On.Celeste.Level.LoadLevel += Level_LoadLevel;
    }


    public static void Unload()
    {
        On.Celeste.Level.LoadLevel -= Level_LoadLevel;
    }

    private static void Level_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
    {
        if(isFromLoader) 
        {
            doTheThing(self.Session);
        }
        orig(self, playerIntro, isFromLoader);
    }

    private static void doTheThing(Session session)
    {
        // uhhh sorry for random usage? idk how to improve this
        Random random = new Random((int)SaveData.Instance.Time);

        LevelData levelData = session.LevelData;
        EntityData entityData = levelData?.Entities.Find((ed) =>
        {
            return ed.Name == "SS2024/RandomizeStartRoomController";
        });

        string rooms = entityData?.Attr("RoomNames", "");
        Console.WriteLine(rooms);
        if (rooms != null && rooms != "" && rooms.Contains(","))
        {
            string[] roomNames = rooms.Split(',');
            String newRoom = roomNames[random.Range(0, roomNames.Length)];
            LevelData level = session?.MapData?.Levels.Find(ld => ld.Name == newRoom);
            if(level != null && level.Spawns.Count > 0)
            {
                session.Level = newRoom;
                session.RespawnPoint = level.Spawns[0];
            }
        }
    }
}