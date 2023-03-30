using Expedition;
using ImprovedInput;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using static MonoMod.InlineRT.MonoModRule;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
using OpCodes = Mono.Cecil.Cil.OpCodes;
using Random = UnityEngine.Random;

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

            fastRollLabel.x = 40.0f;
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
        private static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
        {
            orig(self);

            FastRoll(self);

            CheckIfFastRoll(self);
        }


        private static void FastRoll(Player self)
        {
            if (!IsFastRollInput(self)) return;


            if (self.animation != Player.AnimationIndex.Roll) return;


            // No full down input, apply just enough analogue down input to trigger downDiagonal
            self.input[0].y = 0;
            self.input[0].analogueDir = new Vector2(self.input[0].x, -0.06f);


            // Taken verbatim from PlayerInputLogic
            if (ModManager.MMF)
            {
                if (self.input[0].analogueDir.y < -0.05f || self.input[0].y < 0)
                {
                    if (self.input[0].analogueDir.x < -0.05f || self.input[0].x < 0)
                        self.input[0].downDiagonal = -1;

                    else if (self.input[0].analogueDir.x > 0.05f || self.input[0].x > 0)
                        self.input[0].downDiagonal = 1;
                }
            }
            else if (self.input[0].analogueDir.y < -0.05f)
            {
                if (self.input[0].analogueDir.x < -0.05f)
                    self.input[0].downDiagonal = -1;

                else if (self.input[0].analogueDir.x > 0.05f)
                    self.input[0].downDiagonal = 1;
            }
        }

        private static bool IsFastRollInput(Player self)
        {
            if (Options.isFastRollAutomatic.Value) return true;

            return self.playerState.playerNumber switch
            {
                0 => Input.GetKey(Options.keybindPlayer1.Value) || Input.GetKey(Options.keybindKeyboard.Value),
                1 => Input.GetKey(Options.keybindPlayer2.Value),
                2 => Input.GetKey(Options.keybindPlayer3.Value),
                3 => Input.GetKey(Options.keybindPlayer4.Value),

                _ => false
            };
        }


        private static void CheckIfFastRoll(Player self)
        {
            fastRollLabel.isVisible = false;

            if (!Options.debugDisplay.Value) return;



            if (self.animation != Player.AnimationIndex.Roll) return;

            if (self.input[0].downDiagonal == 0) return;

            if (self.input[0].y == -1) return;


            fastRollLabel.isVisible = true;
        }
    }
}
