using System;

namespace Celeste.Mod.SantasGifts24;

public class SantasGiftsModule : EverestModule {

	public static SantasGiftsModule Instance { get; private set; }

	public override Type SettingsType => typeof(SantasGiftsSettings);
	public static SantasGiftsSettings Settings => (SantasGiftsSettings) Instance._Settings;

	public override Type SessionType => typeof(SantasGiftsSession);
	public static SantasGiftsSession Session => (SantasGiftsSession) Instance._Session;
	
	public override Type SaveDataType => typeof(SantasGiftsSaveData);
	public static SantasGiftsSaveData SaveData => (SantasGiftsSaveData) Instance._SaveData;

	public SantasGiftsModule() {
		Instance = this;
#if DEBUG
		// debug builds use verbose logging
		Logger.SetLogLevel(nameof(SantasGiftsModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(SantasGifts24Module), LogLevel.Info);
#endif
	}

	public override void Load() {
            
	}

	public override void Unload() {
            
	}
}