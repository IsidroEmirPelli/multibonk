using Multibonk.Networking.Comms.Base.Packet.Multibonk.Networking.Comms.Base.Packet;
using Multibonk.Networking.Comms.Base;
using Multibonk.Networking.Comms.Packet.Base.Multibonk.Networking.Comms;
using Multibonk.Networking.Lobby;
using Multibonk.Networking.Comms.Base.Packet;
using Multibonk.Game.Handlers;
using Multibonk.Game;
using MelonLoader;

namespace Multibonk.Networking.Comms.Server.Handlers
{
    public class PlayerAttackPacketHandler : IServerPacketHandler
    {
        public byte PacketId => (byte)ClientSentPacketId.PLAYER_ATTACK_PACKET;

        private readonly LobbyContext _lobbyContext;

        public PlayerAttackPacketHandler(LobbyContext lobbyContext)
        {
            _lobbyContext = lobbyContext;
        }

        public void Handle(IncomingMessage msg, Connection conn)
        {
            var packet = new PlayerAttackPacket(msg);

            var playerId = _lobbyContext.GetPlayer(conn).UUID;

            MelonLogger.Msg($"[SERVER] Reenviando ataque recibido del cliente (UUID: {playerId})");
            foreach (var player in _lobbyContext.GetPlayers())
            {
                if (player.Connection == null || player.UUID == playerId)
                    continue;

                var sendPacket = new SendPlayerAttackedPacket(
                    playerId,
                    packet.Position,
                    packet.Rotation
                );

                player.Connection.EnqueuePacket(sendPacket);
            }
        }
    }
}
