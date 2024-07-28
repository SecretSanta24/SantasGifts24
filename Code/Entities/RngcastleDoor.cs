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
        public static int levelsBeaten;
        public static List<string> otherLevels;
        public static List<string> remainingLevels;
        public static int Health;
        public static bool inRNGcastle = false;
        public static bool dying = false;
        TalkComponent Talker;

        Image heart;

        public static string[] roomsClearedFlags = { "ZeroFlag", "OneFlag", "TwoFlag", "ThreeFlag", "FourFlag", "FiveFlag", "SixFlag", "SevenFlag", "EightFlag" };

        public RngcastleDoor(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            startingLevel = data.Attr("startingLevel", "");
            bossLevel = data.Attr("bossLevel", "");
            endLevel = data.Attr("endLevel", "");
            otherLevels = new List<string>(data.Attr("rooms", "").Split(','));
            string image = data.Attr("heartImage", "checkpoint");
            heart = new Image(GFX.Gui[image]);


            if (remainingLevels == null)
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
            Depth = Depths.Above;
        }

        //float offset = 1;
        int healthCache;
        public override void Render()
        {
            base.Render();
            if (!dying) healthCache = Health;
            if (inRNGcastle)
            {
                //string text = levelsBeaten;
                //Vector2 textMeasure = ActiveFont.Measure(text);
                //if(offset != 0) ActiveFont.DrawOutline(text, pos, Vector2.One, Vector2.One, Color.Yellow * (Engine.Scene.Paused ? 0.5f : 1f) * offset, 2f, Color.Black *offset);

                Vector2 pos = new(1920, 10);
                for (int i = 0; i < healthCache; i++)
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
            dying = false;

            if((scene as Level)?.Session?.Level == startingLevel)
            {
                inRNGcastle = false;
                Health = 3;
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
            if(!self.finished) {
                Level lvl = Engine.Scene as Level;
                RngcastleDoor rcd = lvl.Tracker.GetEntity<RngcastleDoor>();

                if (rcd != null && rcd.Active && startingLevel != lvl.Session.Level) {
                    dying = true;
                    NextRoom(lvl, lvl.Session.Level, true, Health - 1);
                }
            }
            orig(self);
        }

        public static void NextRoom(Level lvl, string ignoredLevel, bool noTeleport = false, int newHealth = -2)
        {
            if (newHealth == -2) newHealth = Health;
            int randomIndex = 0;
            if(remainingLevels.Count > 1)
            {
                randomIndex = Calc.Random.Next(remainingLevels.Count-1);
                if(randomIndex >= remainingLevels.FindIndex(r => r == ignoredLevel)) {
                    randomIndex = (randomIndex+1)%remainingLevels.Count;
                }
            }

            string newRoom = bossLevel;
            if(newHealth <= 0)
            {
                newRoom = startingLevel;
            } else if(remainingLevels.Count != 0)
            {
                newRoom = remainingLevels[randomIndex];
            }

            if(newRoom == lvl.Session.Level && newRoom == bossLevel)
            {
                Health = newHealth;
            }
            else if(!noTeleport)
            {
                Player player = lvl.Tracker.GetEntity<Player>();
                player.StateMachine.State = Player.StDummy;
                lvl.DoScreenWipe(false, delegate () { }, false);
                Alarm.Set(player, 0.5f, delegate ()
                {
                    lvl.OnEndOfFrame += delegate ()
                    {
                        Health = newHealth;
                        lvl.TeleportTo(player, newRoom, Player.IntroTypes.Respawn);
                        inRNGcastle = true;
                        lvl.Session.SetFlag(roomsClearedFlags[levelsBeaten], false);
                        levelsBeaten = (otherLevels.Count - remainingLevels.Count);
                        lvl.Session.SetFlag(roomsClearedFlags[levelsBeaten], true);

                    };
                });
                
            } else
            {
                lvl.Session.Level = newRoom;
                lvl.Session.RespawnPoint = new Vector2?(
                    lvl.GetSpawnPoint(new Vector2((float)lvl.Bounds.Left,
                    (float)lvl.Bounds.Top)));

                Health = newHealth;
            }
        }


        //float wait = 1;
        //float t = 1;
        public override void Update()
        {
            base.Update();
            if (lvl == null)
            {
                lvl = (Engine.Scene as Level);
                return;
            }
            if(lvl?.Session?.GetFlag("hasSilver") ?? false)
            {
                Health = Math.Min(Health, 1);
            }
            //if (wait > 0) wait -= Engine.DeltaTime;
            //if (wait < 0 && wait != -1)
            //{
            //    t = 0;
            //    wait = -1;
            //}
            //if (t < 1)
            //{
            //    t += Engine.DeltaTime;
            //    offset = 1 - Ease.SineIn(t);
            //} else if (wait == -1)
           // {
            //    offset = 0;
            //}
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
                NextRoom(lvl, lvl.Session.Level, false, 3);
            } else
            {
                string cleared = lvl.Session.Level;
                remainingLevels.Remove(cleared);
                NextRoom(lvl, lvl.Session.Level, false, Health+1);
            }

        }
    }

}
