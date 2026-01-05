

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Actors.Player;
using Il2CppAssets.Scripts.Inventory.Stats;
using Il2CppAssets.Scripts.Inventory__Items__Pickups.Stats;
// EStat will be resolved at runtime using reflection
using Il2CppRewired.Utils;
using Il2CppTMPro;
using MelonLoader;
using Multibonk.Networking.Lobby;
using UnityEngine;
using UnityEngine.UI;

namespace Multibonk.Game.Patches
{
    public static class MainMenuPatches
    {
#pragma warning disable IDE0051
        private static bool IsHovered(MyButtonCharacter btn) => btn.hoverOverlay.activeSelf;

        // Helper methods to get EStat enum values using reflection
        // EStat type will be resolved at runtime since it's not available at compile time
        private static System.Type cachedEStatType = null;
        
        private static System.Type FindEStatType()
        {
            if (cachedEStatType != null)
                return cachedEStatType;

            var possibleNamespaces = new[]
            {
                "Il2CppAssets.Scripts.Inventory.Stats.EStat",
                "Il2CppAssets.Scripts.Inventory__Items__Pickups.Stats.EStat",
                "Il2CppAssets.Scripts.Menu.Shop.EStat"
            };

            // Try using Il2CppSystem.Type.GetType first
            foreach (var fullTypeName in possibleNamespaces)
            {
                try
                {
                    var enumType = Il2CppSystem.Type.GetType(fullTypeName);
                    if (enumType != null)
                    {
                        var systemType = System.Type.GetType(enumType.FullName);
                        if (systemType != null && systemType.IsEnum)
                        {
                            cachedEStatType = systemType;
                            return systemType;
                        }
                    }
                }
                catch { }
            }

            // Fallback: Search in all loaded assemblies
            try
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        foreach (var fullTypeName in possibleNamespaces)
                        {
                            var type = assembly.GetType(fullTypeName);
                            if (type != null && type.IsEnum)
                            {
                                cachedEStatType = type;
                                return type;
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }

            return null;
        }

        private static dynamic TryGetStat(string[] statNames)
        {
            // Try each stat name until one works
            // This uses dynamic typing to resolve EStat at runtime
            var eStatType = FindEStatType();
            if (eStatType == null)
                return null;

            foreach (var statName in statNames)
            {
                try
                {
                    var enumValue = Enum.Parse(eStatType, statName);
                    // Use dynamic to let the runtime resolve the type
                    return PlayerStats.GetStat((dynamic)enumValue);
                }
                catch { }
            }
            return null;
        }

        // Helper method to get player level using reflection
        private static int? TryGetPlayerLevel()
        {
            try
            {
                // First try MyPlayer.inventory.playerXp.level (confirmed location from inspection)
                try
                {
                    var myPlayer = MyPlayer.Instance;
                    if (myPlayer != null && myPlayer.inventory != null)
                    {
                        var playerXp = myPlayer.inventory.playerXp;
                        if (playerXp != null)
                        {
                            // Direct property access - we know it's called "level"
                            try
                            {
                                var levelProp = playerXp.GetType().GetProperty("level", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                if (levelProp != null)
                                {
                                    var value = levelProp.GetValue(playerXp);
                                    return Convert.ToInt32(value);
                                }
                            }
                            catch { }
                            
                            // Fallback: try getter method
                            try
                            {
                                var getLevelMethod = playerXp.GetType().GetMethod("get_level", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                if (getLevelMethod != null)
                                {
                                    var value = getLevelMethod.Invoke(playerXp, null);
                                    return Convert.ToInt32(value);
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch { }
                
                // Then try MyPlayer directly
                try
                {
                    var myPlayer = MyPlayer.Instance;
                    if (myPlayer != null)
                    {
                        var myPlayerType = typeof(MyPlayer);
                        var myPlayerLevelNames = new[] { "Level", "CurrentLevel", "PlayerLevel", "level", "currentLevel" };
                        
                        foreach (var name in myPlayerLevelNames)
                        {
                            try
                            {
                                var prop = myPlayerType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                                if (prop != null)
                                {
                                    var value = prop.GetValue(myPlayer);
                                    if (value != null)
                                    {
                                        return Convert.ToInt32(value);
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch { }

                // Then try PlayerStats
                var playerStatsType = typeof(PlayerStats);
                var possibleNames = new[] { "Level", "CurrentLevel", "PlayerLevel", "level", "currentLevel" };
                
                foreach (var name in possibleNames)
                {
                    try
                    {
                        // Try as static property first
                        var staticProp = playerStatsType.GetProperty(name, BindingFlags.Public | BindingFlags.Static);
                        if (staticProp != null)
                        {
                            try
                            {
                                var value = staticProp.GetValue(null);
                                if (value != null)
                                {
                                    return Convert.ToInt32(value);
                                }
                            }
                            catch { }
                        }
                        
                        // Try as instance property
                        var prop = playerStatsType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                        if (prop != null)
                        {
                            try
                            {
                                // Try to get instance property if PlayerStats has an Instance
                                var instanceProp = playerStatsType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                                if (instanceProp != null)
                                {
                                    var instance = instanceProp.GetValue(null);
                                    if (instance != null)
                                    {
                                        var value = prop.GetValue(instance);
                                        if (value != null)
                                        {
                                            return Convert.ToInt32(value);
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }

                // Try methods
                foreach (var name in possibleNames)
                {
                    try
                    {
                        var method = playerStatsType.GetMethod($"Get{name}", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                        if (method != null)
                        {
                            var value = method.Invoke(null, null);
                            if (value != null)
                            {
                                return Convert.ToInt32(value);
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
            
            return null;
        }

        // Helper method to get player XP using reflection
        private static int? TryGetPlayerXP()
        {
            try
            {
                // First try MyPlayer.inventory.playerXp.xp (confirmed location from inspection)
                try
                {
                    var myPlayer = MyPlayer.Instance;
                    if (myPlayer != null && myPlayer.inventory != null)
                    {
                        var playerXp = myPlayer.inventory.playerXp;
                        if (playerXp != null)
                        {
                            // Direct property access - we know it's called "xp"
                            try
                            {
                                var xpProp = playerXp.GetType().GetProperty("xp", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                if (xpProp != null)
                                {
                                    var value = xpProp.GetValue(playerXp);
                                    return Convert.ToInt32(value);
                                }
                            }
                            catch { }
                            
                            // Fallback: try getter method
                            try
                            {
                                var getXpMethod = playerXp.GetType().GetMethod("get_xp", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                if (getXpMethod != null)
                                {
                                    var value = getXpMethod.Invoke(playerXp, null);
                                    return Convert.ToInt32(value);
                                }
                            }
                            catch { }
                            
                            // Also try GetXpInt method
                            try
                            {
                                var getXpIntMethod = playerXp.GetType().GetMethod("GetXpInt", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                if (getXpIntMethod != null)
                                {
                                    var value = getXpIntMethod.Invoke(playerXp, null);
                                    return Convert.ToInt32(value);
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch { }
                
                // Then try MyPlayer directly
                try
                {
                    var myPlayer = MyPlayer.Instance;
                    if (myPlayer != null)
                    {
                        var myPlayerType = typeof(MyPlayer);
                        var myPlayerXPNames = new[] { "XP", "Experience", "Exp", "CurrentXP", "CurrentExperience", "xp", "experience" };
                        
                        foreach (var name in myPlayerXPNames)
                        {
                            try
                            {
                                var prop = myPlayerType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                                if (prop != null)
                                {
                                    var value = prop.GetValue(myPlayer);
                                    if (value != null)
                                    {
                                        return Convert.ToInt32(value);
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch { }

                // Then try PlayerStats
                var playerStatsType = typeof(PlayerStats);
                var possibleNames = new[] { "XP", "Experience", "Exp", "CurrentXP", "CurrentExperience", "xp", "experience" };
                
                foreach (var name in possibleNames)
                {
                    try
                    {
                        // Try as static property first
                        var staticProp = playerStatsType.GetProperty(name, BindingFlags.Public | BindingFlags.Static);
                        if (staticProp != null)
                        {
                            try
                            {
                                var value = staticProp.GetValue(null);
                                if (value != null)
                                {
                                    return Convert.ToInt32(value);
                                }
                            }
                            catch { }
                        }
                        
                        // Try as instance property
                        var prop = playerStatsType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                        if (prop != null)
                        {
                            try
                            {
                                // Try to get instance property if PlayerStats has an Instance
                                var instanceProp = playerStatsType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                                if (instanceProp != null)
                                {
                                    var instance = instanceProp.GetValue(null);
                                    if (instance != null)
                                    {
                                        var value = prop.GetValue(instance);
                                        if (value != null)
                                        {
                                            return Convert.ToInt32(value);
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }

                // Try methods
                foreach (var name in possibleNames)
                {
                    try
                    {
                        var method = playerStatsType.GetMethod($"Get{name}", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                        if (method != null)
                        {
                            var value = method.Invoke(null, null);
                            if (value != null)
                            {
                                return Convert.ToInt32(value);
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
            
            return null;
        }


        [HarmonyPatch(typeof(MainMenu), "GoToMapSelection")]
        class ConfirmCharacterPatch
        {
            static bool Prefix()
            {
                if (LobbyPatchFlags.IsHosting)
                    return true;

                var ui = UnityEngine.Object.FindObjectOfType<CharacterInfoUI>();

                if (ui)
                {
                    var buttonConfirm = ui.GetComponentsInChildren<Button>(true)
                      .FirstOrDefault(b => b.name == "B_Confirm");

                    buttonConfirm.GetComponentInChildren<TMP_Text>().text = "Aguardando host...";
                    buttonConfirm.GetComponent<ResizeOnLocalization>().DelayedRefresh();

                    GameEvents.TriggerConfirmCharacter();
                }

                //return false;
                return true;
            }
        }

        [HarmonyPatch(typeof(MyPlayer), "FixedUpdate")]
        class PlayerUpdatedPatch
        {
            static void Postfix()
            {
                var myPlayer = MyPlayer.Instance;
                if (myPlayer == null) return;
                if (myPlayer.gameObject.IsNullOrDestroyed()) return;

                const float positionThreshold = 0.05f;
                const float rotationThreshold = 10f;

                bool isMoving = (myPlayer.transform.position - GamePatchFlags.LastPlayerPosition).sqrMagnitude > positionThreshold * positionThreshold;
                
                if (isMoving)
                {
                    GamePatchFlags.LastPlayerPosition = myPlayer.transform.position;
                    GameEvents.TriggerPlayerMoved(myPlayer.transform.position);
                    
                    // Asegurar que la animación de caminar esté activa para el jugador local
                    if (myPlayer.playerRenderer != null && !myPlayer.playerRenderer.moving)
                    {
                        myPlayer.playerRenderer.moving = true;
                        myPlayer.playerRenderer.ForceMoving(true);
                    }
                }

                var rotation = myPlayer.playerRenderer.transform.rotation;

                if (Quaternion.Angle(rotation, GamePatchFlags.LastPlayerRotation) > rotationThreshold)
                {
                    GamePatchFlags.LastPlayerRotation = rotation;
                    GameEvents.TriggerPlayerRotated(rotation);
                }

                // Intentar detectar ataques monitoreando el inventario
                // Por ahora deshabilitado hasta encontrar el método correcto
                // CheckForAttack(myPlayer);

                // Track level and XP changes using PlayerStats
                try
                {
                    if (PlayerStats.HasStats())
                    {
                        // Obtener nivel usando reflexión
                        var levelResult = TryGetPlayerLevel();
                        if (levelResult.HasValue)
                        {
                            int currentLevel = levelResult.Value;
                            if (currentLevel != GamePatchFlags.LastPlayerLevel)
                            {
                                MelonLogger.Msg($"[SYNC] Nivel cambiado: {GamePatchFlags.LastPlayerLevel} -> {currentLevel}");
                                GamePatchFlags.LastPlayerLevel = currentLevel;
                                GameEvents.TriggerPlayerLevelUp(currentLevel);
                            }
                        }

                        // Obtener XP usando reflexión
                        var xpResult = TryGetPlayerXP();
                        if (xpResult.HasValue)
                        {
                            int currentXP = xpResult.Value;
                            if (currentXP != GamePatchFlags.LastPlayerXP)
                            {
                                MelonLogger.Msg($"[SYNC] XP cambiado: {GamePatchFlags.LastPlayerXP} -> {currentXP}");
                                GamePatchFlags.LastPlayerXP = currentXP;
                                GameEvents.TriggerPlayerXPChanged(currentXP);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    // Log solo la primera vez para debug
                    if (GamePatchFlags.LastPlayerLevel == 1 && GamePatchFlags.LastPlayerXP == 0)
                    {
                        MelonLogger.Msg($"Error obteniendo stats: {e.Message}");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MyPlayer), "Start")]
        class GameStartedPatch
        {
            static void Postfix()
            {
                GameEvents.TriggerGameLoadedEvent();
            }
        }

        // Patch de inspección simplificado - solo muestra información básica de PlayerXp
        [HarmonyPatch(typeof(MyPlayer), "Start")]
        class InspectEStatEnumPatch
        {
            private static bool hasInspected = false;

            static void Postfix()
            {
                if (hasInspected) return;
                hasInspected = true;

                MelonLogger.Msg("=== INSPECCIONANDO PlayerXp (simplificado) ===");

                try
                {
                    var myPlayer = MyPlayer.Instance;
                    if (myPlayer != null && myPlayer.inventory != null && myPlayer.inventory.playerXp != null)
                    {
                        var playerXp = myPlayer.inventory.playerXp;
                        var playerXpType = playerXp.GetType();
                        MelonLogger.Msg($"Tipo: {playerXpType.FullName}");
                        
                        // Solo mostrar las propiedades relevantes (level y xp)
                        try
                        {
                            var levelProp = playerXpType.GetProperty("level", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (levelProp != null)
                            {
                                var levelValue = levelProp.GetValue(playerXp);
                                MelonLogger.Msg($"  - level: {levelValue}");
                            }
                        }
                        catch { }
                        
                        try
                        {
                            var xpProp = playerXpType.GetProperty("xp", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (xpProp != null)
                            {
                                var xpValue = xpProp.GetValue(playerXp);
                                MelonLogger.Msg($"  - xp: {xpValue}");
                            }
                        }
                        catch { }
                        
                        MelonLogger.Msg("=== PlayerXp encontrado correctamente ===");
                    }
                    else
                    {
                        MelonLogger.Msg("PlayerXp no disponible aún");
                    }
                }
                catch (Exception e)
                {
                    MelonLogger.Msg($"Error inspeccionando PlayerXp: {e.Message}");
                }
            }
        }


        [HarmonyPatch(typeof(MapSelectionUi), "StartMap")]
        class ConfirmMapPatch
        {
            static bool Prefix()
            {
                if (!LobbyPatchFlags.IsHosting && !GamePatchFlags.AllowStartMapCall)
                    return false;

                GameEvents.TriggerConfirmMap();

                return true;
            }
        }

        [HarmonyPatch(typeof(CharacterInfoUI), "OnCharacterSelected")]
        class CharacterSelectionPatch
        {
            static void Postfix(MyButtonCharacter btn)
            {
                if (btn != null)
                {
                    GameEvents.TriggerCharacterChanged(btn.characterData);
                }
            }
        }


        [HarmonyPatch(typeof(ProceduralTileGeneration), "Generate")]
        class MapGenerationPatch
        {
            static void Prefix(Il2Cpp.ProceduralTileGeneration __instance, UnityEngine.Vector3 __result, ref UnityEngine.Vector3 __0, Il2Cpp.StageData __1, Il2CppAssets.Scripts.MapGeneration.ProceduralTiles.MapParameters __2, ref bool __3)
            {
                MelonLogger.Msg("Patching debug seed");
                __instance.debugSeed = GamePatchFlags.Seed;
                __3 = true;
            }
        }

        // Patch for level up detection
        [HarmonyPatch(typeof(MyPlayer), "OnLevelUp")]
        class PlayerOnLevelUpPatch
        {
            static void Postfix(MyPlayer __instance)
            {
                try
                {
                    if (PlayerStats.HasStats())
                    {
                        // Obtener el nivel actual después del level up
                        int currentLevel = 0;
                        var levelResult = TryGetStat(new[] { "Level", "PlayerLevel", "CurrentLevel" });
                        if (levelResult != null)
                        {
                            currentLevel = (int)levelResult;
                        }
                        else
                        {
                            return;
                        }

                        // Actualizar el flag y disparar el evento
                        GamePatchFlags.LastPlayerLevel = currentLevel;
                        GameEvents.TriggerPlayerLevelUp(currentLevel);
                    }
                }
                catch (Exception e)
                {
                    MelonLogger.Msg($"Error en OnLevelUp patch: {e.Message}");
                }
            }
        }

        // Patch de depuración para encontrar métodos de ataque
        // Este patch se ejecuta una vez al inicio para listar métodos disponibles
        private static bool _hasLoggedMethods = false;
        
        [HarmonyPatch(typeof(MyPlayer), "Start")]
        class DebugAttackMethodsPatch
        {
            static void Postfix(MyPlayer __instance)
            {
                if (_hasLoggedMethods || __instance == null)
                    return;
                    
                try
                {
                    _hasLoggedMethods = true;
                    MelonLogger.Msg("=== Buscando métodos de ataque en MyPlayer ===");
                    
                    var myPlayerType = typeof(MyPlayer);
                    var methods = myPlayerType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    var attackKeywords = new[] { "shoot", "fire", "attack", "weapon", "projectile", "cast", "ability" };
                    foreach (var method in methods)
                    {
                        var methodName = method.Name.ToLower();
                        if (attackKeywords.Any(keyword => methodName.Contains(keyword)))
                        {
                            MelonLogger.Msg($"Método potencial de ataque encontrado: {method.Name} (parámetros: {string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))})");
                        }
                    }
                    
                    // También buscar en el inventario
                    if (__instance.inventory != null)
                    {
                        MelonLogger.Msg("=== Buscando métodos de ataque en Inventory ===");
                        var invType = __instance.inventory.GetType();
                        var invMethods = invType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        
                        foreach (var method in invMethods)
                        {
                            var methodName = method.Name.ToLower();
                            if (attackKeywords.Any(keyword => methodName.Contains(keyword)))
                            {
                                MelonLogger.Msg($"Método potencial de ataque en Inventory: {method.Name} (parámetros: {string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))})");
                            }
                        }
                    }
                    
                    MelonLogger.Msg("=== Fin de búsqueda de métodos de ataque ===");
                }
                catch (Exception e)
                {
                    MelonLogger.Msg($"Error en depuración de métodos: {e.Message}");
                }
            }
        }

        // TODO: Implementar detección de ataques
        // Por ahora, la detección de ataques está deshabilitada porque no conocemos
        // el método exacto que se llama cuando el jugador ataca.
        // Opciones:
        // 1. Buscar el método correcto usando reflexión en tiempo de ejecución
        // 2. Monitorear cambios en el inventario o proyectiles
        // 3. Usar un patch genérico que intercepte todas las llamadas a métodos relacionados con armas
        private static void CheckForAttack(MyPlayer myPlayer)
        {
            // Por ahora, este método está vacío hasta que encontremos el método correcto
            // para detectar ataques. La infraestructura de red ya está lista,
            // solo necesitamos encontrar cómo detectar cuando el jugador ataca.
        }


        //[HarmonyPatch(typeof(SpawnInteractables), "SpawnChests")]
        //class ChestPatch
        //{
        //    static bool Prefix()
        //    {
        //        return false;
        //    }
        //}

        //[HarmonyPatch(typeof(SpawnInteractables), "SpawnOther")]
        //class SpawnOtherPatch
        //{
        //    static bool Prefix() => false;
        //}

        //[HarmonyPatch(typeof(SpawnInteractables), "SpawnRails")]
        //class SpawnRailsPatch
        //{
        //    static bool Prefix() => false;
        //}

        //[HarmonyPatch(typeof(SpawnInteractables), "SpawnShit")]
        //class SpawnShitPatch
        //{
        //    static bool Prefix() => false;
        //}

        //[HarmonyPatch(typeof(SpawnInteractables), "SpawnShrines")]
        //class SpawnShrinesPatch
        //{
        //    static bool Prefix() => false;
        //}







    }
}
