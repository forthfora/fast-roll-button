using RWCustom;
using System;
using System.Linq;
using UnityEngine;


namespace FastRollButton;

public static partial class Hooks
{
    private static void ApplyInputDisplayHooks()
    {
        On.RoomCamera.ClearAllSprites += RoomCamera_ClearAllSprites;
        On.RainWorldGame.GrafUpdate += RainWorldGame_GrafUpdate;
        On.RoomCamera.ctor += RoomCamera_ctor;
    }
 

    // Input Display
    private static void RoomCamera_ClearAllSprites(On.RoomCamera.orig_ClearAllSprites orig, RoomCamera self)
    {
        if (ModOptions.InputGraphics.Length > self.cameraNumber && ModOptions.InputGraphics[self.cameraNumber]?.cam == self)
        {
            ModOptions.InputGraphics[self.cameraNumber]?.Remove();
            ModOptions.InputGraphics[self.cameraNumber] = null!;
        }

        orig(self);
    }

    private static void RainWorldGame_GrafUpdate(On.RainWorldGame.orig_GrafUpdate orig, RainWorldGame self, float timeStacker)
    {
        orig(self, timeStacker);

        if (!ModOptions.inputDisplay.Value) return;


        foreach (InputGraphic display in ModOptions.InputGraphics)
        {
            display?.Update(timeStacker);
        }
    }

    private static void RoomCamera_ctor(On.RoomCamera.orig_ctor orig, RoomCamera self, RainWorldGame game, int cameraNumber)
    {
        orig(self, game, cameraNumber);

        if (!ModOptions.inputDisplay.Value) return;

        if (ModOptions.InputGraphics.Length <= cameraNumber)
        {
            Array.Resize(ref ModOptions.InputGraphics, cameraNumber + 1);
        }

        ModOptions.InputGraphics[self.cameraNumber]?.Remove();

        var ig = new InputGraphic(self);
        ModOptions.InputGraphics[cameraNumber] = ig;
        
        ig.Move();
    }
}
