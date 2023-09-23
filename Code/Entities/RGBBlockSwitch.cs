using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Data;
using System.Linq;

namespace Celeste.Mod.SantasGifts24.Entities
{
	// by Aurora Aquir
    [CustomEntity("SS2024/RGBBlockSwitch")]
	[Tracked]
    public class RGBBlockSwitch : Solid
    {
		private static MTexture[,] nineSliceTexture;
        private Color ActiveColor;
		private Color DisabledColor;

        private int colorIndex;
		private Color[] colors = { Color.Red, Color.Green, Color.Blue};
		string flag; 
        private Image onTexture;
        private Image offTexture;
        private bool ResetOnDeath;
        private static bool[] FlagStates = { false, false, false};
        private static string flagStateLevel;
        public RGBBlockSwitch(EntityData data, Vector2 offset) : base(data.Position + offset, (float) data.Width, (float) data.Height, false)
        {
            colorIndex = data.Int("ActiveColor", 0);
            ResetOnDeath = data.Bool("ResetColorsOnDeath", true);
			ActiveColor = colors[colorIndex];
			flag = (colorIndex == 0 ? "_rgbblock_red" : (colorIndex == 1 ? "_rgbblock_green" : "_rgbblock_blue"));

            Color color = Calc.HexToColor("667da5");
            DisabledColor = new Color((float)color.R / 255f * ((float)ActiveColor.R / 255f), (float)color.G / 255f * ((float)ActiveColor.G / 255f), (float)color.B / 255f * ((float)ActiveColor.B / 255f), 1f);
			if (nineSliceTexture == null)
            {
                MTexture mtexture = GFX.Game["objects/ss2024/rgbblock/switch"];
                nineSliceTexture = new MTexture[3, 3];
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        nineSliceTexture[i, j] = mtexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                    }
                }
            }
            string textureName = (colorIndex == 0 ? "red" : (colorIndex == 1 ? "green" : "blue"));
            base.Add(onTexture = new(GFX.Game[$"objects/ss2024/rgbblock/{textureName}_on"]));
            onTexture.Position += new Vector2(data.Width, data.Height-3) / 2;
            onTexture.CenterOrigin();
            base.Add(offTexture = new(GFX.Game[$"objects/ss2024/rgbblock/{textureName}_off"]));
            offTexture.Position += new Vector2(data.Width, data.Height-3) / 2;
            offTexture.CenterOrigin();
            this.OnDashCollide = new DashCollision(this.OnDashed);

            
            if (ResetOnDeath)
            {
                //curtesy of frosthelper
                Level lvl = Engine.Scene switch
                {
                    Level level => level,
                    LevelLoader loader => loader.Level,
                    AssetReloadHelper => (Level)AssetReloadHelper.ReturnToScene,
                    _ => throw new Exception("GetCurrentLevel called outside of a level... how did you manage that?")
                };
                Session session = lvl.Session;
                if (session != null)
                {
                    if (flagStateLevel == lvl.Session.Level)
                    {
                        session.SetFlag("_rgbblock_red", FlagStates[0]);
                        session.SetFlag("_rgbblock_green", FlagStates[1]);
                        session.SetFlag("_rgbblock_blue", FlagStates[2]);
                    } else
                    {
                        flagStateLevel = lvl.Session.Level;
                        FlagStates[0] = session.GetFlag("_rgbblock_red");
                        FlagStates[1] = session.GetFlag("_rgbblock_green");
                        FlagStates[2] = session.GetFlag("_rgbblock_blue");
                    } 
                }
            }
        }

        public static void Load()
        {
            On.Celeste.Level.LoadLevel += Level_LoadLevel;
        }


        public static void Unload()
        {
            On.Celeste.Level.LoadLevel -= Level_LoadLevel;
        }

        private static void Level_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            if (playerIntro == Player.IntroTypes.Transition)
            {
                flagStateLevel = null;
            }
            orig(self, playerIntro, isFromLoader);
        }
        private DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
			Session session = (Scene as Level).Session;
			if(session != null)
			{
				session.SetFlag(flag, !session.GetFlag(flag));
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Audio.Play("event:/game/05_mirror_temple/button_activate", this.Position);
            }

            return DashCollisionResults.Rebound;
        }

        public override void Render()
		{
			float width = base.Width;
			float height = base.Height;
			Vector2 pos = base.Position;
            Session session = (Scene as Level).Session;

            Color color = ActiveColor;
            bool flagVal = session.GetFlag(flag);
            if (!flagVal)
			{
                color = DisabledColor;
            }
            onTexture.Visible = flagVal;
            offTexture.Visible = !flagVal;

            MTexture[,] nineSliceTexture = RGBBlockSwitch.nineSliceTexture;

            int num = (int)(width / 8f);
			int num2 = (int)(height / 8f);

            nineSliceTexture[0, 0].Draw(pos + new Vector2(0f, 0f), Vector2.Zero, color);
			nineSliceTexture[2, 0].Draw(pos + new Vector2(width - 8f, 0f), Vector2.Zero, color);
			nineSliceTexture[0, 2].Draw(pos + new Vector2(0f, height - 8f), Vector2.Zero, color);
			nineSliceTexture[2, 2].Draw(pos + new Vector2(width - 8f, height - 8f), Vector2.Zero, color);
			for (int i = 1; i < num - 1; i++)
			{
				nineSliceTexture[1, 0].Draw(pos + new Vector2((float)(i * 8), 0f), Vector2.Zero, color);
				nineSliceTexture[1, 2].Draw(pos + new Vector2((float)(i * 8), height - 8f), Vector2.Zero, color);
			}
			for (int j = 1; j < num2 - 1; j++)
			{
				nineSliceTexture[0, 1].Draw(pos + new Vector2(0f, (float)(j * 8)), Vector2.Zero, color);
				nineSliceTexture[2, 1].Draw(pos + new Vector2(width - 8f, (float)(j * 8)), Vector2.Zero, color);
            }
            for (int k = 1; k < num - 1; k++)
            {
                for (int l = 1; l < num2 - 1; l++)
                {
                    nineSliceTexture[1, 1].Draw(pos + new Vector2((float)k, (float)l) * 8f, Vector2.Zero, color);
                }
            }
            base.Render();
        }

    }
}
