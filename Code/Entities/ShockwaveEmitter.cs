using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        private float[] initialTimers;
        private float[] timers;
        private float[] initialSize;
        private float[] shockwaveThickness;
        private float[] expandRate;
        private float[] breakoutSpeeds;
        private Vector2[] normalizedFocalRatio;
        private int currentWave;
        
        private string flag;
        private bool cycle;

        private bool finished;

        public ShockwaveEmitter(EntityData data, Vector2 offset):base(data.Position + offset) 
        {
            var focalRatio = data.Attr("focalRatio", "1.5").Split(',');
            var initialSize = data.Attr("initialSize", "1").Split(',');
            var shockwaveThickness = data.Attr("shockwaveThickness", "3").Split(',');
            var timers = data.Attr("timers", "5").Split(',');
            var expandRate = data.Attr("expand", "30").Split(',');
            var breakoutSpeeds = data.Attr("breakoutSpeeds", "1000").Split(',');
            normalizedFocalRatio = new Vector2[focalRatio.Length];
            for (int i = 0; i < focalRatio.Length; i++)
            {
                normalizedFocalRatio[i] = (new Vector2(1, float.Parse(focalRatio[i]))).SafeNormalize();
            }
            this.timers = new float[timers.Length];
            this.initialTimers = new float[timers.Length];
            for (int i = 0; i < timers.Length; i++)
            {
                this.timers[i] = float.Parse(timers[i]);
                this.initialTimers[i] = float.Parse(timers[i]);
            }
            this.initialSize = new float[initialSize.Length];
            for (int i = 0; i < initialSize.Length; i++)
            {
                this.initialSize[i] = float.Parse(initialSize[i]);
            }
            this.shockwaveThickness = new float[shockwaveThickness.Length];
            for (int i = 0; i < shockwaveThickness.Length; i++)
            {
                this.shockwaveThickness[i] = float.Parse(shockwaveThickness[i]);
            }
            this.expandRate = new float[expandRate.Length];
            for (int i = 0; i < expandRate.Length; i++)
            {
                this.expandRate[i] = float.Parse(expandRate[i]);
            }
            this.breakoutSpeeds = new float[breakoutSpeeds.Length];
            for (int i = 0; i < breakoutSpeeds.Length; i++)
            {
                this.breakoutSpeeds[i] = float.Parse(breakoutSpeeds[i]);
            }
            flag = data.Attr("flag", "");
            cycle = data.Bool("cycle", false);
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
            if (SceneAs<Level>().Session.GetFlag(flag) && !finished)
            {
                timers[currentWave] -= Engine.DeltaTime;
                if (timers[currentWave] <= 0)
                {
                    var focalRatio = GetElementCapped(this.normalizedFocalRatio);
                    var initialSize = GetElementCapped(this.initialSize);
                    var shockwaveThickness = GetElementCapped(this.shockwaveThickness);
                    var expandRate = GetElementCapped(this.expandRate);
                    var breakoutSpeed = GetElementCapped(this.breakoutSpeeds);
                    Scene.Add(new EllipticalShockwave(Position, focalRatio.X, focalRatio.Y, initialSize, expandRate, shockwaveThickness, breakoutSpeed));
                    currentWave++;
                    if (cycle && currentWave >= timers.Length)
                    {
                        ResetEmitter();
                    } else if (currentWave >= timers.Length)
                    {
                        finished = true;
                    }
                }
            }
        }

        //im lazy dont @ me.
        private T GetElementCapped<T>(T[] array)
        {
            return array[Math.Min(currentWave, array.Length - 1)];
        }

        private void ResetEmitter()
        {
            for (int i = 0; i < timers.Length; i++)
            {
                this.timers[i] = this.initialTimers[i];
            }
            currentWave = 0;
        }
    }
}
