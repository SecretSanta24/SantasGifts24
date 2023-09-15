using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace Celeste.Mod.SantasGifts24.Triggers
{
    [CustomEntity("SantasGifts24/DisableDeathSoundTrigger")]
    public class DisableDeathSoundTrigger : Trigger
    {
        private static ILHook hookDeathRoutine;

        public static void Load()
        {
            IL.Celeste.Player.IntroRespawnBegin += modPlayerIntroRespawnBegin;

            hookDeathRoutine = new ILHook(
                typeof(PlayerDeadBody).GetMethod("DeathRoutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(),
                ilHookDeathRoutine);
        }

        public static void Unload()
        {
            IL.Celeste.Player.IntroRespawnBegin -= modPlayerIntroRespawnBegin;

            hookDeathRoutine?.Dispose();
            hookDeathRoutine = null;
        }

        public static void ilHookDeathRoutine(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCall(typeof(Audio), "Play")))
            {
                Logger.Log("SantasGifts24/DisableDeathSoundTrigger", $"Modding player dead body death routine at {cursor.Index} in IL for {cursor.Method.FullName}");
                cursor.Remove();
                cursor.EmitDelegate<Func<string, Vector2, FMOD.Studio.EventInstance>>((path, pos) => {
                    if (!SantasGiftsModule.Instance.Session.DisableDeathSound)
                    {
                        return Audio.Play(path, pos);
                    }
                    return null;
                });
            }
        }

        public static void modPlayerIntroRespawnBegin(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            Logger.Log("SantasGifts24/DisableDeathSoundTrigger", $"Modding player intro respawn begin at {cursor.Index} in IL for {il.Method.FullName}");
            cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCallvirt<Player>("Play"));
            cursor.Remove();
            cursor.EmitDelegate<Func<Player, string, string, float, FMOD.Studio.EventInstance>>((player, path, param, val) => {
                if (!SantasGiftsModule.Instance.Session.DisableDeathSound)
                {
                    return player.Play(path, param, val);
                }
                return null;
            });
        }
        
        private bool disableDeathSound;

        public DisableDeathSoundTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            disableDeathSound = data.Bool("disable", false);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);

            SantasGiftsModule.Instance.Session.DisableDeathSound = disableDeathSound;
        }
    }
}