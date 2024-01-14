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
        public static int Health;
        public static bool loseHealth = false;
        public static bool inRNGcastle = false;
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

        float offset = 1;
        public override void Render()
        {
            base.Render();
            if (inRNGcastle)
            {
                string text = (otherLevels.Count - remainingLevels.Count).ToString();
                Vector2 textMeasure = ActiveFont.Measure(text);
                Vector2 pos = new Vector2(1920 - textMeasure.X, heart.Height + textMeasure.Y + 10);
                if(offset != 0) ActiveFont.DrawOutline(text, pos, Vector2.One, Vector2.One, Color.Yellow * (Engine.Scene.Paused ? 0.5f : 1f) * offset, 2f, Color.Black *offset);

                pos = new(1920, 10);
                for (int i = 0; i < Health; i++)
                {
                    pos.X -= heart.Width;
                    heart.RenderPosition = pos;
                    heart.Color = Color.White * (Engine.Scene.Paused ? 0.5f : 1f);
                    heart.Render();
                }

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
                NextRoom(lvl, lvl.Session.Level, true);
            }
            orig(self);
        }

        public static void NextRoom(Level lvl, string ignoredLevel, bool noTeleport = false, bool reduceHealth = true)
        {
            inRNGcastle = true;
            loseHealth = reduceHealth;
            int randomIndex = 0;
            if(remainingLevels.Count > 1)
            {
                do {
                    randomIndex = Calc.Random.Next(remainingLevels.Count);
                } while (randomIndex == remainingLevels.FindIndex(r => r == ignoredLevel));
            }

            string newRoom = bossLevel;
            if(Health == 0 && reduceHealth)
            {
                newRoom = startingLevel;
                inRNGcastle = false;
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


        float wait = 1;
        float t = 1;
        public override void Update()
        {
            base.Update();
            if (lvl == null)
            {
                lvl = (Engine.Scene as Level);
                return;
            }
            if (wait > 0) wait -= Engine.DeltaTime;
            if (wait < 0 && wait != -1)
            {
                t = 0;
                wait = -1;
            }
            if (t < 1)
            {
                t += Engine.DeltaTime;
                offset = 1 - Ease.SineIn(t);
            } else if (wait == -1)
            {
                offset = 0;
            }
        }

        public void OnTalk(Player player)
        {
            if (lvl.Session.Level == bossLevel)
            {
                lvl.OnEndOfFrame += delegate ()
                {
                    lvl.TeleportTo(player, endLevel, Player.IntroTypes.Respawn);
                };
                inRNGcastle = false;
            } else if (lvl.Session.Level == startingLevel)
            {
                remainingLevels = otherLevels;
                Health = 3;
                NextRoom(lvl, lvl.Session.Level, false, false);
            } else
            {
                string cleared = lvl.Session.Level;
                remainingLevels.Remove(cleared);
                Health++;
                NextRoom(lvl, lvl.Session.Level, false, false);
            }

        }
    }

}
