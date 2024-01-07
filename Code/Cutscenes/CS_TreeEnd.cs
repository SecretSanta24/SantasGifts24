using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod;
using System.Collections;
using System.Reflection;
using Celeste.Mod.Entities;
using MonoMod.Utils;
using Celeste.Mod.SantasGifts24.Code.Entities;


namespace Celeste.Mod.SantasGifts24.Code.Cutscenes
{
    public class CS_TreeEnd : CutsceneEntity
    {
        private Level level;
        private Player player;
        private MysteriousTree tree;

        public CS_TreeEnd(MysteriousTree mt)
        {
            tree = mt;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void OnBegin(Level level)
        {
            tree.Die();
            Add(new Coroutine(Cutscene()));
        }

        private IEnumerator Cutscene()
        {
            while (player == null)
            {
                player = Scene.Tracker.GetEntity<Player>();
                if (player != null)
                {
                    break;
                }
                yield return null;
            }

            player.StateMachine.State = 11;
            player.DummyAutoAnimate = false;
            while (!player.OnGround() || player.Speed.Y < 0f)
            {
                yield return null;
            }

            yield return 3f;
            Audio.Play("event:/Kataiser/sfx/ww2_ssc24_explosion.wav", tree.Position);
            yield return FadeOutLevel();

            EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            if (player != null)
            {
                //player.StateMachine.Locked = false;
                player.StateMachine.State = 0;
                player.Speed.Y = 0f;
                while (player.CollideCheck<Solid>())
                {
                    player.Y--;
                }
                player.DummyAutoAnimate = true;
                player.DummyGravity = true;
                TeleportToEnd();
            }
        }

        private IEnumerator FadeOutLevel()
        {
            new FadeWipe(level, wipeIn: false)
            {
                Duration = 1f
            };
            ScreenWipe.WipeColor = Color.White;
            yield return 1f;
        }

        private void TeleportToEnd()
        {
            LevelData leveldata = level.Session.LevelData;
            Player player = level.Tracker.GetEntity<Player>();
            if (player != null)
            {
                level.OnEndOfFrame += delegate
                {
                    Vector2 position = player.Position;
                    player.Position -= leveldata.Position;
                    level.Camera.Position -= leveldata.Position;
                    int dashes = player.Dashes;
                    float stamina = player.Stamina;
                    Facings facing = player.Facing;
                    level.Session.Level = tree.RoomName;

                    Leader leader = player.Get<Leader>();
                    foreach (Follower follower in leader.Followers)
                    {
                        if (follower.Entity != null)
                        {
                            follower.Entity.AddTag(Tags.Global);
                            level.Session.DoNotLoad.Add(follower.ParentEntityID);
                        }
                    }

                    level.Remove(player);
                    level.UnloadLevel();
                    level.Add(player);
                    level.LoadLevel(Player.IntroTypes.Transition);

                    leveldata = level.Session.LevelData;

                    level.Session.RespawnPoint = level.Session.LevelData.Spawns.ClosestTo(new Vector2(-3000f, 690f));
                    player.Position = level.Session.RespawnPoint.Value;
                    player.Dashes = dashes;
                    player.Stamina = stamina;
                    player.Facing = Facings.Right;
                    level.Camera.Position = level.GetFullCameraTargetAt(player, player.Position);

                    foreach (Follower follower in leader.Followers)
                    {
                        if (follower.Entity != null)
                        {
                            follower.Entity.Position += player.Position - position;
                            follower.Entity.RemoveTag(Tags.Global);
                            level.Session.DoNotLoad.Remove(follower.ParentEntityID);
                        }
                    }

                    for (int i = 0; i < leader.PastPoints.Count; i++)
                    {
                        leader.PastPoints[i] += player.Position - position; ;
                    }
                    leader.TransferFollowers();
                    FadeWipe wipe2 = new FadeWipe(level, wipeIn: true) { Duration = 1f };
                    ScreenWipe.WipeColor = Color.White;
                    wipe2.OnComplete += () =>
                    {
                        ScreenWipe.WipeColor = Color.Black;
                    };
                };
            }
        }

    }
}
