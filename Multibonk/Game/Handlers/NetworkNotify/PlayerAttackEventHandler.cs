using Multibonk.Networking.Comms.Base.Packet.Multibonk.Networking.Comms.Base.Packet;
using Multibonk.Networking.Comms.Base.Packet;
using Multibonk.Networking.Lobby;
using Multibonk.Networking.Comms.Multibonk.Networking.Comms;
using MelonLoader;
using UnityEngine;

namespace Multibonk.Game.Handlers.NetworkNotify
{
    public class PlayerAttackEventHandler : GameEventHandler
    {
        public PlayerAttackEventHandler(
            NetworkService network,
            LobbyContext lobbyContext
        )
        {
            GameEvents.PlayerAttackEvent += (position, rotation) =>
            {
                if (LobbyPatchFlags.IsHosting)
                {
                    var myUUID = lobbyContext.GetMyself().UUID;
                    MelonLogger.Msg($"[SERVER] Enviando ataque del host (UUID: {myUUID})");
                    lobbyContext.GetPlayers().ForEach(player =>
                    {
                        if (player.Connection != null && player.UUID != myUUID)
                        {
                            var attackPacket = new SendPlayerAttackedPacket(myUUID, position, rotation);
                            player.Connection?.EnqueuePacket(attackPacket);
                        }
                    });
                }

                if (!LobbyPatchFlags.IsHosting)
                {
                    MelonLogger.Msg($"[CLIENT] Enviando ataque al servidor");
                    var attackPacket = new SendPlayerAttackPacket(position, rotation);
                    network.GetClientService().Enqueue(attackPacket);
                }
            };
        }
    }
}
