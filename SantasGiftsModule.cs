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

[assembly: IgnoresAccessChecksTo("Celeste")]
namespace Celeste.Mod.SantasGifts24
{
    public class SantasGiftsModule : EverestModule
	{

		public static SantasGiftsModule Instance { get; private set; }

		public override Type SettingsType => typeof(SantasGiftsSettings);
		public SantasGiftsSettings Settings => (SantasGiftsSettings)_Settings;

		public override Type SessionType => typeof(SantasGiftsSession);
		public SantasGiftsSession Session => (SantasGiftsSession)_Session;

		public override Type SaveDataType => typeof(SantasGiftsSaveData);
		public SantasGiftsSaveData SaveData => (SantasGiftsSaveData)_SaveData;

		public SantasGiftsModule() {
			Instance = this;
		}

		public override void Load()
		{
			RandomizeStartRoomController.Load();
			DisableDeathSoundTrigger.Load();
			CursedRefill.Load();
			RGBBlockSwitch.Load();
			BoosterZipper.Load();
			LightDarkSwapMethods.Load();
            RocketFly.RocketRenderer.Load();
            RewindController.Load();
            Minecart.Load();
			
			On.Celeste.Player.ctor += AddCustomStates;
        }

		public override void Unload()
        {
            RandomizeStartRoomController.Unload();
            DisableDeathSoundTrigger.Unload();
            CursedRefill.Unload();
            RGBBlockSwitch.Unload();
            BoosterZipper.Unload();
            LightDarkSwapMethods.Unload();
            RocketFly.RocketRenderer.Unload();
            RewindController.Unload();
            Minecart.Unload();

            On.Celeste.Player.ctor -= AddCustomStates;
        }
        
        private void AddCustomStates(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
        {
            orig.Invoke(self, position, spriteMode);
            RocketFly.StateNumber = self.StateMachine.AddState(RocketFly.Update, RocketFly.Coroutine, RocketFly.Begin, RocketFly.End);
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