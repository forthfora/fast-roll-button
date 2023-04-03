using BepInEx;
using BepInEx.Logging;
using System.Security.Permissions;
using System.Security;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
#pragma warning restore CS0618 // Type or member is obsolete


namespace FastRollButton
{
    [BepInPlugin(AUTHOR + "." + MOD_ID, MOD_NAME, VERSION)]
    internal class Plugin : BaseUnityPlugin
    {
        public static new ManualLogSource Logger { get; private set; } = null!;

        public const string VERSION = "1.0.5";  
        public const string MOD_NAME = "Fast Roll Button";
        public const string MOD_ID = "fastrollbutton";
        public const string AUTHOR = "forthbridge";

        public void OnEnable()
        {
            Logger = base.Logger;
            Hooks.ApplyHooks();
        }
    }
}
