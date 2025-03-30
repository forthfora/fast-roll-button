namespace FastRollButton;

public static class InputDisplay_Hooks
{
    public static void ApplyHooks()
    {
        On.RoomCamera.ClearAllSprites += RoomCamera_ClearAllSprites;
        On.RainWorldGame.GrafUpdate += RainWorldGame_GrafUpdate;
        On.RoomCamera.ctor += RoomCamera_ctor;
    }

    // Input Display
    private static void RoomCamera_ClearAllSprites(On.RoomCamera.orig_ClearAllSprites orig, RoomCamera self)
    {
        if (ModOptions.InputGraphics.TryGetValue(self.cameraNumber, out var inputGraphic))
        {
            inputGraphic.Destroy();
        }

        ModOptions.InputGraphics.Remove(self.cameraNumber);

        orig(self);
    }

    private static void RainWorldGame_GrafUpdate(On.RainWorldGame.orig_GrafUpdate orig, RainWorldGame self, float timeStacker)
    {
        orig(self, timeStacker);

        if (!ModOptions.inputDisplay.Value)
        {
            return;
        }

        foreach (var display in ModOptions.InputGraphics.Values)
        {
            display.Update();
        }
    }

    private static void RoomCamera_ctor(On.RoomCamera.orig_ctor orig, RoomCamera self, RainWorldGame game, int cameraNumber)
    {
        orig(self, game, cameraNumber);

        if (ModOptions.InputGraphics.TryGetValue(self.cameraNumber, out var inputGraphic))
        {
            inputGraphic.Destroy();
        }

        inputGraphic = new InputGraphic();

        ModOptions.InputGraphics[cameraNumber] = inputGraphic;
        inputGraphic.Move();
    }
}
