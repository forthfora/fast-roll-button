using RWCustom;
using System;
using System.Linq;
using UnityEngine;


namespace FastRollButton;

public static partial class Hooks
{
    public static void ApplyInit() => On.RainWorld.OnModsInit += RainWorld_OnModsInit;

    private static bool IsInit { get; set; } = false;

    private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        try
        {
            if (IsInit) return;
            IsInit = true;

            ApplyHooks();

            var mod = ModManager.ActiveMods.FirstOrDefault(mod => mod.id == Plugin.MOD_ID);

            Plugin.MOD_NAME = mod.name;
            Plugin.VERSION = mod.version;
            Plugin.AUTHORS = mod.authors;

            MachineConnector.SetRegisteredOI(Plugin.MOD_ID, ModOptions.Instance);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("OnModsInit:\n" + e.Message);
        }
        finally
        {
            orig(self);
        }
    }

    private static void ApplyHooks()
    {
        ApplySaveDataHooks();

        On.Player.checkInput += Player_checkInput;
        On.RWInput.PlayerInput += RWInput_PlayerInput;

        On.RainWorldGame.ctor += RainWorldGame_ctor;
        On.RainWorldGame.ShutDownProcess += RainWorldGame_ShutDownProcess;


        On.RoomCamera.ClearAllSprites += RoomCamera_ClearAllSprites;
        On.RainWorldGame.GrafUpdate += RainWorldGame_GrafUpdate;
        On.RoomCamera.ctor += RoomCamera_ctor;
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
            0 => Input.GetKey(ModOptions.keybindPlayer1.Value) || Input.GetKey(ModOptions.keybindKeyboard.Value),
            1 => Input.GetKey(ModOptions.keybindPlayer2.Value),
            2 => Input.GetKey(ModOptions.keybindPlayer3.Value),
            3 => Input.GetKey(ModOptions.keybindPlayer4.Value),

            _ => false
        };
    }

    public static bool IsAnyFastRollInput()
    {
        for (int i  = 0; i < 4; i++)
            if (IsFastRollInput(i))
                return true;

        return false;
    }


    // Debug Fast Roll Check
    private static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
    {
        orig(self);

        if (self.Template.type == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC) return;
        
        
        CheckIfFastRoll(self);


        if (FastRollLabel == null) return;

        FastRollLabel.text = "Fast Rolling";
        FastRollLabel.isVisible = fastRollingPlayers.Any(isFastRolling => isFastRolling);

        if (self.room.game.Players.Count == 1) return;


        for (int i = fastRollingPlayers.Length -1; i >= 0; i--)
        {
            if (!fastRollingPlayers[i]) continue;

            FastRollLabel.text = $"P{i + 1}, {FastRollLabel.text}"; 
        }
    }


    public static readonly bool[] fastRollingPlayers = new bool[4];

    private static void CheckIfFastRoll(Player self)
    {
        fastRollingPlayers[self.playerState.playerNumber] = false;


        if (self.animation != Player.AnimationIndex.Roll) return;

        if (self.input[0].downDiagonal == 0) return;

        if (self.input[0].y == -1) return;


        fastRollingPlayers[self.playerState.playerNumber] = true;
    }


    // Debug Label
    private static FLabel? FastRollLabel { get; set; }

    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);

        if (!ModOptions.debugDisplay.Value) return;

        FastRollLabel = new FLabel(Custom.GetFont(), "Fast Rolling");

        Futile.stage.AddChild(FastRollLabel);

        FastRollLabel.alignment = FLabelAlignment.Left;
        FastRollLabel.x = 10.0f;
        FastRollLabel.y = self.rainWorld.options.ScreenSize.y - 10.0f;
        FastRollLabel.color = new Color(1.0f, 1.0f, 0.5f);
        FastRollLabel.isVisible = false;
    }

    private static void RainWorldGame_ShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
    {
        orig(self);

        FastRollLabel?.RemoveFromContainer();
    }


    // Input Display
    private static void RoomCamera_ClearAllSprites(On.RoomCamera.orig_ClearAllSprites orig, RoomCamera self)
    {
        if (ModOptions.inputGraphics.Length > self.cameraNumber && ModOptions.inputGraphics[self.cameraNumber]?.cam == self)
        {
            ModOptions.inputGraphics[self.cameraNumber]?.Remove();
            ModOptions.inputGraphics[self.cameraNumber] = null!;
        }

        orig(self);
    }

    private static void RainWorldGame_GrafUpdate(On.RainWorldGame.orig_GrafUpdate orig, RainWorldGame self, float timeStacker)
    {
        orig(self, timeStacker);

        if (!ModOptions.inputDisplay.Value) return;

        foreach (InputGraphic display in ModOptions.inputGraphics)
            display?.Update(timeStacker);

        var save = self.rainWorld.GetMiscProgression();

        save.InputDisplayPosX = ModOptions.inputDisplayOrigin.x;
        save.InputDisplayPosY = ModOptions.inputDisplayOrigin.y;
    }

    private static void RoomCamera_ctor(On.RoomCamera.orig_ctor orig, RoomCamera self, RainWorldGame game, int cameraNumber)
    {
        orig(self, game, cameraNumber);

        if (!ModOptions.inputDisplay.Value) return;

        var save = self.game.rainWorld.GetMiscProgression();

        ModOptions.inputDisplayOrigin.x = save.InputDisplayPosX;
        ModOptions.inputDisplayOrigin.y = save.InputDisplayPosY;


        if (ModOptions.inputGraphics.Length <= cameraNumber)
            Array.Resize(ref ModOptions.inputGraphics, cameraNumber + 1);
        
        ModOptions.inputGraphics[self.cameraNumber]?.Remove();

        InputGraphic ig = new(self);
        ModOptions.inputGraphics[cameraNumber] = ig;
        
        ig.Move();
    }
}
