using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

using static FastRollButton.FastRoll_Helpers;

namespace FastRollButton;

public static class FastRoll_Hooks
{
    public static void ApplyHooks()
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

    private static void RWInput_PlayerInputLogic_int_int(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.Before,
            x => x.MatchLdcR4(0.5f)))
        {
            throw new Exception("Goto Failed (1)");
        }

        if (!c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<ModManager>(nameof(ModManager.MMF))))
        {
            throw new Exception("Goto Failed (2)");
        }

        c.Emit(OpCodes.Pop);

        c.Emit(OpCodes.Ldloc_0);
        c.Emit(OpCodes.Ldarg_1);
        c.EmitDelegate<Func<Player.InputPackage, int, Player.InputPackage>>((input, playerNumber) =>
        {
            if (!IsFastRollInput(playerNumber))
            {
                return input;
            }

            if (input.y != 0)
            {
                return input;
            }

            // No full down input, apply just enough analogue down input to trigger downDiagonal
            input.analogueDir.y = FAST_ROLL_Y_INPUT;
            return input;
        });
        c.Emit(OpCodes.Stloc_0);

        c.EmitDelegate(() => ModManager.MMF);
    }
}
