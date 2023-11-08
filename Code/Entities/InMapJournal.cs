using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.SantasGifts24.Code.Entities;

[CustomEntity("SS2024/InMapJournal")]
public class InMapJournalController : Entity{
	public override void Update(){
		base.Update();

		if(SantasGiftsModule.Instance.Settings.InMapJournalBind.Pressed){
			var existingJournal = Scene.Tracker.GetEntity<InMapJournal>();
			var player = Scene.Tracker.GetEntity<Player>();
			if(existingJournal == null && player != null && player.StateMachine == Player.StNormal && player.OnSafeGround){
				player.Drop();
				player.StateMachine.State = Player.StDummy;
				Scene.Add(new InMapJournal(player));
			}
		}
	}
}

// claims to be a journal, isn't even Oui...
[Tracked]
public class InMapJournal : Entity{
	private static readonly Color BackColor = Color.Lerp(Color.White, Color.Black, 0.2f);

	private VirtualRenderTarget CurrentPageBuffer;
	private VirtualRenderTarget NextPageBuffer;

	private int pageIdx = 0;
	private int pageCount = 12;

	private bool turningPage;
	private float turningScale = 1, dot, dotTarget, dotEase, leftArrowEase, rightArrowEase, rotation = -0.025f;

	public InMapJournal(Player p){
		Tag |= TagsExt.SubHUD;

		NextPageBuffer = VirtualContent.CreateRenderTarget("SSC24:journal-a", 1610, 1000);
		CurrentPageBuffer = VirtualContent.CreateRenderTarget("SSC24:journal-b", 1610, 1000);

		Add(new Coroutine(Routine(p)));
	}

