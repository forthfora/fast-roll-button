using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Inputs = Player.InputPackage;


namespace FastRollButton
{
    // Adapted from Input Display: https://github.com/SlimeCubed/RWInputDisplay/blob/remix
    // Thanks Slime_Cubed!
    public class InputButton
    {
        public static float Size => Mathf.Floor(24f * Options.Scale) * 2f;

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

            back = new FSprite("pixel") { anchorX = 0f, anchorY = 0f, scale = Size, color = Options.backColor.Value };
            front = new FSprite("pixel") { anchorX = 0f, anchorY = 0f, scale = Size - 2f };
            fastRollIndicator = new FSprite("deerEyeB") { anchorX = 0f, anchorY = 0f };
        }

        public InputButton(InputGraphic parent, Vector2 pos, string keyName, Func<bool, bool> inputGetter) : this(parent, pos, inputGetter)
        {
            key = new FLabel(Custom.GetFont(), keyName);

            Move(Vector2.zero);
            AddToContainer();

            if (Options.Scale < 0.75f)
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
                mp.x -= Options.inputDisplayOrigin.x + relPos.x;
                mp.y -= Options.inputDisplayOrigin.y + relPos.y;
                if (mp.x < 0f || mp.y < 0f) return false;
                if (mp.x > Size || mp.y > Size) return false;
                return true;
            }
        }



        public void AddToContainer()
        {
            FContainer c = parent.buttonContainer;
            c.AddChild(back);
            c.AddChild(front);

            if (key != null) c.AddChild(key);
            if (keySprite != null) c.AddChild(keySprite);

            c.AddChild(fastRollIndicator);
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
            
            float rtIndOffset = 1f + Mathf.Floor(5f * Mathf.Min(Options.Scale, 1f));
            fastRollIndicator.x = pos.x + rtIndOffset;
            fastRollIndicator.y = pos.y + rtIndOffset;
        }

        public void Update()
        {
            bool isInput = inputGetter(false);
            bool isIndicator = inputGetter(true);


            front.color = isInput ? Options.onColor.Value : Options.offColor.Value;
            fastRollIndicator.color = isIndicator ? isInput ? Options.offColor.Value : Options.onColor.Value : front.color;


            if (key != null)
                key.color = Options.outlineColorLabels.Value ? Options.backColor.Value : (isInput ? Options.offColor.Value : Options.onColor.Value);

            if (keySprite != null)
                keySprite.color = Options.outlineColorLabels.Value ? Options.backColor.Value : (isInput ? Options.offColor.Value : Options.onColor.Value);
        }
    }
}
