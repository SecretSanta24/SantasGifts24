using System;
using Celeste.Mod.SantasGifts24.Triggers;

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

		public SantasGiftsModule()
		{
			Instance = this;
#if DEBUG
			// debug builds use verbose logging
			Logger.SetLogLevel(nameof(SantasGiftsModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(SantasGifts24Module), LogLevel.Info);
#endif
		}

		public override void Load()
		{
			//DisableDeathSoundTrigger.Load();
		}

		public override void Unload()
		{
			//DisableDeathSoundTrigger.Unload();
		}
	}
}