using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using Netcode.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ATTMorePlayers;

[BepInPlugin("com.michaelrooplall.mods.attmoreplayers", "More Players", "0.9.2")]
public class Plugin : BaseUnityPlugin
{

    private ConfigEntry<int> maxPlayers;

    internal static new ManualLogSource Logger;

    private void Awake() {

        maxPlayers = Config.Bind("Settings", "MaxPlayers", 8, "<int> Max Players (maximum : 8)");

        // Plugin startup logic
        Logger = base.Logger;
        // Enable logging to file
        HarmonyFileLog.Enabled = true;

        Logger.LogInfo($"[MORE PLAYERS] Plugin {MyPluginInfo.PLUGIN_GUID} - v{MyPluginInfo.PLUGIN_VERSION} is loaded!");

        var harmony = new Harmony("com.michaelrooplall.mods.attmoreplayers.patch");

        Debug.Log("\n\n==========================\n[MORE PLAYERS][PATCHES][HARMONY]\n==========================\n");
        // harmony.PatchAll(typeof(Patches));
        harmony.PatchAll(typeof(MainMenu));
        harmony.PatchAll(typeof(SteamManagerTranspilerPatch));
        harmony.PatchAll(typeof(GenNewGameName_Patch));
        harmony.PatchAll(typeof(PatchContainerManager_Awake));
        harmony.PatchAll(typeof(PatchPlayerManager_Awake));
        harmony.PatchAll(typeof(PatchPlayerManager_GetFreePlayerIndex));
        harmony.PatchAll(typeof(PatchSpawnManager_Awake));
        harmony.PatchAll(typeof(PatchPlayerList_Refresh));
        harmony.PatchAll(typeof(RecipeManagerTranspilerPatch));
        harmony.PatchAll(typeof(CaveSpawnManager_SpawnCreatures_Patch));
        harmony.PatchAll(typeof(CaveSpawnManager_SpawnLoot_Patch));
        harmony.PatchAll(typeof(CaveSpawnManager_SpawnChests_Patch));
        harmony.PatchAll(typeof(DefenceQuestControllerTranspilerPatch));
        harmony.PatchAll(typeof(HerbalistQuestControllerPatch));
        Debug.Log("\n==========================\n\n");
        
        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    private void OnDestroy() {
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        // Check if this is the scene you want
        if (scene.name == "Playtest") {
            Debug.Log("\n\n[MORE PLAYERS] SCENE CHANGE DETECTED");
            Invoke("build", 2f);
        }
    }

    private void build() {
        Debug.Log("\n\n==========================\n[MORE PLAYERS][PATCHES][UNITY]\n==========================\n");
        DebugLog.Instance.logAll = true;
        try {
            this.createMorePlayerSpawns();
        } catch (System.Exception e) {
            Logger.LogError(e);
        }
        // try {
        //     this.expandPlayerListMenu();
        // } catch (System.Exception e) {
        //     Logger.LogError(e);
        // }
        Debug.Log("\n==========================\n\n");
    }

    [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.Awake))]
    public static class SteamManagerTranspilerPatch
    {

        static int maxPlayers = 8;

        [HarmonyPrefix]
        public static void Prefix(SteamManager __instance)
        {
            Debug.Log("[MOREPLAYERS] Patching maxMembers on SteamManager");
            __instance.maxMembers = maxPlayers;  // Change the value of maxMembers to 8
            if (SteamManager.Instance) {
                SteamManager.Instance.maxMembers = maxPlayers;
            }
        }

    }

    public static class GenNewGameName_Patch {

        [HarmonyDebug]
        [HarmonyPatch(typeof(MainMenu), "GenNewGameName")] 
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool found = false;

            Debug.Log("\n\nPatching MainMenu GenNewGameName method");

            foreach (var instruction in instructions)
            {

                // Check if the instruction loads the string "Tavern"
                if (instruction.opcode.Equals(OpCodes.Ldstr) == true) {
                    Debug.Log($"Found OpCode String variable with value \"{instruction.operand.ToString()}\"");
                }

                if (instruction.opcode.Equals(OpCodes.Ldstr) == true && instruction.operand.ToString() == "Tavern")
                {
                    found = true;
                    // Replace "Tavern" with "BiggerTavern"
                    yield return new CodeInstruction(OpCodes.Ldstr, "BiggerTavern");
                    Debug.Log($"Changed OpCode String variable with value \"{instruction.operand.ToString()}\"");
                }
                else
                {
                    // Otherwise, yield the original instruction
                    yield return instruction;
                }
            }

            if (found == false){
                Debug.Log($"Failed to set OpCode String to BiggerTavern");
            }
        }
    }


    /* <-- PlayerManager Patches --> */

    public class PatchPlayerManager_Awake {

        [HarmonyDebug]
        [HarmonyPatch(typeof(PlayerManager), "Awake")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            int maxPlayers = 8;

            bool changed = false;

            Debug.Log("\n\nPatching PlayerManager Awake method to edit lastPlayerPlatformIdByPlayerIndex");

            foreach (var instruction in instructions) {

                // Check if the opcode is ldc.i4.4 (load constant 4)
                if (instruction.opcode.Equals(OpCodes.Ldc_I4_4) == true) {

                    // Todo
                    // Maybe we can just use an sbyte for each condition since the range is (-128 to 127).

                    if (maxPlayers >= 0 && maxPlayers <= 8) {
                        yield return new CodeInstruction(OpCodes.Ldc_I4, maxPlayers);
                    } else if (maxPlayers >= sbyte.MinValue && maxPlayers <= sbyte.MaxValue) {
                        // Use Ldc_I4_S for short integers (-128 to 127)
                        yield return new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)maxPlayers);
                    } else {
                        // Use Ldc_I4 for larger or smaller integers
                        yield return new CodeInstruction(OpCodes.Ldc_I4, maxPlayers);
                    }
                    changed = true;

                } else {
                    yield return instruction;
                }
            }

            if (changed) {
                Debug.Log($"Patched lastPlayerPlatformIdByPlayerIndex");
            } else {
                Debug.Log($"Failed to patch lastPlayerPlatformIdByPlayerIndex");
            }
        }
    }


    [HarmonyPatch(typeof(PlayerManager), "GetFreePlayerIndex")]
    public static class PatchPlayerManager_GetFreePlayerIndex
    {

        [HarmonyPrefix]
        static bool Prefix(PlayerManager __instance, ref byte __result, ulong playerId = 0uL) {

            Debug.Log($"\n\n[MORE PLAYERS] Patching PlayerManager GetFreePlayerIndex method");

            // Your new custom implementation of the method
            List<byte> list = new List<byte>() { 0, 1, 2, 3, 4, 5, 6, 7 };
            foreach (KeyValuePair<ulong, byte> item in PlayerManager.Instance.playerIndex)
            {
                list.Remove(item.Value);
            }

            if (playerId != 0)
            {
                for (byte b = 0; b < PlayerManager.Instance.lastPlayerPlatformIdByPlayerIndex.Length; b++)
                {
                    if (PlayerManager.Instance.lastPlayerPlatformIdByPlayerIndex[b] == playerId && list.Contains(b))
                    {
                        __result = b;
                        return false;
                    }
                }
            }

            __result = list[0]; // Return the first free player index
            return false; // Skip the original method
        }

    }

    /* <-- ContainerManager Patches --> */

    public class PatchContainerManager_Awake {

        [HarmonyDebug]
        [HarmonyPatch(typeof(ContainerManager), "Awake")]
        [HarmonyPrefix]
        static void Prefix(ContainerManager __instance) {
            Debug.Log($"[MORE PLAYERS] ContainerManager Instance {(ContainerManager.Instance != null).ToString()}");
            if (ContainerManager.Instance != null) return;
            expandPlayerContainers(__instance);
        }

        [HarmonyDebug]
        [HarmonyPatch(typeof(ContainerManager), "Awake")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            int maxPlayers = 8;

            bool changed = false;

            Debug.Log("\n\n[MORE PLAYERS] Patching ContainerManager Awake method");

            foreach (var instruction in instructions) {

                if (instruction.opcode.Equals(OpCodes.Ldc_I4_5) == true) {

                    yield return new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)(1 + maxPlayers));

                    changed = true;

                } else {
                    yield return instruction;
                }
            }

            if (changed) {
                Debug.Log($"[MORE PLAYERS] Patched ContainerManager");
            } else {
                Debug.Log($"[MORE PLAYERS] Failed to patch ContainerManager");
            }
        }

        static void expandPlayerContainers(ContainerManager __instance) {

            int maxPlayers = 8;

            Debug.Log("\n\n[MORE PLAYERS] [Adding More Player Containers]");

            GameObject playerContainerPrefab = GameObject.Find("Common/Game/PlayerContainers/ContainerP0");
            GameObject playerContainers = GameObject.Find("Common/Game/PlayerContainers");

            Debug.Log($"[MORE PLAYERS] (Before adding more player containers) Found number of slots: {__instance.playerContainers.Length} in playerContainers");
            Array.Resize(ref __instance.playerContainers, maxPlayers);

            for (int player_index = 4; player_index < maxPlayers; player_index++) {

                Debug.Log($"[MORE PLAYERS] Creating Networked Container ContainerP{player_index.ToString()}");

                // Create a new GameObject instead of instantiating an existing one
                GameObject newBackpack = Instantiate(playerContainerPrefab);
                newBackpack.name = $"ContainerP{player_index}";

                ContainerNet containerNet = newBackpack.GetComponent<ContainerNet>();
                if (containerNet == null) {
                    containerNet = newBackpack.AddComponent<ContainerNet>();
                }

                // Initialize ContainerNet properties
                containerNet.id.Value = (ushort)(player_index + 1);

                newBackpack.transform.SetParent(playerContainerPrefab.transform.parent.transform);

                // __instance.AddContainer(containerNet);
                __instance.playerContainers[player_index] = containerNet;

                Debug.Log($"[MORE PLAYERS] Pushed Networked ContainerP{player_index.ToString()} to ContainerManager and playerContainers");

            } 

            Debug.Log($"[MORE PLAYERS] Found number of slots: {__instance.playerContainers.Length} in playerContainers");

            buildBackpacks();     

        }

        

        static void buildBackpacks() {

            Debug.Log("\n\n[MORE PLAYERS] [Adding More Backpacks]");

            modifyBackpackBaseboard();

            int maxPlayers = 8;

            // Find the original GameObject in the scene
            GameObject originalBackpack3 = GameObject.Find("Tavern/Interactive/Backpacks/3");
            GameObject originalBackpack2 = GameObject.Find("Tavern/Interactive/Backpacks/2");

            float positionDif = originalBackpack3.transform.position.z - originalBackpack2.transform.position.z;

            if (originalBackpack3 != null)
            {
                for (int player_index = 4; player_index < maxPlayers; player_index++) {

                    // Clone the original GameObject
                    GameObject newBackpack = Instantiate(originalBackpack3);

                    newBackpack.name = player_index.ToString();
                    
                    newBackpack.transform.SetParent(originalBackpack3.transform.parent.transform, false);

                    // Change the position of the cloned object
                    newBackpack.transform.position = new Vector3(originalBackpack3.transform.position.x, originalBackpack3.transform.position.y, originalBackpack3.transform.position.z + ((player_index - 3) * positionDif));  // Set the desired new position
                    newBackpack.transform.rotation = originalBackpack3.transform.rotation;

                    // Get the 'PlayerContainerRef' component from the cloned object
                    var playerContainerRef = newBackpack.GetComponent<PlayerContainerRef>();
                    if (playerContainerRef != null) {

                        // Use Reflection to set playerIndex to 4
                        FieldInfo playerIndexField = typeof(PlayerContainerRef).GetField("playerIndex", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (playerIndexField != null) {
                            playerIndexField.SetValue(playerContainerRef, (byte)player_index);
                        } else {
                            Logger.LogWarning("[MORE PLAYERS] Unable to find playerIndex field via reflection.");
                        }

                        Interactive interactiveComponent = newBackpack.GetComponent<Interactive>();
                        if (interactiveComponent != null) {

                            interactiveComponent.ObjectTitle = $"Player {player_index + 1} Inventory";
                            interactiveComponent.name = player_index.ToString();
                            
                        } else {
                            Logger.LogWarning("[MORE PLAYERS] Unable to find Interactive Component.");
                        }

                    }  else {
                        Logger.LogWarning("[MORE PLAYERS] Unable to find PlayerContainerRef Component.");
                    }

                    Logger.LogInfo($"[MORE PLAYERS] Created wall backpack for {player_index + 1}.");

                }
            } else {
                Logger.LogWarning("[MORE PLAYERS] Original object originalBackpack3 not found at Tavern/Interactive/Backpacks/3.");
            }
        }

        static void modifyBackpackBaseboard() {

            int maxPlayers = 8;

            Debug.Log("\n\n[MORE PLAYERS] [Modifying Backpack Baseboard]");

            // Find the GameObject by its path in the hierarchy
            GameObject backpacks = GameObject.Find("Tavern/Interactive/Backpacks");
            GameObject modularDoor = GameObject.Find("Tavern/Interactive/Backpacks/Modular_Door_A");

            backpacks.transform.position = new Vector3(backpacks.transform.position.x, backpacks.transform.position.y, 46.60f);

            if (modularDoor != null)
            {
                // Modify the position
                modularDoor.transform.position = new Vector3(-22.05f, 4.65f, 46.90f + (0.38f * (float)(maxPlayers - 4)));  // Example position

                // Modify the localScale
                modularDoor.transform.localScale = new Vector3(1.0f, 1.0f + (0.3f * (float)(maxPlayers - 4)), 1.0f); // Example scale

                Logger.LogInfo("[MORE PLAYERS] Backpack backboard position and scale modified!");
            } else {
                Logger.LogWarning("[MORE PLAYERS] Backpack backboard not found!");
            }
        }
    }


    /* <-- SpawnManager Patches --> */

    // SpawnManager
        // Awake
            // _playerCheckPoint

    public class PatchSpawnManager_Awake {

        [HarmonyDebug]
        [HarmonyPatch(typeof(SpawnManager), "Awake")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            int maxPlayers = 8;

            bool changed = false;

            Debug.Log("\n\n[MORE PLAYERS] Patching SpawnManager Awake method to edit _playerCheckPoint");

            foreach (var instruction in instructions) {

                // Check if the opcode is ldc.i4.4 (load constant 4)
                if (instruction.opcode.Equals(OpCodes.Ldc_I4_4) == true) {

                    if (maxPlayers >= 0 && maxPlayers <= 8) {
                        yield return new CodeInstruction(OpCodes.Ldc_I4, maxPlayers);
                    } else if (maxPlayers >= sbyte.MinValue && maxPlayers <= sbyte.MaxValue) {
                        // Use Ldc_I4_S for short integers (-128 to 127)
                        yield return new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)maxPlayers);
                    } else {
                        // Use Ldc_I4 for larger or smaller integers
                        yield return new CodeInstruction(OpCodes.Ldc_I4, maxPlayers);
                    }
                    changed = true;

                } else {
                    yield return instruction;
                }
            }

            if (changed) {
                Debug.Log($"[MORE PLAYERS] Patched SpawnManager _playerCheckPoint");
            } else {
                Debug.Log($"[MORE PLAYERS] Failed to patch SpawnManager _playerCheckPoint");
            }
        }
    }

    private void createMorePlayerSpawns() {

        Debug.Log("\n\n[MORE PLAYERS] [Adding Spawn Points for Extra Players]");

        var gameObject = GameObject.Find("Common/Game");

        if (gameObject == null) {
            Debug.Log("[MORE PLAYERS] Failed to find Common/Game GameObject");
            return;
        }

        PlayerManager playerManager = gameObject.GetComponent<PlayerManager>(); 

        Transform[] newPlayerSpawnPos = new Transform[maxPlayers.Value];

        for (int i = 0; i < playerManager.playerSpawnPos.Length; i++) {
            newPlayerSpawnPos[i] = playerManager.playerSpawnPos[i];
        }

        for (int player_index = 4; player_index < maxPlayers.Value; player_index++) {
            newPlayerSpawnPos[player_index] = playerManager.playerSpawnPos[ player_index % 4];
        }

        playerManager.playerSpawnPos = newPlayerSpawnPos;

        Debug.Log("[MORE PLAYERS] Added more spawnpoints");

    }

    /* <-- PlayerList UI Patches --> */

    public class PatchPlayerList_Refresh {

        [HarmonyDebug]
        [HarmonyPatch(typeof(PlayerList), "Refresh")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {

            int maxPlayers = 8;

            bool changed = false;

            Debug.Log("\n\n[MORE PLAYERS] Patching PlayerList Refresh method");

            foreach (var instruction in instructions) {

                // Check if the opcode is ldc.i4.4 (load constant 4)
                if (instruction.opcode.Equals(OpCodes.Ldc_I4_4) == true) {

                    // Todo
                    // Maybe we can just use an sbyte for each condition since the range is (-128 to 127).

                    if (maxPlayers >= 0 && maxPlayers <= 8) {
                        yield return new CodeInstruction(OpCodes.Ldc_I4, maxPlayers);
                    } else if (maxPlayers >= sbyte.MinValue && maxPlayers <= sbyte.MaxValue) {
                        // Use Ldc_I4_S for short integers (-128 to 127)
                        yield return new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)maxPlayers);
                    } else {
                        // Use Ldc_I4 for larger or smaller integers
                        yield return new CodeInstruction(OpCodes.Ldc_I4, maxPlayers);
                    }
                    changed = true;

                } else {
                    yield return instruction;
                }
            }

            if (changed) {
                Debug.Log($"[MORE PLAYERS] Patched PlayerList[Refresh]");
            } else {
                Debug.Log($"[MORE PLAYERS] Failed to patch PlayerList[Refresh]");
            }
        }
    }


    /* <-- RecipeManager EXP multiplier Patches --> */

    // RecipeManager
        // playerCountK

    [HarmonyPatch(typeof(RecipeManager), "Awake")]
    public static class RecipeManagerTranspilerPatch {

        static int maxPlayers = 8;

        [HarmonyPrefix]
        public static void Prefix(RecipeManager __instance)
        {
            Debug.Log("[MORE PLAYERS] Patching RecipeManager playerCountK EXP Multiplier");

            FieldInfo playerCountKField = __instance.GetType().GetField("playerCountK", BindingFlags.NonPublic | BindingFlags.Instance);

            if (playerCountKField != null) {
                // Create a new array for maxPlayers
                float[] playerCountK = new float[8] { 2f, 1.5f, 1.25f, 1f, 1f, 1f, 1f, 1f };
                
                // Set the value of the field
                playerCountKField.SetValue(__instance, playerCountK);
            } else {
                Debug.LogError("[MORE PLAYERS] playerCountK field not found!");
            }

        }

    }

    /* <-- Quest Managers Patches --> */

    // CaveSpawnManager
        // SpawnCreatures
        // SpawnLoot
        // SpawnChests

    [HarmonyPatch(typeof(CaveSpawnManager), "SpawnCreatures")]
    public static class CaveSpawnManager_SpawnCreatures_Patch
    {

        [HarmonyPrefix]
        static bool Prefix(CaveSpawnManager __instance, ref int creaturesCount) {

            Debug.Log($"\n\n[MORE PLAYERS] Patching (Overwriting) CaveSpawnManager SpawnCreatures method");
            Debug.Log("CaveSpawnManager.SpawnCreatures");

            int num = 2 * UnityEngine.Random.Range(PlayerManager.Instance.players.Count, PlayerManager.Instance.players.Count * 3 + 1);
            creaturesCount = ((creaturesCount == -1) ? num : creaturesCount);

            FieldInfo creaturesSpawnPointsField = CaveSpawnManager.Instance.GetType().GetField("creaturesSpawnPoints", BindingFlags.NonPublic | BindingFlags.Instance);
            List<Transform> creaturesSpawnPoints = (List<Transform>)creaturesSpawnPointsField.GetValue(__instance);

            FieldInfo creatureTypesField = CaveSpawnManager.Instance.GetType().GetField("creatureTypes", BindingFlags.NonPublic | BindingFlags.Instance);
            List<Spawnable.Type> creatureTypes = (List<Spawnable.Type>)creatureTypesField.GetValue(__instance);

            FieldInfo spawnedCreaturesField = CaveSpawnManager.Instance.GetType().GetField("spawnedCreatures", BindingFlags.NonPublic | BindingFlags.Instance);
            List<Vulnerable> spawnedCreatures = (List<Vulnerable>)spawnedCreaturesField.GetValue(__instance);

            MethodInfo OnCreatureDeathMethod = __instance.GetType().GetMethod("OnCreatureDeath", BindingFlags.NonPublic | BindingFlags.Instance);


            List<Transform> list = creaturesSpawnPoints.OrderBy((Transform n) => Guid.NewGuid()).ToList();
            creaturesCount = Math.Min(creaturesCount, list.Count - 1);

            for (int i = 0; i < creaturesCount; i++)
            {
                Spawnable.Type type = creatureTypes[UnityEngine.Random.Range(0, creatureTypes.Count)];
                SpawnManager.Instance.ManualSpawn(type, list[i].position, Quaternion.identity, out var spawnable, forcePosition: true);
                if (spawnable.TryGetComponent<Vulnerable>(out var component))
                {
                    spawnedCreatures.Add(component);
                    Vulnerable vulnerable = component;

                    // Dynamically invoke the OnLootDestroy method using reflection
                    Action<Vulnerable> onDeathAction = (Action<Vulnerable>)Delegate.CreateDelegate(typeof(Action<Vulnerable>), __instance, OnCreatureDeathMethod);
                    vulnerable.onDeath = (Action<Vulnerable>)Delegate.Combine(vulnerable.onDeath, onDeathAction);

                }
            }

            return false; // Skip the original method
        }

    }

    [HarmonyPatch(typeof(CaveSpawnManager), "SpawnLoot")]
    public static class CaveSpawnManager_SpawnLoot_Patch
    {

        [HarmonyPrefix]
        static bool Prefix(CaveSpawnManager __instance, ref int lootItemsCount) {

            Debug.Log($"\n\n[MORE PLAYERS] Patching (Overwriting) CaveSpawnManager SpawnLoot method");
            Debug.Log("CaveSpawnManager.SpawnLoot");
            
            int numPlayers = PlayerManager.Instance.players.Count;

            FieldInfo minLootObjectsPerPlayerField = CaveSpawnManager.Instance.GetType().GetField("minLootObjectsPerPlayer", BindingFlags.NonPublic | BindingFlags.Instance);
            int minLootObjectsPerPlayer = (int)minLootObjectsPerPlayerField.GetValue(CaveSpawnManager.Instance);

            FieldInfo maxLootObjectsPerPlayerField = CaveSpawnManager.Instance.GetType().GetField("maxLootObjectsPerPlayer", BindingFlags.NonPublic | BindingFlags.Instance);
            int maxLootObjectsPerPlayer = (int)maxLootObjectsPerPlayerField.GetValue(CaveSpawnManager.Instance);
            
            FieldInfo lootSpawnPositionsField = CaveSpawnManager.Instance.GetType().GetField("lootSpawnPositions", BindingFlags.NonPublic | BindingFlags.Instance);
            List<Transform> lootSpawnPositions = (List<Transform>)lootSpawnPositionsField.GetValue(CaveSpawnManager.Instance);

            FieldInfo spawnedLootField = CaveSpawnManager.Instance.GetType().GetField("spawnedLoot", BindingFlags.NonPublic | BindingFlags.Instance);
            List<Vulnerable> spawnedLoot = (List<Vulnerable>)spawnedLootField.GetValue(__instance);

            MethodInfo onLootDestroyMethod = __instance.GetType().GetMethod("OnLootDestroy", BindingFlags.NonPublic | BindingFlags.Instance);

            int num = 2 * UnityEngine.Random.Range(PlayerManager.Instance.players.Count * minLootObjectsPerPlayer, PlayerManager.Instance.players.Count * maxLootObjectsPerPlayer + 1);
            lootItemsCount = ((lootItemsCount == -1) ? num : lootItemsCount);

            List<Transform> list = lootSpawnPositions.OrderBy((Transform n) => Guid.NewGuid()).ToList();
            
            lootItemsCount = Math.Min(lootItemsCount, list.Count - 1);

            for (int i = 0; i < lootItemsCount; i++)
            {
                Vector3 position = list[i].position;
                Quaternion rotation = list[i].rotation;
                Spawnable.Type type = (((double)UnityEngine.Random.value > 0.5) ? Spawnable.Type.LootBarrel : Spawnable.Type.LootCrate);
                if (SpawnManager.Instance.ManualSpawn(type, position, rotation, out var spawnable, forcePosition: true) && spawnable.TryGetComponent<Vulnerable>(out var component))
                {
                    spawnedLoot.Add(component);
                    Vulnerable vulnerable = component;

                    // Dynamically invoke the OnLootDestroy method using reflection
                    Action<Vulnerable> onDeathAction = (Action<Vulnerable>)Delegate.CreateDelegate(typeof(Action<Vulnerable>), __instance, onLootDestroyMethod);
                    vulnerable.onDeath = (Action<Vulnerable>)Delegate.Combine(vulnerable.onDeath, onDeathAction);
                }
            }

            return false;

        }

    }

    [HarmonyPatch(typeof(CaveSpawnManager), "SpawnChests")]
    public static class CaveSpawnManager_SpawnChests_Patch
    {

        [HarmonyPrefix]
        static bool Prefix(CaveSpawnManager __instance) {

            Debug.Log($"\n\n[MORE PLAYERS] Patching (Overwriting) CaveSpawnManager SpawnChests method");
            Debug.Log("CaveSpawnManager.SpawnChests");

            FieldInfo minChestsPerPlayerField = CaveSpawnManager.Instance.GetType().GetField("minChestsPerPlayer", BindingFlags.NonPublic | BindingFlags.Instance);
            int minChestsPerPlayer = (int)minChestsPerPlayerField.GetValue(CaveSpawnManager.Instance);

            FieldInfo maxChestsPerPlayerField = CaveSpawnManager.Instance.GetType().GetField("maxChestsPerPlayer", BindingFlags.NonPublic | BindingFlags.Instance);
            int maxChestsPerPlayer = (int)maxChestsPerPlayerField.GetValue(CaveSpawnManager.Instance);

            FieldInfo chestSpawnPositionsField = CaveSpawnManager.Instance.GetType().GetField("chestSpawnPositions", BindingFlags.NonPublic | BindingFlags.Instance);
            List<Transform> chestSpawnPositions = (List<Transform>)chestSpawnPositionsField.GetValue(CaveSpawnManager.Instance);

            FieldInfo chestPrefabField = CaveSpawnManager.Instance.GetType().GetField("chestPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
            NetworkObject chestPrefab = (NetworkObject)chestPrefabField.GetValue(CaveSpawnManager.Instance);

            FieldInfo spawnedChestsField = CaveSpawnManager.Instance.GetType().GetField("spawnedChests", BindingFlags.NonPublic | BindingFlags.Instance);
            List<NetworkObject> spawnedChests = (List<NetworkObject>)spawnedChestsField.GetValue(CaveSpawnManager.Instance);

            MethodInfo fillChestWithLootMethod = __instance.GetType().GetMethod("FillChestWithLoot", BindingFlags.NonPublic | BindingFlags.Instance);

            int num = UnityEngine.Random.Range(PlayerManager.Instance.players.Count * minChestsPerPlayer, PlayerManager.Instance.players.Count * maxChestsPerPlayer + 1);
            List<Transform> list = chestSpawnPositions.OrderBy((Transform n) => Guid.NewGuid()).ToList();

            num = Math.Min(num, list.Count - 1);

            for (int i = 0; i < num; i++)
            {
                Vector3 position = list[i].position;
                Quaternion rotation = list[i].rotation;
                NetworkObject networkObject = Instantiate(chestPrefab, position, rotation);
                spawnedChests.Add(networkObject);
                NetworkObject networkObject2 = networkObject;
                if (Master.Instance.HasConnectingClients() && AppSettingsManager.Instance.appSettings.system.useSpawnQueue)
                {
                    Game.Instance.SpawnEnqueue(networkObject2);
                }
                else
                {
                    networkObject2.Spawn();
                }

                if (networkObject.TryGetComponent<ContainerNet>(out var component))
                {
                    fillChestWithLootMethod.Invoke(__instance, new object[] { component });
                }
            }

            return false;

        }

    }

    // DefenceQuestController
        // delayPerEnemyByPlayers

    [HarmonyPatch(typeof(DefenceQuestController), "Awake")]
    public static class DefenceQuestControllerTranspilerPatch
    {

        [HarmonyPrefix]
        public static void Prefix(DefenceQuestController __instance)
        {
            Debug.Log("[MORE PLAYERS] Patching DefenceQuestController delayPerEnemyByPlayers List");

            FieldInfo delayField = __instance.GetType().GetField("delayPerEnemyByPlayers", BindingFlags.NonPublic | BindingFlags.Instance);

            if (delayField != null) {

                List<int> newDelays = new List<int> {7, 5, 4, 3, 3, 3, 3, 3};

                delayField.SetValue(__instance, newDelays);

                Debug.Log("[MORE PLAYERS] Successfully added values to delayField for more players");

            } else {

                Debug.LogWarning("[MORE PLAYERS] delayPerEnemyByPlayers field not found");

            }

        }

    }

    // HerbalistQuestController
        // SpawnBears

    [HarmonyPatch(typeof(HerbalistQuestController), "Awake")]
    public static class HerbalistQuestControllerPatch
    {

        [HarmonyPrefix]
        public static void Prefix(HerbalistQuestController __instance)
        {
            Debug.Log("[MORE PLAYERS] Adding more HerbalistQuestController Bear Spawns");

            var bearSpawnPointField = __instance.GetType().GetField("_bearSpawnPoint", BindingFlags.NonPublic | BindingFlags.Instance);

            if (bearSpawnPointField != null) {

                Transform[] originalBearSpawnPoints = (Transform[])bearSpawnPointField.GetValue(__instance);

                Transform[] bearSpawnPoints = [originalBearSpawnPoints[0], originalBearSpawnPoints[1], originalBearSpawnPoints[0], originalBearSpawnPoints[1], originalBearSpawnPoints[0], originalBearSpawnPoints[1]];
                
                bearSpawnPointField.SetValue(__instance, bearSpawnPoints);

                Debug.Log("[MORE PLAYERS] Re-using bearSpawnPoints for more players");

            } else {
                Debug.LogError("[MORE PLAYERS] _bearSpawnPoint field not found!");
            }

            
        }

    }

}
