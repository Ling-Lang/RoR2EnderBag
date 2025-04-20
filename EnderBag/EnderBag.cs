using BepInEx;
using BepInEx.Logging;
using R2API;
using R2API.Utils;

namespace EnderBag
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI))]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [System.Obsolete]
    public class EnderBag : BaseUnityPlugin
    {
        private const string ModVer = "0.1.0";
        private const string ModName = "EnderBag";
        public const string ModGuid = "com.DylanDerEchte.EnderbagItem";

        internal new static ManualLogSource Logger; // allow access to the logger across the plugin classes

        public void Awake()
        {
            Logger = base.Logger;

            Assets.Init();
            //Hooks.Init();
        }
    }
}