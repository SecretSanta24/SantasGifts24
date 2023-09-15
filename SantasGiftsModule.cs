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
	}

	public override void Load() {
		AuroraAquir.Load();  
	}

	public override void Unload() {
        AuroraAquir.Unload();
    }
}