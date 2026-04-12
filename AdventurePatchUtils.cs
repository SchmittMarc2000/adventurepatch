using Assets.Scripts;
using BrilliantSkies.Core;
using BrilliantSkies.Core.Control.Tuning;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Id;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.Networking;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Core.UniverseRepresentation;
using BrilliantSkies.Core.Widgets;
using BrilliantSkies.FromTheDepths.Planets;
using BrilliantSkies.Ftd.Constructs.Modules.All.FireDamage;
using BrilliantSkies.Ftd.Maps.Editors.Factions;
using BrilliantSkies.Ftd.Multiplayer.NetworkCommunication;
using BrilliantSkies.Ftd.Multiplayer.Requests;
using BrilliantSkies.Ftd.Planets;
using BrilliantSkies.Ftd.Planets.Factions;
using BrilliantSkies.Ftd.Planets.Factions.Designs;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ftd.Planets.Instances.Factions;
using BrilliantSkies.Ftd.Terrain;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Ui.Special.InfoStore;
using BrilliantSkies.Ui.Tips;
using HarmonyLib;
using NetInfrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;


namespace AdventurePatch
{
    public static class AdventurePatchUtils
    {
        public static string currentplanet = "";
        public static int clearEnemies()
        {
            AdvLogger.LogInfo("[AdventurePatch UTILS] Clearing all enemies. ");
            int count = 0;
            for (int i = StaticConstructablesManager.Constructables.Count - 1; i >= 0; i--)
            {
                var construct = StaticConstructablesManager.Constructables[i];
                if (construct != null && construct.GetTeam() != GAME_STATE.MyTeam)
                {
                    count++;
                    construct.DestroyCompletely(DestroyReason.Wiped, true);
                }
            }
            return count;
        }
        public static void RepairConstruct(AllConstruct C)
        {
            if (C.State == enumConstructableState.scrapping)
            {
                return;
            }

            for (int num = C.Main.FireRestricted.Fires.Count; num > 0; num--)
            {
                Fire fire = C.Main.FireRestricted.Fires[num - 1];
                fire.FuelInfo.Fuel = 0f;
            }

            List<Block> blocks = C.AllBasics.AliveAndDead.Blocks;
            for (int num2 = blocks.Count - 1; num2 >= 0; num2--)
            {
                Block block = blocks[num2];
                if (!block.IsAlive)
                {
                    block.RepairToBlock();
                    C.BlockRepairedSoPerformAllActions(block);
                }
                else if (block.GetCurrentHealth() != block.MaximumHealth)
                {
                    block.SetCurrentHealth(block.MaximumHealth);
                    block.ChunkStuff.RequestChunkColorOrVisibilityChange();
                    block.FireDamageFraction = 0f;
                }
            }

            foreach (SubConstruct subConstruct in C.AllBasics.SubConstructList)
            {
                RepairConstruct(subConstruct);
            }
        }
        public static int GetEnemyCount()
        {
            int count = 0;
            for (int i = 0; i < StaticConstructablesManager.Constructables.Count; i++)
            {
                var construct = StaticConstructablesManager.Constructables[i];
                if (construct != null && construct.GetTeam() != GAME_STATE.MyTeam)
                {
                    count++;
                }
            }
            return count;
        }
        public static int CalculatePlayerTeamCost()
        {
            float materialCost = 0f;
            int count = 0;
            for (int i = 0; i < StaticConstructablesManager.Constructables.Count; i++)
            {
                var construct = StaticConstructablesManager.Constructables[i];
                var force = construct?.GetForce();

                if (force == null) continue;
                if (force.Source == null) continue;
                if (force.FactionId != GAME_STATE.MyTeam)
                {
                    continue;
                }
                count++;
                materialCost += force.Source.GetResourceCost(ValueQueryType.AliveSub).Material;
            }
            return (int)materialCost;
        }
        public static int CalculateDifficulty(double materialCost)
        {
            // Base formula which is precise in mid ranges
            double baseDiff = 0.01648 * Math.Pow(materialCost, 0.6242);
            double correction = 1.0;
            
            if (materialCost < 50000)
            {
                // Lower range correction (brings difficulty down at very low costs)
                correction = 0.2 + 0.8 * (materialCost / 50000);
            }
            else if (materialCost > 1000000)
            {
                // Upper range correction (brings difficulty down at very high costs)
                correction = 1.0 - 0.15 * ((materialCost - 1000000) / 500000);
                correction = Math.Max(0.85, correction);
            }
            
            return (int)(baseDiff * correction);
        }



