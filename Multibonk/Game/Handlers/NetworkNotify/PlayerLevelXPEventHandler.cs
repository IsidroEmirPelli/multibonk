using Multibonk.Networking.Comms.Base.Packet.Multibonk.Networking.Comms.Base.Packet;
using Multibonk.Networking.Comms.Base.Packet;
using Multibonk.Networking.Lobby;
using Multibonk.Networking.Comms.Multibonk.Networking.Comms;
using MelonLoader;

namespace Multibonk.Game.Handlers.NetworkNotify
{
    public class PlayerLevelXPEventHandler : GameEventHandler
    {
        public PlayerLevelXPEventHandler(
            NetworkService network,
            LobbyContext lobbyContext
        )
        {
            GameEvents.PlayerLevelUpEvent += (level) =>
            {
                if (LobbyPatchFlags.IsHosting)
                {
                    var myUUID = lobbyContext.GetMyself().UUID;
                    MelonLogger.Msg($"[SERVER] Enviando nivel del host (UUID: {myUUID}): {level}");
                    lobbyContext.GetPlayers().ForEach(player =>
                    {
                        var levelPacket = new SendPlayerLevelUpdatedPacket(myUUID, level);
                        player.Connection?.EnqueuePacket(levelPacket);
                    });
                }

                if (!LobbyPatchFlags.IsHosting)
                {
                    MelonLogger.Msg($"[CLIENT] Enviando nivel al servidor: {level}");
                    var levelPacket = new SendPlayerLevelPacket(level);
                    network.GetClientService().Enqueue(levelPacket);
                }
            };

            GameEvents.PlayerXPChangedEvent += (xp) =>
            {
                if (LobbyPatchFlags.IsHosting)
                {
                    var myUUID = lobbyContext.GetMyself().UUID;
                    MelonLogger.Msg($"[SERVER] Enviando XP del host (UUID: {myUUID}): {xp}");
                    lobbyContext.GetPlayers().ForEach(player =>
                    {
                        var xpPacket = new SendPlayerXPUpdatedPacket(myUUID, xp);
                        player.Connection?.EnqueuePacket(xpPacket);
                    });
                }

                if (!LobbyPatchFlags.IsHosting)
                {
                    MelonLogger.Msg($"[CLIENT] Enviando XP al servidor: {xp}");
                    var xpPacket = new SendPlayerXPPacket(xp);
                    network.GetClientService().Enqueue(xpPacket);
                }
            };
        }
    }
}
