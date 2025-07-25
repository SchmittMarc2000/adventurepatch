using Assets.Scripts;
using BrilliantSkies.Core;
using BrilliantSkies.Core.Control.Tuning;
using BrilliantSkies.Core.Help;
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
using BrilliantSkies.Ui.Tips;
using HarmonyLib;
using NetInfrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;
using BrilliantSkies.PlayerProfiles;

namespace AdventurePatch
{

    [HarmonyPatch(typeof(AdventureModeProgression))]
    [HarmonyPatch("SpawnForce")]
    class SpawnForcePatch
    {
        public static Vector3d FindAPointInFrontOfPrimaryForce(float range)
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
            return vector3d;
        }
        static bool Prefix(ref Vector3d universePosition, Quaternion r)
        {

            //ModSettings settings;
            //settings = ModSettings.Reload();
            //if (!settings.EnemySpawnDistancePatch) { return true; }
            if(!ProfileManager.Instance.GetModule<AP_MConfig>().EnemySpawnDistancePatch) { return true; }
            int num = 0;
            float num2 = 0f;


            for (int i = 0; i < StaticConstructablesManager.Constructables.Count; i++)
            {
                MainConstruct mainConstruct = StaticConstructablesManager.Constructables[i];
                bool flag = mainConstruct != null && mainConstruct.GetTeam() != GAME_STATE.MyTeam;
                if (flag)
                {
                    num++;
                    num2 += mainConstruct.AllBasics.VolumeAliveUsed;
                }
            }

            bool flag2 = num >= 10;
            if (!flag2)
            {
                bool flag3 = num2 > 30000f;
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
                                randomSelection.Add(worldSpecificationFactionDesign);
                                //if(worldSpecificationFactionDesign.AdventureModeChance>1) AdvLogger.LogError(string.Format("A unit \"{0}\" has been added. the difficutly is {1}", worldSpecificationFactionDesign.Name, worldSpecificationFactionDesign.AdventureModeDifficultyMean), LogOptions.OnlyInDeveloperLog);
                            }
                        }
                    }

                    WorldSpecificationFactionDesign worldSpecificationFactionDesign2 = randomSelection.Select(delegate (WorldSpecificationFactionDesign t)
                    {
                        float num3 = Maths.NormalProbability(InstanceSpecification.i.Adventure.WarpPlaneDifficulty, t.AdventureModeDifficultyMean, t.AdventureModeDifficultySigma) * t.AdventureModeChance;
                        return (double)num3;
                    });

                    bool flag6 = worldSpecificationFactionDesign2 != null;
                    if (flag6)
                    {
                        //AdvLogger.LogError(string.Format("A unit \"{0}\" has been chosen, and the difficutly is {1}", worldSpecificationFactionDesign2.Name, worldSpecificationFactionDesign2.AdventureModeDifficultyMean), LogOptions.OnlyInDeveloperLog);
                        
                        //float spawnrange = Mathf.Max(worldSpecificationFactionDesign2.DesiredEngagementRange + settings.SpawnBonusDistance, settings.MinimumSpawnrange);
                        float bonusDistance = ProfileManager.Instance.GetModule<AP_MConfig>().SpawnBonusDistance;
                        float minimumSpawnRange = ProfileManager.Instance.GetModule<AP_MConfig>().MinimumSpawnrange;
                        float spawnrange = Mathf.Max(worldSpecificationFactionDesign2.DesiredEngagementRange + bonusDistance, minimumSpawnRange);
                        universePosition = SpawnForcePatch.FindAPointInFrontOfPrimaryForce(spawnrange);

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
            return false;
        }
    }
}
