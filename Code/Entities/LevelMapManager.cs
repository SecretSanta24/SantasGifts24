using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.SantasGifts24;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    [Tracked]
    public class LevelMapManager : Entity
    {
        private bool isShowing;
        private float alpha;
        private Image map;
        private Image cursor;

        public LevelMapManager()
        {
            Tag = Tags.Global | Tags.HUD | Tags.PauseUpdate | Tags.TransitionUpdate;
            Depth = -100000;
            alpha = 0f;
            Add(map = new Image(GFX.Gui["notes/SS2024/ricky06/blueprintGrid"]));
            Add(cursor = new Image(GFX.Gui["notes/SS2024/ricky06/cursor"]));
            map.Position = new Vector2(0f, 0f);
            map.Color = Color.White * alpha;
            cursor.Color = Color.White * alpha;
        }

        public static void Load()
        {
            On.Celeste.Level.LoadLevel += initLevelMap;
        }

        public static void Unload()
        {
            On.Celeste.Level.LoadLevel -= initLevelMap;
        }

        private static void initLevelMap(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader = false)
        {
            orig(self, playerIntro, isFromLoader);
            if (self.Tracker.GetEntity<LevelMapManager>() != null)
            {
                return;
            }
            self.Add(new LevelMapManager());

        }

        public override void Update()
        {
            base.Update();
            Level level = SceneAs<Level>();
            if(level == null || level.Transitioning || level.Paused || level.Tracker.GetEntity<Player>() == null)
            {
                if(level != null)
                {
                    level.FormationBackdrop.Display = false;
                }
                alpha = 0f;
                map.Color = Color.White * alpha;
                cursor.Color = Color.White * alpha;
                isShowing = false;
                return;
            }
            if (!level.Session.GetFlag("SS2024_ricky06_map_obtained"))
            {
                return;
            }

            if (SantasGiftsModule.Settings.MiniMapShow.Pressed)
            {
                isShowing = true;
                level.FormationBackdrop.Display = true;
                level.FormationBackdrop.Alpha = 1f;
            }
            else if(SantasGiftsModule.Settings.MiniMapShow.Released)
            {
                isShowing = false;
                level.FormationBackdrop.Display = false;
            }

            if(isShowing)
            {
                if (alpha < 1f)
                {
                    alpha += 0.04f;
                }
            }
            else
            {
                if(alpha > 0f)
                {
                    alpha -= 0.04f;
                }
            }
            map.Color = Color.White * alpha;
            cursor.Color = Color.White * alpha;
            cursor.Position = calculateCursorPosition(level);
        }

        private Vector2 calculateCursorPosition(Level level)
        {
            if(level == null)
            {
                return cursor.Position;
            }
            Player player = level.Tracker.GetEntity<Player>();
            if(player == null)
            {
                return cursor.Position;
            }
            return 0.233f * player.Position + new Vector2(294.884f, 254.884f);
        }

    }
}
