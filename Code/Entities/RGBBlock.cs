using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
	// by Aurora Aquir
    [CustomEntity("SS2024/RGBBlock")]
	[Tracked]
    public class RGBBlock : Solid
    {
		private static MTexture[,] nineSliceTexture;
        private static MTexture[,] nineSliceTextureInverse;
        private Color ActiveColor;
		private Color DisabledColor;

        private int colorIndex;
		private Color[] colors = { Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Magenta, Color.Cyan, Color.White, new Color(50, 50, 50)};
		private int[] flags = { 0b100, 0b010, 0b001, 0b110, 0b101, 0b011, 0b111, 0b1000};
        private string[] textureName = { "red", "green", "blue", "yellow", "magenta", "cyan", "white", "black" };
		private Image onTexture;
		private Image offTexture;
        private bool inverse = false;

        private bool lastCollideState = false;
        public RGBBlock(EntityData data, Vector2 offset) : base(data.Position + offset, (float) data.Width, (float) data.Height, false)
        {
            inverse = data.Bool("Inverse", false);
            colorIndex = data.Int("ActiveColor", 0);
			ActiveColor = colors[colorIndex];
            Color color = Calc.HexToColor("667da5");
            DisabledColor = new Color((float)color.R / 255f * ((float)ActiveColor.R / 255f), (float)color.G / 255f * ((float)ActiveColor.G / 255f), (float)color.B / 255f * ((float)ActiveColor.B / 255f), 1f);
            if(colorIndex == 7)
			{
				DisabledColor = ActiveColor;
				ActiveColor = Color.Black;
			}
			if (nineSliceTexture == null)
            {
                MTexture mtexture = GFX.Game["objects/ss2024/rgbblock/block"];
                nineSliceTexture = new MTexture[3, 3];
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        nineSliceTexture[i, j] = mtexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                    }
                }
            }
            if (nineSliceTextureInverse == null)
            {
                MTexture mtexture = GFX.Game["objects/ss2024/rgbblock/block_inverse"];
                nineSliceTextureInverse = new MTexture[3, 3];
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        nineSliceTextureInverse[i, j] = mtexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                    }
                }
            }

            base.Add(onTexture = new(GFX.Game[$"objects/ss2024/rgbblock/{textureName[colorIndex]}_on"]));
            onTexture.Position += new Vector2(data.Width, data.Height)/2;
            onTexture.CenterOrigin();
            base.Add(offTexture = new(GFX.Game[$"objects/ss2024/rgbblock/{textureName[colorIndex]}_off"]));
            offTexture.Position += new Vector2(data.Width, data.Height)/2;
			offTexture.CenterOrigin();
            offTexture.Color = new Color(100, 100, 100);

            if(inverse)
            {
                onTexture.FlipX = true;
                onTexture.FlipY = true;
                offTexture.FlipX = true;
                offTexture.FlipY = true;
            }

           
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (StaticMover staticMover in this.staticMovers)
            {
                Spikes spikes = staticMover.Entity as Spikes;
                if (spikes != null)
                {
                    spikes.EnabledColor = ActiveColor;
                    spikes.DisabledColor = DisabledColor;
                    spikes.VisibleWhenDisabled = true;
                    spikes.SetSpikeColor(ActiveColor);
                }
                Spring spring = staticMover.Entity as Spring;
                if (spring != null)
                {
                    spring.DisabledColor = DisabledColor;
                    spring.VisibleWhenDisabled = true;
                }
            }

            //update
            Level lvl = Engine.Scene switch
            {
                Level level => level,
                LevelLoader loader => loader.Level,
                AssetReloadHelper => (Level)AssetReloadHelper.ReturnToScene,
                _ => throw new Exception("GetCurrentLevel called outside of a level... how did you manage that?")
            };
            Session session = lvl.Session;
            Player player = (Engine.Scene as Level)?.Tracker?.GetEntity<Player>();
            if (session == null) return;

            int flagByte = 0;
            flagByte = (flagByte << 1) | (session.GetFlag("_rgbblock_red") ? 1 : 0);
            flagByte = (flagByte << 1) | (session.GetFlag("_rgbblock_green") ? 1 : 0);
            flagByte = (flagByte << 1) | (session.GetFlag("_rgbblock_blue") ? 1 : 0);

            this.Collidable = (flagByte & flags[colorIndex]) == flags[colorIndex] || (colorIndex == 7 && flagByte == 0);
            if (inverse) this.Collidable = !this.Collidable;
            if (player != null && player.CollideCheck(this)) this.Collidable = false;
            if (this.Collidable)
            {
                this.EnableStaticMovers();
                this.Depth = -2;
                foreach (StaticMover staticMover in this.staticMovers)
                {
                    staticMover.Entity.Depth = -1;
                }
            }
            else
            {
                this.DisableStaticMovers();
                this.Depth = 1;
                foreach (StaticMover staticMover in this.staticMovers)
                {
                    staticMover.Entity.Depth = 2;
                }
            }
            offTexture.Visible = !this.Collidable;
            onTexture.Visible = this.Collidable;
            lastCollideState = this.Collidable;
        }

        public override void Update()
        {
			base.Update();
			Session session = (Engine.Scene as Level)?.Session;
			Player player = (Engine.Scene as Level)?.Tracker?.GetEntity<Player>();
			if (session == null ||  player == null) return;

            int flagByte = 0;
			flagByte = (flagByte << 1) | (session.GetFlag("_rgbblock_red") ? 1 : 0);
            flagByte = (flagByte << 1) | (session.GetFlag("_rgbblock_green") ? 1 : 0);
            flagByte = (flagByte << 1) | (session.GetFlag("_rgbblock_blue") ? 1 : 0);

            this.Collidable = (flagByte & flags[colorIndex]) == flags[colorIndex] || (colorIndex == 7 && flagByte == 0);
            if (inverse) this.Collidable = !this.Collidable;
            if (player.CollideCheck(this)) this.Collidable = false;
            if (lastCollideState != this.Collidable)
            {
                if (this.Collidable)
                {
                    this.EnableStaticMovers();
                    this.Depth = -2;
                    foreach (StaticMover staticMover in this.staticMovers)
                    {
                        staticMover.Entity.Depth = -1;
                    }
                }
                else
                {
                    this.DisableStaticMovers();
                    this.Depth = 1;
                    foreach (StaticMover staticMover in this.staticMovers)
                    {
                        staticMover.Entity.Depth = 2;
                    }
                }
            }
			offTexture.Visible = !this.Collidable;
            onTexture.Visible = this.Collidable;
            lastCollideState = this.Collidable;
        }

        public override void Render()
		{
			float width = base.Width;
			float height = base.Height;
			Vector2 pos = base.Position;
			Color color = ActiveColor;
			if (!this.Collidable) color = DisabledColor;

			MTexture[,] nineSliceTexture = (inverse ? RGBBlock.nineSliceTextureInverse : RGBBlock.nineSliceTexture);

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
