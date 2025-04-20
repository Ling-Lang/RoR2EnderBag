using System;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using EnderBag.Networking;
using R2API;

namespace EnderBag
{
    public static class ShareItem
    {
        public static void UnHook()
        {
            On.RoR2.GenericPickupController.AttemptGrant -= OnGrantItem;
            //IL.RoR2.GenericPickupController.AttemptGrant -= RemoveDefaultPickupMessage;
        }

        public static void Hook()
        {

            Debug.LogError("ENDERBAG: Hooking AttemptGrant");
            On.RoR2.GenericPickupController.AttemptGrant += OnGrantItem;
            //if (EnderBag.RichMessagesEnabled.Value) IL.RoR2.GenericPickupController.AttemptGrant += RemoveDefaultPickupMessage;
        }
        private static void OnGrantItem(On.RoR2.GenericPickupController.orig_AttemptGrant orig, GenericPickupController self, CharacterBody body)
        {
            Debug.LogError("ENDERBAG: AttemptGrant called");
            var item = PickupCatalog.GetPickupDef(self.pickupIndex);
            Debug.LogError("ENDERBAG: 1AttemptGrant called");
            var itemDef = ItemCatalog.GetItemDef(item.itemIndex);
            Debug.LogError("ENDERBAG: 2AttemptGrant called");
            var randomizedPlayerDict = new Dictionary<CharacterMaster, PickupDef>();

            var master = body?.master ?? body.inventory?.GetComponent<CharacterMaster>();

            //if(NetworkServer.active)
            //{
                if (NetworkServer.active
                    && IsValidItemPickup(self.pickupIndex)
                    && IsValidPickupObject(self, body))
            {
                    Debug.LogError("ENDERBAG: 3AttemptGrant called");
                //int itemCount = body.inventory.GetItemCount(Assets.EnderBagItem);
                
                EnderBag.ItemShareChance = 15f + (10f * body.inventory.GetItemCount(Assets.EnderBagItem) - 10);
                Debug.Log($"ENDERBAG: ItemShareChance: {EnderBag.ItemShareChance}");
                Debug.Log($"ENDERBAG: AttemptGrant called for {itemDef.name} with pickupIndex {self.pickupIndex} and itemCount {body.inventory.GetItemCount(Assets.EnderBagItem)}");
                // Check if any other player has the EnderBag item
                bool otherPlayerHasEnderBag = PlayerCharacterMasterController.instances
                    .Select(p => p.master)
                    .Any(player => player != master && player.inventory.GetItemCount(Assets.EnderBagItem) > 0);

                if (!otherPlayerHasEnderBag)
                {
                    Debug.Log("No other player has the EnderBag item. Skipping sharing logic.");
                    orig(self, body);
                    return;
                }

                foreach (var player in PlayerCharacterMasterController.instances
                             .Select(p => p.master))
                {
                    if (player.IsDeadAndOutOfLivesServer()) continue;

                    if (player.inventory == body.inventory)
                    {
                        if (player.isLocalPlayer)
                        {
                            continue;
                        }

                        NetworkHandler.SendItemPickupMessage(player.playerCharacterMasterController.networkUser.connectionToClient.connectionId, item.pickupIndex);
                        continue;
                    }
                    EnderBag.UpdateEnderBagDescription();
                    float randomValue = UnityEngine.Random.Range(0f, 100f);
                    Debug.Log($"Random Value: {randomValue}, ItemShareChance: {EnderBag.ItemShareChance}");

                    if (randomValue > EnderBag.ItemShareChance)
                    {
                        Debug.Log("Skipping item sharing due to chance logic.");
                        continue;
                    }
                    Debug.Log($"Giving item {itemDef.name} to ");
                    HandleGiveItem(player, item);
                }
            }

            orig(self, body);
            ChatHandler.SendRichPickupMessage(master, item);
            HandleRichMessageUnlockAndNotification(master, item.pickupIndex);
        }


        public static bool IsValidItemPickup(PickupIndex pickup)
        {
            var pickupDef = PickupCatalog.GetPickupDef(pickup);
            if (pickupDef != null && pickupDef.itemIndex != ItemIndex.None)
            {
                var itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);
                if(itemDef == Assets.EnderBagItem)
                    return false;
                switch (itemDef.tier)
                {
                    case ItemTier.Tier1:
                        return true;
                    case ItemTier.Tier2:
                        return true;
                    case ItemTier.Tier3:
                        return true;
                    case ItemTier.Lunar:
                        return false;
                    case ItemTier.Boss:
                        return true;
                    case ItemTier.VoidTier1:
                        return false;
                    case ItemTier.VoidTier2:
                        return false;
                    case ItemTier.VoidTier3:
                        return false;
                    case ItemTier.VoidBoss:
                        return false;
                    case ItemTier.AssignedAtRuntime:
                        return true;
                    default:
                        return false;
                }
            }

            return false;
        }

        public static bool IsValidPickupObject(GenericPickupController pickup, CharacterBody picker)
        {
            if (AdditionalPickupValidityChecks == null)
                return true;

            var retv = true;
            foreach (Func<GenericPickupController, CharacterBody, bool> f in AdditionalPickupValidityChecks.GetInvocationList())
                retv &= f(pickup, picker);

            return retv;
        }

        private static PickupIndex? GetRandomItemOfTier(ItemTier tier, PickupIndex orDefault)
        {
            switch (tier)
            {
                case ItemTier.Tier1:
                    return PickRandomOf(Blacklist.AvailableTier1DropList);
                case ItemTier.Tier2:
                    return PickRandomOf(Blacklist.AvailableTier2DropList);
                case ItemTier.Tier3:
                    return PickRandomOf(Blacklist.AvailableTier3DropList);
                case ItemTier.Lunar:
                    break;
                case ItemTier.Boss:
                    break;
                case ItemTier.VoidBoss:
                case ItemTier.VoidTier1:
                case ItemTier.VoidTier2:
                case ItemTier.VoidTier3:
                    break;
            }

            var pickupDef = PickupCatalog.GetPickupDef(orDefault);
            if (Blacklist.HasItem(pickupDef.itemIndex))
                return null;

            return orDefault;
        }

        private static T? PickRandomOf<T>(IList<T> collection) where T : struct =>
            collection.Count > 0
                ? collection[UnityEngine.Random.Range(0, collection.Count)]
                : (T?)null;

        private static void HandleGiveItem(CharacterMaster characterMaster, PickupDef pickupDef)
        {
            Debug.Log($"Giving item {pickupDef.itemIndex} to ");
            characterMaster.inventory.GiveItem(pickupDef.itemIndex);

            var connectionId = characterMaster.playerCharacterMasterController.networkUser?.connectionToClient?.connectionId;

            if (connectionId != null)
            {
                NetworkHandler.SendItemPickupMessage(connectionId.Value, pickupDef.pickupIndex);
            }
        }

        public static void HandleRichMessageUnlockAndNotification(CharacterMaster characterMaster, PickupIndex pickupIndex)
        {
            if (!characterMaster.isLocalPlayer)
            {
                return;
            }

            characterMaster.playerCharacterMasterController?.networkUser?.localUser?.userProfile.DiscoverPickup(pickupIndex);

            if (characterMaster.inventory.GetItemCount(PickupCatalog.GetPickupDef(pickupIndex).itemIndex) <= 1)
            {
                CharacterMasterNotificationQueue.PushPickupNotification(characterMaster, pickupIndex);
            }
        }

        public static event Func<GenericPickupController, CharacterBody, bool> AdditionalPickupValidityChecks;
    }
}