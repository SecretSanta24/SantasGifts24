using System;
using System.Collections.Generic;
using System.Diagnostics;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    // by aurora
    [CustomEntity("SS2024/AudioEventReplacerController")]
    [Tracked]
    public class AudioEventReplacerController : Entity
    {

        private string from;
        public static Dictionary<string, string> replacers;
        public static bool debug = false;
        public static bool active = false;
        public AudioEventReplacerController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            from = data.Attr("OldAudioEvent", "");
            string to = data.Attr("NewAudioEvent", "event:/none");
            replacers ??= new Dictionary<string, string>();
            replacers[from] = to;
            debug = data.Bool("LogAudioPlaying", false);
        }

        public static void Load()
        {
            On.Celeste.Audio.GetEventDescription += Audio_GetEventDescription;
        }

        public static void Unload()
        {
            On.Celeste.Audio.GetEventDescription -= Audio_GetEventDescription;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if(replacers != null)
            {
                replacers.Remove(from);
            }
        }
        private static FMOD.Studio.EventDescription Audio_GetEventDescription(On.Celeste.Audio.orig_GetEventDescription orig, string path)
        {
            if (debug) Logger.Log(LogLevel.Info, "Aurora's Audio Event Replacer Controller audio log", path);
            if (replacers?.ContainsKey(path) ?? false)
            {
                return orig(replacers[path]);
            } else
            {
                return orig(path);
            }
        }

    }

}
