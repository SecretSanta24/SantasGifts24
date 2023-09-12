using Celeste.Mod.Entities;
using Monocle;

namespace Celeste.Mod.SantasGifts24.Flagpole;

[CustomEntity("SantasGifts24/FlagpoleJournalActivator")]
public class JournalActivator : Entity{

	public JournalActivator(){
		// Tag |= Tags.Global;
	}
	
	public override void Update(){
		base.Update();

		if(SantasGiftsModule.Settings.FlagpoleJournalBind.Pressed){
			var player = Scene.Tracker.GetEntity<Player>();
			if(player != null && player.StateMachine == Player.StNormal){
				player.StateMachine.State = Player.StDummy;
				OverworldHelper.OpenOverworldInto<OuiJournal>(SceneAs<Level>());
			}
		}
	}
}