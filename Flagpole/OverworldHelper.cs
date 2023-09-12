using System;
using System.Collections;
using Celeste.Mod.Entities;
using Monocle;

namespace Celeste.Mod.SantasGifts24.Flagpole;

public static class OverworldHelper{

	// adapted from CU2: https://github.com/EverestAPI/CelesteCollabUtils2/blob/7ffbd9bc6fec680e204d8947d21edf10a12ff99b/UI/InGameOverworldHelper.cs#L1003
	public static void OpenOverworldInto<T>(Level level, Action<Overworld> callback = null) where T : Oui{
		OuiTransfer.Next = typeof(T);
		
		var overworldWrapper = new SceneWrappingEntity<Overworld>(new Overworld(new OverworldLoader((Overworld.StartMode) (-1),
			new HiresSnow{
				Alpha = 0f,
				ParticleAlpha = 0.25f
			}
		)));
		overworldWrapper.OnBegin += overworld => {
			overworld.RendererList.Remove(overworld.RendererList.Renderers.Find(r => r is MountainRenderer));
			overworld.RendererList.Remove(overworld.RendererList.Renderers.Find(r => r is ScreenWipe));
			overworld.RendererList.UpdateLists();
		};
		overworldWrapper.OnEnd += overworld => {
			if (overworldWrapper?.WrappedScene == overworld) {
				overworldWrapper = null;
			}
		};

		level.Add(overworldWrapper);
		callback?.Invoke(overworldWrapper.WrappedScene);
	}

	// reflectively instantiated by Overworld
	// ReSharper disable once ClassNeverInstantiated.Global
	public class OuiTransfer : Oui{

		internal static Type Next = null;

		public override bool IsStart(Overworld overworld, Overworld.StartMode start){
			if(Next != null){
				Add(new Coroutine(Routine(Next)));
				Next = null;
				return true;
			}

			return false;
		}

		public IEnumerator Routine(Type next){
			if(next != null){
				Audio.Play("event:/ui/world_map/journal/select");
				// Overworld.Goto<Next>();
				Overworld.GetType().GetMethod("Goto").MakeGenericMethod(next).Invoke(Overworld, new object[0]);
			}

			yield break;
		}

		public override IEnumerator Enter(Oui from){
			yield break;
		}
		
		public override IEnumerator Leave(Oui next){
			yield break;
		}
	}
}