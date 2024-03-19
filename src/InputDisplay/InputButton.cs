using RWCustom;
using System;
using UnityEngine;

namespace FastRollButton;

// Adapted from Input Display: https://github.com/SlimeCubed/RWInputDisplay/blob/remix
// Thanks Slime_Cubed!
public class InputButton
{
    public static float Size => Mathf.Floor(24f * ModOptions.Scale) * 2f;

    public static float Spacing => Size + Mathf.Floor(Size / 6f);

    public InputGraphic parent;
    public Vector2 relPos;

    private readonly FSprite back;
    private readonly FSprite front;

    private readonly FLabel key = null!;
    private readonly FSprite keySprite = null!;

    private readonly FSprite fastRollIndicator;

    private readonly Func<bool, bool> inputGetter;


    private InputButton(InputGraphic parent, Vector2 pos, Func<bool, bool> inputGetter)
    {
        this.parent = parent;
        this.inputGetter = inputGetter;

        relPos = pos;

        back = new FSprite("pixel") { anchorX = 0f, anchorY = 0f, scale = Size, color = ModOptions.backColor.Value };
        front = new FSprite("pixel") { anchorX = 0f, anchorY = 0f, scale = Size - 2f };
        fastRollIndicator = new FSprite("deerEyeB") { anchorX = 0f, anchorY = 0f };
    }

    public InputButton(InputGraphic parent, Vector2 pos, string keyName, Func<bool, bool> inputGetter) : this(parent, pos, inputGetter)
    {
        key = new FLabel(Custom.GetFont(), keyName);

        Move(Vector2.zero);
        AddToContainer();

        if (ModOptions.Scale < 0.75f)
            key.text = keyName.Substring(0, 1);
    }

    public InputButton(InputGraphic parent, Vector2 pos, FSprite keySprite, Func<bool, bool> inputGetter) : this(parent, pos, inputGetter)
    {
        this.keySprite = keySprite;
        Move(Vector2.zero);
        AddToContainer();
    }

    public bool IsMouseOver
    {
        get
        {
            Vector2 mp = Input.mousePosition;
            mp.x -= ModOptions.InputDisplayPos.x + relPos.x;
            mp.y -= ModOptions.InputDisplayPos.y + relPos.y;

            if (mp.x < 0f || mp.y < 0f)
                return false;
            
            if (mp.x > Size || mp.y > Size)
                return false;
            
            return true;
        }
    }

    public void AddToContainer()
    {
        FContainer c = parent.buttonContainer;
        c.AddChild(back);
        c.AddChild(front);

        c.AddChild(fastRollIndicator);
        
        if (key != null) c.AddChild(key);
        if (keySprite != null) c.AddChild(keySprite);
    }

    public void RemoveFromContainer()
    {
        back.RemoveFromContainer();
        front.RemoveFromContainer();

        key?.RemoveFromContainer();
        keySprite?.RemoveFromContainer();

        fastRollIndicator.RemoveFromContainer();
    }

    public void Move(Vector2 origin)
    {
        Vector2 pos = origin + relPos + Vector2.one * 0.01f;

        back.SetPosition(pos);
        front.x = pos.x + 1f;
        front.y = pos.y + 1f;


        if (key != null)
        {
            key.x = pos.x + Size / 2f;
            key.y = pos.y + Size / 2f;
        }

        if (keySprite != null)
        {
            keySprite.x = pos.x + Size / 2f;
            keySprite.y = pos.y + Size / 2f;
        }
        
        float rtIndOffset = 1f + Mathf.Floor(5f * Mathf.Min(ModOptions.Scale, 1f));
        fastRollIndicator.x = pos.x + rtIndOffset;
        fastRollIndicator.y = pos.y + rtIndOffset;
    }

    public void Update()
    {
        bool isInput = inputGetter(false);
        bool isIndicator = inputGetter(true);


        front.color = isInput ? ModOptions.onColor.Value : ModOptions.offColor.Value;
        fastRollIndicator.color = isIndicator ? isInput ? ModOptions.offColor.Value : ModOptions.onColor.Value : front.color;


        if (key != null)
            key.color = ModOptions.outlineColorLabels.Value ? ModOptions.backColor.Value : (isInput ? ModOptions.offColor.Value : ModOptions.onColor.Value);

        if (keySprite != null)
            keySprite.color = ModOptions.outlineColorLabels.Value ? ModOptions.backColor.Value : (isInput ? ModOptions.offColor.Value : ModOptions.onColor.Value);
    }
}
