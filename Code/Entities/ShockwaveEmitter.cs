﻿using Celeste.Mod.Entities;
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
        private int[] absoluteMaxGlobs;
        private int[] renderPointsOnMesh;
        private int currentWave;
        
        private string flag;
        private bool cycle;

        private bool finished;
        private Sprite sprite;

        public ShockwaveEmitter(EntityData data, Vector2 offset):base(data.Position + offset) 
        {
            var focalRatio = data.Attr("focalRatio", "1.5").Split(',');
            var initialSize = data.Attr("initialSize", "1").Split(',');
            var shockwaveThickness = data.Attr("shockwaveThickness", "3").Split(',');
            var timers = data.Attr("timers", "5").Split(',');
            var expandRate = data.Attr("expand", "30").Split(',');
            var breakoutSpeeds = data.Attr("breakoutSpeeds", "1000").Split(',');
            var absoluteMaxGlobs = data.Attr("absoluteMaxGlobs", "4000").Split(',');
            var renderPointsOnMesh = data.Attr("renderPointsOnMesh", "2000").Split(',');
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
            this.absoluteMaxGlobs = new int[absoluteMaxGlobs.Length];
            for (int i = 0; i < absoluteMaxGlobs.Length; i++)
            {
                this.absoluteMaxGlobs[i] = int.Parse(absoluteMaxGlobs[i]);
            }
            this.renderPointsOnMesh = new int[renderPointsOnMesh.Length];
            for (int i = 0; i < renderPointsOnMesh.Length; i++)
            {
                this.renderPointsOnMesh[i] = int.Parse(renderPointsOnMesh[i]);
            }
            flag = data.Attr("flag", "");
            cycle = data.Bool("cycle", false);
            Add(sprite = GFX.SpriteBank.Create("shockwaveEmitter"));
            sprite.Play("idle");
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
                if (currentWave == 0 && timers[0] - Engine.DeltaTime * 6 * 4 <= 0 && sprite.CurrentAnimationID == "idle")
                {
                    sprite.Play("explode");
                } 
                if (timers[currentWave] <= 0)
                {
                    Audio.Play("event:/Kataiser/sfx/hydro_ancientgenerator_explosion");
                    var focalRatio = GetElementCapped(this.normalizedFocalRatio);
                    var initialSize = GetElementCapped(this.initialSize);
                    var shockwaveThickness = GetElementCapped(this.shockwaveThickness);
                    var expandRate = GetElementCapped(this.expandRate);
                    var breakoutSpeed = GetElementCapped(this.breakoutSpeeds);
                    var absoluteMaxGlobs = GetElementCapped(this.absoluteMaxGlobs);
                    var renderPointsOnMesh = GetElementCapped(this.renderPointsOnMesh);
                    Scene.Add(new EllipticalShockwave(Position, focalRatio.X, focalRatio.Y, initialSize, expandRate, shockwaveThickness, breakoutSpeed, absoluteMaxGlobs, renderPointsOnMesh));
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
