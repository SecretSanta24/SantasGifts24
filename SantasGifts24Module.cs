using System;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.SantasGifts24 {
    public class SantasGifts24Module : EverestModule {
        public static SantasGifts24Module Instance { get; private set; }

        public override Type SettingsType => typeof(SantasGifts24ModuleSettings);
        public static SantasGifts24ModuleSettings Settings => (SantasGifts24ModuleSettings) Instance._Settings;

        public override Type SessionType => typeof(SantasGifts24ModuleSession);
        public static SantasGifts24ModuleSession Session => (SantasGifts24ModuleSession) Instance._Session;

        public SantasGifts24Module() {
            Instance = this;
#if DEBUG
            // debug builds use verbose logging
            Logger.SetLogLevel(nameof(SantasGifts24Module), LogLevel.Verbose);
#else
            // release builds use info logging to reduce spam in log files
            Logger.SetLogLevel(nameof(SantasGifts24Module), LogLevel.Info);
#endif
        }

        public override void Load() {
            // TODO: apply any hooks that should always be active
        }

        public override void Unload() {
            // TODO: unapply any hooks applied in Load()
        }
    }
}