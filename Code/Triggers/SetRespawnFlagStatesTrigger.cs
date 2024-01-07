//using Celeste.Mod.Entities;
//using Microsoft.Xna.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static Celeste.GaussianBlur;

//namespace Celeste.Mod.SantasGifts24.Code.Triggers
//{
//    [CustomEntity("SS2024/SetRespawnFlagStatesTrigger")]
//    public class SetRespawnFlagStatesTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
//    {
//        private List<string> flagWatch = [.. data.Attr("flags", "").Split(',')];
//        private static SantasGiftsSession session => SantasGiftsModule.Instance.Session;

//        public override void OnEnter(Player player)
//        {
//            base.OnEnter(player);
//            session.respawnFlagMonitor.Clear();
//            foreach (var flag in flagWatch)
//            {
//                session.respawnFlagMonitor[flag] = SceneAs<Level>().Session.GetFlag(flag);
//            }
//        }

//        public static void Load()
//        {
//            On.Celeste.Level.LoadLevel += Level_LoadLevel;
//        }

//        private static void Level_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
//        {
//            orig.Invoke(self, playerIntro, isFromLoader);
//            foreach (KeyValuePair<string, bool> kvp in session.respawnFlagMonitor)
//                self.Session.SetFlag(kvp.Key, kvp.Value);
//        }


//        public static void Unload()
//        {
//            On.Celeste.Level.LoadLevel -= Level_LoadLevel;
//        }
//    }
//}