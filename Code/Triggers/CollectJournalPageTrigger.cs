using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.SantasGifts24.Code.Triggers;

[CustomEntity("SS2024/CollectJournalPageTrigger")]
public class CollectJournalPageTrigger : Trigger{

	private readonly string image;

	public CollectJournalPageTrigger(EntityData data, Vector2 offset) : base(data, offset) => image = data.Attr("image");

	public override void OnEnter(Player player){
		base.OnEnter(player);
		List<string> pages = SantasGiftsModule.Instance.Session.JournalPages;
		if(!pages.Contains(image))
			pages.Add(image);
	}
}