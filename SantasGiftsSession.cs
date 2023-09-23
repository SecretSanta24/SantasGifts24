namespace Celeste.Mod.SantasGifts24
{
    public class SantasGiftsSession : EverestModuleSession
    {
        public bool DisableDeathSound { get; set; } = false;


        //cursed refill
        public bool playerCursed = false;
        public bool ignoreDash = false;
        public bool killPlayerWhenSafe = false;

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
    }
}