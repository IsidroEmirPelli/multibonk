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
    public class PlayerLevelPacketHandler : IServerPacketHandler
    {
        public byte PacketId => (byte)ClientSentPacketId.PLAYER_LEVEL_PACKET;

        private readonly LobbyContext _lobbyContext;

        public PlayerLevelPacketHandler(LobbyContext lobbyContext)
        {
            _lobbyContext = lobbyContext;
        }

        public void Handle(IncomingMessage msg, Connection conn)
        {
            var packet = new PlayerLevelPacket(msg);

            var playerId = _lobbyContext.GetPlayer(conn).UUID;

            GameDispatcher.Enqueue(() =>
            {
                var go = GameFunctions.GetSpawnedPlayerFromId(playerId);

                if (go != null)
                {
                    go.UpdateLevel(packet.Level);
                }
            });

            MelonLogger.Msg($"[SERVER] Reenviando nivel recibido del cliente (UUID: {playerId}): {packet.Level}");
            foreach (var player in _lobbyContext.GetPlayers())
            {
                if (player.Connection == null || player.UUID == playerId)
                    continue;

                var sendPacket = new SendPlayerLevelUpdatedPacket(
                    playerId,
                    packet.Level
                );

                player.Connection.EnqueuePacket(sendPacket);
            }
        }
    }
}
