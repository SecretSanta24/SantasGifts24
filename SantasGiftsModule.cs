using System;
using System.Runtime.CompilerServices;
using Celeste.Mod.SantasGifts24.Code.Entities;
using Celeste.Mod.SantasGifts24.Entities;
using Celeste.Mod.SantasGifts24.Triggers;

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
			Everest.Events.Level.OnTransitionTo += Code.Mechanics.LightDarkSwapMethods.OnTransition;
		}

		public override void Unload()
        {
            RandomizeStartRoomController.Unload();
            DisableDeathSoundTrigger.Unload();
            CursedRefill.Unload();
            RGBBlockSwitch.Unload();
			Everest.Events.Level.OnTransitionTo -= Code.Mechanics.LightDarkSwapMethods.OnTransition;
		}
	}
}