using Multibonk.Networking.Comms.Packet.Base.Multibonk.Networking.Comms;

namespace Multibonk.Networking.Comms.Base.Packet
{
    public class SendPlayerXPUpdatedPacket : OutgoingPacket
    {
        public readonly byte Id = (byte)ServerSentPacketId.PLAYER_XP_UPDATED_PACKET;

        public SendPlayerXPUpdatedPacket(ushort playerId, int xp)
        {
            Message.WriteByte(Id);
            Message.WriteUShort(playerId);
            Message.WriteInt(xp);
        }
    }

    internal class PlayerXPUpdatedPacket
    {
        public ushort PlayerId { get; private set; }
        public int XP { get; private set; }

        public PlayerXPUpdatedPacket(IncomingMessage msg)
        {
            PlayerId = msg.ReadUShort();
            XP = msg.ReadInt();
        }
    }
}
