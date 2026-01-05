using Multibonk.Networking.Comms.Packet.Base.Multibonk.Networking.Comms;

namespace Multibonk.Networking.Comms.Base.Packet
{
    public class SendPlayerLevelUpdatedPacket : OutgoingPacket
    {
        public readonly byte Id = (byte)ServerSentPacketId.PLAYER_LEVEL_UPDATED_PACKET;

        public SendPlayerLevelUpdatedPacket(ushort playerId, int level)
        {
            Message.WriteByte(Id);
            Message.WriteUShort(playerId);
            Message.WriteInt(level);
        }
    }

    internal class PlayerLevelUpdatedPacket
    {
        public ushort PlayerId { get; private set; }
        public int Level { get; private set; }

        public PlayerLevelUpdatedPacket(IncomingMessage msg)
        {
            PlayerId = msg.ReadUShort();
            Level = msg.ReadInt();
        }
    }
}
