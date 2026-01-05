using Multibonk.Networking.Comms.Packet.Base.Multibonk.Networking.Comms;

namespace Multibonk.Networking.Comms.Base.Packet
{
    namespace Multibonk.Networking.Comms.Base.Packet
    {
        public class SendPlayerLevelPacket : OutgoingPacket
        {
            public readonly byte Id = (byte)ClientSentPacketId.PLAYER_LEVEL_PACKET;

            public SendPlayerLevelPacket(int level)
            {
                Message.WriteByte(Id);
                Message.WriteInt(level);
            }
        }

        internal class PlayerLevelPacket
        {
            public int Level { get; private set; }

            public PlayerLevelPacket(IncomingMessage msg)
            {
                Level = msg.ReadInt();
            }
        }
    }
}
