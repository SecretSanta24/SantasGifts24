using System;
using System.Collections;
using System.Reflection;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using On.Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
	public class ZipLineRender : Entity
	{
		private ElectricZipLine zipline;
        private bool bright;
		public ZipLineRender(ElectricZipLine instance, bool bright)
		{
			this.zipline = instance;
            base.Depth = -100;
            this.bright = bright;

        }

        public static ParticleType P_ZiplineElectricity = new ParticleType
        {
            Source = GFX.Game["particles/rect"],
            Color = Calc.HexToColor("dade71"),
            Color2 = Calc.HexToColor("81eef0"),
            ColorMode = ParticleType.ColorModes.Choose,
            FadeMode = ParticleType.FadeModes.Late,
            Size = 0.5f,
            SizeRange = 0.2f,
            RotationMode = ParticleType.RotationModes.Random,
            LifeMin = 0.05f,
            LifeMax = 0.1f,
            SpeedMin = 50f,
            SpeedMax = 70f,
            DirectionRange = (float)Math.PI * 2f
        };

        public override void Update()
        {
			base.Update();
			if(zipline.ElectricityIsOn() /*&& Scene.OnInterval(0.05f)*/)
            {
                float t = (float)Calc.Random.NextDouble();

                Vector2 randomPoint = zipline.left + t * (zipline.right - zipline.left);

                SceneAs<Level>().Particles.Emit(P_ZiplineElectricity, randomPoint);
            }
        }

        public override void Render()
		{
			base.Render();
            Color lightColor = Calc.HexToColor("e3cf81");
            Color darkColor = Calc.HexToColor("59433c");
            Draw.Line(zipline.left - 2*Vector2.UnitY, zipline.right - 2*Vector2.UnitY, Calc.HexToColor("000000"));
			Draw.Line(zipline.left - 1*Vector2.UnitY, zipline.right - 1*Vector2.UnitY, lightColor);
			Draw.Line(zipline.left, zipline.right, darkColor);
			Draw.Line(zipline.left + Vector2.UnitY, zipline.right + Vector2.UnitY, darkColor);
			Draw.Line(zipline.left + 2 * Vector2.UnitY, zipline.right + 2 * Vector2.UnitY, Calc.HexToColor("000000"));
		}
	}
}
