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

                // Spawn all clients for the host
                lobbyContext.GetPlayers()
                .Where(player => player.Connection != null)
                .ToList()
                .ForEach(player =>
                {
                    var character = Enum.Parse<ECharacter>(player.SelectedCharacter);
                    var data = GamePatchFlags.CharacterData.Find(d => d.eCharacter == character);
                    GameFunctions.SpawnNetworkPlayer(player.UUID, character, MyPlayer.Instance.transform.position, MyPlayer.Instance.transform.rotation);

                    // Send information about other players (including the host) to this client
                    lobbyContext.GetPlayers()
                        .Where(target => target != player)
                        .ToList()
                        .ForEach(target =>
                        {
                            var targetCharacter = Enum.Parse<ECharacter>(target.SelectedCharacter);
                            var packet = new SendSpawnPlayerPacket(targetCharacter, target.UUID, MyPlayer.Instance.transform.position, MyPlayer.Instance.transform.rotation);
                            player.Connection.EnqueuePacket(packet);
                        });

                    // Also send the host's own character to this client
                    var hostCharacter = Enum.Parse<ECharacter>(lobbyContext.GetMyself().SelectedCharacter);
                    var hostPacket = new SendSpawnPlayerPacket(hostCharacter, lobbyContext.GetMyself().UUID, MyPlayer.Instance.transform.position, MyPlayer.Instance.transform.rotation);
                    player.Connection.EnqueuePacket(hostPacket);
                });
            };
        }

    }
}
