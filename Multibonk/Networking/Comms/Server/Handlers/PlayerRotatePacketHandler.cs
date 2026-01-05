using Multibonk.Networking.Comms.Base.Packet;
using Multibonk.Networking.Lobby;
using Multibonk.Networking.Comms.Base;
using Multibonk.Networking.Comms.Packet.Base.Multibonk.Networking.Comms;
using Multibonk.Game.Handlers;
using Multibonk.Game;
using MelonLoader;

namespace Multibonk.Networking.Comms.Server.Handlers
{
    /// <summary>
    /// Class copied from PlayerMovePacketHandler
    /// </summary>
    public class PlayerRotatePacketHandler : IServerPacketHandler
    {
        public byte PacketId => (byte)ClientSentPacketId.PLAYER_ROTATE_PACKET;
        private readonly LobbyContext _lobbyContext;

        public PlayerRotatePacketHandler(LobbyContext lobbyContext)
        {
            _lobbyContext = lobbyContext;
        }


        public void Handle(IncomingMessage msg, Connection conn)
        {
            var packet = new PlayerRotatePacket(msg);

            var playerId = _lobbyContext.GetPlayer(conn).UUID;
            var eulerAngles = packet.Rotation.eulerAngles;

            GameDispatcher.Enqueue(() =>
            {
                var go = GameFunctions.GetSpawnedPlayerFromId(playerId);

                if (go != null)
                {
                    go.Rotate(eulerAngles);
                }
            });

            MelonLogger.Msg($"[SERVER] Reenviando rotación recibida del cliente (UUID: {playerId}): ({eulerAngles.x:F2}, {eulerAngles.y:F2}, {eulerAngles.z:F2})");
            foreach (var player in _lobbyContext.GetPlayers())
            {
                if (player.Connection == null || player.UUID == playerId)
                    continue;

                var sendPacket = new SendPlayerRotatedPacket(
                    playerId,
                    eulerAngles
                );

                player.Connection.EnqueuePacket(sendPacket);
            }
        }
    }
}




