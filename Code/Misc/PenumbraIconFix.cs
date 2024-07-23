using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.SantasGifts24.Code.Misc;

public static class PenumbraIconFix{

    private static ILHook Cu2IconCellHook;
    private static FieldInfo Cu2IconCellField;
    
    public static void Load(){
        if(Everest.Modules.FirstOrDefault(x => x.Metadata.Name == "CollabUtils2") is {} cu2Module
        && cu2Module.GetType().Assembly.GetType("Celeste.Mod.CollabUtils2.UI.OuiJournalCollabProgressInLobby+IconCellFromGui") is {} iconCellType){
            Cu2IconCellField = iconCellType.GetField("icon", BindingFlags.NonPublic | BindingFlags.Instance);
            Cu2IconCellHook = new ILHook(
                iconCellType.GetMethod("Render"),
                ModIconCellRender
            );
        }else
            Logger.Log(LogLevel.Error, "SantasGifts24/PenumbraIconFix", "Failed to find CU2 type for icon fix!");
    }

    public static void Unload(){
        Cu2IconCellField = null;
        Cu2IconCellHook?.Dispose();
    }

    private static void ModIconCellRender(ILContext il){
        // faster this way }:3
        ILCursor cursor = new(il);
        // after it calculates the wrong scale...
        cursor.GotoNext(MoveType.After, instr => instr.MatchCall(typeof(Math), "Min"));
        // push this
        cursor.Emit(OpCodes.Ldarg_0);
        // get this icon
        cursor.Emit(OpCodes.Ldfld, Cu2IconCellField);
        // custom scaling logic
        cursor.EmitDelegate<Func<float, string, float>>((oldScale, icon) => {
            if(icon.EndsWith("-SSC-fullscale", StringComparison.Ordinal))
                return 65f / GFX.Gui[icon].Width;
            return oldScale;
        });
    }
}