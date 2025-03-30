﻿using Menu.Remix.MixedUI;
using UnityEngine;

namespace FastRollButton;

public class ModOptions : OptionsTemplate
{
    public static ModOptions Instance { get; } = new();

    public static void RegisterOI()
    {
        if (MachineConnector.GetRegisteredOI(Plugin.MOD_ID) != Instance)
        {
            MachineConnector.SetRegisteredOI(Plugin.MOD_ID, Instance);
        }
    }

    // Configurables
    public static Configurable<bool> inputDisplay = Instance.config.Bind("inputDisplay", true, new ConfigurableInfo(
        "When checked, a display will appear indicating whether a fast roll is being performed and whether the fast roll button is pressed." +
        "\nAdapted from Input Display by Slime_Cubed, and fully customizable!",
        null, "", "Input Display?"));


    public static Configurable<KeyCode> keybindKeyboard = Instance.config.Bind("keybindKeyboard", KeyCode.LeftAlt, new ConfigurableInfo(
        "Keybind to fast roll for Keyboard.", null, "", "Keyboard"));

    public static Configurable<KeyCode> keybindPlayer1 = Instance.config.Bind("keybindPlayer1", KeyCode.Joystick1Button4, new ConfigurableInfo(
        "Keybind to fast roll for Player 1.", null, "", "Player 1"));

    public static Configurable<KeyCode> keybindPlayer2 = Instance.config.Bind("keybindPlayer2", KeyCode.Joystick2Button4, new ConfigurableInfo(
        "Keybind to fast roll for Player 2.", null, "", "Player 2"));

    public static Configurable<KeyCode> keybindPlayer3 = Instance.config.Bind("keybindPlayer3", KeyCode.Joystick3Button4, new ConfigurableInfo(
        "Keybind to fast roll for Player 3.", null, "", "Player 3"));

    public static Configurable<KeyCode> keybindPlayer4 = Instance.config.Bind("keybindPlayer4", KeyCode.Joystick4Button4, new ConfigurableInfo(
        "Keybind to fast roll for Player 4.", null, "", "Player 4"));


    private const int NUMBER_OF_TABS = 2;

    public override void Initialize()
    {
        base.Initialize();
        Tabs = new OpTab[NUMBER_OF_TABS];
        var tabIndex = -1;

        AddInputTab(ref tabIndex);
        AddInputDisplayTab(ref tabIndex);
    }

    private void AddInputTab(ref int tabIndex)
    {
        AddTab(ref tabIndex, "Input");

        AddCheckBox(inputDisplay);
        DrawCheckBoxes(ref Tabs[tabIndex], 150.0f);
        AddNewLine(4);

        AddAndDrawKeybinder(keybindKeyboard, ref Tabs[tabIndex]);
        AddNewLine();

        AddAndDrawKeybinder(keybindPlayer1, ref Tabs[tabIndex]);
        AddNewLine();

        AddAndDrawKeybinder(keybindPlayer2, ref Tabs[tabIndex]);
        AddNewLine();

        AddAndDrawKeybinder(keybindPlayer3, ref Tabs[tabIndex]);
        AddNewLine();

        AddAndDrawKeybinder(keybindPlayer4, ref Tabs[tabIndex]);

        DrawBox(ref Tabs[tabIndex]);
    }

    
    // Input Display
    public static Dictionary<int, InputGraphic> InputGraphics { get; } = [];

    public static Vector2 InputDisplayPos
    {
        get => new(SavedPosX.Value, SavedPosY.Value);
        set
        {
            if (SavedPosX.Value == value.x && SavedPosY.Value == value.y)
            {
                return;
            }

            IsDisplayPosDirty = true;
            SavedPosX.Value = value.x;
            SavedPosY.Value = value.y;
        }
    }

    public static bool IsDisplayPosDirty { get; set; }

    public static Configurable<float> SavedPosX { get; } = Instance.config.Bind(nameof(SavedPosX), MachineConnector.IsThisModActive("slime-cubed.inputdisplay") ? 8.0f : 64.0f);
    public static Configurable<float> SavedPosY { get; } = Instance.config.Bind(nameof(SavedPosY), 64.0f);


