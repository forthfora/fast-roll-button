using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using UnityEngine;

namespace FastRollButton;

public static class FastRoll_Helpers
{
    // Input when the roll button is held
    public const float FAST_ROLL_Y_INPUT = -0.06f;

    public static bool IsFastRollInput(int playerNumber)
    {
        return playerNumber switch
        {
            0 => Input.GetKey(ModOptions.keybindPlayer1.Value) || Input.GetKey(ModOptions.keybindKeyboard.Value),
            1 => Input.GetKey(ModOptions.keybindPlayer2.Value),
            2 => Input.GetKey(ModOptions.keybindPlayer3.Value),
            3 => Input.GetKey(ModOptions.keybindPlayer4.Value),

            _ => false,
        };
    }

    public static bool IsAnyFastRollInput()
    {
        for (var i = 0; i < 4; i++)
        {
            if (IsFastRollInput(i))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsAnyPlayerFastRolling()
    {
        var mainLoop = Custom.rainWorld?.processManager?.currentMainLoop;

        if (mainLoop is not RainWorldGame game)
        {
            return false;
        }

        foreach (var abstractPlayers in game.AlivePlayers)
        {
            if (abstractPlayers.realizedCreature is not Player player)
            {
                continue;
            }

            if (player.IsFastRolling())
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsFastRolling(this Player self)
    {
        if (self.animation != Player.AnimationIndex.Roll)
        {
            return false;
        }

        if (self.input[0].downDiagonal == 0)
        {
            return false;
        }

        if (self.input[0].y == -1)
        {
            return false;
        }

        return true;
    }
}
