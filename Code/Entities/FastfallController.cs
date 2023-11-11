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
                        cursor.Emit(OpCodes.Pop);
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.EmitDelegate((Player player) =>
                        {
                            return (player.SceneAs<Level>().Session.GetFlag("SS24Fastfall")) ? 10000F : 1f;
                        });
                        break;
                    }
                    break;
                }
                break;
            }
        }
    }
}
