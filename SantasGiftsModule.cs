using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Celeste.Mod.SantasGifts24.Code.Entities;
using Celeste.Mod.SantasGifts24.Code.States;
using Celeste.Mod.SantasGifts24.Triggers;
using Celeste.Mod.SantasGifts24.Code.Mechanics;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.SantasGifts24.Code.Triggers;
using Celeste.Mod.NeutronHelper;
using Celeste.Mod.SantasGifts24.Entities;
using MonoMod.ModInterop;
using System.Security.Policy;
using static Celeste.Mod.SantasGifts24.SantasGiftsModule;

[assembly: IgnoresAccessChecksTo("Celeste")]
namespace Celeste.Mod.SantasGifts24
{
    public class SantasGiftsModule : EverestModule
	{


        [ModImportName("FemtoHelper.SMWBlockInfo")]
        public static class FemtoHelperImports
        {
            public static Func<Entity, bool> IsActive;
            public static Func<Entity, bool> CanHitTop;
            public static Func<Entity, bool> CanHitBottom;
            public static Func<Entity, bool> CanHitLeft;
            public static Func<Entity, bool> CanHitRight;
            public static Action<Entity, Player, int> GetHitMethod;
        }

        [ModImportName("GravityHelper")]
        public static class GravityHelperImports
        {
            public static Func<int> GetPlayerGravity;
        }

        public static SantasGiftsModule Instance { get; private set; }

		public override Type SettingsType => typeof(SantasGiftsSettings);
		public static SantasGiftsSettings Settings => (SantasGiftsSettings)Instance._Settings;

		public override Type SessionType => typeof(SantasGiftsSession);
		public SantasGiftsSession Session => (SantasGiftsSession)_Session;

		public override Type SaveDataType => typeof(SantasGiftsSaveData);
		public SantasGiftsSaveData SaveData => (SantasGiftsSaveData)_SaveData;

		public SantasGiftsModule() {
			Instance = this;
		}

		public override void Load()
		{
            NoShadowClutterController.Load();
            TechialSeason.Load();
            UpdogCarriable.Load();
            Monopticon.Load();
            GaseousGrandControl.Load();
            Thalassocardiologist.Load();
            NulledVoltage.Load();
            RandomizeStartRoomController.Load();
			DisableDeathSoundTrigger.Load();
			CursedRefill.Load();
			SMWKey.Load();
			RGBBlockSwitch.Load();
			BoosterZipper.Load();
			LightDarkSwapMethods.Load();
            RocketFly.RocketRenderer.Load();
            RewindController.Load();
            Minecart.Load();
            FastfallController.Load();
            RebindElytra.Load();
            CrashLanding.Load();
            WaterLightningRenderer.Load();
            ElectricZipLine.Load();
            LevelMapManager.Load();
            HoldableLiftBoostCancelController.Load();
            //SetRespawnFlagStatesTrigger.Load();

            typeof(FemtoHelperImports).ModInterop();

            typeof(GravityHelperImports).ModInterop();

            On.Celeste.Player.ctor += AddCustomStates;
        }

		public override void Unload()
        {
            NoShadowClutterController.Unload();
            TechialSeason.Unload();
            UpdogCarriable.Unload();
            Monopticon.Unload();
            GaseousGrandControl.Unload();
            Thalassocardiologist.Unload();
            NulledVoltage.Unload();
            RandomizeStartRoomController.Unload();
            DisableDeathSoundTrigger.Unload();
            CursedRefill.Unload();
            SMWKey.Unload();
            RGBBlockSwitch.Unload();
            BoosterZipper.Unload();
            LightDarkSwapMethods.Unload();
            RocketFly.RocketRenderer.Unload();
            RewindController.Unload();
            Minecart.Unload();
            FastfallController.Unload();
            RebindElytra.Unload();
            CrashLanding.Unload();
            WaterLightningRenderer.Unload();
            ElectricZipLine.Unload();
            LevelMapManager.Unload();

            HoldableLiftBoostCancelController.Unload();
            //SetRespawnFlagStatesTrigger.Unload();

            On.Celeste.Player.ctor -= AddCustomStates;
        }
        
        private void AddCustomStates(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
        {
            orig.Invoke(self, position, spriteMode);
            RocketFly.StateNumber = self.StateMachine.AddState(RocketFly.Update, RocketFly.Coroutine, RocketFly.Begin, RocketFly.End);
            ElectricZipLine.ZipLineState = self.StateMachine.AddState(ElectricZipLine.ZipLineUpdate, begin: ElectricZipLine.ZipLineBegin, end: ElectricZipLine.ZipLineEnd, coroutine: ElectricZipLine.ZipLineCoroutine);
            StMinecart.Id = self.StateMachine.AddState(self.MinecartUpdate);
        }
    }

    // I think I got this from frosthelper? Modified to work with publicizer - Aurora
    public static class Extensions
    {
        public static int AddState(this StateMachine machine, Func<int> onUpdate, Func<IEnumerator> coroutine = null, Action begin = null, Action end = null)
        {
            Action[] begins = machine.begins;
            Func<int>[] updates = machine.updates;
            Action[] ends = machine.ends;
            Func<IEnumerator>[] coroutines = machine.coroutines;
            int nextIndex = begins.Length;
            Array.Resize(ref begins, begins.Length + 1);
            Array.Resize(ref updates, begins.Length + 1);
            Array.Resize(ref ends, begins.Length + 1);
            Array.Resize(ref coroutines, coroutines.Length + 1);
            machine.begins = begins;
            machine.updates = updates;
            machine.ends = ends;
            machine.coroutines = coroutines;

            machine.SetCallbacks(nextIndex, onUpdate, coroutine, begin, end);
            return nextIndex;
        }

    }
}