	public override void Render(){
		base.Render();

		// fairly straightforward adaptation of OuiJournal rendering
		Vector2 position = Position + new Vector2(128, 120);
		float turnRight = Ease.CubeInOut(Math.Max(0, turningScale));
		float turnLeft = Ease.CubeInOut(Math.Abs(Math.Min(0, turningScale)));
		if(SaveData.Instance.CheatMode)
			MTN.FileSelect["cheatmode"].DrawCentered(position + new Vector2(80, 360), Color.White, 1, MathHelper.PiOver2);
		if(SaveData.Instance.AssistMode)
			MTN.FileSelect["assist"].DrawCentered(position + new Vector2(100, 370), Color.White, 1, MathHelper.PiOver2);
		MTexture edge = MTN.Journal["edge"];
		edge.Draw(position + new Vector2(-edge.Width, 0), Vector2.Zero, Color.White, 1, rotation);
		if(pageIdx > 0)
			MTN.Journal[pageIdx == 1 ? "cover" : "page"].Draw(position, Vector2.Zero, BackColor, new Vector2(-1, 1), rotation);
		if(turningPage){
			MTN.Journal["page"].Draw(position, Vector2.Zero, Color.White, 1, rotation);
			Draw.SpriteBatch.Draw(NextPageBuffer, position, NextPageBuffer.Bounds, Color.White, rotation, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
		}

		if(turningPage && turnLeft > 0)
			MTN.Journal[pageIdx == 0 ? "cover" : "page"].Draw(position, Vector2.Zero, BackColor, new Vector2(-1 * turnLeft, 1), rotation);
		if(turnRight > 0){
			MTN.Journal[pageIdx == 0 ? "cover" : "page"].Draw(position, Vector2.Zero, Color.White, new Vector2(turnRight, 1), rotation);
			Draw.SpriteBatch.Draw(CurrentPageBuffer, position, CurrentPageBuffer.Bounds, Color.White, rotation, Vector2.Zero, new Vector2(turnRight, 1), SpriteEffects.None, 0);
		}

		if(pageCount <= 0)
			return;
		MTexture dotBg = GFX.Gui["dot_outline"];
		int num2 = dotBg.Width * pageCount;
		Vector2 vector2 = new Vector2(960f, (float)(1040.0 - 40.0 * Ease.CubeOut(dotEase)));
		for(int index = 0; index < pageCount; ++index)
			dotBg.DrawCentered(vector2 + new Vector2(-num2 / 2 + dotBg.Width * (index + 0.5f), 0.0f), Color.White * 0.25f);
		float x2 = (float)(1.0 + Calc.YoYo(dot % 1f) * 4.0);
		dotBg.DrawCentered(vector2 + new Vector2(-num2 / 2 + dotBg.Width * (dot + 0.5f), 0.0f), Color.White, new Vector2(x2, 1f));
		GFX.Gui["dotarrow_outline"].DrawCentered(vector2 + new Vector2(-num2 / 2 - 50, 32 * (1 - Ease.CubeOut(leftArrowEase))), Color.White * leftArrowEase, new Vector2(-1, 1));
		GFX.Gui["dotarrow_outline"].DrawCentered(vector2 + new Vector2(num2 / 2 + 50, 32 * (1 - Ease.CubeOut(rightArrowEase))), Color.White * rightArrowEase);
	}

	private IEnumerator Routine(Player player){
		Audio.Play("event:/ui/world_map/journal/page_cover_forward");

		for(float p = 0; p < 1; p += Engine.DeltaTime / .4f){
			rotation = -0.025f * Ease.BackOut(p);
			X = 1920 * Ease.CubeInOut(p) - 1920;
			dotEase = p;
			yield return null;
		}
		dotEase = 1f;
		
		while(!Input.MenuCancel.Pressed){
			dot = Calc.Approach(dot, dotTarget, Engine.DeltaTime * 8);
			leftArrowEase = Calc.Approach(leftArrowEase, dotTarget > 0 ? 1 : 0, Engine.DeltaTime * 5) * dotEase;
			rightArrowEase = Calc.Approach(rightArrowEase, dotTarget < pageCount - 1 ? 1 : 0, Engine.DeltaTime * 5) * dotEase;

			if(!turningPage){
				if(Input.MenuLeft.Pressed && pageIdx > 0){
					Audio.Play(pageIdx == 1 ? "event:/ui/world_map/journal/page_cover_back" : "event:/ui/world_map/journal/page_main_back");
					Add(new Coroutine(TurnPage(-1)));
				}else if(Input.MenuRight.Pressed && pageIdx < pageCount - 1){
					Audio.Play(pageIdx == 0 ? "event:/ui/world_map/journal/page_cover_forward" : "event:/ui/world_map/journal/page_main_forward");
					Add(new Coroutine(TurnPage(1)));
				}
			}

			yield return null;
		}
		
		for(float p = 1; p > 0; p -= Engine.DeltaTime / .4f){
			rotation = -0.025f * Ease.BackOut(p);
			X = 1920 * Ease.CubeInOut(p) - 1920;
			dotEase = p;
			yield return null;
		}
		dotEase = 1f;

		Visible = false;
		CurrentPageBuffer.Dispose();
		NextPageBuffer.Dispose();

		player.StateMachine.State = Player.StNormal;
		RemoveSelf();
	}

	public IEnumerator TurnPage(int direction){
		turningPage = true;
		if(direction < 0){
			pageIdx--;
			turningScale = -1;
			dotTarget--;
			//this.Page.Redraw(this.CurrentPageBuffer);
			//this.NextPage.Redraw(this.NextPageBuffer);
			while((turningScale = Calc.Approach(turningScale, 1, Engine.DeltaTime * 8)) < 1)
				yield return null;
		} else{
			//this.NextPage.Redraw(this.NextPageBuffer);
			turningScale = 1;
			dotTarget++;
			while((turningScale = Calc.Approach(turningScale, -1, Engine.DeltaTime * 8)) > -1)
				yield return null;
			pageIdx++;
			//this.Page.Redraw(this.CurrentPageBuffer);
		}

		turningScale = 1;
		turningPage = false;
	}
}