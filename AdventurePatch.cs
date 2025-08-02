using Assets.Scripts;
using BrilliantSkies.Core;
using BrilliantSkies.Core.Control.Tuning;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Id;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Core.Widgets;
using BrilliantSkies.FromTheDepths.Planets;
using BrilliantSkies.Ftd.Multiplayer.Requests;
using BrilliantSkies.Ftd.Planets.Factions;
using BrilliantSkies.Ftd.Planets.Factions.Designs;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ftd.Planets.Instances.Factions;
using BrilliantSkies.Ftd.Terrain;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Ui.Tips;
using HarmonyLib;
using NetInfrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace AdventurePatch
{

    [HarmonyPatch(typeof(AdventureModeProgression))]
    [HarmonyPatch("SpawnForce")]
    class SpawnForcePatch
    {
        public static Vector3d FindAPointInFrontOfPrimaryForce(float range, out Quaternion rotation)
        {
            float y = EngineSplines.Instance.AdventureAngleCurve.Evaluate(Random.Range(-1f, 1f));
            Quaternion quaternion = InstanceSpecification.i.Adventure.PrimaryForce.C.myTransform.rotation;
            quaternion = Angles.RemoveRollAndPitch(quaternion);
            Vector3d primaryForceUniversePosition = InstanceSpecification.i.Adventure.PrimaryForceUniversePosition;
            Vector3d vector3d = InstanceSpecification.i.Adventure.PrimaryForceUniversePosition + Quaternion.Euler(0f, y, 0f) * quaternion * new Vector3(0f, 0f, range);

            bool flag = AdventureModeProgression.AdventuringType == AdventureType.Land;
            if (flag)
            {
                vector3d.y = (double)StaticTerrainAltitude.AltitudeForUniversalPosition(vector3d);
            }
            else
            {
                vector3d.y = 0.0;
            }
            Vector3 forward = (primaryForceUniversePosition - vector3d).ToSingle();
            forward.y = 0f;
            rotation = Quaternion.LookRotation(forward, Vector3.up);
            return vector3d;
        }
        static bool Prefix(ref Vector3d universePosition, Quaternion r)
        {
            //ModSettings settings;
            //settings = ModSettings.Reload();
            //if (!settings.EnemySpawnDistancePatch) { return true; }
            if(!ProfileManager.Instance.GetModule<AP_MConfig>().EnemySpawnDistancePatch) { return true; }
            int enemycount = 0;
            float enemyvolume = 0f;


            for (int i = 0; i < StaticConstructablesManager.Constructables.Count; i++)
            {
                MainConstruct mainConstruct = StaticConstructablesManager.Constructables[i];
                bool flag = mainConstruct != null && mainConstruct.GetTeam() != GAME_STATE.MyTeam;
                if (flag)
                {
                    enemycount++;
                    enemyvolume += mainConstruct.AllBasics.VolumeAliveUsed;
                }
            }
            uint maxenemycount = ProfileManager.Instance.GetModule<AP_MConfig>().MaxEnemyCount;
            bool flag2 = enemycount >= (int)maxenemycount;
            if (!flag2)
            {
                uint maxenemyvolume = ProfileManager.Instance.GetModule<AP_MConfig>().MaxEnemyVolume;
                bool flag3 = enemyvolume > (float)maxenemyvolume;
                if (!flag3)
                {
                    RandomSelection<WorldSpecificationFactionDesign> randomSelection = new RandomSelection<WorldSpecificationFactionDesign>();
                    IEnumerable<WorldSpecificationFactionDesign> enumerable = (from t in FactionSpecifications.i.Factions
                                                                               where t.InstanceOfFaction.eController == FactionController.AI_General
                                                                               select t).SelectMany((FactionSpecificationFaction t) => t.Designs.Designs);

                    foreach (WorldSpecificationFactionDesign worldSpecificationFactionDesign in enumerable)
                    {
                        bool flag4 = worldSpecificationFactionDesign != null;
                        if (flag4)
                        {

                            bool flag5 = worldSpecificationFactionDesign.IsTypeIncludedInAdventure(AdventureModeProgression.AdventuringType);
                            if (flag5)
                            {
                                //AdvLogger.LogInfo("Enemy name: " + worldSpecificationFactionDesign.Name +
                                //  " blueprinttype: " + worldSpecificationFactionDesign.BlueprintType +
                                //  " difficulty: " + worldSpecificationFactionDesign.AdventureModeDifficultyMean);
                                randomSelection.Add(worldSpecificationFactionDesign);
                            }
                        }
                    }
                    CustomEncounter encounter = null;
                    int enemies = 1;
                    float difficultyfactor = 1;
                    AdvLogger.LogInfo("customencounterspawnchance:");
                    bool isCustomEncounter = UnityEngine.Random.Range(0f, 1f) > (1 - (float)ProfileManager.Instance.GetModule<AP_MConfig>().CustomEncounterSpawnChance / 100) && ProfileManager.Instance.GetModule<AP_MConfig>().EnableCustomEncounters;
                    float dropoff = 0;
                    FactionObjectId faction = null;
                    List<String> enemynames = new List<String>();
                    if (isCustomEncounter)
                    {
                        encounter = CustomEncounterDatabase.SelectRandomEncounter(InstanceSpecification.i.Adventure.WarpPlaneDifficulty);
                        difficultyfactor = encounter.PrimaryDifficultyFactor;
                        dropoff = encounter.DifficultyFactorDropoff;
                        enemies = encounter.Count;
                    }
                    for (int i = 0; i < enemies; i++)
                    {
                        WorldSpecificationFactionDesign worldSpecificationFactionDesign2 = null;
                        if (!isCustomEncounter || encounter == null)
                        {
                            worldSpecificationFactionDesign2 = randomSelection.Select(delegate (WorldSpecificationFactionDesign t)
                            {
                                float difficulty = InstanceSpecification.i.Adventure.WarpPlaneDifficulty;
                                if (ProfileManager.Instance.GetModule<AP_MConfig>().OverrideSpawnDifficulty) difficulty = ProfileManager.Instance.GetModule<AP_MConfig>().SpawnDifficulty;
                                float num3 = Maths.NormalProbability(difficulty, t.AdventureModeDifficultyMean, t.AdventureModeDifficultySigma) * t.AdventureModeChance;
                                return (double)num3;
                            });
                        }
                        else
                        {
                            EnemyType enemytype = encounter.Enemies[i].Type;
                            worldSpecificationFactionDesign2 = randomSelection.Select(delegate (WorldSpecificationFactionDesign t)
                            {
                                if (encounter.Enemies[i].Name != "not set")
                                {
                                    if (!t.Name.Contains(encounter.Enemies[i].Name)) return 0.0f; // use substring match instead of exact match
                                }
                                if (faction != null && (t.CorporationID != faction && encounter.ForceSameFaction == true) || (enemynames.Contains(t.Name) && encounter.FilterDuplicates)) {
                                    //AdvLogger.LogInfo("did not spawn enemy due to wrong faction or duplicate: " + t.Name + " faction: " + t.CorporationID.ToString());
                                    return 0.0f;
                                    } //filter out enemies from other factions and duplicates unless wanted
                                if (!CustomEncounter.ShouldSpawn(enemytype, t)) {
                                    //AdvLogger.LogInfo("did not spawn enemy due to wrong type: " + t.Name + " faction: " + t.CorporationID.ToString());
                                    return 0.0f;
                                    };
                                
                                float difficulty = InstanceSpecification.i.Adventure.WarpPlaneDifficulty;
                                difficulty = Math.Min(100, Math.Max(1, difficultyfactor * difficulty));
                                if (Math.Abs(difficulty - t.AdventureModeDifficultyMean) > 15f && encounter.MinDifficutly == 0) return 0.0f;
                                AdvLogger.LogInfo("Added enemy to selection: " + t.Name + " difficulty: " + t.AdventureModeDifficultyMean + " type: " + t.BlueprintType.ToString() + " faction: " + t.CorporationID.ToString());
                                float num3 = Maths.NormalProbability(difficulty, t.AdventureModeDifficultyMean, t.AdventureModeDifficultySigma) * t.AdventureModeChance;
                                return (double)num3;
                            });
                        }
                        difficultyfactor -= dropoff;

                        bool flag6 = worldSpecificationFactionDesign2 != null;
                        if (flag6)
                        {
                            enemynames.Add(worldSpecificationFactionDesign2.Name);
                            faction = worldSpecificationFactionDesign2.CorporationID;
                            AdvLogger.LogInfo($"Spawning enemy {i+1} of {enemies}" + " Enemy name: " + enemynames[i]+ " enemy faction: " + faction.ToString() + " blueprinttype: " + worldSpecificationFactionDesign2.BlueprintType + " difficulty: " + worldSpecificationFactionDesign2.AdventureModeDifficultyMean);
                            
                            //AdvLogger.LogError(string.Format("A unit \"{0}\" has been chosen, and the difficutly is {1}", worldSpecificationFactionDesign2.Name, worldSpecificationFactionDesign2.AdventureModeDifficultyMean), LogOptions.OnlyInDeveloperLog);

                            //float spawnrange = Mathf.Max(worldSpecificationFactionDesign2.DesiredEngagementRange + settings.SpawnBonusDistance, settings.MinimumSpawnrange);
                            float bonusDistance = ProfileManager.Instance.GetModule<AP_MConfig>().SpawnBonusDistance;
                            float minimumSpawnRange = ProfileManager.Instance.GetModule<AP_MConfig>().MinimumSpawnrange;
                            float spawnrange = Mathf.Max(worldSpecificationFactionDesign2.DesiredEngagementRange + bonusDistance, minimumSpawnRange);
                            universePosition = SpawnForcePatch.FindAPointInFrontOfPrimaryForce(spawnrange,out r);

                            universePosition.y = (double)worldSpecificationFactionDesign2.GetRandomSpawnPointOffset().y;



                            bool flag7 = AdventureModeProgression.AdventuringType == AdventureType.Land;
                            if (flag7)
                            {
                                universePosition.y += (double)StaticTerrainAltitude.AltitudeForUniversalPosition(universePosition);
                            }

                            SpawnInstructions spawnInstructions = SpawnInstructions.IgnoreDamage | SpawnInstructions.Creative;
                            bool isConnected = Net.IsConnected;
                            if (isConnected)
                            {
                                SpawnRequest spawnRequest = SpawnRequest.CreatePlanetRequest(worldSpecificationFactionDesign2.Id, universePosition, r, worldSpecificationFactionDesign2.Id.FactionId, SpawnInstructions.None);
                                spawnRequest.SpawningInstructions = spawnInstructions;
                                spawnRequest.SetSilent(true);
                                SpawnRequestManager.Instance.NewSpawnRequest(spawnRequest);
                            }
                            else
                            {
                                LoadHelper.SpawnAndInitialiseResourcesBlueprint(worldSpecificationFactionDesign2.Id, universePosition, r, worldSpecificationFactionDesign2.Id.FactionId, new NetworkIdentityId(), spawnInstructions, null, null);
                            }
                        }
                    
                    }
                }
            }
            return false;
        }
    }
}