    public static Configurable<bool> inputIsIndicator = Instance.config.Bind("inputIsIndicator", false, new ConfigurableInfo(
        "When checked, input is shown by the small indicator, fast roll being performed is shown by the button." +
        "\nWhen unchecked, this is reversed.",
        null, "", "Input is Indicator?"));

    public static Configurable<bool> outlineColorLabels = Instance.config.Bind("outlineColorLabels", false, new ConfigurableInfo(
        "When checked, sets button labels as the outline color, as opposed to the opposite on/off color.",
        null, "", "Outline Color Labels?"));



    public static readonly Configurable<float> alpha = Instance.config.Bind("alpha", 1.0f, new ConfigurableInfo(
        "How opaque the display is (100% by default).",
        new ConfigAcceptableRange<float>(0.0f, 1.0f), "", "Alpha"));

    public static readonly Configurable<float> scale = Instance.config.Bind("scale", 0.5f, new ConfigurableInfo(
        "The scale factor of the display (50% by default).",
        new ConfigAcceptableRange<float>(0.2f, 1.0f), "", "Scale"));



    public static Configurable<string> inputDisplayText = Instance.config.Bind("inputDisplayText", "Roll", new ConfigurableInfo(
        "Text shown on the Input Display.", null, "", "Input Display Text"));



    public static readonly Configurable<Color> backColor = Instance.config.Bind("backColor", Color.white, new ConfigurableInfo(
        "...",
        null, "", "Back Color"));

    public static readonly Configurable<Color> onColor = Instance.config.Bind("onColor", new Color(0.75f, 0.75f, 0.75f), new ConfigurableInfo(
        "...",
        null, "", "On Color"));

    public static readonly Configurable<Color> offColor = Instance.config.Bind("offColor", new Color(0.1f, 0.1f, 0.1f), new ConfigurableInfo(
        "...",
        null, "", "Off Color"));


    public static float Scale => scale.Value * 2.0f;

    public void AddInputDisplayTab(ref int tabIndex)
    {
        AddTab(ref tabIndex, "Input Display");

        AddCheckBox(inputIsIndicator, (string)inputIsIndicator.info.Tags[0]);
        AddCheckBox(outlineColorLabels, (string)outlineColorLabels.info.Tags[0]);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine();

        AddFloatSlider(alpha, (string)alpha.info.Tags[0], "0%", "100%");
        AddFloatSlider(scale, (string)scale.info.Tags[0], "20%", "100%");
        DrawFloatSliders(ref Tabs[tabIndex]);


        var _inputDisplayText = new OpTextBox(inputDisplayText, new Vector2(235.0f, 207.5f), 150.0f);
        Tabs[tabIndex].AddItems(_inputDisplayText, new OpLabel(new Vector2(90.0f, 210.0f), new Vector2(150f, 16.0f), (string)inputDisplayText.info.Tags[0]));


        AddNewLine(2);


        var offset = new Vector2(0.0f, -150.0f);

        var _backCol = new OpColorPicker(backColor, new Vector2(32f + offset.x, 159f + offset.y));
        Tabs[tabIndex].AddItems(_backCol, new OpLabel(new Vector2(32f + offset.x, 317f + offset.y), new Vector2(150f + offset.x, 16f + offset.y), "Outline Color"));

        var _offCol = new OpColorPicker(offColor, new Vector2(225f + offset.x, 159f + offset.y));
        Tabs[tabIndex].AddItems(_offCol, new OpLabel(new Vector2(225f + offset.x, 317f + offset.y), new Vector2(150f + offset.x, 16f + offset.y), "Off Color"));

        var _onCol = new OpColorPicker(onColor, new Vector2(418f + offset.x, 159f + offset.y));
        Tabs[tabIndex].AddItems(_onCol, new OpLabel(new Vector2(418f + offset.x, 317f + offset.y), new Vector2(150f + offset.x, 16f + offset.y), "On Color"));


        DrawBox(ref Tabs[tabIndex]);
    }
}