        public static Vector3d FindAPointInFrontOfPrimaryForce(float range, out Quaternion rotation)
        {
            // Get random horizontal angle (full 360 degrees)
            float horizontalAngle = Random.Range(0f, 360f);

            Quaternion quaternion = InstanceSpecification.i.Adventure.PrimaryForce.C.myTransform.rotation;
            quaternion = Angles.RemoveRollAndPitch(quaternion);

            Vector3d primaryForceUniversePosition = InstanceSpecification.i.Adventure.PrimaryForceUniversePosition;

            // Calculate direction on flat XZ plane only (no vertical component)
            Vector3 flatDirection = Quaternion.Euler(0f, horizontalAngle, 0f) * quaternion * new Vector3(0f, 0f, 1f);
            flatDirection.y = 0f;
            flatDirection.Normalize();

            // Position at exact range on the flat plane
            Vector3d vector3d = primaryForceUniversePosition + (Vector3d)(flatDirection * range);


            Vector3 forward = (primaryForceUniversePosition - vector3d).ToSingle();
            forward.y = 0f;
            rotation = Quaternion.LookRotation(forward, Vector3.up);
            return vector3d;
        }

        public static void TakeRedPortal()
        {
            if (Net.IsClient) return;
            AdventureModeProgression_Common_DoWarp_Patch.shouldRecordAndRestore = true;
            if (Net.IsServer)
            {
                InstantComs.GetSingleton().SendRpc(new RpcRequest(delegate (INetworkIdentity n)
                {
                    ServerOutgoingRpcs.ChangeWarpPlane(n, WarpGateType.Harder, InstanceSpecification.i.Adventure.PrimaryForceUniversePosition);
                }));
            }
            AdventureModeProgression.Common_DoWarp(WarpGateType.Harder, InstanceSpecification.i.Adventure.PrimaryForceUniversePosition);
        }
        public static void SpawnEnemyCostRange(float min, float max)
       {
           uint enemycount = 0;
           float enemyvolume = 0f;
           for (int i = 0; i < StaticConstructablesManager.Constructables.Count; i++)
           {
               MainConstruct mainConstruct = StaticConstructablesManager.Constructables[i];
               if (mainConstruct != null && mainConstruct.GetTeam() != GAME_STATE.MyTeam)
               {
                   enemycount++;
                   enemyvolume += mainConstruct.AllBasics.VolumeAliveUsed;
               }
           }
           uint maxenemycount = ProfileManager.Instance.GetModule<AP_MConfig>().MaxEnemyCount;
           if (enemycount < maxenemycount)
           {
                uint maxenemyvolume = ProfileManager.Instance.GetModule<AP_MConfig>().MaxEnemyVolume;
                List<String> exceptions = new List<String>(); //false positives for rammer/nuke detection
                exceptions.Add("Scrapper");
                exceptions.Add("Piraiba");
                exceptions.Add("Panjandrum");
                exceptions.Add("Pandemonium");
                if (maxenemyvolume > enemyvolume)
               {
                    AdvLogger.LogInfo("[AdventurePatch UTILS] We are allowed to spawn an enemy. ");

                    IEnumerable<WorldSpecificationFactionDesign> fullEnemyList = (from t in FactionSpecifications.i.Factions
                                                                                  where t.InstanceOfFaction.eController == FactionController.AI_General
                                                                                  select t).SelectMany((FactionSpecificationFaction t) => t.Designs.Designs);
                    RandomSelection<WorldSpecificationFactionDesign> randomSelection = new RandomSelection<WorldSpecificationFactionDesign>();
                    int counter = 0;
                    if (ProfileManager.Instance.GetModule<AP_MConfig>().waveMode)
                    {
                        if (SpawnWaveMode.detectedSubmarine)
                        {
                            min *= 1.3f;
                            max *= 1.3f;
                            AdvLogger.LogInfo("[AdventurePatch UTILS] increased min/max cost by 30% because the player uses submarines ");
                        }
                    }
                    foreach (WorldSpecificationFactionDesign design in fullEnemyList)
                    {
                        if (design != null)
                        {
                            if (design.IsTypeIncludedInAdventure(AdventureModeProgression.AdventuringType))
                            {
                                //extract enemyspawncost
                                int enemyspawncost = (int)design.GetBlueprintFile().MaterialCost;
                                if(enemyspawncost < 50000 && design.CSI.TopSpeed > 120) //fast fliers be damned
                                {
                                    continue;
                                }
                                if (enemyspawncost < 100000 && (design.CSI.MeleePower > 4 || (design.CSI.TotalFirepower < 1 && enemyspawncost > 5000))) //nuke or rammers
                                {
                                    if(!exceptions.Contains(design.Name))
                                    {
                                        AdvLogger.LogInfo("detected as rammer or nuke: " + design.Name);
                                        if (enemyspawncost < 25000)
                                        {
                                            continue;
                                        }
                                        else if (enemyspawncost < 35000)
                                        {
                                            enemyspawncost *= 2;
                                            design.AdventureModeChance *= 0.5f;
                                        }
                                        else
                                        {
                                            enemyspawncost *= 2;
                                            design.AdventureModeChance *= 0.25f;
                                        }
                                    } else
                                    {
                                        AdvLogger.LogInfo("allowed to spawn as default: " + design.Name);
                                    }
                                    
                                }
                                if (design.BlueprintType == enumBlueprintType.Spacecraft)
                                {
                                    enemyspawncost *= 2;
                                    design.AdventureModeChance *= 0.5f;
                                }
                                if (enemyspawncost > min && enemyspawncost < max)
                                {
                                    randomSelection.Add(design);
                                    counter++;
                                }
                            }
                        }

                    }
                    // Pre-calculate segments for each design
                    int segmentCount = 8;
                    float offset = 1.75f;
                    float segmentWidth = (max - min) / segmentCount;
                    Dictionary<WorldSpecificationFactionDesign, int> designSegments = new Dictionary<WorldSpecificationFactionDesign, int>();
                    foreach (var design in randomSelection.Things)
                    {
                        float cost = (float)design.GetBlueprintFile().MaterialCost;
                        int segment = Mathf.Clamp(Mathf.FloorToInt((cost - min) / segmentWidth), 0, segmentCount - 1);
                        designSegments[design] = segment;
                    }

                    // Track last spawn segments (store in SpawnWaveMode.lastSpawnSegments)
                    if (SpawnWaveMode.lastSpawnSegments == null)
                    {
                        SpawnWaveMode.lastSpawnSegments = new List<int>();
                    }

                    WorldSpecificationFactionDesign worldSpecificationFactionDesign2 = null;
                    float chanceFactor = 1f;
                    AdvLogger.LogInfo($"[AdventurePatch UTILS] starting enemy selection from {counter} choices.");

                    // Calculate target segment based on spawn history
                    int targetSegment = segmentCount / 2; // default middle
                    if (SpawnWaveMode.lastSpawnSegments != null && SpawnWaveMode.lastSpawnSegments.Count > 0)
                    {
                        int historyCount = Mathf.Min(SpawnWaveMode.lastSpawnSegments.Count, 3);
                        float avgSegment = 0f;
                        for (int idx = 0; idx < historyCount; idx++)
                        {
                            avgSegment += SpawnWaveMode.lastSpawnSegments[idx];
                        }
                        avgSegment /= historyCount;

                        if (avgSegment < (2f+offset)) targetSegment = segmentCount - 2; // was low, go high
                        else if (avgSegment > segmentCount - (3f+offset)) targetSegment = 1; // was high, go low
                        else targetSegment = (segmentCount - 1) - Mathf.RoundToInt(avgSegment); // opposite side

                        AdvLogger.LogInfo($"[UTILS] History avg segment: {avgSegment:F1}, target segment: {targetSegment}");
                    }
                    else
                    {
                        AdvLogger.LogInfo($"Using default target segment: {targetSegment}");
                    }
                    float difficultyFactor = SpawnWaveMode.getDifficultyFactor();
                    randomSelection.NullChance = 0f;
                    if (!ProfileManager.Instance.GetModule<AP_MConfig>().testDistribution) goto spawn;
                    // ========== TEST START ==========

                    int testSegmentCount = 8;
                    float testoffset = 0;
                repeat:
                    float testSegmentWidth = (max - min) / testSegmentCount;
                    Dictionary<WorldSpecificationFactionDesign, int> testDesignSegments = new Dictionary<WorldSpecificationFactionDesign, int>();
                    foreach (var design in randomSelection.Things)
                    {
                        float cost = (float)design.GetBlueprintFile().MaterialCost;
                        int segment = Mathf.Clamp(Mathf.FloorToInt((cost - min) / testSegmentWidth), 0, testSegmentCount - 1);
                        testDesignSegments[design] = segment;
                    }

                    List<int> testLastSegments = new List<int>();
                    List<float> testSelectedCosts = new List<float>();
                    List<int> testSelectedSegments = new List<int>();

                    for (int testIdx = 0; testIdx < 1000; testIdx++)
                    {
                        int testTargetSegment = testSegmentCount / 2;
                        if (testLastSegments.Count > 0)
                        {
                            int historyCount = Mathf.Min(testLastSegments.Count, 3);
                            float avgSegment = 0f;
                            for (int idx = 0; idx < historyCount; idx++)
                            {
                                avgSegment += testLastSegments[idx];
                            }
                            avgSegment /= historyCount;

                            if (avgSegment < (2f + testoffset)) testTargetSegment = testSegmentCount - 2;
                            else if (avgSegment > testSegmentCount - (3f + testoffset)) testTargetSegment = 1;
                            else testTargetSegment = Mathf.Clamp((testSegmentCount - 1) - Mathf.RoundToInt(avgSegment), 1, testSegmentCount - 2);
                        }

                        WorldSpecificationFactionDesign testSelection = randomSelection.Select(delegate (WorldSpecificationFactionDesign t)
                        {
                            float testChanceFactor = 1f;
                            int segment = testDesignSegments[t];
                            int distanceToTarget = Mathf.Abs(segment - testTargetSegment);

                            float segmentMultiplier = 1f;
                            if (distanceToTarget == 0) segmentMultiplier = 4f;
                            else if (distanceToTarget == 1) segmentMultiplier = 3f;
                            else if (distanceToTarget == 2) segmentMultiplier = 2f;
                            else if (distanceToTarget > 2) segmentMultiplier = 3 / distanceToTarget;
                            

                            float testDifficultyBias = 1f;
                            if (difficultyFactor < 1f)
                            {
                                testDifficultyBias = 1f + (1f - difficultyFactor) * (segment / (float)testSegmentCount);
                            }
                            else if (difficultyFactor > 1f)
                            {
                                testDifficultyBias = 1f - (difficultyFactor - 1f) * (1f - segment / (float)testSegmentCount);
                                testDifficultyBias = Mathf.Max(0.5f, testDifficultyBias);
                            }

                            float testNum3 = 0.13f * t.AdventureModeChance * testChanceFactor * testDifficultyBias * segmentMultiplier;
                            return (double)testNum3;
                        });

                        if (testSelection == null) break;

                        float testCost = (float)testSelection.GetBlueprintFile().MaterialCost;
                        int testSegment = testDesignSegments[testSelection];

                        testLastSegments.Insert(0, testSegment);
                        if (testLastSegments.Count > 5) testLastSegments.RemoveAt(testLastSegments.Count - 1);

                        testSelectedCosts.Add(testCost);
                        testSelectedSegments.Add(testSegment);
                    }

                    if (testSelectedSegments.Count > 0)
                    {
                        // Calculate segment distribution
                        int[] segmentCounts = new int[testSegmentCount];
                        foreach (int seg in testSelectedSegments)
                        {
                            segmentCounts[seg]++;
                        }

                        AdvLogger.LogInfo("========== TEST RESULTS (1000 spawns) ==========");
                        AdvLogger.LogInfo($"Total successful spawns: {testSelectedSegments.Count}");
                        AdvLogger.LogInfo("Segment distribution:");
                        for (int i = 0; i < testSegmentCount; i++)
                        {
                            float percentage = (segmentCounts[i] / (float)testSelectedSegments.Count) * 100f;
                            AdvLogger.LogInfo($"  Segment {i}: {segmentCounts[i]} spawns ({percentage:F1}%)");
                        }

                        // Calculate alternation quality
                        int alternationCount = 0;
                        for (int i = 1; i < testSelectedSegments.Count; i++)
                        {
                            int diff = Mathf.Abs(testSelectedSegments[i] - testSelectedSegments[i - 1]);
                            if (diff >= testSegmentCount / 2) alternationCount++;
                        }
                        float alternationRate = (alternationCount / (float)(testSelectedSegments.Count - 1)) * 100f;
                        AdvLogger.LogInfo($"Alternation rate (jump >= 4 segments): {alternationRate:F1}%");

                        // Show first 20 spawns to see pattern
                        AdvLogger.LogInfo("First 100 spawn segments:");
                        string first100 = "";
                        for (int i = 0; i < Mathf.Min(100, testSelectedSegments.Count); i++)
                        {
                            first100 += testSelectedSegments[i] + (i < 19 ? " → " : ",");
                        }
                        AdvLogger.LogInfo(first100);
                        AdvLogger.LogInfo("===============================================");
                    }
                    testoffset += 0.25f;
                    if (testoffset <= 2f) {

                        AdvLogger.LogInfo($"Segment test: repeating test with offset: {offset}");
                        goto repeat; 
                    }
                    // ========== TEST END ==========
                spawn:
                    worldSpecificationFactionDesign2 = randomSelection.Select(delegate (WorldSpecificationFactionDesign t)
                    {
                        chanceFactor = 1f;
                        int segment = designSegments[t];
                        int distanceToTarget = Mathf.Abs(segment - targetSegment);

                        float segmentMultiplier = 1f;
                        if (distanceToTarget == 0) segmentMultiplier = 4f;
                        else if (distanceToTarget == 1) segmentMultiplier = 3f;
                        else if (distanceToTarget == 2) segmentMultiplier = 2f;
                        else if (distanceToTarget > 2) segmentMultiplier = 3 / distanceToTarget;

                        float difficultyBias = 1f;
                        if (difficultyFactor < 1f)
                        {
                            difficultyBias = 1f + (1f - difficultyFactor) * (segment / (float)segmentCount);
                        }
                        else if (difficultyFactor > 1f)
                        {
                            difficultyBias = 1f - (difficultyFactor - 1f) * (1f - segment / (float)segmentCount);
                            difficultyBias = Mathf.Max(0.5f, difficultyBias);
                        }

                        if (ProfileManager.Instance.GetModule<AP_MConfig>().waveMode)
                        {
                            if (SpawnWaveMode.encounteredEnemies.Contains(t.Name))
                            {
                                AdvLogger.LogInfo("[UTILS] encounter was among enemy names list: " + t.Name);
                                chanceFactor = 0.1f;
                            }
                            if (SpawnWaveMode.detectedSubmarine)
                            {
                                if (t.BlueprintType == enumBlueprintType.Submarine)
                                {
                                    chanceFactor *= 10;
                                    AdvLogger.LogInfo("[UTILS] increased spawning odds for submarine tenfold! " + t.Name);
                                }
                            }
                        }
                        float num3 = 0.13f * t.AdventureModeChance * chanceFactor * difficultyBias * segmentMultiplier;
                        return (double)num3;
                    });

                    if (worldSpecificationFactionDesign2 != null)
                    {
                        int spawnedSegment = designSegments[worldSpecificationFactionDesign2];
                        SpawnWaveMode.lastSpawnSegments.Insert(0, spawnedSegment);
                        if (SpawnWaveMode.lastSpawnSegments.Count > 5)
                            SpawnWaveMode.lastSpawnSegments.RemoveAt(SpawnWaveMode.lastSpawnSegments.Count - 1);
                        AdvLogger.LogInfo($"[AdventurePatch UTILS] An enemy has been selected. the cost is: {worldSpecificationFactionDesign2.GetBlueprintFile().MaterialCost:F2} with min: {min:F2} and max: {max:F2}. segment {spawnedSegment + 1} of total: {segmentCount}");
                        ObjectId Faction = worldSpecificationFactionDesign2.Id.FactionId;
                        if (ProfileManager.Instance.GetModule<AP_MConfig>().waveMode)
                        {
                            if(SpawnWaveMode.factionToSpawn == null)
                            {
                                SpawnWaveMode.factionToSpawn = worldSpecificationFactionDesign2.Id.FactionId;
                            } else
                            {
                                Faction = SpawnWaveMode.factionToSpawn;
                            }
                            SpawnWaveMode.encounteredEnemies.Add(worldSpecificationFactionDesign2.Name);
                            AdvLogger.LogInfo($"[UTILS] {worldSpecificationFactionDesign2.Name} was added to the Encounters in wave mode.");
                        }
                        float bonusDistance = ProfileManager.Instance.GetModule<AP_MConfig>().SpawnBonusDistance;
                        float minimumSpawnRange = ProfileManager.Instance.GetModule<AP_MConfig>().MinimumSpawnrange;
                        float spawnrange = Mathf.Max(worldSpecificationFactionDesign2.DesiredEngagementRange + bonusDistance, minimumSpawnRange);
                        Quaternion r = InstanceSpecification.i.Adventure.PrimaryForce.C.myTransform.rotation;
                        r = Angles.RemoveRollAndPitch(r);
                        int enemies = GetEnemyCount();
                        int multiplier = Math.Max(10 + enemies * 5, 20);
                        if(worldSpecificationFactionDesign2.CSI.TopSpeed * multiplier > spawnrange)
                        {
                            spawnrange = worldSpecificationFactionDesign2.CSI.TopSpeed * multiplier * 1.5f;
                            spawnrange = Math.Min(spawnrange, 4000);
                            AdvLogger.LogInfo($"[AdventurePatch UTILS] A fast craft has been shifted back, to allow the player some more breathing room. new spawnrange: {spawnrange}");
                        }
                        Vector3d spawnPosition = FindAPointInFrontOfPrimaryForce(spawnrange,out r);
                        spawnPosition.y = (double)worldSpecificationFactionDesign2.GetRandomSpawnPointOffset().y;
                        if (AdventureModeProgression.AdventuringType == AdventureType.Land)
                        {
                            spawnPosition.y += (double)StaticTerrainAltitude.AltitudeForUniversalPosition(spawnPosition);
                            AdvLogger.LogInfo("[AdventurePatch UTILS] Land adventure height offset applied. ");
                        }

                        SpawnInstructions spawnInstructions = SpawnInstructions.IgnoreDamage | SpawnInstructions.Creative;
                        bool isConnected = Net.IsConnected;
                        AdvLogger.LogInfo($"[AdventurePatch UTILS] issuing spawnrequest. factiondesignid: ({worldSpecificationFactionDesign2.Id}), factionid: ({worldSpecificationFactionDesign2.Id.FactionId}), instancename:{worldSpecificationFactionDesign2.Id.FactionId.FactionInst().FactionSpec.Name}");
                        if (isConnected)
                        {
                            SpawnRequest spawnRequest = SpawnRequest.CreatePlanetRequest(
                                worldSpecificationFactionDesign2.Id,
                                spawnPosition,
                                r,
                                Faction,
                                SpawnInstructions.None);
                            spawnRequest.SpawningInstructions = spawnInstructions;
                            spawnRequest.SetSilent(true);
                            SpawnRequestManager.Instance.NewSpawnRequest(spawnRequest);
                        }
                        else
                        {
                            LoadHelper.SpawnAndInitialiseResourcesBlueprint(
                                worldSpecificationFactionDesign2.Id,
                                spawnPosition,
                                r,
                                Faction,
                                new NetworkIdentityId(),
                                spawnInstructions,
                                null,
                                null);
                        }
                    }
               }
           }
       } 
    }
}