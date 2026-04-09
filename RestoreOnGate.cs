using Assets.Scripts;
using BrilliantSkies.Core;
using BrilliantSkies.Core.Id;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.Networking;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Ftd.Constructs;
using BrilliantSkies.Ftd.Multiplayer.NetworkCommunication;
using BrilliantSkies.Ftd.Multiplayer.Requests;
using BrilliantSkies.Ftd.Planets.Factions;
using BrilliantSkies.Ftd.Planets.Factions.Designs;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ftd.Planets.Instances.Adventure;
using BrilliantSkies.Ftd.Planets.Instances.Factions;
using BrilliantSkies.Ftd.Planets.Instances.Resources;
using BrilliantSkies.Ftd.Terrain;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Ui.Tips;
using HarmonyLib;
using NetInfrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
namespace AdventurePatch
{
    [HarmonyPatch(typeof(AdventureModeProgression), nameof(AdventureModeProgression.Common_DoWarp))]
    public static class AdventureModeProgression_Common_DoWarp_Patch
    {
        // Store data before warp
        public static bool shouldRecordAndRestore = false;
        private static List<EnemySpawnData> _recordedEnemies = new List<EnemySpawnData>();
        private static int materialsToRestore = 0;
        private static void addMaterial(int amount)
        {
            materialsToRestore += amount;
        }
        private class EnemySpawnData
        {
            public float Spawnaltitude {  get; set; }
            public FactionObjectId DesignId { get; set; }
            public Vector3d Position { get; set; }
            public Quaternion Rotation { get; set; }
            public ObjectId FactionId { get; set; }
        }
        static bool Prefix(WarpGateType gateType, Vector3d warpGatePosition)
        {
            if (!shouldRecordAndRestore) return true;

            int resourcesToAdd = 0;
            materialsToRestore = 0;

            try
            {
                AdvLogger.LogInfo($"[WarpPatch] Recording pre-warp state for warp type: {gateType}");
                NetworkedInfoStore.Add("starting gate prefix method. materials to restore are: " + materialsToRestore);

                List<WorldSpecificationFactionDesign> allDesigns = FactionSpecifications.i.Factions
                    .Where(f => f.InstanceOfFaction.eController == FactionController.AI_General)
                    .SelectMany(f => f.Designs.Designs)
                    .Where(d => d != null)
                    .ToList();

                _recordedEnemies.Clear();

                for (int i = 0; i < StaticConstructablesManager.Constructables.Count; i++)
                {
                    var construct = StaticConstructablesManager.Constructables[i];
                    if (construct == null)
                        continue;

                    var force = construct.GetForce();
                    if (force == null)
                        continue;

                    if (force.FactionId == GAME_STATE.MyTeam)
                        continue;

                    string constructName = construct.GetName();
                    FactionObjectId designId = null;
                    WorldSpecificationFactionDesign matchedDesign = null;

                    foreach (var d in allDesigns)
                    {
                        if (d == null) continue;

                        if (!string.IsNullOrEmpty(d.Name) && constructName.Contains(d.Name))
                        {
                            matchedDesign = d;
                            designId = d.Id;
                            break;
                        }
                    }

                    // Secondary exact match fallback
                    if (designId == null)
                    {
                        foreach (var d in allDesigns)
                        {
                            if (d.Name == constructName)
                            {
                                matchedDesign = d;
                                designId = d.Id;
                                break;
                            }
                        }
                    }
                    float altitude = 0f;
                    if(matchedDesign != null)
                    {
                        altitude = matchedDesign.GetRandomSpawnPointOffset().y;
                    }
                    // Final fallback → Source DesignId
                    if (designId == null && force.Source != null && force.Source.DesignIdValid)
                    {
                        designId = force.Source.DesignId;
                        AdvLogger.LogInfo($"[WarpPatch] Fallback to Source DesignId for {constructName}: {designId}");
                    }

                    if (designId == null)
                    {
                        AdvLogger.LogInfo($"[WarpPatch] Failed to resolve design for: {constructName}");
                        continue;
                    }

                    Quaternion rotation = construct.SafeRotation;

                    _recordedEnemies.Add(new EnemySpawnData
                    {
                        DesignId = designId,
                        Spawnaltitude = altitude,
                        Position = new Vector3d(construct.GetPositionForForce()),
                        Rotation = rotation,
                        FactionId = force.FactionId
                    });

                    AdvLogger.LogInfo(
                        $"[WarpPatch] Recorded enemy: {constructName} " +
                        $"at {new Vector3d(construct.GetPositionForForce())} using design: {(matchedDesign != null ? matchedDesign.Name : "SourceID")} ({designId})"
                    );
                }

                var resourceZones = InstanceSpecification.i?.ResourceZones?.Zones;

                if (resourceZones != null)
                {
                    foreach (var zone in resourceZones)
                    {
                        if (zone == null) continue;
                        int amount = (int)(zone.Material.ReserveAmount + zone.Material.Quantity);
                        AdvLogger.LogInfo($"zone.Material.ReserveAmount: {zone.Material.ReserveAmount}, zone.MergedMaterial.ReserveAmount: {zone.MergedMaterial.ReserveAmount}, zone.MergedMaterial.ReserveRemaining: {zone.MergedMaterial.ReserveRemaining}");
                        resourcesToAdd += amount;
                        AdvLogger.LogInfo($"[WarpPatch] Added {amount} from rz to total: {resourcesToAdd}");
                    }
                }

                var dumps = InstanceSpecification.i?.ResourceZones?.Dumps;
                if (dumps != null)
                {
                    foreach (var dump in dumps)
                    {
                        if (dump == null) continue;
                        int amount = (int)(dump.Material.Quantity * InstanceSpecification.i.Header.CommonSettings.EnemyBlockDestroyedResourceDrop);
                        resourcesToAdd += amount;
                        AdvLogger.LogInfo($"[WarpPatch] Added {amount} from dump to total: {resourcesToAdd}");
                    }
                }

                AdvLogger.LogInfo($"[WarpPatch] Recorded {_recordedEnemies.Count} enemies");
            }
            catch (System.Exception ex)
            {
                AdvLogger.LogInfo($"[WarpPatch] Error in Prefix: {ex.Message}");
            }

            materialsToRestore = resourcesToAdd;

            if (ProfileManager.Instance.GetModule<AP_MConfig>().waveMode &&
                InstanceSpecification.i.Adventure.WarpPlaneDifficulty < 5 &&
                ProfileManager.Instance.GetModule<AP_MConfig>().bonusStartingMaterial > 0)
            {
                materialsToRestore += ProfileManager.Instance.GetModule<AP_MConfig>().bonusStartingMaterial;
                AdvLogger.LogInfo($"[WarpPatch] Added {ProfileManager.Instance.GetModule<AP_MConfig>().bonusStartingMaterial} from bonus in wavemode to total: {materialsToRestore}");
            }

            return true;
        }


