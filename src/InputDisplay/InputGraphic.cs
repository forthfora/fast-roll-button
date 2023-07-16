using System.Linq;
using UnityEngine;

namespace FastRollButton;

// Adapted from Input Display: https://github.com/SlimeCubed/RWInputDisplay/blob/remix/RWInputDisplay
// Thanks Slime_Cubed!
public class InputGraphic
{
    public RoomCamera cam;
    public InputButton[] buttons = null!;

    public bool IsMouseOver
    {
        get
        {
            foreach (InputButton button in buttons) if (button.IsMouseOver) return true;
            return false;
        }
    }

    private bool isDragging;
    private Vector2 dragOffset;

    public FSprite displaySprite = null!;
    public FContainer buttonContainer;


    public InputGraphic(RoomCamera cam)
    {
        this.cam = cam;

        buttonContainer = new FContainer();
        Futile.stage.AddChild(buttonContainer);

        InitSprites();
    }

    public void InitSprites()
    {
        buttons = new InputButton[]
        {
            new InputButton(this, new Vector2(0.0f, 0.0f) * InputButton.Spacing, ModOptions.inputDisplayText.Value , isIndicator => {
                if (isIndicator == ModOptions.inputIsIndicator.Value)
                    return Hooks.IsAnyFastRollInput();
                
                return Hooks.fastRollingPlayers.Contains(true);
            }),
        };

        Move();
    }

    public void Remove() => buttonContainer.RemoveFromContainer();

    public void Update(float timeStacker)
    {
        // Move the input display when left bracket is pressed
        if (Input.GetKey(KeyCode.LeftBracket))
        {
            ModOptions.inputDisplayOrigin = Input.mousePosition;

            if (MachineConnector.IsThisModActive("slime-cubed.inputdisplay"))
                ModOptions.inputDisplayOrigin -= new Vector2(((InputButton.Size + InputButton.Spacing) / 2.0f) + 5.0f, 0.0f);

            Move();
        }

        // Allow dragging the input display
        if (isDragging)
        {
            if (!Input.GetMouseButton(0))
            {
                isDragging = false;
            }
            else
            {
                ModOptions.inputDisplayOrigin = (Vector2)Input.mousePosition + dragOffset;
                Move();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && IsMouseOver)
            {
                isDragging = true;
                dragOffset = ModOptions.inputDisplayOrigin - (Vector2)Input.mousePosition;
            }
        }

        foreach (InputButton button in buttons)
            button.Update();

        buttonContainer.MoveToFront();
    }

    public void Move()
    {
        buttonContainer.SetPosition(ModOptions.inputDisplayOrigin - Vector2.one * 0.5f);
        buttonContainer.alpha = ModOptions.alpha.Value;


        foreach (InputButton button in buttons)
            button.Move(new Vector2(0.0f, 0.0f));
    }
}
