using System;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.SantasGifts24.Code.Misc;

public static class YsmwTitleSwap{
    
    public static void Load(){
        using(var _ = new DetourContext(-1, "YsmwTitleSwap")){
            IL.Celeste.OuiChapterPanel.Render += ModChapterPanelRender;
        }
        
        // now let's do a funny one!
        On.Celeste.OuiChapterPanel.Render += HookChapterPanelRender;
    }

    public static void Unload(){
        IL.Celeste.OuiChapterPanel.Render -= ModChapterPanelRender;
        On.Celeste.OuiChapterPanel.Render -= HookChapterPanelRender;
    }
    
    private static void ModChapterPanelRender(ILContext il){
        ILCursor cursor = new(il);
        // ldfld string Celeste.AreaData::Name
        cursor.GotoNext(MoveType.After, instr => instr.MatchLdfld<AreaData>("Name"));
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate<Func<string, OuiChapterPanel, string>>((oldName, panel) => {
            string suffix = panel.RealStats.Modes[0].Completed ? "_SSC_posttext" : "_SSC_pretext";
            if(Dialog.Language.Cleaned.ContainsKey((oldName + suffix).DialogKeyify()))
                return oldName + suffix;
            return oldName;
        });
    }
    
    private static void HookChapterPanelRender(On.Celeste.OuiChapterPanel.orig_Render orig, OuiChapterPanel self){
        if(self.Data.SID == "SecretSanta2024/1-Easy/leppa" && !self.RealStats.Modes[0].Completed){
            AreaData data = self.Data;
            Color oldTitleBase = data.TitleBaseColor, oldTitleAccent = data.TitleAccentColor, oldTitleText = data.TitleTextColor;
            try{
                data.TitleBaseColor = Color.White;
                data.TitleAccentColor = Color.Gray;
                data.TitleTextColor = Color.Black;
                orig(self);
            }finally{
                data.TitleBaseColor = oldTitleBase;
                data.TitleAccentColor = oldTitleAccent;
                data.TitleTextColor = oldTitleText;
            }
        }else
            orig(self);
    }
}