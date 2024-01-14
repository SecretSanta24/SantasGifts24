using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using IL.MonoMod;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    // by aurora
    [CustomEntity("SS2024/RngcastleDoor")]
    [Tracked]
    public class RngcastleDoor : Entity
    {
        private Level lvl;
        public static string startingLevel;
        public static string bossLevel;
        public static string endLevel;
        public List<string> otherLevels;
        public static List<string> remainingLevels;
        public static int Health = 0;
        public static bool loseHealth = false;

        TalkComponent Talker;

        Image heart;

        public RngcastleDoor(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            startingLevel = data.Attr("startingLevel", "");
            bossLevel = data.Attr("bossLevel", "");
            endLevel = data.Attr("endLevel", "");
            otherLevels = new List<string>(data.Attr("rooms", "").Split(','));
            string image = data.Attr("heartImage", "checkpoint");
            heart = new Image(GFX.Gui[image]);
            if(remainingLevels == null)
            {
                remainingLevels = otherLevels;
            }
            Vector2 drawAt = new Vector2((float)(data.Width / 2), 0f);
            if (data.Nodes.Length != 0)
            {
                drawAt = data.Nodes[0] - data.Position;
            }
            base.Add(this.Talker = new TalkComponent(new Rectangle(0, 0, data.Width, data.Height+1), drawAt, new Action<Player>(this.OnTalk), null));
            this.Talker.PlayerMustBeFacing = false;
            Tag = Tags.HUD;
        }

        public override void Render()
        {
            base.Render();
            Vector2 pos = new Vector2(1920, 0);
            for(int i = 0; i < Health; i++)
            {
                pos.X -= heart.Width + 10;
                heart.RenderPosition = pos;
                heart.Render();
            }
        }

        public override void Added(Scene scene)
        {
            foreach(RngcastleDoor rngcd in scene.Tracker.GetEntities<RngcastleDoor>())
            {
                if(rngcd != this) rngcd.RemoveSelf();
            }

            base.Added(scene);
            if(loseHealth)
            {
                loseHealth = false;
                Health--;
            }
        }

        public static void Load()
        {
            On.Celeste.PlayerDeadBody.End += PlayerDeadBody_End;
        }

        public static void Unload()
        {
            On.Celeste.PlayerDeadBody.End -= PlayerDeadBody_End;
        }

        private static void PlayerDeadBody_End(On.Celeste.PlayerDeadBody.orig_End orig, PlayerDeadBody self)
        {
            Level lvl = Engine.Scene as Level;
            RngcastleDoor rcd = lvl.Tracker.GetEntity<RngcastleDoor>();

            if(rcd != null && rcd.Active && startingLevel != lvl.Session.Level)
            {
                NextRoom(lvl, true);
            }
            orig(self);
        }

        public static void NextRoom(Level lvl, bool noTeleport = false, bool reduceHealth = true)
        {
            loseHealth = reduceHealth;
            int randomIndex = 0;
            if(remainingLevels.Count > 1)
            {
                do {
                    randomIndex = Calc.Random.Next(remainingLevels.Count);
                } while (randomIndex == remainingLevels.FindIndex(r => r == lvl.Session.Level));
            }

            string newRoom = bossLevel;
            if(Health == 0 && reduceHealth)
            {
                newRoom = startingLevel;
            } else if(remainingLevels.Count != 0)
            {
                newRoom = remainingLevels[randomIndex];
            }
            if(!noTeleport)
            {
                lvl.OnEndOfFrame += delegate ()
                {
                    lvl.TeleportTo(lvl.Tracker.GetEntity<Player>(), newRoom, Player.IntroTypes.Respawn);
                };
            } else
            {
                lvl.Session.Level = newRoom;
                lvl.Session.RespawnPoint = new Vector2?(
                    lvl.GetSpawnPoint(new Vector2((float)lvl.Bounds.Left,
                    (float)lvl.Bounds.Top)));
            }
        }

        public override void Update()
        {
            base.Update();
            if (lvl == null)
            {
                lvl = (Engine.Scene as Level);
                return;
            }
            if (lvl.Session.Level == startingLevel) Health = 0;
        }

        public void OnTalk(Player player)
        {
            if (lvl.Session.Level == bossLevel)
            {
                lvl.OnEndOfFrame += delegate ()
                {
                    lvl.TeleportTo(player, endLevel, Player.IntroTypes.Respawn);
                };
            } else if (lvl.Session.Level == startingLevel)
            {
                remainingLevels = otherLevels;
                Health = 3;
                NextRoom(lvl, false, false);
            } else
            {
                string cleared = lvl.Session.Level;
                remainingLevels.Remove(cleared);
                Health++;
                NextRoom(lvl, false, false);
            }

        }
    }

}
