using UnityEngine;
using Multibonk.Networking.Comms.Packet.Base.Multibonk.Networking.Comms;

namespace Multibonk.Networking.Comms.Base.Packet
{
    public class SendPlayerAttackedPacket : OutgoingPacket
    {
        public readonly byte Id = (byte)ServerSentPacketId.PLAYER_ATTACKED_PACKET;

        public SendPlayerAttackedPacket(ushort playerId, Vector3 position, Quaternion rotation)
        {
            Message.WriteByte(Id);
            Message.WriteUShort(playerId);
            Message.WriteFloat(position.x);
            Message.WriteFloat(position.y);
            Message.WriteFloat(position.z);
            Message.WriteFloat(rotation.x);
            Message.WriteFloat(rotation.y);
            Message.WriteFloat(rotation.z);
            Message.WriteFloat(rotation.w);
        }
    }

    internal class PlayerAttackedPacket
    {
        public ushort PlayerId { get; private set; }
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }

        public PlayerAttackedPacket(IncomingMessage msg)
        {
            PlayerId = msg.ReadUShort();
            Position = new Vector3(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
            Rotation = new Quaternion(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
        }
    }
}
