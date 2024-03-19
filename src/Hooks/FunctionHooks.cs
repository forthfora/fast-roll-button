using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using UnityEngine;

namespace FastRollButton;

public static partial class Hooks
{
    private static void ApplyFunctionHooks()
    {
        Application.quitting += Application_quitting;

        try
        {
            IL.RWInput.PlayerInputLogic_int_int += RWInput_PlayerInputLogic_int_int;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Input IL Exception:\n" + e);
        }
    }


    // So that the position of the display saves on quit
    private static void Application_quitting()
    {
        if (ModOptions.IsDisplayPosDirty)
        {
            MachineConnector.SaveConfig(ModOptions.Instance);
        }
    }


    // Input when the roll button is held
    const float FAST_ROLL_Y_INPUT = -0.06f;

    private static void RWInput_PlayerInputLogic_int_int(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(MoveType.Before,
            x => x.MatchLdcR4(0.5f));

        c.GotoNext(MoveType.After,
            x => x.MatchLdsfld<ModManager>(nameof(ModManager.MMF)));

        c.Emit(OpCodes.Pop);

        c.Emit(OpCodes.Ldloc_0);
        c.Emit(OpCodes.Ldarg_1);
        c.EmitDelegate<Func<Player.InputPackage, int, Player.InputPackage>>((input, playerNumber) =>
        {
            if (!IsFastRollInput(playerNumber)) return input;

            if (input.y != 0) return input;

            // No full down input, apply just enough analogue down input to trigger downDiagonal
            input.analogueDir.y = FAST_ROLL_Y_INPUT;
            return input;
        });
        c.Emit(OpCodes.Stloc_0);

        c.EmitDelegate(() => ModManager.MMF);
    }

    public static bool IsFastRollInput(int playerNumber)
    {
        return playerNumber switch
        {
            0 => Input.GetKey(ModOptions.keybindPlayer1.Value) || Input.GetKey(ModOptions.keybindKeyboard.Value),
            1 => Input.GetKey(ModOptions.keybindPlayer2.Value),
            2 => Input.GetKey(ModOptions.keybindPlayer3.Value),
            3 => Input.GetKey(ModOptions.keybindPlayer4.Value),

            _ => false
        };
    }

    public static bool IsAnyFastRollInput()
    {
        for (int i = 0; i < 4; i++)
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

        if (mainLoop is not RainWorldGame game) return false;

        foreach (var abstractPlayers in game.AlivePlayers)
        {
            if (abstractPlayers.realizedCreature is not Player player) continue;
            
            if (CheckIfFastRoll(player))
            {
                return true;
            }
        }

        return false;
    }

    private static bool CheckIfFastRoll(Player self)
    {
        if (self.animation != Player.AnimationIndex.Roll) return false;

        if (self.input[0].downDiagonal == 0) return false;

        if (self.input[0].y == -1) return false;

        return true;
    }
}
