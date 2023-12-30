using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    public class MysteriousTree : Entity
    {
        public Vector2 BeamOrigin, ShotOrigin;

        private Sprite sprite;
        private StateMachine stateMachine;
        private bool attacking;
        private SoundSource laserSfx, chargeSfx;
        private Level level;
        private Solid nose, trunk;
        private TreeKeyBarrier barrier;
        private string flag1, flag2, flag3;
        private string roomName;

        private int hp = 3;
        
        public MysteriousTree(Vector2 position, string flag1, string flag2, string flag3, string roomName)
            : base(position)
        {
            this.flag1 = flag1;
            this.flag2 = flag2;
            this.flag3 = flag3;
            this.roomName = roomName;
            Depth = 100;
            Add(laserSfx = new SoundSource());
            Add(chargeSfx = new SoundSource());
            Add(sprite = GFX.SpriteBank.Create("mysteriousTree"));
            sprite.Position.X -= 32f;
            Add(stateMachine = new StateMachine());

            stateMachine.SetCallbacks(0, idleUpdate);
            stateMachine.SetCallbacks(1, attackUpdate, attackCoroutine, attackBegin, attackEnd);
            stateMachine.SetCallbacks(2, hurtUpdate, hurtCoroutine, hurtBegin);
            stateMachine.SetCallbacks(3, deathUpdate, deathCoroutine);
            BeamOrigin = Center; //TODO : change with sprite
            ShotOrigin = Center + Vector2.UnitY * 64f; //TODO : change with sprite
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            level = SceneAs<Level>();
            scene.Add(nose = new Solid(new Vector2(Position.X - 32f, Position.Y + 64f), 32f, 16f, false));
            scene.Add(trunk = new Solid(new Vector2(Position.X + 1, Position.Y), 56f, 150f, false));
            scene.Add(barrier = new TreeKeyBarrier(new Vector2(Position.X, Position.Y + 32f), 32f, 32f, this));
        }

        // idle
        private int idleUpdate()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && player.Speed != Vector2.Zero)
            {
                return 1;
            }

            return 0;
        }

        // attack
        private void attackBegin()
        {
            attacking = true;
        }

        private void attackEnd()
        {
            attacking = false;
            foreach (MysteriousTreeBeam mtb in level.Tracker.GetEntities<MysteriousTreeBeam>())
            {
                level.Remove(mtb);
                mtb.RemoveSelf();
            }
        }

        private int attackUpdate()
        {
            return 1;
        }

        private IEnumerator attackCoroutine()
        {
            while (attacking)
            {
                switch (hp)
                {
                    case 3:
                        yield return Beam();
                        yield return 1f;
                        break;
                    case 2:
                        Shoot();
                        yield return 2f;
                        break;
                    default:
                        Add(new Coroutine(Beam()));
                        Shoot();
                        yield return 2f;
                        Shoot();
                        yield return 2f;
                        break;
                }
            }
        }

        // hurt

        private void hurtBegin()
        {
            attacking = false;
        }

        private int hurtUpdate()
        {
            if(attacking)
            {
                return 1;
            }
            return 2;
        }

        private IEnumerator hurtCoroutine()
        {
            yield return 3f;
            attacking = true;
        }

        // death

        private int deathUpdate()
        {
            return 3;
        }

        private IEnumerator deathCoroutine()
        {
            yield return 3f;
            yield return FadeOutLevel();
            TeleportToEnd();
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
                    level.Session.Level = roomName;

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

        private IEnumerator Beam()
        {
            laserSfx.Play("event:/char/badeline/boss_laser_charge");
            yield return 0.1f;
            Player entity = level.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                level.Add(Engine.Pooler.Create<MysteriousTreeBeam>().Init(this, entity));
            }
            yield return 1.4f;
            laserSfx.Stop();
            Audio.Play("event:/char/badeline/boss_laser_fire", Position);
        }

        private void Shoot()
        {

            Audio.Play("event:/char/badeline/boss_bullet", "end", 1f);

            Player entity = level.Tracker.GetEntity<Player>();
            Vector2 at = ShotOrigin;
            level.Add(Engine.Pooler.Create<MysteriousTreeShot>().Init(this, at, entity));
            level.Add(Engine.Pooler.Create<MysteriousTreeShot>().Init(this, at + (Vector2.UnitX * 16f), entity));
            level.Add(Engine.Pooler.Create<MysteriousTreeShot>().Init(this, at - (Vector2.UnitX * 16f), entity));
        }

        public void TakeDamage()
        {
            switch (hp)
            {
                case 3:
                    level.Session.SetFlag(flag1);
                    break;
                case 2:
                    level.Session.SetFlag(flag2);
                    break;
                case 1:
                    level.Session.SetFlag(flag3);
                    break;
            }
            hp -= 1;
            if (hp <= 0)
            {
                stateMachine.State = 3;
            }
            else
            {
                stateMachine.State = 2;
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Remove(nose);
            scene.Remove(trunk);
            scene.Remove(barrier);
        }
    }
}
