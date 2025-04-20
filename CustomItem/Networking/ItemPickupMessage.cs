using RoR2;
using UnityEngine.Networking;
namespace EnderBag.Networking
{
    public class ItemPickupMessage : MessageBase
    {
        public PickupIndex PickupIndex { get; private set; }

        // ReSharper disable once UnusedMember.Global
        public ItemPickupMessage()
        {

        }

        public ItemPickupMessage(PickupIndex pickupIndex)
        {
            PickupIndex = pickupIndex;
        }

        public override void Serialize(UnityEngine.Networking.NetworkWriter writer)
        {
            writer.Write(PickupIndex);
        }

        public override void Deserialize(UnityEngine.Networking.NetworkReader reader)
        {
            PickupIndex = reader.ReadPickupIndex();
        }
    }
}