        // Postfix after the warp => reward and respawn
        static void Postfix(WarpGateType gateType, Vector3d warpGatePosition)
        {
            if (!shouldRecordAndRestore) return;
            if (Net.IsClient) return;
            try
            {
                AdvLogger.LogInfo($"[WarpPatch] Post-warp restoration started");

                // Add reward to player
                if (materialsToRestore > 0)
                {
                    var playerFaction = InstanceSpecification.i?.GetFirstActivePlayerFaction(true);
                    if (playerFaction != null && playerFaction.ResourceStore != null)
                    {
                        playerFaction.ResourceStore.Material.Quantity += materialsToRestore;

                        // Sync resources across clients in multiplayer
                        if (Net.IsConnected)
                        {
                            InstantComs.GetSingleton().SendRpc(new RpcRequest(delegate (INetworkIdentity n)
                            {
                                ServerOutgoingRpcs.SetResources(n, playerFaction);
                            }));
                        }

                        NetworkedInfoStore.Add($"Warp reward: {materialsToRestore:F0} materials added!", 3f);
                        AdvLogger.LogInfo($"[WarpPatch] Added {materialsToRestore:F0} materials to player faction");
                    }
                }
                
                // Respawn enemies
                foreach (var enemy in _recordedEnemies)
                {
                    try
                    {
                        SpawnInstructions spawnInstructions = SpawnInstructions.IgnoreDamage | SpawnInstructions.Creative;
                        bool isConnected = Net.IsConnected;

                        if (isConnected)
                        {
                            enemy.Position = SpawnForcePatch.FindAPointInFrontOfPrimaryForce(1500, out Quaternion r);
                            var copy = enemy.Position;
                            if (enemy.Spawnaltitude != 0)
                            {
                                copy.y = enemy.Spawnaltitude;
                            }
                            SpawnRequest spawnRequest = SpawnRequest.CreatePlanetRequest(
                                enemy.DesignId,
                                copy,
                                enemy.Rotation,
                                enemy.FactionId,
                                SpawnInstructions.None);
                            spawnRequest.SpawningInstructions = spawnInstructions;
                            spawnRequest.SetSilent(true);
                            SpawnRequestManager.Instance.NewSpawnRequest(spawnRequest);
                        }
                        else
                        {
                            LoadHelper.SpawnAndInitialiseResourcesBlueprint(
                                enemy.DesignId,
                                enemy.Position,
                                enemy.Rotation,
                                enemy.FactionId,
                                new NetworkIdentityId(),
                                spawnInstructions,
                                null,
                                null);
                        }

                        AdvLogger.LogInfo($"[WarpPatch] Respawned enemy at {enemy.Position}");
                    }
                    catch (System.Exception ex)
                    {
                        AdvLogger.LogInfo($"[WarpPatch] Error respawning enemy: {ex.Message}");
                    }
                }

                // Clear recorded data
                _recordedEnemies.Clear();

                AdvLogger.LogInfo($"[WarpPatch] Post-warp restoration completed");
            }
            catch (System.Exception ex)
            {
                AdvLogger.LogInfo($"[WarpPatch] Error in Postfix: {ex.Message}");
            }
            shouldRecordAndRestore = false;
            materialsToRestore = 0;
        }
    }
}