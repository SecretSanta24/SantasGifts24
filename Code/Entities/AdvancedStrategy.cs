using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Celeste.Mod.SantasGifts24.Entities.AdvancedStrategy.AdvancedStrategyCell;

namespace Celeste.Mod.SantasGifts24.Entities
{
    [Tracked]
    [CustomEntity("SS2024/AdvancedStrategy")]
    public class AdvancedStrategy : Entity
    {
        public class AdvancedStrategyCell : Entity
        {
            public enum CellState
            {
                Covered = 0,
                Uncovered = 1,
                Flagged = 2
            }

            public AdvancedStrategy parent;
            public Point index;
            public int value;
            public CellState state;
            public Rectangle button;
            public bool selected;
            public float flagCooldown;
            public AdvancedStrategyCell(AdvancedStrategy parent, Point index, int value) : base(parent.Position + new Vector2(index.X * 16, index.Y * 16))
            {
                button = new Rectangle((int)X, (int)Y, 16, 16);
                state = CellState.Covered;
                Depth = 1000;
                this.parent = parent;
                this.index = index;
                this.value = value;
            }

            public override void Update()
            {

                base.Update();
                flagCooldown = Math.Max(flagCooldown - Engine.DeltaTime, 0);
                if(parent.player != null)
                {
                    if(button.Contains((int)parent.player.X, (int)parent.player.Y))
                    {
                        selected = true;
                        if((Scene as Level).Session.GetFlag("minesweeper_button") && state != CellState.Uncovered)
                        {
                            (Scene as Level).Session.SetFlag("minesweeper_button", false);
                            if (!parent.generated)
                            {
                                parent.GenerateField(this);
                            }
                            Audio.Play("event:/char/granny/cane_tap", Position);
                            Uncover();
                            CheckWinCondition();
                        }
                        if ((Scene as Level).Session.GetFlag("minesweeper_flag") && state != CellState.Uncovered)
                        {
                            (Scene as Level).Session.SetFlag("minesweeper_flag", false);
                            if (flagCooldown <= 0)
                            {
                                CellState s = state;
                                if (s == CellState.Covered)
                                {
                                    state = CellState.Flagged;
                                }
                                else if (s == CellState.Flagged)
                                {
                                    state = CellState.Covered;
                                }
                                flagCooldown = 0.4f;
                                Audio.Play("event:/char/madeline/core_hair_charged", parent.player.Position);
                            }
                            CheckWinCondition();
                        }
                    } else
                    {
                        selected = false;
                    }
                }
            }

            public void CheckWinCondition()
            {
                if (!parent.won)
                {
                    int num = 0;
                    List<Vector2> poss = new List<Vector2>();
                    for (int i = 0; i < parent.cellArray.GetLength(0); i++)
                    {
                        for (int j = 0; j < parent.cellArray.GetLength(1); j++)
                        {
                            if (parent.cellArray[i, j].state == CellState.Uncovered)
                            {
                                if (parent.cellArray[i, j].value != -1) num++;
                            }
                            else if (parent.cellArray[i, j].state == CellState.Flagged)
                            {
                                if (parent.cellArray[i, j].value == -1) num++;
                                poss.Add(parent.cellArray[i, j].Position + (Vector2.One * 8));
                            }
                        }
                    }
                    if (num >= parent.Size.X * parent.Size.Y)
                    {
                        parent.won = true;
                        (Scene as Level).Session.SetFlag("minesweeper_win", true);
                        for(int i = 0; i < poss.Count; i++)
                        {
                            (Scene as Level).Add(new SummitCheckpoint.ConfettiRenderer(poss[i]));
                            
                        }
                        Audio.Play("event:/new_content/game/10_farewell/pico8_flag", parent.player.Position);
                    }
                }
            }
            public IEnumerator Die()
            {

                for (int i = 0; i < parent.cellArray.GetLength(0); i++)
                {
                    for (int j = 0; j < parent.cellArray.GetLength(1); j++)
                    {
                        parent.cellArray[i, j].state = CellState.Uncovered;
                    }
                }
                yield return 0.5f;
                Player pl = Scene.Tracker.GetEntity<Player>();
                if (pl != null) pl.Die(Vector2.Zero, false, true);
            }

