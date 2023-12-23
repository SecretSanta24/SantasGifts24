using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    // by aurora
    [CustomEntity("SS2024/RewindController")]
    [Tracked]
    public class RewindController : Entity
    {

        public class RewindState
        {

            public Vector2 Position;
            public Facings Facing;
            public Color HairColor;
            public int Depth;
            public string Animation;
            public int RepeatFrames;
               
            public RewindState(Player player) 
            {
                this.RepeatFrames = 1;
                this.Position = player.Position;
                this.Animation = player.Sprite.CurrentAnimationID;
                this.Facing = player.Facing;
                this.HairColor = player.Hair.Color;
                this.Depth = player.Depth;
            }

            public override bool Equals(object obj)
            {
                if (obj ==  this) return true;
                if (obj is RewindState rs) return this.Position == rs.Position && this.Facing == rs.Facing && this.HairColor == rs.HairColor;
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return Position.GetHashCode() ^ Facing.GetHashCode() ^ HairColor.GetHashCode();
            }
        }

        public static List<RewindState> states;
        public static float timeSinceReset = 0f;
        public static bool reversing = false;
        public static bool blocked = false;
        private string requiredFlag;
        private bool ignoreIfNotMoving;
        private Player player;
        private Level level;
        private Coroutine coroutine;

        private Vector2 secondHand = new Vector2(0, -33f);
        private Vector2 minuteHand = new Vector2(16f, 0f);
        private FMOD.Studio.EventInstance underwater;

        private struct Line
        {
            public Vector2 from;
            public Vector2 to;

            public Line(Vector2 from, Vector2 to)
            {
                this.from = from;
                this.to = to;
            }

        }

        private List<Line> lines;

        public RewindController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            ignoreIfNotMoving = data.Bool("ignoreIfNotMoving", false);
            requiredFlag = data.Attr("requiredFlag", "");
            blocked = false;
            states = new();
            lines = new();
            timeSinceReset = 0f;
            underwater = Audio.CreateSnapshot("snapshot:/underwater", false);
            Depth = Depths.Top;
        }

        public static void Load()
        {
            On.Celeste.Player.Die += Player_Die;
        }


        public static void Unload()
        {
            On.Celeste.Player.Die -= Player_Die;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Audio.EndSnapshot(underwater);
            reversing = false;
            states = new();
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Audio.EndSnapshot(underwater);
            reversing = false;
            states = new();
        }
        private static PlayerDeadBody Player_Die(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            if (reversing) return null;
            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }

        public static RewindState[] StatesToArray()
        {
            int count = 0;
            foreach(var state in states)
            {
                count += state.RepeatFrames;
            }

            RewindState[] rewindStates = new RewindState[count];

            int counter = 0;
            foreach(var state in states)
            {
                for(int i = 0; i < state.RepeatFrames; i++)
                {
                    rewindStates[counter++] = state;
                }
            }

            return rewindStates;
        }

        public static void AddState(Player player, bool doNotRepeatPositions = false)
        {
            RewindState state = new(player);
            if(states.Count == 0)
            {
                states.Add(state);
                return;
            }

            RewindState last = states.Last();
            if (state.Equals(last))
            {
                if (!doNotRepeatPositions) last.RepeatFrames++;
            }
            else states.Add(state);
        }

        public override void Added(Scene scene)
        {
            List<Entity> rewinds = scene.Tracker.GetEntities<RewindController>();

            if (rewinds != null && rewinds.Count > 0)
            {
                foreach(var entity in rewinds)
                {
                    entity.RemoveSelf();
                }
            }
            base.Added(scene);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = scene.Tracker?.GetEntity<Player>();
            level = (scene as Level);
            if (player == null || level == null) RemoveSelf();
        }

        public override void Update()
        {
            if (player == null || level == null)
            {
                player = Engine.Scene.Tracker?.GetEntity<Player>();
                level = (Engine.Scene as Level);
                return;
            }

            base.Update();
            timeSinceReset += Engine.DeltaTime;
            if (reversing)
            {
                float rotation = (float)(-2 * Math.PI) * Engine.DeltaTime;
                secondHand = secondHand.Rotate(rotation);
                minuteHand = minuteHand.Rotate(rotation/60);
            }

            if (!reversing && !blocked && player != null) AddState(player, ignoreIfNotMoving);
            if (!blocked && Input.Grab.Check && timeSinceReset > 1f && states.Count > 0 
                && player != null && !player.Dead && (requiredFlag == "" || level.Session.GetFlag(requiredFlag)))
            {
                if (coroutine != null) return;
                
                Vector2? lastPos = null;
                foreach(RewindState state in states.ToArray())
                {
                    Vector2 pos = state.Position - Vector2.UnitY * 4;
                    if(lastPos != null)
                    {
                        lines.Add(new((Vector2)lastPos, pos));
                    }
                    lastPos = pos;
                }
                base.Add(coroutine = new Coroutine(this.RewindRoutine(player, level), true));
            }
        }

        public override void Render()
        {
            base.Render();
            if (reversing && level != null)
            {
                Color color = Color.White;
                Vector2 pos = level.Camera.Position + new Vector2(level.Camera.Viewport.Width/2, level.Camera.Viewport.Height/2);
                Draw.Line(pos, pos + secondHand, color, 5f);
                Draw.Line(pos, pos + minuteHand, color, 3f);
                Draw.Rect(pos - new Vector2(2f, 2f), 4f, 4f, color);
                Draw.Circle(pos, 34f, color, 5);
                Draw.Circle(pos, 35f, color, 5);
                Draw.Circle(pos, 36f, color, 5);

                foreach(Line line in lines)
                {
                    Draw.Line(line.from, line.to, Color.White, 2f);
                }
            }
        }

        private IEnumerator RewindRoutine(Player player, Level level)
        {
            if (player == null || level == null || player.Dead) yield break;

            Audio.ResumeSnapshot(underwater);
            Audio.SetMusicParam("Ticking", 1);
            reversing = true;
            level.Session.SetFlag("rewinding_time", true);
            string currColorGrade = level.Session.ColorGrade;
            int dashes = player.Dashes;
            level.SnapColorGrade("SS2024/phant/rollback");
            player.StateMachine.State = Player.StDummy;
            player.DummyGravity = false;
            player.ForceCameraUpdate = true;

            foreach (RewindState state in StatesToArray().Reverse())
            {
                if (!Input.Grab.Check || player == null || player.Dead)
                {
                    break;
                }
                //Console.WriteLine($"{state.Position} {state.Facing} {state.HairColor} {state.Depth} {state.Animation} {state.Scale}");
                player.Position = state.Position;
                player.Facing = state.Facing;
                player.Hair.Color = state.HairColor;
                player.Depth = Depths.Top - 1; //state.Depth;
                if(state.Animation != null && state.Animation.Length > 0) player.Sprite.Play(state.Animation);
                //player.Sprite.Scale = state.Scale;
                //player.Hair.Sprite.Position = state.Position;
                state.RepeatFrames--;
                if(lines.Count > 0 && state.RepeatFrames < 1) lines.RemoveAt(lines.Count() - 1);
                yield return 0;
            }

            Audio.MusicUnderwater = false;
            level.SnapColorGrade(currColorGrade);
            Audio.EndSnapshot(underwater);
            int depth = (states.Count > 0 ? states.First().Depth : 0);
            states.Clear();
            lines.Clear();
            timeSinceReset = 0f;
            if (player != null && !player.Dead)
            {
                player.Dashes = dashes;
                player.depth = depth;
                AddState(player);
                player.Speed = Vector2.Zero;
                if (player != null && !player.Dead && player.CollideCheck<Solid>())
                {
                    reversing = false;
                    player.Die(Vector2.Zero);
                }
                player.StateMachine.State = Player.StNormal;
                player.ForceCameraUpdate = false;
                player.DummyGravity = true;
            }
            reversing = false;
            level.Session.SetFlag("rewinding_time", false);
            Audio.SetMusicParam("Ticking", 0);
            coroutine = null;
            yield break;
        }
    }

}
