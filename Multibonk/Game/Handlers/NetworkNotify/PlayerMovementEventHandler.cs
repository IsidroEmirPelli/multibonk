using Multibonk.Networking.Comms.Base.Packet.Multibonk.Networking.Comms.Base.Packet;
using Multibonk.Networking.Comms.Base.Packet;
using Multibonk.Networking.Lobby;
using Multibonk.Networking.Comms.Multibonk.Networking.Comms;
using MelonLoader;

namespace Multibonk.Game.Handlers.NetworkNotify
{
    public class PlayerMovementEventHandler : GameEventHandler
    {
        public PlayerMovementEventHandler(
            NetworkService network,
            LobbyContext lobbyContext
        )
        {


            GameEvents.PlayerMoveEvent += (pos) =>
            {
                if (LobbyPatchFlags.IsHosting)
                {
                    var myUUID = lobbyContext.GetMyself().UUID;
                    MelonLogger.Msg($"[SERVER] Enviando posición del host (UUID: {myUUID}): ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})");
                    lobbyContext.GetPlayers().ForEach(player =>
                    {
                        var moved = new SendPlayerMovedPacket(myUUID, pos);
                        player.Connection?.EnqueuePacket(moved);
                    });
                }

                if (!LobbyPatchFlags.IsHosting)
                {
                    MelonLogger.Msg($"[CLIENT] Enviando posición al servidor: ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})");
                    var characterSelection = new SendPlayerMovePacket(pos);
                    network.GetClientService().Enqueue(characterSelection);
                }
            };

            GameEvents.PlayerRotateEvent += (rot) =>
            {
                if (LobbyPatchFlags.IsHosting)
                {
                    var myUUID = lobbyContext.GetMyself().UUID;
                    var eulerAngles = rot.eulerAngles;
                    MelonLogger.Msg($"[SERVER] Enviando rotación del host (UUID: {myUUID}): ({eulerAngles.x:F2}, {eulerAngles.y:F2}, {eulerAngles.z:F2})");
                    lobbyContext.GetPlayers().ForEach(player =>
                    {
                        var rotated = new SendPlayerRotatedPacket(myUUID, eulerAngles);
                        player.Connection?.EnqueuePacket(rotated);
                    });
                }

                if (!LobbyPatchFlags.IsHosting)
                {
                    var eulerAngles = rot.eulerAngles;
                    MelonLogger.Msg($"[CLIENT] Enviando rotación al servidor: ({eulerAngles.x:F2}, {eulerAngles.y:F2}, {eulerAngles.z:F2})");
                    var characterSelection = new SendPlayerRotatePacket(rot);
                    network.GetClientService().Enqueue(characterSelection);
                }
            };
        }
    }
}
