using Celeste.Mod.Entities;
using FMOD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.SantasGifts24.Code.Entities;

// by Aurora Aquir
// this is so hacky lol
[Tracked]
public class RebindElytra : Entity
{
    // setup stuff so it can be used in lua and at all

    public static EverestModule CommunalHelper;
    public static bool CommunalHelperLoaded;
    public static PropertyInfo CommunalHelperLoadedElytraGlideProperty;

    public static void Load()
    {
        CommunalHelperLoaded = Everest.Loader.TryGetDependency(new EverestModuleMetadata { Name = "CommunalHelper", Version = new Version(1, 19, 0) }, out CommunalHelper);
        if (CommunalHelperLoaded)
        {
            CommunalHelperLoadedElytraGlideProperty = CommunalHelper?.GetType()?.Assembly
                        ?.GetType("Celeste.Mod.CommunalHelper.CommunalHelperSettings")
                        ?.GetProperty("DeployElytra", BindingFlags.Public | BindingFlags.Instance);
            if (CommunalHelperLoadedElytraGlideProperty == null) CommunalHelperLoaded = false;
        }
    }

    public static ButtonBinding GetElytraGlideButtonBinding()
    {
        return (ButtonBinding)CommunalHelperLoadedElytraGlideProperty.GetValue(CommunalHelper._Settings);
    }

    public static void Unload()
    {

    }

    public static IEnumerator RebindElytraButton()
    {
        Level level = (Engine.Scene as Level);
        Player player = level.Tracker.GetEntity<Player>();
        level.Add(new RebindElytra());
        level.Frozen = true;

        while (level.Tracker.GetEntities<RebindElytra>()?.Count > 0)
        {
            yield return 1f;
        }

        yield break;
    }

