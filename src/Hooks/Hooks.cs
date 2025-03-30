namespace FastRollButton;

public static class Hooks
{
    private static bool IsInit { get; set; }

    public static void ApplyInit()
    {
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
    }

    private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        try
        {
            ModOptions.RegisterOI();

            if (IsInit)
            {
                return;
            }

            IsInit = true;

            ApplyHooks();

            // Init Info
            var mod = ModManager.ActiveMods.FirstOrDefault(mod => mod.id == Plugin.MOD_ID);

            if (mod is null)
            {
                Plugin.Logger.LogError($"Failed to initialize: ID '{Plugin.MOD_ID}' wasn't found in the active mods list!");
                return;
            }

            Plugin.ModName = mod.name;
            Plugin.Version = mod.version;
            Plugin.Authors = mod.authors;
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
        FastRoll_Hooks.ApplyHooks();
        InputDisplay_Hooks.ApplyHooks();
    }
}
