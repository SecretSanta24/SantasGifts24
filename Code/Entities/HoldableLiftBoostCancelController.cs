using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace Celeste.Mod.SantasGifts24.Entities;

// Made by Snip
[CustomEntity("LiftBoostCancel/HoldableLiftBoostCancelController")]
[Tracked]
public class HoldableLiftBoostCancelController : Entity
{
    private static readonly MethodInfo m_LiftBoostSetter = typeof(Actor).GetProperty("LiftSpeed").GetSetMethod();
    private static IDetour Hook_LiftBoostSetter;

    public HoldableLiftBoostCancelController(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
    }

    public static void Load()
    {
        Hook_LiftBoostSetter = new Hook(m_LiftBoostSetter, LiftBoostSetter);
    }

    public static void Unload()
    {
        Hook_LiftBoostSetter?.Dispose();
    }

    private static void LiftBoostSetter(Action<Actor, Vector2> orig, Actor self, Vector2 liftBoost)
    {
        if (self.Scene?.Tracker.GetEntity<HoldableLiftBoostCancelController>() != null && self.Components.Get<Holdable>() != null)
            return; // don't set liftboost if the entity is in the scene and it has a holdable
        orig(self, liftBoost);
    }
}
