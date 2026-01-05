using Multibonk.Networking.Comms.Packet.Base.Multibonk.Networking.Comms;
using UnityEngine;

namespace Multibonk.Networking.Comms.Base.Packet
{

    namespace Multibonk.Networking.Comms.Base.Packet
    {
        public class SendPlayerAttackPacket : OutgoingPacket
        {
            public readonly byte Id = (byte)ClientSentPacketId.PLAYER_ATTACK_PACKET;

            public SendPlayerAttackPacket(Vector3 position, Quaternion rotation)
            {
                Message.WriteByte(Id);
                Message.WriteFloat(position.x);
                Message.WriteFloat(position.y);
                Message.WriteFloat(position.z);
                Message.WriteFloat(rotation.x);
                Message.WriteFloat(rotation.y);
                Message.WriteFloat(rotation.z);
                Message.WriteFloat(rotation.w);
            }
        }

        internal class PlayerAttackPacket
        {
            public Vector3 Position { get; private set; }
            public Quaternion Rotation { get; private set; }

            public PlayerAttackPacket(IncomingMessage msg)
            {
                Position = new Vector3(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
                Rotation = new Quaternion(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
            }
        }
    }
}
