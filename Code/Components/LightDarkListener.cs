using Celeste.Mod.SantasGifts24.Code.Mechanics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Components {
	[Tracked]
	internal class LightDarkListener : Component {
		private Action<LightDarkMode> callback;
		public LightDarkListener(Action<LightDarkMode> callback) : base(true, false) {
			this.callback = callback;
		}

		internal void NotifyChange(LightDarkMode mode) {
			callback?.Invoke(mode);
		}
	}
}
