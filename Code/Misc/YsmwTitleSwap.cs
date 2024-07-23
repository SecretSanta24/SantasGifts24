using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.SantasGifts24.Code.Misc;

public static class YsmwTitleSwap{
    
    public static void Load(){
        using(var _ = new DetourContext(-1, "YsmwTitleSwap")){
            IL.Celeste.OuiChapterPanel.Render += ModChapterPanelRender;
        }
    }

    public static void Unload(){
        IL.Celeste.OuiChapterPanel.Render -= ModChapterPanelRender;
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
}