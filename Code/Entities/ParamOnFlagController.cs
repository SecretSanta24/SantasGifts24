using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    // by Aurora Aquir
    [CustomEntity("SS2024/ParamOnFlagController")]
    public class ParamOnFlagController : Entity
    {
        private static string Flag;
        private static float paramState;
        private static string Parameter;
        private static float onValue;
        private static float offValue;
        public ParamOnFlagController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Flag = data.Attr("Flag", "");
            Parameter = data.Attr("Parameter", "");
            offValue = data.Float("OffValue", 0f);
            onValue = data.Float("OnValue", 1f);
        }

        public override void Added(Scene scene)
        {
            base.Awake(scene);
            paramState = -1;
            Session session = (scene as Level)?.Session;
            if (session != null)
            {
                bool val = session.GetFlag(Flag);
                if ((val ? onValue : offValue) != paramState)
                {
                    Audio.SetMusicParam(Parameter, val ? onValue : offValue);
                }
            }
        }
        public override void Update()
        {
            base.Update();
            Session session = (Engine.Scene as Level)?.Session;
            if (session != null)
            {
                bool val = session.GetFlag(Flag);
                if ((val ? onValue : offValue) != paramState)
                {
                    Audio.SetMusicParam(Parameter, val ? onValue : offValue);
                }
            }
        }

    }

}
