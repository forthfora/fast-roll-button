using UnityEngine;

namespace FastRollButton;

// Adapted from Input Display: https://github.com/SlimeCubed/RWInputDisplay/blob/remix/RWInputDisplay
// Thanks Slime_Cubed!
public class InputGraphic
{
    public FContainer ButtonContainer { get; }
    public InputButton[] Buttons { get; set; } = null!;

    private bool IsDragging { get; set; }
    private Vector2 DragOffset { get; set; }

    public bool IsMouseOver => Buttons.Any(button => button.IsMouseOver);

    public InputGraphic()
    {
        ButtonContainer = new FContainer();
        Futile.stage.AddChild(ButtonContainer);

        InitSprites();
    }

    public void InitSprites()
    {
        Buttons =
        [
            new InputButton(this,
                new Vector2(0.0f, 0.0f) * InputButton.Spacing,
                ModOptions.inputDisplayText.Value,
                isIndicator => { return isIndicator == ModOptions.inputIsIndicator.Value ? FastRoll_Helpers.IsAnyFastRollInput() : FastRoll_Helpers.IsAnyPlayerFastRolling(); }
            ),
        ];

        Move();
    }

    public void Update()
    {
        // Move the input display when left bracket is pressed
        if (Input.GetKey(KeyCode.LeftBracket))
        {
            ModOptions.InputDisplayPos = Input.mousePosition;

            if (MachineConnector.IsThisModActive("slime-cubed.inputdisplay"))
            {
                ModOptions.InputDisplayPos -= new Vector2(((InputButton.Size + InputButton.Spacing) / 2.0f) + 5.0f, 0.0f);
            }

            Move();
        }

        // Allow dragging the input display
        if (IsDragging)
        {
            if (!Input.GetMouseButton(0))
            {
                IsDragging = false;
            }
            else
            {
                ModOptions.InputDisplayPos = (Vector2)Input.mousePosition + DragOffset;
                Move();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && IsMouseOver)
            {
                IsDragging = true;
                DragOffset = ModOptions.InputDisplayPos - (Vector2)Input.mousePosition;
            }
        }

        foreach (var button in Buttons)
        {
            button.Update();
        }

        ButtonContainer.MoveToFront();
    }

    public void Move()
    {
        ButtonContainer.SetPosition(ModOptions.InputDisplayPos - Vector2.one * 0.5f);
        ButtonContainer.alpha = ModOptions.alpha.Value;

        foreach (var button in Buttons)
        {
            button.Move(new Vector2(0.0f, 0.0f));
        }
    }
}
