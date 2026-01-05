using Il2Cpp;
using Il2CppAssets.Scripts.Actors.Player;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using Multibonk.Networking.Comms.Base.Packet;
using Multibonk.Networking.Lobby;
using UnityEngine;
using static Il2Cpp.AnimatedMeshScriptableObject;
using static MelonLoader.MelonLaunchOptions;

namespace Multibonk.Game.Handlers.NetworkNotify
{
    public class GameLoadedEventHandler : GameEventHandler
    {
        public GameLoadedEventHandler(LobbyContext lobbyContext)
        {

            GameEvents.GameLoadedEvent += () =>
            {
                if (!LobbyPatchFlags.IsHosting)
                    return;

                var hostUUID = lobbyContext.GetMyself().UUID;

                // Spawn all clients for the host (excluding the host itself)
                lobbyContext.GetPlayers()
                .Where(player => player.Connection != null && player.UUID != hostUUID)
                .ToList()
                .ForEach(player =>
                {
                    var character = Enum.Parse<ECharacter>(player.SelectedCharacter);
                    var data = GamePatchFlags.CharacterData.Find(d => d.eCharacter == character);
                    GameFunctions.SpawnNetworkPlayer(player.UUID, character, MyPlayer.Instance.transform.position, MyPlayer.Instance.transform.rotation);

                    // Send information about other players (excluding the host and the current player) to this client
                    lobbyContext.GetPlayers()
                        .Where(target => target != player && target.UUID != hostUUID)
                        .ToList()
                        .ForEach(target =>
                        {
                            var targetCharacter = Enum.Parse<ECharacter>(target.SelectedCharacter);
                            var packet = new SendSpawnPlayerPacket(targetCharacter, target.UUID, MyPlayer.Instance.transform.position, MyPlayer.Instance.transform.rotation);
                            player.Connection.EnqueuePacket(packet);
                        });

                    // Send the host's own character to this client (so they can see the host)
                    var hostCharacter = Enum.Parse<ECharacter>(lobbyContext.GetMyself().SelectedCharacter);
                    var hostPacket = new SendSpawnPlayerPacket(hostCharacter, hostUUID, MyPlayer.Instance.transform.position, MyPlayer.Instance.transform.rotation);
                    player.Connection.EnqueuePacket(hostPacket);
                });
            };
        }

    }
}
