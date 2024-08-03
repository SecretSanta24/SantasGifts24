using System.Collections.Generic;
using Celeste.Mod.SantasGifts24.Code.Mechanics;

namespace Celeste.Mod.SantasGifts24
{
    public class SantasGiftsSession : EverestModuleSession
    {
        // Crystal
        public int CrystalsCollected = 0;

        // Light/Dark Mode
        public LightDarkMode LightDark { get; set; } = LightDarkMode.Normal;
        public LightDarkMode LightDarkPersistent { get; set; } = LightDarkMode.Normal;

        // 
        public bool DisableDeathSound { get; set; } = false;


		//cursed refill
		public bool playerCursed { get; set; } = false;
        public bool ignoreDash { get; set; } = false;
        public bool killPlayerWhenSafe { get; set; } = false;

        // in-map journal
        public List<string> JournalPages { get; set; } = new();

        public void ResetCurse()
        {
            playerCursed = false;
            ignoreDash = false;
            killPlayerWhenSafe = false;
        }

        public void SetCurse(bool ignoreDashNew)
        {
            this.ignoreDash = ignoreDashNew;
            this.playerCursed = playerCursed || !ignoreDashNew;
        }

        //fastfallcontroller
        public bool fastfallActive { get; set; } = true;

        public Dictionary<string, bool> respawnFlagMonitor = new();
    }
}