using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
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
        public static bool
        WhiteItemsShared,
        OverrideMultiplayerCheck,
        GreenItemsShared,
        RedItemsShared,
        LunarItemsShared,
        BossItemsShared,
        VoidItemsShared = true;
        private static bool _previouslyEnabled = false;
        private static bool _modEnabled = false;
        public static double ItemShareChance = 0f;
        public static ConfigEntry<string> ItemBlacklist, EquipmentBlacklist;
        public static ConfigEntry<short> NetworkMessageType;
        private const string ModVer = "0.1.0";
        private const string ModName = "EnderBag";
        public const string ModGuid = "com.DylanDerEchte.EnderbagItem";

        internal new static ManualLogSource Logger; // allow access to the logger across the plugin classes

        private void InitConfig()
        {
            NetworkMessageType = Config.Bind(
            "Settings",
            "NetworkMessageType",
            (short)1021,
            "The identifier for network message for this mod. Must be unique across all mods."
            );
        }
        private void LoadHooks()
        {
            //if(_previouslyEnabled && !_modEnabled)
            //{
            //    //Hooks.Init();
            //    GeneralHooks.UnHook();
            //    ShareItem.UnHook();
            //    ChatHandler.UnHook();
            //    _previouslyEnabled = false;
            //}
            //else if (!_previouslyEnabled && _modEnabled)
            //{
                //Hooks.Init();
                GeneralHooks.Hook();
                ShareItem.Hook();
                ChatHandler.Hook();
                _previouslyEnabled = true;
            //}
        }
        public static void UpdateEnderBagDescription()
        {
            string description = $"Picking up an item has a <style=cIsUtility>10%</style> <style=cStack>(+10% per stack)</style> chance to send it to allies holding an <style=cIsUtility>Ender Bag</style>. \nItems are shared between players that possess this item. \nStored items may return to you at random after multiple pickups if no allies are eligible.\nTotal Chance: <style=cIsUtility>{EnderBag.ItemShareChance}%</style>";
            LanguageAPI.Add("ENDERBAG_DESC", description); // The `true` parameter forces an overwrite of the existing token.
        }
        public void Awake()
        {
            Logger = base.Logger;

            Assets.Init();

            //Hooks.Init();
        }
        public EnderBag()
        {
            InitConfig();
            LoadHooks();
        }
    }
}