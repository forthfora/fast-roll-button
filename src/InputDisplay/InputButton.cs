using RWCustom;
using UnityEngine;

namespace FastRollButton;

// Adapted from Input Display: https://github.com/SlimeCubed/RWInputDisplay/blob/remix
// Thanks Slime_Cubed!
public class InputButton
{
    public static float Size => Mathf.Floor(24f * ModOptions.Scale) * 2f;
    public static float Spacing => Size + Mathf.Floor(Size / 6f);

    public InputGraphic Parent { get; }
    public Vector2 RelativePos { get; }

    private FSprite Back { get; }
    private FSprite Front { get; }

    private FLabel? KeyLabel { get; }
    private FSprite? KeySprite { get; }

    private FSprite FastRollIndicator { get; }
    private Func<bool, bool> InputGetter { get; }

    private InputButton(InputGraphic parent, Vector2 pos, Func<bool, bool> inputGetter)
    {
        this.Parent = parent;
        this.InputGetter = inputGetter;

        RelativePos = pos;

        Back = new FSprite("pixel") { anchorX = 0f, anchorY = 0f, scale = Size, color = ModOptions.backColor.Value };
        Front = new FSprite("pixel") { anchorX = 0f, anchorY = 0f, scale = Size - 2f };
        FastRollIndicator = new FSprite("deerEyeB") { anchorX = 0f, anchorY = 0f };
    }

    public InputButton(InputGraphic parent, Vector2 pos, string keyName, Func<bool, bool> inputGetter) : this(parent, pos, inputGetter)
    {
        KeyLabel = new FLabel(Custom.GetFont(), keyName);

        Move(Vector2.zero);
        AddToContainer();

        if (ModOptions.Scale < 0.75f)
        {
            KeyLabel.text = keyName.Substring(0, 1);
        }
    }

    public InputButton(InputGraphic parent, Vector2 pos, FSprite keySprite, Func<bool, bool> inputGetter) : this(parent, pos, inputGetter)
    {
        this.KeySprite = keySprite;
        Move(Vector2.zero);
        AddToContainer();
    }

    public bool IsMouseOver
    {
        get
        {
            Vector2 mp = Input.mousePosition;
            mp.x -= ModOptions.InputDisplayPos.x + RelativePos.x;
            mp.y -= ModOptions.InputDisplayPos.y + RelativePos.y;

            if (mp.x < 0f || mp.y < 0f)
            {
                return false;
            }

            if (mp.x > Size || mp.y > Size)
            {
                return false;
            }

            return true;
        }
    }

    public void AddToContainer()
    {
        var c = Parent.ButtonContainer;
        c.AddChild(Back);
        c.AddChild(Front);

        c.AddChild(FastRollIndicator);
        
        if (KeyLabel != null)
        {
            c.AddChild(KeyLabel);
        }

        if (KeySprite != null)
        {
            c.AddChild(KeySprite);
        }
    }

    public void RemoveFromContainer()
    {
        Back.RemoveFromContainer();
        Front.RemoveFromContainer();

        KeyLabel?.RemoveFromContainer();
        KeySprite?.RemoveFromContainer();

        FastRollIndicator.RemoveFromContainer();
    }

    public void Move(Vector2 origin)
    {
        var pos = origin + RelativePos + Vector2.one * 0.01f;

        Back.SetPosition(pos);
        Front.x = pos.x + 1f;
        Front.y = pos.y + 1f;


        if (KeyLabel != null)
        {
            KeyLabel.x = pos.x + Size / 2f;
            KeyLabel.y = pos.y + Size / 2f;
        }

        if (KeySprite != null)
        {
            KeySprite.x = pos.x + Size / 2f;
            KeySprite.y = pos.y + Size / 2f;
        }
        
        var rtIndOffset = 1f + Mathf.Floor(5f * Mathf.Min(ModOptions.Scale, 1f));
        FastRollIndicator.x = pos.x + rtIndOffset;
        FastRollIndicator.y = pos.y + rtIndOffset;
    }

    public void Update()
    {
        var isInput = InputGetter(false);
        var isIndicator = InputGetter(true);


        Front.color = isInput ? ModOptions.onColor.Value : ModOptions.offColor.Value;
        FastRollIndicator.color = isIndicator ? isInput ? ModOptions.offColor.Value : ModOptions.onColor.Value : Front.color;


        if (KeyLabel != null)
        {
            KeyLabel.color = ModOptions.outlineColorLabels.Value ? ModOptions.backColor.Value : (isInput ? ModOptions.offColor.Value : ModOptions.onColor.Value);
        }

        if (KeySprite != null)
        {
            KeySprite.color = ModOptions.outlineColorLabels.Value ? ModOptions.backColor.Value : (isInput ? ModOptions.offColor.Value : ModOptions.onColor.Value);
        }
    }
}
