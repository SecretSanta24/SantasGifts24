using Celeste.Mod.SantasGifts24.Code.Components;
using Celeste.Mod.SantasGifts24.Code.Entities.LightDark;
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
		private static readonly string LDNormalFlag = "SantasGifts24_LDNormal";
		private static readonly string LDDarkFlag = "SantasGifts24_LDDark";

		internal static void Load() {
			Everest.Events.Level.OnTransitionTo += OnTransition;
			On.Celeste.Level.LoadLevel += OnLevelLoadLevel;
			On.Celeste.Solid.ctor += OnSolidCtor;
			On.Celeste.FakeWall.ctor_EntityID_EntityData_Vector2_Modes += OnFakeWallCtor;
		}

		internal static void Unload() {
			Everest.Events.Level.OnTransitionTo -= OnTransition;
			On.Celeste.Level.LoadLevel -= OnLevelLoadLevel;
			On.Celeste.Solid.ctor -= OnSolidCtor;
			On.Celeste.FakeWall.ctor_EntityID_EntityData_Vector2_Modes -= OnFakeWallCtor;
		}

		private static void OnLevelLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
			orig(self, playerIntro, isFromLoader);
			foreach (var controller in self.Tracker.GetEntities<LightDarkTilesController>().Cast<LightDarkTilesController>()) {
				controller.OnAfterLevelLoad(self);
			}
			self.LightDarkSet(SantasGiftsModule.Instance?.Session?.LightDarkPersistent ?? LightDarkMode.Normal);
		}

		private static void OnSolidCtor(On.Celeste.Solid.orig_ctor orig, Solid self, Vector2 position, float width, float height, bool safe) {
			orig(self, position, width, height, safe);
			if (self is DashBlock or FallingBlock) {
				self.Add(new LightDarkTilesHandler());
			}
		}

		private static void OnFakeWallCtor(On.Celeste.FakeWall.orig_ctor_EntityID_EntityData_Vector2_Modes orig, FakeWall self, EntityID eid, EntityData data, Vector2 offset, FakeWall.Modes mode) {
			orig(self, eid, data, offset, mode);
			self.Add(new LightDarkTilesHandler());
		}

		internal static void OnTransition(Level level, LevelData next, Vector2 _direction) {
			if (SantasGiftsModule.Instance.Session != null) {
				SantasGiftsModule.Instance.Session.LightDarkPersistent = level.LightDarkGet();
			}
		}

		internal static LightDarkMode LightDarkGet(this Level level) {
			return SantasGiftsModule.Instance.Session?.LightDark ?? LightDarkMode.Normal;
		}

		internal static LightDarkMode? LightDarkGetRaw(this Level level) {
			return SantasGiftsModule.Instance.Session?.LightDark;
		}

		internal static void LightDarkSet(this Level level, LightDarkMode newMode, bool persistent = false) {
			LightDarkMode? oldLDM = level.LightDarkGetRaw();
			if (level.LightDarkGetRaw() == newMode) return;
			if (oldLDM != null) level.Flash(newMode == LightDarkMode.Dark ? Color.Black * 0.6f : Color.AntiqueWhite * 0.2f);
			SantasGiftsModule.Instance.Session.LightDark = newMode;
			if (persistent) {
				SantasGiftsModule.Instance.Session.LightDarkPersistent = newMode;
			}
			level.Session.Audio.Music.Layer(1, newMode == LightDarkMode.Dark);
			level.Session.Audio.Apply();
			level.Session.SetFlag(LDNormalFlag, newMode == LightDarkMode.Normal);
			level.Session.SetFlag(LDDarkFlag, newMode == LightDarkMode.Dark);
			List<Component> listeners = level.Tracker.GetComponents<LightDarkListener>();
			foreach (LightDarkListener listener in listeners.Cast<LightDarkListener>()) {
				listener.NotifyChange(newMode);
			}
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
