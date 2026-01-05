using Multibonk.Networking.Comms.Packet.Base.Multibonk.Networking.Comms;

namespace Multibonk.Networking.Comms.Base.Packet
{
    namespace Multibonk.Networking.Comms.Base.Packet
    {
        public class SendPlayerXPPacket : OutgoingPacket
        {
            public readonly byte Id = (byte)ClientSentPacketId.PLAYER_XP_PACKET;

            public SendPlayerXPPacket(int xp)
            {
                Message.WriteByte(Id);
                Message.WriteInt(xp);
            }
        }

        internal class PlayerXPPacket
        {
            public int XP { get; private set; }

            public PlayerXPPacket(IncomingMessage msg)
            {
                XP = msg.ReadInt();
            }
        }
    }
}
