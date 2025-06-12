using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    public class FastfallController : Entity
    {
        private static bool hooksLoaded;


        public static void Load()
        {
            if (hooksLoaded) return;
            hooksLoaded = true;

            IL.Celeste.Player.NormalUpdate += FastfallPatch;
        }
        public static void Unload()
        {
            if(!hooksLoaded) return;
            hooksLoaded = false;

            IL.Celeste.Player.NormalUpdate -= FastfallPatch;
        }

        private static void FastfallPatch(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f)))
            {
                while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld("Celeste.Input", "MoveY")))
                {
                    while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(1)))
                    {
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.EmitDelegate(OverrideFastFall);
                        break;
                    }
                    break;
                }
                break;
            }
        }
        
        
        //This trigger has a port in LylyraHelper.
        //Both triggers work by setting the condition for the input of the move value to be equal to 10000 instead of the expected value of 1
        //We don't know which loads first, so both triggers grab the value from the stack and simply pass it back if it has been modified (that is, not equal to 1).
        //This way both triggers can exist in the same codespace.
        private static float OverrideFastFall(float f, Player player)
        {
            if (f > 1) return f;
            return player.SceneAs<Level>().Session.GetFlag("SS24Fastfall") ? 10000F : 1f;
        } 
    }
}
