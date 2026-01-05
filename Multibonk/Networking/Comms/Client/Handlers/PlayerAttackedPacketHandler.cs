using Multibonk.Game;
using Multibonk.Game.Handlers;
using Multibonk.Networking.Comms.Base;
using Multibonk.Networking.Comms.Base.Packet;
using Multibonk.Networking.Comms.Packet.Base.Multibonk.Networking.Comms;
using UnityEngine;
using Il2Cpp;
using Il2CppRewired.Utils;

namespace Multibonk.Networking.Comms.Client.Handlers
{
    public class PlayerAttackedPacketHandler : IClientPacketHandler
    {
        public byte PacketId => (byte)ServerSentPacketId.PLAYER_ATTACKED_PACKET;

        public void Handle(IncomingMessage msg, Connection conn)
        {
            var packet = new PlayerAttackedPacket(msg);

            GameDispatcher.Enqueue(() =>
            {
                var spawnedPlayer = GameFunctions.GetSpawnedPlayerFromId(packet.PlayerId);

                if (spawnedPlayer != null && !spawnedPlayer.PlayerObject.IsNullOrDestroyed())
                {
                    var renderer = spawnedPlayer.PlayerObject.GetComponentInChildren<PlayerRenderer>();
                    if (renderer != null)
                    {
                        // Actualizar posición y rotación del jugador
                        spawnedPlayer.PlayerObject.transform.position = packet.Position;
                        spawnedPlayer.PlayerObject.transform.rotation = packet.Rotation;
                        
                        // Intentar reproducir el efecto visual del ataque usando reflexión
                        try
                        {
                            var rendererType = renderer.GetType();
                            
                            // Intentar métodos comunes de ataque
                            var attackMethods = new[] { "Shoot", "Fire", "Attack", "TriggerAttack", "PlayAttack" };
                            foreach (var methodName in attackMethods)
                            {
                                try
                                {
                                    var method = rendererType.GetMethod(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                    if (method != null)
                                    {
                                        method.Invoke(renderer, null);
                                        break;
                                    }
                                }
                                catch { }
                            }
                            
                            // Si el renderer tiene un inventario o arma, intentar disparar desde ahí
                            try
                            {
                                var inventoryProp = rendererType.GetProperty("inventory", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                if (inventoryProp != null)
                                {
                                    var inventory = inventoryProp.GetValue(renderer);
                                    if (inventory != null)
                                    {
                                        var invType = inventory.GetType();
                                        var shootMethod = invType.GetMethod("Shoot", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                        if (shootMethod != null)
                                        {
                                            shootMethod.Invoke(inventory, null);
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                        catch (System.Exception ex)
                        {
                            MelonLoader.MelonLogger.Msg($"Error reproduciendo ataque: {ex.Message}");
                        }
                    }
                }
            });
        }
    }
}
