using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SantasGifts24.Code.Entities
{
    [CustomEntity("SS2024/ShockwaveEmitter")]
    public class ShockwaveEmitter : Entity
    {
        private float countdown = 0;
        private float countdownStart = 5;
        private float initialSize;
        private float expandRate;
        private float shockwaveThickness;
        private Vector2 normalizedFocalRatio;
        private string flag;

        public ShockwaveEmitter(EntityData data, Vector2 offset):base(data.Position + offset) 
        {
            var focalRatio = data.Float("focalRatio", 1.5F);
            normalizedFocalRatio = (new Vector2(1, focalRatio)).SafeNormalize();
            countdownStart = data.Float("frequency", 5F);
            initialSize = data.Float("initialSize", 1F);
            expandRate = data.Float("expand", 30);
            shockwaveThickness = data.Float("shockwaveThickness", 3);
            countdown = data.Float("firstSpawn", 5F);
            flag = data.Attr("flag", "");
        }

        public override void Update()
        {
            base.Update();

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null)
            {
                return;
            }
            if (player.Dead)
            {
                return;
            }
            if (SceneAs<Level>().Session.GetFlag(flag))
            {
                countdown -= Engine.DeltaTime;
                if (countdown <= 0)
                {
                    countdown = countdownStart;
                    Scene.Add(new EllipticalShockwave(Position, normalizedFocalRatio.X, normalizedFocalRatio.Y, initialSize, expandRate, shockwaveThickness));
                    Audio.Play("event:/game/general/diamond_touch");
                }
            }
        }
    }
}
