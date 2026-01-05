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
    public class PlayerXPPacketHandler : IServerPacketHandler
    {
        public byte PacketId => (byte)ClientSentPacketId.PLAYER_XP_PACKET;

        private readonly LobbyContext _lobbyContext;

        public PlayerXPPacketHandler(LobbyContext lobbyContext)
        {
            _lobbyContext = lobbyContext;
        }

        public void Handle(IncomingMessage msg, Connection conn)
        {
            var packet = new PlayerXPPacket(msg);

            var playerId = _lobbyContext.GetPlayer(conn).UUID;

            GameDispatcher.Enqueue(() =>
            {
                var go = GameFunctions.GetSpawnedPlayerFromId(playerId);

                if (go != null)
                {
                    go.UpdateXP(packet.XP);
                }
            });

            MelonLogger.Msg($"[SERVER] Reenviando XP recibida del cliente (UUID: {playerId}): {packet.XP}");
            foreach (var player in _lobbyContext.GetPlayers())
            {
                if (player.Connection == null || player.UUID == playerId)
                    continue;

                var sendPacket = new SendPlayerXPUpdatedPacket(
                    playerId,
                    packet.XP
                );

                player.Connection.EnqueuePacket(sendPacket);
            }
        }
    }
}