            public void Uncover()
            {
                state = CellState.Uncovered;
                if (value == -1)
                {
                    Audio.Play("event:/char/madeline/mirrortemple_big_landing", Position);
                    Audio.Play("event:/char/oshiro/boss_enter_screen", Position);
                    Add(new Coroutine(Die()));
                }
                if (value == 0)
                {
                    int i = index.X;
                    int j = index.Y;
                    if ((i - 1) >= 0 && (j - 1) >= 0)
                    {
                        if (parent.cellArray[i - 1, j - 1].state != CellState.Uncovered) parent.cellArray[i - 1, j - 1].Uncover();
                    }
                    if ((j - 1) >= 0)
                    {
                        if(parent.cellArray[i, j - 1].state != CellState.Uncovered) parent.cellArray[i, j - 1].Uncover();
                    }
                    if ((i + 1) < parent.cellArray.GetLength(0) && (j - 1) >= 0)
                    {
                        if (parent.cellArray[i + 1, j - 1].state != CellState.Uncovered) parent.cellArray[i + 1, j - 1].Uncover();
                    }
                    if ((i - 1) >= 0)
                    {
                        if (parent.cellArray[i - 1, j].state != CellState.Uncovered) parent.cellArray[i - 1, j].Uncover();
                    }
                    if ((i + 1) < parent.cellArray.GetLength(0))
                    {
                        if (parent.cellArray[i + 1, j].state != CellState.Uncovered) parent.cellArray[i + 1, j].Uncover();
                    }
                    if ((i - 1) >= 0 && (j + 1) < parent.cellArray.GetLength(1))
                    {
                        if (parent.cellArray[i - 1, j + 1].state != CellState.Uncovered) parent.cellArray[i - 1, j + 1].Uncover();
                    }
                    if ((j + 1) < parent.cellArray.GetLength(1))
                    {
                        if (parent.cellArray[i, j + 1].state != CellState.Uncovered) parent.cellArray[i, j + 1].Uncover();
                    }
                    if ((i + 1) < parent.cellArray.GetLength(0) && (j + 1) < parent.cellArray.GetLength(1))
                    {
                        if (parent.cellArray[i + 1, j + 1].state != CellState.Uncovered) parent.cellArray[i + 1, j + 1].Uncover();
                    }
                }
            }



            public override void Render()
            {
                base.Render();


                parent.states[(int)state].Draw(Position);
                if(state == CellState.Uncovered)
                {
                    parent.numbers[value + 1].Draw(Position);
                }
                if (selected && state != CellState.Uncovered) parent.states[3].Draw(Position);
            }

            public override void DebugRender(Camera camera)
            {
                base.DebugRender(camera);
                Draw.HollowRect(button, Color.Red);
                if(value == -1)
                {
                    Draw.Circle(Position + (Vector2.One * 8), 6, Color.Red, 10);
                }
            }
        }

        public Point Size;
        public int mineAmount;
        public bool generated;

        public AdvancedStrategyCell[,] cellArray;
        public List<AdvancedStrategyCell> cellList;

        public MTexture[] numbers;
        public MTexture[] states;

        public Player player;

        public bool won;

