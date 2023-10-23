﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.NeutronHelper
{
    [Tracked]
    [CustomEntity("SS2024/GaseousControl")]
    public class GaseousGrandControl : Entity
    {
        public float Oxygen = 500f;
        public bool FastDeath;
        public float DrainRate;
        public float RecoverRate;
        public string Flag;
        public string StyleTagIn;
        public string StyleTagOut;
        public string ColorgradeA;
        public string ColorgradeB;
        public GaseousGrandControl(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Tag = Tags.Persistent;
            FastDeath = data.Bool("fastDeath", true);
            DrainRate = data.Float("drainRate", 150f);
            RecoverRate = data.Float("recoverRate", 1000f);
            Flag = data.Attr("flag", "o2_flag");
            StyleTagIn = data.Attr("fadeInTag", "o2_in_tag");
            StyleTagOut = data.Attr("fadeOutTag", "o2_out_tag");
            ColorgradeA = data.Attr("colorgradeA", "none");
            ColorgradeB = data.Attr("colorgradeB", "none");
            Oxygen = 500f;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);

            foreach(GaseousGrandControl gas in scene.Tracker.GetEntities<GaseousGrandControl>())
            {
                if (gas != this)
                {
                    RemoveSelf();
                }
            }
            Add(new Coroutine(StylegroundRoutine()));
        }

        public static void Load()
        {
            On.Celeste.Level.Update += ColorgradeHook;
        }

        public static void Unload()
        {
            On.Celeste.Level.Update -= ColorgradeHook;
        }

        private static void ColorgradeHook(On.Celeste.Level.orig_Update orig, Level self)
        {
            orig(self);
            GaseousGrandControl gas = self.Tracker.GetEntity<GaseousGrandControl>();
            if(gas != null)
            {
                float lerp = Calc.Clamp(gas.Oxygen, 0f, 500f) / 500f;
                if (lerp > 0.5f)
                {
                    self.lastColorGrade = gas.ColorgradeB;
                    self.Session.ColorGrade = gas.ColorgradeA;
                    self.colorGradeEase = lerp;
                }
                else
                {
                    self.lastColorGrade = gas.ColorgradeA;
                    self.Session.ColorGrade = gas.ColorgradeB;
                    self.colorGradeEase = 1f - lerp;
                }
                self.colorGradeEaseSpeed = 1f;
            }
            
        }


        public override void Update()
        {
            base.Update();
            Player player = Scene.Tracker.GetEntity<Player>();
            if(player != null)
            {
                if (!player.CollideCheck<GaseousTrigger>())
                {
                    (Scene as Level).Session.SetFlag(Flag, true);
                    Oxygen = Math.Max(Oxygen - DrainRate * Engine.DeltaTime, 0f);

                } 
                else
                {
                    (Scene as Level).Session.SetFlag(Flag, false);
                    Oxygen = Calc.Clamp(Oxygen + RecoverRate * Engine.DeltaTime, 0f, 500f);
                }
                if (Oxygen <= 0f)
                {
                    player.Die(FastDeath ? Vector2.Zero : Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 1), false, true);
                }
            }
            

            

        }
        public override void Render()
        {
            base.Render();
        }

        public IEnumerator StylegroundRoutine()
        {
            while (true)
            {
                float fade = Calc.Clamp(Oxygen, 0f, 500f) / 500f;
                float fadeInv = 1 - fade;
                foreach (Backdrop item in (Scene as Level).Background.GetEach<Backdrop>(StyleTagIn))
                {
                    item.ForceVisible = true;
                    item.FadeAlphaMultiplier = fadeInv;
                }
                foreach (Backdrop item in (Scene as Level).Background.GetEach<Backdrop>(StyleTagOut))
                {
                    item.ForceVisible = true;
                    item.FadeAlphaMultiplier = fade;
                }
                foreach (Backdrop item in (Scene as Level).Foreground.GetEach<Backdrop>(StyleTagIn))
                {
                    item.ForceVisible = true;
                    item.FadeAlphaMultiplier = fadeInv;
                }
                foreach (Backdrop item in (Scene as Level).Foreground.GetEach<Backdrop>(StyleTagOut))
                {
                    item.ForceVisible = true;
                    item.FadeAlphaMultiplier = fade;
                }
                yield return null;
            }
            
        }
    }
}
