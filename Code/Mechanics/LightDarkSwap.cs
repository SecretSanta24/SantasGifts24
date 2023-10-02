using Celeste.Mod.SantasGifts24.Code.Components;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Mechanics {

	public enum LightDarkMode {
		Normal = 0,
		Dark = 1,
	}

	internal static class LightDarkSwapMethods {
		private static readonly string LDModeKey = "SantasGifts24_LDMode";
		private static readonly string LDNormalFlag = "SantasGifts24_LDNormal";
		private static readonly string LDDarkFlag = "SantasGifts24_LDDark";

		internal static void Load() {
			Everest.Events.Level.OnTransitionTo += OnTransition;
			Everest.Events.Level.OnLoadLevel += OnLoadLevel;
		}

		internal static void Unload() {
			Everest.Events.Level.OnTransitionTo -= OnTransition;
			Everest.Events.Level.OnLoadLevel -= OnLoadLevel;
		}

		private static void OnLoadLevel(Level level, Player.IntroTypes introType, bool isFromLoader) {
			level.LightDarkSet(SantasGiftsModule.Instance?.Session?.LightDark ?? LightDarkMode.Normal);
		}

		internal static void OnTransition(Level level, LevelData next, Vector2 _direction) {
			if (SantasGiftsModule.Instance.Session != null) {
				SantasGiftsModule.Instance.Session.LightDark = level.LightDarkGet();
			}
		}

		internal static LightDarkMode LightDarkGet(this Level level) {
			DynamicData dd = DynamicData.For(level);
			return dd.Data.ContainsKey(LDModeKey) ? dd.Get<LightDarkMode>(LDModeKey) : SantasGiftsModule.Instance.Session?.LightDark ?? LightDarkMode.Normal;
		}

		internal static void LightDarkSet(this Level level, LightDarkMode newMode, bool persistent = false) {
			if (level.LightDarkGet() == newMode) return;
			level.Flash(newMode == LightDarkMode.Dark ? Color.Black * 0.6f : Color.AntiqueWhite * 0.25f);
			DynamicData dd = DynamicData.For(level);
			dd.Set(LDModeKey, newMode);
			if (persistent) {
				SantasGiftsModule.Instance.Session.LightDark = newMode;
			}
			List<Component> listeners = level.Tracker.GetComponents<LightDarkListener>();
			foreach (LightDarkListener listener in listeners.Cast<LightDarkListener>()) {
				listener.NotifyChange(newMode);
			}
			level.Session.SetFlag(LDNormalFlag, newMode == LightDarkMode.Normal);
			level.Session.SetFlag(LDDarkFlag, newMode == LightDarkMode.Dark);
		}

		internal static void LightDarkSwap(this Level level, bool persistent = false) {
			switch (level.LightDarkGet()) {
				case LightDarkMode.Normal:
					level.LightDarkSet(LightDarkMode.Dark, persistent);
					break;
				case LightDarkMode.Dark:
					level.LightDarkSet(LightDarkMode.Normal, persistent);
					break;
			}
		}

	}
}
