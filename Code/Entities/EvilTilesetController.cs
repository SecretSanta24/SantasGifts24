using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    // by aurora
    [CustomEntity("SS2024/EvilTilesetController")]
    [Tracked]
    public class EvilTilesetController : Entity
    {
        public char evilTileset;
        Player player;
        Level level;
        public EvilTilesetController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            evilTileset = data.Char("tileset", '\0');
        }

        public override void Update()
        {
            base.Update();
            if (evilTileset == '\0') RemoveSelf();
            if (level == null)
            {
                level = (Engine.Scene as Level);
            }
            if (player == null)
            {
                player = (Engine.Scene as Level)?.Tracker.GetEntity<Player>();
                return;
            }

            if (!player.Dead && player.CollideCheck<SolidTiles>())
            {
                int x = (int)(player.Center.X - level.SolidTiles.X) / 8;
                int y = (int)(player.Center.Y - level.SolidTiles.Y) / 8;

                if(level.SolidsData.SafeCheck(x, y) == evilTileset)
                {
                    Vector2 dir = (player.Center - new Vector2(x * 8, y * 8));
                    dir.Normalize();
                    player.Die(dir);
                }
            }
        }
    }

}
