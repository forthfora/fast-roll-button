﻿using RWCustom;
using System;
using System.Linq;
using UnityEngine;


namespace FastRollButton
{
    internal static class Hooks
    {
        public static void ApplyHooks() => On.RainWorld.OnModsInit += RainWorld_OnModsInit;


        private static bool isInit = false;

        private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            try
            {
                if (isInit) return;
                isInit = true;

                MachineConnector.SetRegisteredOI(Plugin.MOD_ID, Options.instance);

                On.RainWorldGame.ctor += RainWorldGame_ctor;
                On.RainWorldGame.ShutDownProcess += RainWorldGame_ShutDownProcess;

                On.Player.checkInput += Player_checkInput;
                On.RWInput.PlayerInput += RWInput_PlayerInput;
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex);
            }
            finally
            {
                orig(self);
            }
        }



        // Debug Label
        private static FLabel fastRollLabel = null!;

        private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
        {
            orig(self, manager);

            fastRollLabel = new FLabel(Custom.GetFont(), "Fast Rolling");

            Futile.stage.AddChild(fastRollLabel);

            fastRollLabel.alignment = FLabelAlignment.Left;
            fastRollLabel.x = 10.0f;
            fastRollLabel.y = self.rainWorld.options.ScreenSize.y - 10.0f;
            fastRollLabel.color = new Color(1.0f, 1.0f, 0.5f);
            fastRollLabel.isVisible = false;
        }

        private static void RainWorldGame_ShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
        {
            orig(self);

            fastRollLabel.RemoveFromContainer();
        }



        // Fast Roll
        const float FAST_ROLL_Y_INPUT = -0.06f;

        private static Player.InputPackage RWInput_PlayerInput(On.RWInput.orig_PlayerInput orig, int playerNumber, RainWorld rainWorld)
        {
            Player.InputPackage input = orig(playerNumber, rainWorld);

            if (!IsFastRollInput(playerNumber)) return input;

            if (input.y != 0) return input;


            // No full down input, apply just enough analogue down input to trigger downDiagonal
            input.analogueDir.y = FAST_ROLL_Y_INPUT;


            // Taken verbatim from PlayerInputLogic
            if (ModManager.MMF)
            {
                if (input.analogueDir.y < -0.05f || input.y < 0)
                {
                    if (input.analogueDir.x < -0.05f || input.x < 0)
                        input.downDiagonal = -1;

                    else if (input.analogueDir.x > 0.05f || input.x > 0)
                        input.downDiagonal = 1;
                }
            }
            else if (input.analogueDir.y < -0.05f)
            {
                if (input.analogueDir.x < -0.05f)
                    input.downDiagonal = -1;

                else if (input.analogueDir.x > 0.05f)
                    input.downDiagonal = 1;
            }

            return input;
        }

        private static bool IsFastRollInput(int playerNumber)
        {
            return playerNumber switch
            {
                0 => Input.GetKey(Options.keybindPlayer1.Value) || Input.GetKey(Options.keybindKeyboard.Value),
                1 => Input.GetKey(Options.keybindPlayer2.Value),
                2 => Input.GetKey(Options.keybindPlayer3.Value),
                3 => Input.GetKey(Options.keybindPlayer4.Value),

                _ => false
            };
        }



        // Debug Fast Roll Check
        private static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
        {
            orig(self);

            CheckIfFastRoll(self);


            fastRollLabel.text = "Fast Rolling";
            fastRollLabel.isVisible = fastRollingPlayers.Any(isFastRolling => isFastRolling);

            if (self.room.game.Players.Count == 1) return;


            for (int i = fastRollingPlayers.Length -1; i >= 0; i--)
            {
                if (!fastRollingPlayers[i]) continue;

                fastRollLabel.text = $"P{i + 1}, {fastRollLabel.text}"; 
            }
        }


        private static readonly bool[] fastRollingPlayers = new bool[4];

        private static void CheckIfFastRoll(Player self)
        {
            fastRollingPlayers[self.playerState.playerNumber] = false;

            if (!Options.debugDisplay.Value) return;



            if (self.animation != Player.AnimationIndex.Roll) return;

            if (self.input[0].downDiagonal == 0) return;

            if (self.input[0].y == -1) return;


            fastRollingPlayers[self.playerState.playerNumber] = true;
        }
    }
}
