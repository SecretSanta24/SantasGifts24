using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    [Tracked]
    public class TreeKeyBarrier : Solid
    {
		private MysteriousTree tree;
		public TreeKeyBarrier(Vector2 position, float width, float height, MysteriousTree tree)
			: base(position, width, height, safe: false)
		{
			this.tree = tree;
			Collidable = false;
		}

		public void HandleKey()
        {
			tree.TakeDamage();
        }
	}
}
