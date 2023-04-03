using System.Collections.Generic;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace FastRollButton
{
    // Based on the options script from SBCameraScroll by SchuhBaum
    // https://github.com/SchuhBaum/SBCameraScroll/blob/Rain-World-v1.9/SourceCode/MainModOptions.cs
    public class Options : OptionInterface
    {
        public static Options instance = new Options();
        private const string AUTHORS_NAME = "forthbridge";

        #region Options

        public static Configurable<bool> inputDisplay = instance.config.Bind("inputDisplay", true, new ConfigurableInfo(
            "When checked, a display will appear indicating whether a fast roll is being performed and whether the fast roll button is pressed." +
            "\nAdapted from Input Display by Slime_Cubed, and fully customizable!",
            null, "", "Input Display?"));

        public static Configurable<bool> debugDisplay = instance.config.Bind("debugDisplay", false, new ConfigurableInfo(
            "When checked, enables the debug display - text will appear in the top left hand corner indicating whether a fast roll is being performed." +
            "\nUnlike the input display, supports indicating multiple players, but doesn't indicate whether the fast roll button is pressed.",
            null, "", "Debug Display?"));



        public static Configurable<KeyCode> keybindKeyboard = instance.config.Bind("keybindKeyboard", KeyCode.LeftAlt, new ConfigurableInfo(
            "Keybind to fast roll for Keyboard.", null, "", "Keyboard"));

        public static Configurable<KeyCode> keybindPlayer1 = instance.config.Bind("keybindPlayer1", KeyCode.Joystick1Button4, new ConfigurableInfo(
            "Keybind to fast roll for Player 1.", null, "", "Player 1"));

        public static Configurable<KeyCode> keybindPlayer2 = instance.config.Bind("keybindPlayer2", KeyCode.Joystick2Button4, new ConfigurableInfo(
            "Keybind to fast roll for Player 2.", null, "", "Player 2"));

        public static Configurable<KeyCode> keybindPlayer3 = instance.config.Bind("keybindPlayer3", KeyCode.Joystick3Button4, new ConfigurableInfo(
            "Keybind to fast roll for Player 3.", null, "", "Player 3"));

        public static Configurable<KeyCode> keybindPlayer4 = instance.config.Bind("keybindPlayer4", KeyCode.Joystick4Button4, new ConfigurableInfo(
            "Keybind to fast roll for Player 4.", null, "", "Player 4"));

        #endregion

        #region Parameters

        private readonly float spacing = 20f;
        private readonly float fontHeight = 20f;
        private readonly int numberOfCheckboxes = 2;
        private readonly float checkBoxSize = 60.0f;
        private float CheckBoxWithSpacing => checkBoxSize + 0.25f * spacing;


        private Vector2 marginX = new();
        private Vector2 pos = new();

        private readonly List<float> boxEndPositions = new();

        private readonly List<Configurable<bool>> checkBoxConfigurables = new();
        private readonly List<OpLabel> checkBoxesTextLabels = new();

        private readonly List<OpLabel> textLabels = new();

        private readonly List<Configurable<float>> floatSliderConfigurables = new();
        private readonly List<string> floatSliderMainTextLabels = new();
        private readonly List<OpLabel> floatSliderTextLabelsLeft = new();
        private readonly List<OpLabel> floatSliderTextLabelsRight = new();

        #endregion



        private const int NUMBER_OF_TABS = 2;

        public override void Initialize()
        {
            base.Initialize();
            Tabs = new OpTab[NUMBER_OF_TABS];
            int tabIndex = -1;

            AddTab(ref tabIndex, "Input");

            AddCheckBox(inputDisplay, (string)inputDisplay.info.Tags[0]);
            AddCheckBox(debugDisplay, (string)debugDisplay.info.Tags[0]);
            DrawCheckBoxes(ref Tabs[tabIndex]);
            AddNewLine(4);

            DrawKeybinder(keybindKeyboard, ref Tabs[tabIndex]);
            AddNewLine(1);

            DrawKeybinder(keybindPlayer1, ref Tabs[tabIndex]);
            AddNewLine(1);

            DrawKeybinder(keybindPlayer2, ref Tabs[tabIndex]);
            AddNewLine(1);

            DrawKeybinder(keybindPlayer3, ref Tabs[tabIndex]);
            AddNewLine(1);

            DrawKeybinder(keybindPlayer4, ref Tabs[tabIndex]);
            

            DrawBox(ref Tabs[tabIndex]);


            AddInputDisplay(ref tabIndex);
        }



        #region UI Elements

        private void AddTab(ref int tabIndex, string tabName)
        {
            tabIndex++;
            Tabs[tabIndex] = new OpTab(this, tabName);
            InitializeMarginAndPos();

            AddNewLine();
            AddTextLabel(Plugin.MOD_NAME, bigText: true);
            DrawTextLabels(ref Tabs[tabIndex]);

            AddNewLine(0.5f);
            AddTextLabel("Version " + Plugin.VERSION, FLabelAlignment.Left);
            AddTextLabel("by " + AUTHORS_NAME, FLabelAlignment.Right);
            DrawTextLabels(ref Tabs[tabIndex]);

            AddNewLine();
            AddBox();
        }

        private void InitializeMarginAndPos()
        {
            marginX = new Vector2(50f, 550f);
            pos = new Vector2(50f, 600f);
        }

        private void AddNewLine(float spacingModifier = 1f)
        {
            pos.x = marginX.x; // left margin
            pos.y -= spacingModifier * spacing;
        }

        

        private void AddBox()
        {
            marginX += new Vector2(spacing, -spacing);
            boxEndPositions.Add(pos.y); // end position > start position
            AddNewLine();
        }

        private void DrawBox(ref OpTab tab)
        {
            marginX += new Vector2(-spacing, spacing);
            AddNewLine();

            float boxWidth = marginX.y - marginX.x;
            int lastIndex = boxEndPositions.Count - 1;

            tab.AddItems(new OpRect(pos, new Vector2(boxWidth, boxEndPositions[lastIndex] - pos.y)));
            boxEndPositions.RemoveAt(lastIndex);
        }


        private void DrawKeybinder(Configurable<KeyCode> configurable, ref OpTab tab)
        {
            string name = (string)configurable.info.Tags[0];

            tab.AddItems(
                new OpLabel(new Vector2(115.0f, pos.y), new Vector2(100f, 34f), name)
                {
                    alignment = FLabelAlignment.Right,
                    verticalAlignment = OpLabel.LabelVAlignment.Center,
                    description = configurable.info?.description
                },
                new OpKeyBinder(configurable, new Vector2(235.0f, pos.y), new Vector2(146f, 30f), false)
            );

            AddNewLine(2);
        }


        private void AddCheckBox(Configurable<bool> configurable, string text)
        {
            checkBoxConfigurables.Add(configurable);
            checkBoxesTextLabels.Add(new OpLabel(new Vector2(), new Vector2(), text, FLabelAlignment.Left));
        }

        private void DrawCheckBoxes(ref OpTab tab) // changes pos.y but not pos.x
        {
            if (checkBoxConfigurables.Count != checkBoxesTextLabels.Count) return;

            float width = marginX.y - marginX.x;
            float elementWidth = (width - (numberOfCheckboxes - 1) * 0.5f * spacing) / numberOfCheckboxes;
            pos.y -= checkBoxSize;
            float _posX = pos.x;

            for (int checkBoxIndex = 0; checkBoxIndex < checkBoxConfigurables.Count; ++checkBoxIndex)
            {
                Configurable<bool> configurable = checkBoxConfigurables[checkBoxIndex];
                OpCheckBox checkBox = new(configurable, new Vector2(_posX, pos.y))
                {
                    description = configurable.info?.description ?? ""
                };
                tab.AddItems(checkBox);
                _posX += CheckBoxWithSpacing;

                OpLabel checkBoxLabel = checkBoxesTextLabels[checkBoxIndex];
                checkBoxLabel.pos = new Vector2(_posX, pos.y + 2f);
                checkBoxLabel.size = new Vector2(elementWidth - CheckBoxWithSpacing, fontHeight);
                tab.AddItems(checkBoxLabel);

                if (checkBoxIndex < checkBoxConfigurables.Count - 1)
                {
                    if ((checkBoxIndex + 1) % numberOfCheckboxes == 0)
                    {
                        AddNewLine();
                        pos.y -= checkBoxSize;
                        _posX = pos.x;
                    }
                    else
                    {
                        _posX += elementWidth - CheckBoxWithSpacing + 0.5f * spacing;
                    }
                }
            }

            checkBoxConfigurables.Clear();
            checkBoxesTextLabels.Clear();
        }


        private void AddTextLabel(string text, FLabelAlignment alignment = FLabelAlignment.Center, bool bigText = false)
        {
            float textHeight = (bigText ? 2f : 1f) * fontHeight;
            if (textLabels.Count == 0)
            {
                pos.y -= textHeight;
            }

            OpLabel textLabel = new(new Vector2(), new Vector2(20f, textHeight), text, alignment, bigText) // minimal size.x = 20f
            {
                autoWrap = true
            };
            textLabels.Add(textLabel);
        }

        private void DrawTextLabels(ref OpTab tab)
        {
            if (textLabels.Count == 0)
            {
                return;
            }

            float width = (marginX.y - marginX.x) / textLabels.Count;
            foreach (OpLabel textLabel in textLabels)
            {
                textLabel.pos = pos;
                textLabel.size += new Vector2(width - 20f, 0.0f);
                tab.AddItems(textLabel);
                pos.x += width;
            }

            pos.x = marginX.x;
            textLabels.Clear();
        }


        private void AddFloatSlider(Configurable<float> configurable, string text, string sliderTextLeft = "", string sliderTextRight = "")
        {
            floatSliderConfigurables.Add(configurable);
            floatSliderMainTextLabels.Add(text);
            floatSliderTextLabelsLeft.Add(new OpLabel(new Vector2(), new Vector2(), sliderTextLeft, alignment: FLabelAlignment.Right)); // set pos and size when drawing
            floatSliderTextLabelsRight.Add(new OpLabel(new Vector2(), new Vector2(), sliderTextRight, alignment: FLabelAlignment.Left));
        }

        private void DrawFloatSliders(ref OpTab tab)
        {
            if (floatSliderConfigurables.Count != floatSliderMainTextLabels.Count) return;
            if (floatSliderConfigurables.Count != floatSliderTextLabelsLeft.Count) return;
            if (floatSliderConfigurables.Count != floatSliderTextLabelsRight.Count) return;

            float width = marginX.y - marginX.x;
            float sliderCenter = marginX.x + 0.5f * width;
            float sliderLabelSizeX = 0.2f * width;
            float sliderSizeX = width - 2.0f * sliderLabelSizeX - spacing;

            for (int sliderIndex = 0; sliderIndex < floatSliderConfigurables.Count; ++sliderIndex)
            {
                AddNewLine(2.0f);

                OpLabel opLabel = floatSliderTextLabelsLeft[sliderIndex];
                opLabel.pos = new Vector2(marginX.x, pos.y + 5.0f);
                opLabel.size = new Vector2(sliderLabelSizeX, fontHeight);
                tab.AddItems(opLabel);

                Configurable<float> configurable = floatSliderConfigurables[sliderIndex];
                OpFloatSlider slider = new(configurable, new Vector2(sliderCenter - 0.5f * sliderSizeX, pos.y), (int)sliderSizeX, 2)
                {
                    size = new Vector2(sliderSizeX, fontHeight),
                    description = configurable.info?.description ?? ""
                };
                tab.AddItems(slider);

                opLabel = floatSliderTextLabelsRight[sliderIndex];
                opLabel.pos = new Vector2(sliderCenter + 0.5f * sliderSizeX + 0.5f * spacing, pos.y + 5f);
                opLabel.size = new Vector2(sliderLabelSizeX, fontHeight);
                tab.AddItems(opLabel);

                AddTextLabel(floatSliderMainTextLabels[sliderIndex]);
                DrawTextLabels(ref tab);

                if (sliderIndex < floatSliderConfigurables.Count - 1)
                    AddNewLine();
            }

            floatSliderConfigurables.Clear();
            floatSliderMainTextLabels.Clear();
            floatSliderTextLabelsLeft.Clear();
            floatSliderTextLabelsRight.Clear();
        }

        #endregion

        #region Input Display

        public static InputGraphic[] inputGraphics = new InputGraphic[1];
        public static Vector2 inputDisplayOrigin = Vector2.zero;


        public static Configurable<bool> inputIsIndicator = instance.config.Bind("inputIsIndicator", false, new ConfigurableInfo(
            "When checked, input is shown by the small indicator, fast roll being performed is shown by the button." +
            "\nWhen unchecked, this is reversed.",
            null, "", "Input is Indicator?"));

        public static Configurable<bool> outlineColorLabels = instance.config.Bind("outlineColorLabels", false, new ConfigurableInfo(
            "When checked, sets button labels as the outline color, as opposed to the opposite on/off color.",
            null, "", "Outline Color Labels?"));



        public static readonly Configurable<float> alpha = instance.config.Bind("alpha", 1.0f, new ConfigurableInfo(
            "How opaque the display is (100% by default).",
            new ConfigAcceptableRange<float>(0.0f, 1.0f), "", "Alpha"));

        public static readonly Configurable<float> scale = instance.config.Bind("scale", 0.5f, new ConfigurableInfo(
            "The scale factor of the display (50% by default).",
            new ConfigAcceptableRange<float>(0.2f, 1.0f), "", "Scale"));



        public static Configurable<string> inputDisplayText = instance.config.Bind("inputDisplayText", "Roll", new ConfigurableInfo(
            "Text shown on the Input Display.", null, "", "Input Display Text"));



        public static readonly Configurable<Color> backColor = instance.config.Bind("backColor", Color.white, new ConfigurableInfo(
            "...",
            null, "", "Back Color"));

        public static readonly Configurable<Color> onColor = instance.config.Bind("onColor", new Color(0.75f, 0.75f, 0.75f), new ConfigurableInfo(
            "...",
            null, "", "On Color"));

        public static readonly Configurable<Color> offColor = instance.config.Bind("offColor", new Color(0.1f, 0.1f, 0.1f), new ConfigurableInfo(
            "...",
            null, "", "Off Color"));



        public static float Scale => scale.Value * 2.0f;

        public void AddInputDisplay(ref int tabIndex)
        {
            AddTab(ref tabIndex, "Input Display");

            AddCheckBox(inputIsIndicator, (string)inputIsIndicator.info.Tags[0]);
            AddCheckBox(outlineColorLabels, (string)outlineColorLabels.info.Tags[0]);
            DrawCheckBoxes(ref Tabs[tabIndex]);

            AddNewLine(1);

            AddFloatSlider(alpha, (string)alpha.info.Tags[0], "0%", "100%");
            AddFloatSlider(scale, (string)scale.info.Tags[0], "20%", "100%");
            DrawFloatSliders(ref Tabs[tabIndex]);


            var _inputDisplayText = new OpTextBox(inputDisplayText, new Vector2(235.0f, 207.5f), 150.0f);
            Tabs[tabIndex].AddItems(_inputDisplayText, new OpLabel(new Vector2(90.0f, 210.0f), new Vector2(150f, 16.0f), (string)inputDisplayText.info.Tags[0]));


            AddNewLine(2);
            

            Vector2 offset = new Vector2(0.0f, -150.0f);

            var _backCol = new OpColorPicker(backColor, new Vector2(32f + offset.x, 159f + offset.y));
            Tabs[tabIndex].AddItems(_backCol, new OpLabel(new Vector2(32f + offset.x, 317f + offset.y), new Vector2(150f + offset.x, 16f + offset.y), "Outline Color"));
            
            var _offCol = new OpColorPicker(offColor, new Vector2(225f + offset.x, 159f + offset.y));
            Tabs[tabIndex].AddItems(_offCol, new OpLabel(new Vector2(225f + offset.x, 317f + offset.y), new Vector2(150f + offset.x, 16f + offset.y), "Off Color"));

            var _onCol = new OpColorPicker(onColor, new Vector2(418f + offset.x, 159f + offset.y));
            Tabs[tabIndex].AddItems(_onCol, new OpLabel(new Vector2(418f + offset.x, 317f + offset.y), new Vector2(150f + offset.x, 16f + offset.y), "On Color"));


            DrawBox(ref Tabs[tabIndex]);
        }

        #endregion
    }
}