    private Level level;
    private float timeout = 5f;
    private float remappingEase = 0f;
    private float Alpha = 0.5f;
    private bool closing = false;
    private ButtonBinding ElytraButtonBinding;
    private string currentButtons = "";
    private string otherButtons = "";
    public RebindElytra() {
        this.level = Engine.Scene as Level;
        Tag = Tags.HUD | Tags.FrozenUpdate;
        level.PauseLock = true;
        ElytraButtonBinding = GetElytraGlideButtonBinding();
    }
    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (!CommunalHelperLoaded)
        {
            RemoveSelf();
        }
    }

    public override void Update()
    {
        base.Update();

        if(!Input.GuiInputController(Input.PrefixMode.Latest))
        {
            currentButtons = "Current keys: " + String.Join(", ", ElytraButtonBinding.Keys);
            otherButtons = "Current buttons: " + String.Join(", ", ElytraButtonBinding.Buttons);
        }
        else
        {
            currentButtons = "Current buttons: " + String.Join(", ", ElytraButtonBinding.Buttons);
            otherButtons = "Current keys: " + String.Join(", ", ElytraButtonBinding.Keys);
        }

        this.remappingEase = Calc.Approach(this.remappingEase, 1, Engine.RawDeltaTime * 4f);
        if (this.remappingEase >= 0.25f && !closing)
        {
            if (Input.ESC.Pressed || this.timeout <= 0f)
            {
                closing = true;
            }
            else
            {
                if(!Input.GuiInputController(Input.PrefixMode.Latest))
                {
                    Keys[] pressedKeys = MInput.Keyboard.CurrentState.GetPressedKeys();
                    if (pressedKeys != null && pressedKeys.Length != 0 && MInput.Keyboard.Pressed(pressedKeys[pressedKeys.Length - 1]))
                    {
                        if (ElytraButtonBinding.Keys.Contains(pressedKeys[pressedKeys.Length - 1]))
                        {
                            ElytraButtonBinding.Keys.Remove(pressedKeys[pressedKeys.Length - 1]);
                        }
                        else
                        {
                            ElytraButtonBinding.Keys.Add(pressedKeys[pressedKeys.Length - 1]);
                        }
                        closing = true;
                    }
                } else
                {
                    MInput.GamePadData gamePadData = MInput.GamePads[Input.Gamepad];

                    void AddRemap(Buttons button)
                    {

                        if (ElytraButtonBinding.Buttons.Contains(button))
                        {
                            ElytraButtonBinding.Buttons.Remove(button);
                        }
                        else
                        {
                            ElytraButtonBinding.Buttons.Add(button);
                        }
                        closing = true;
                    }

                    float num = 0.25f;
                    if (gamePadData.LeftStickLeftPressed(num))
                    {
                        AddRemap(Buttons.LeftThumbstickLeft);
                    }
                    else if (gamePadData.LeftStickRightPressed(num))
                    {
                        AddRemap(Buttons.LeftThumbstickRight);
                    }
                    else if (gamePadData.LeftStickUpPressed(num))
                    {
                        AddRemap(Buttons.LeftThumbstickUp);
                    }
                    else if (gamePadData.LeftStickDownPressed(num))
                    {
                        AddRemap(Buttons.LeftThumbstickDown);
                    }
                    else if (gamePadData.RightStickLeftPressed(num))
                    {
                        AddRemap(Buttons.RightThumbstickLeft);
                    }
                    else if (gamePadData.RightStickRightPressed(num))
                    {
                        AddRemap(Buttons.RightThumbstickRight);
                    }
                    else if (gamePadData.RightStickDownPressed(num))
                    {
                        AddRemap(Buttons.RightThumbstickDown);
                    }
                    else if (gamePadData.RightStickUpPressed(num))
                    {
                        AddRemap(Buttons.RightThumbstickUp);
                    }
                    else if (gamePadData.LeftTriggerPressed(num))
                    {
                        AddRemap(Buttons.LeftTrigger);
                    }
                    else if (gamePadData.RightTriggerPressed(num))
                    {
                        AddRemap(Buttons.RightTrigger);
                    }
                    else if (gamePadData.Pressed(Buttons.DPadLeft))
                    {
                        AddRemap(Buttons.DPadLeft);
                    }
                    else if (gamePadData.Pressed(Buttons.DPadRight))
                    {
                        AddRemap(Buttons.DPadRight);
                    }
                    else if (gamePadData.Pressed(Buttons.DPadUp))
                    {
                        AddRemap(Buttons.DPadUp);
                    }
                    else if (gamePadData.Pressed(Buttons.DPadDown))
                    {
                        AddRemap(Buttons.DPadDown);
                    }
                    else if (gamePadData.Pressed(Buttons.A))
                    {
                        AddRemap(Buttons.A);
                    }
                    else if (gamePadData.Pressed(Buttons.B))
                    {
                        AddRemap(Buttons.B);
                    }
                    else if (gamePadData.Pressed(Buttons.X))
                    {
                        AddRemap(Buttons.X);
                    }
                    else if (gamePadData.Pressed(Buttons.Y))
                    {
                        AddRemap(Buttons.Y);
                    }
                    else if (gamePadData.Pressed(Buttons.Start))
                    {
                        AddRemap(Buttons.Start);
                    }
                    else if (gamePadData.Pressed(Buttons.Back))
                    {
                        AddRemap(Buttons.Back);
                    }
                    else if (gamePadData.Pressed(Buttons.LeftShoulder))
                    {
                        AddRemap(Buttons.LeftShoulder);
                    }
                    else if (gamePadData.Pressed(Buttons.RightShoulder))
                    {
                        AddRemap(Buttons.RightShoulder);
                    }
                    else if (gamePadData.Pressed(Buttons.LeftStick))
                    {
                        AddRemap(Buttons.LeftStick);
                    }
                    else if (gamePadData.Pressed(Buttons.RightStick))
                    {
                        AddRemap(Buttons.RightStick);
                    }
                }
            }
            
            this.timeout -= Engine.RawDeltaTime;
        }

        if (closing)
        {
            this.Alpha = Calc.Approach(this.Alpha, 0, Engine.RawDeltaTime * 8f);
            this.remappingEase = Calc.Approach(this.remappingEase, 0, Engine.RawDeltaTime * 8f);
        }
        if (Alpha <= 0 && remappingEase <= 0)
        {
            level.Frozen = false;
            level.PauseLock = false;
            RemoveSelf();
        }
    }

    public override void Render()
    {
        Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * Ease.CubeOut(this.Alpha));
        Vector2 value = new Vector2(1920f, 1080f) * 0.5f;
        base.Render();
        if (this.remappingEase > 0f)
        {
            //Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * 0.95f * Ease.CubeInOut(this.remappingEase));
            ActiveFont.Draw(Dialog.Get("KEY_CONFIG_CHANGING", null), value + new Vector2(0f, -8f), new Vector2(0.5f, 1f), Vector2.One * 0.7f, Color.LightGray * Ease.CubeIn(this.remappingEase));
            ActiveFont.Draw(currentButtons, new Vector2(1920f / 2, 1080f - 100f), new Vector2(0.5f, 1f), Vector2.One * 0.5f, Color.LightGray * Ease.CubeIn(this.remappingEase));
            ActiveFont.Draw(otherButtons, new Vector2(1920f / 2, 1080f - 100f + ActiveFont.Measure(currentButtons).Y*0.5f), new Vector2(0.5f, 1f), Vector2.One * 0.5f, Color.LightGray * Ease.CubeIn(this.remappingEase));
            ActiveFont.Draw("Deploy Elytra", value + new Vector2(0f, 8f), new Vector2(0.5f, 0f), Vector2.One * 2f, Color.White * Ease.CubeIn(this.remappingEase));
        }
    }

}
