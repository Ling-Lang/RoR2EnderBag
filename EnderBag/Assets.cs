using System.Reflection;
using R2API;
using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;

namespace EnderBag
{
    internal static class Assets
    {
        internal static GameObject EnderBagPrefab;
        internal static Sprite EnderBagIcon;

        internal static ItemDef EnderBagItem;
        //internal static EquipmentDef EnderBagEquipment;

        private const string ModPrefix = "@EnderBag:";

        internal static void Init()
        {
            // First registering your AssetBundle into the ResourcesAPI with a modPrefix that'll also be used for your prefab and icon paths
            // note that the string parameter of this GetManifestResourceStream call will change depending on
            // your namespace and file name
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EnderBag.enderbag")) 
            {
                var bundle = AssetBundle.LoadFromStream(stream);

                EnderBagPrefab = bundle.LoadAsset<GameObject>("Assets/Import/enderbag/enderbag.prefab");
                EnderBagIcon = bundle.LoadAsset<Sprite>("Assets/Import/enderbag_icon/ender_bag_icon.png");
            }

            EnderBagAsRedTierItem();
            //BiscoLeashAsRedTierItem();
            //BiscoLeashAsEquipment();

            AddLanguageTokens();
        }
        private static void EnderBagAsRedTierItem()
        {
            EnderBagItem = new ItemDef
            {
                name = "EnderBag", // its the internal name, no spaces, apostrophes and stuff like that
                tier = ItemTier.Tier3,
                pickupModelPrefab = EnderBagPrefab,
                pickupIconSprite = EnderBagIcon,
                nameToken = "ENDERBAG_NAME", // stylised name
                pickupToken = "ENDERBAG_PICKUP",
                descriptionToken = "ENDERBAG_DESC",
                loreToken = "ENDERBAG_LORE",
                tags = new[]
                {
                    ItemTag.Utility
                }

            }; 
            var itemDisplayRules = new ItemDisplayRule[0]; // keep this null if you don't want the item to show up on the survivor 3d model. You can also have multiple rules !
            //itemDisplayRules[0].followerPrefab = EnderBagPrefab; // the prefab that will show up on the survivor
            //itemDisplayRules[0].childName = "Chest"; // Set the item to attach to the back
            //itemDisplayRules[0].localScale = new Vector3(0.15f, 0.15f, 0.15f); // Keep the same scale
            //itemDisplayRules[0].localAngles = new Vector3(0f, 0f, 0f); // Remove rotation
            //itemDisplayRules[0].localPos = new Vector3(0f, 0f, 0f); // Adjust position to align with the back
            var enderBag = new R2API.CustomItem(EnderBagItem, itemDisplayRules);
            ItemAPI.Add(enderBag); // ItemAPI sends back the ItemIndex of your item
        }

        private static void AddLanguageTokens()
        {
            //The Name should be self explanatory
            LanguageAPI.Add("ENDERBAG_NAME", 
                "Ender Bag");
            //The Pickup is the short text that appears when you first pick this up. This text should be short and to the point, nuimbers are generally ommited.
            LanguageAPI.Add("ENDERBAG_PICKUP",
                "Chance to share picked-up items with teammates.");
            //The Description is where you put the actual numbers and give an advanced description.
            LanguageAPI.Add("ENDERBAG_DESC",
                "Picking up an item has a <style=cIsUtility>10%</style> <style=cStack>(+10% per stack)</style> chance to send it to allies holding an <style=cIsUtility>Ender Bag</style>. \nItems are shared between players that possess this item. \nStored items may return to you at random after multiple pickups if no allies are eligible.");
            //The Lore is, well, flavor. You can write pretty much whatever you want here.
            LanguageAPI.Add("ENDERBAG_LORE",
                "A velvet pouch stitched with strands of spatial thread, its interior infinitely larger than its shell. \\n\\nDiscovered sealed in an obsidian vault behind the Fourth Altar, it whispers across dimensions when opened. The Archivists believe it once belonged to a long-dead courier who could never decide *what* to carry or *who* to trust.\\n\\nNow, it seems, neither choice matters.\"");
        }
    }
}