        public AdvancedStrategy(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            won = false;
            numbers = new MTexture[10];
            states = new MTexture[4];
            for(int i = 0; i < numbers.Length; i++)
            {
                numbers[i] = GFX.Game["objects/ss2024/strategycells/numbers"].GetSubtexture(i * 16, 0, 16, 16);  
            }
            for (int i = 0; i < states.Length; i++)
            {
                states[i] = GFX.Game["objects/ss2024/strategycells/states"].GetSubtexture(i * 16, 0, 16, 16);
            }

            Size = new Point(data.Width / 16, data.Height / 16);
            mineAmount = data.Int("mines", 9);

            cellArray = new AdvancedStrategyCell[Size.X, Size.Y];
            cellList = new List<AdvancedStrategyCell>();

        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            for (int i = 0; i < cellArray.GetLength(0); i++)
            {
                for (int j = 0; j < cellArray.GetLength(1); j++)
                {
                    cellArray[i, j] = new AdvancedStrategyCell(this, new Point(i, j), 0);
                    cellList.Add(cellArray[i, j]);
                    Scene.Add(cellArray[i, j]);
                }
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = Scene.Tracker.GetEntity<Player>();
        }



        public void GenerateField(AdvancedStrategyCell except)
        {
            Calc.PushRandom((int)(Scene.RawTimeActive * 100));

            List<AdvancedStrategyCell> list = new List<AdvancedStrategyCell>();
            list = cellList;
            list.Remove(except);
            list.Remove(cellArray[except.index.X, except.index.Y]);
            int i2 = except.index.X;
            int j2 = except.index.Y;
            if ((i2 - 1) >= 0 && (j2 - 1) >= 0)
            {
                list.Remove(cellArray[i2 - 1, j2 - 1]);
            }
            if ((j2 - 1) >= 0)
            {
                list.Remove(cellArray[i2, j2 - 1]);
            }
            if ((i2 + 1) < cellArray.GetLength(0) && (j2 - 1) >= 0)
            {
                list.Remove(cellArray[i2 + 1, j2 - 1]);
            }
            if ((i2 - 1) >= 0)
            {
                list.Remove(cellArray[i2 - 1, j2]);
            }
            if ((i2 + 1) < cellArray.GetLength(0))
            {
                list.Remove(cellArray[i2 + 1, j2]);
            }
            if ((i2 - 1) >= 0 && (j2 + 1) < cellArray.GetLength(1))
            {
                list.Remove(cellArray[i2 - 1, j2 + 1]);
            }
            if ((j2 + 1) < cellArray.GetLength(1))
            {
                list.Remove(cellArray[i2, j2 + 1]);
            }
            if ((i2 + 1) < cellArray.GetLength(0) && (j2 + 1) < cellArray.GetLength(1))
            {
                list.Remove(cellArray[i2 + 1, j2 + 1]);
            }

            for (int i = 0; i < mineAmount; i++)
            {
                int rand = Calc.Random.Next(list.Count);
                cellList[rand].value = -1;
                list.Remove(cellList[rand]);
                
            }

            for (int i = 0; i < cellArray.GetLength(0); i++)
            {
                for (int j = 0; j < cellArray.GetLength(1); j++)
                {
                    if (cellArray[i,j].value != -1)
                    {
                        int num = 0;
                        if ((i - 1) >= 0 && (j - 1) >= 0)
                        {
                            if (cellArray[i - 1, j - 1].value == -1) num++;
                        }
                        if ((j - 1) >= 0)
                        {
                            if (cellArray[i, j - 1].value == -1) num++;
                        }
                        if ((i + 1) < cellArray.GetLength(0) && (j - 1) >= 0)
                        {
                            if (cellArray[i + 1, j - 1].value == -1) num++;
                        }
                        if ((i - 1) >= 0)
                        {
                            if (cellArray[i - 1, j].value == -1) num++;
                        }
                        if ((i + 1) < cellArray.GetLength(0))
                        {
                            if (cellArray[i + 1, j].value == -1) num++;
                        }
                        if ((i - 1) >= 0 && (j + 1) < cellArray.GetLength(1))
                        {
                            if (cellArray[i - 1, j + 1].value == -1) num++;
                        }
                        if ((j + 1) < cellArray.GetLength(1))
                        {
                            if (cellArray[i, j + 1].value == -1) num++;
                        }
                        if ((i + 1) < cellArray.GetLength(0) && (j + 1) < cellArray.GetLength(1))
                        {
                            if (cellArray[i + 1, j + 1].value == -1) num++;
                        }
                        cellArray[i, j].value = num;
                    }
                }
            }
            generated = true;
            Calc.PopRandom();
        }
    }
}
