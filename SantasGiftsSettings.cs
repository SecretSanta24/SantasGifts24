using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.SantasGifts24;

public class SantasGiftsSettings : EverestModuleSettings{

	[DefaultButtonBinding(0, Keys.J)]
	public ButtonBinding FlagpoleJournalBind { get; set; }
}