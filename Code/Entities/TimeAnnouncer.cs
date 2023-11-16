using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    [CustomEntity("SS2024/TimeAnnouncer")]
    internal class TimeAnnouncer : Entity
    {
        public float fade;
        public float fade2;
        public MTexture texture;
        public float alpha;
        public bool started;
        public SoundSource sound;
        public TimeAnnouncer(EntityData data, Vector2 offset) : base(data.Position + offset) 
        {
            base.Tag = Tags.PauseUpdate | Tags.HUD;

            base.Collider = new Hitbox(data.Width, data.Height);
            Add(new PlayerCollider(OnPlayer));
            texture = GFX.Game["objects/ss2024/timecard/six"];
            alpha = 0;
            fade2 = 0;
            sound = new SoundSource();
        }
        public void OnPlayer(Player player)
        {
            if (!started)
            {
                Add(new Coroutine(Routine(player)));
            }
            
        }
        public IEnumerator Routine(Player player)
        {
            started = true;

            sound.Play("event:/sunset_secretsanta/timecard");
            yield return 0.5f;
            alpha = 1;
            player.StateMachine.state = 11;
            yield return 2.0f;
            for (float i = 0; i < 1; i += Engine.DeltaTime * 2)
            {
                fade2 = Ease.SineOut(i);
                yield return null;
            }
            tp(player);
        }

        public void tp(Player player)
        {
            if (player != null && Scene != null)
            {
                Engine.TimeRate = 1;
                Level level = Scene as Level;
                level.OnEndOfFrame += delegate
                {
                    new Vector2(level.LevelOffset.X + (float)level.Bounds.Width - player.X, player.Y - level.LevelOffset.Y);
                    Vector2 levelOffset = level.LevelOffset;
                    Vector2 vector2 = level.Camera.Position - level.LevelOffset;
                    Facings facing = player.Facing;
                    Vector2 pos = player.Position;

                    Leader leader = player.Get<Leader>();
                    foreach (Follower item in leader.Followers.Where((Follower f) => f.Entity != null))
                    {
                        item.Entity.AddTag(Tags.Global);
                        level.Session.DoNotLoad.Add(item.ParentEntityID);
                    }

                    Vector2 cameraDelta = level.Camera.Position - pos;
                    level.Remove(player);
                    level.UnloadLevel();
                    level.Session.Level = "b-00";
                    level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top));

                    level.Session.FirstLevel = false;
                    level.Add(player);
                    level.LoadLevel(Player.IntroTypes.Transition);



                    player.Position = level.Session.RespawnPoint.Value;
                    Vector2 playerDelta = player.Position - pos;
                    level.Camera.Position = player.Position + cameraDelta;
                    if (level.Camera.Position.X < level.Bounds.Left) level.Camera.Position = new Vector2(level.Bounds.Left, level.Camera.Position.Y);
                    if (level.Camera.Position.Y < level.Bounds.Top) level.Camera.Position = new Vector2(level.Camera.Position.X, level.Bounds.Top);
                    if (level.Camera.Position.X + 320 > level.Bounds.Right) level.Camera.Position = new Vector2(level.Bounds.Right, level.Camera.Position.Y);
                    if (level.Camera.Position.Y + 180 > level.Bounds.Bottom) level.Camera.Position = new Vector2(level.Camera.Position.X, level.Bounds.Bottom);


                    player.Facing = facing;
                    player.Hair.MoveHairBy(level.LevelOffset - levelOffset);
                    /*
                    if (level.Wipe != null)
                    {
                        level.Wipe.Cancel();
                    }
                    */
                    player.Visible = true;
                    player.Sprite.Visible = true;

                    foreach (Follower item2 in leader.Followers.Where((Follower f) => f.Entity != null))
                    {
                        item2.Entity.Position += playerDelta;
                        item2.Entity.RemoveTag(Tags.Global);
                        level.Session.DoNotLoad.Remove(item2.ParentEntityID);
                    }
                    for (int i = 0; i < leader.PastPoints.Count; i++)
                    {
                        leader.PastPoints[i] += playerDelta;
                    }
                    leader.TransferFollowers();
                    player.Speed = Vector2.Zero;
                    level.DoScreenWipe(wipeIn: true);
                    level.Add(new DelayedCameraRequest(player, false));
                };
            }
        }
        public override void Update()
        {
            if (!(Scene as Level).Paused)
            {
                base.Update();
            }
            fade = Calc.Approach(fade, (Scene as Level).Paused ? 1f : 0f, 8f * Engine.RawDeltaTime);
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null)
            {
                sound.Stop();
            }
        }

        public override void Render()
        {
            base.Render();
            Color color = Color.Lerp(Color.Lerp(Color.White, Color.Black, fade * 0.7f), Color.Black, fade2) * alpha;

            texture.Draw(Vector2.Zero, Vector2.Zero, color);
        }

    }
}
