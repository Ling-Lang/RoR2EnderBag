using System.Linq;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EnderBag.Networking
{
    public static class NetworkHandler
    {
        public static void RegisterHandlers()
        {
            var client = NetworkManager.singleton?.client;

            if (client == null || client.handlers.ContainsKey(EnderBag.NetworkMessageType.Value))
            {
                return;
            }

            client.RegisterHandler(EnderBag.NetworkMessageType.Value, ItemPickupHandler);
        }

        public static void SendItemPickupMessage(int connectionId, PickupIndex pickupIndex)
        {
            //NetworkServer.SendToClient(connectionId, EnderBag.NetworkMessageType.Value, new ItemPickupMessage(pickupIndex)); temporary to prevent communication error
        }

        private static void ItemPickupHandler(NetworkMessage networkMessage)
        {
            var itemPickupMessage = networkMessage.ReadMessage<ItemPickupMessage>();
            var localPlayer = PlayerCharacterMasterController.instances.FirstOrDefault(x => x.networkUser.isLocalPlayer);

            if (localPlayer == null || !itemPickupMessage.PickupIndex.isValid)
            {
                return;
            }
            ShareItem.HandleRichMessageUnlockAndNotification(localPlayer.master, itemPickupMessage.PickupIndex);
        }
    }
}
