using Celeste.Mod;
using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.SantasGifts24
{
    public class SantasGiftsSettings : EverestModuleSettings
    {
        [SettingName("Settings_minimapBind")]
        [DefaultButtonBinding(Buttons.LeftTrigger, Keys.Tab)]
        public ButtonBinding MiniMapShow { get; set; }

        [DefaultButtonBinding(0, Keys.J)]
        public ButtonBinding InMapJournalBind { get; set; }
    }
}