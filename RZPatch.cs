using BrilliantSkies.Common;
using BrilliantSkies.Common.StatusChecking;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Core.UniverseRepresentation;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ftd.Planets.Instances.Resources;
using BrilliantSkies.Ftd.Planets.World;
using BrilliantSkies.PlayerProfiles;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;
using NetInfrastructure;

using BrilliantSkies.Ftd.Multiplayer.NetworkCommunication;
using BrilliantSkies.Ftd.Multiplayer.Requests;

namespace AdventurePatch
{
    using BrilliantSkies.Core;
    using BrilliantSkies.Core.Networking;
    using BrilliantSkies.FromTheDepths.Planets;
    using BrilliantSkies.Ftd.Terrain;
    //[HarmonyPatch(typeof(AdventureModeProgression))]
    //[HarmonyPatch("Common_SpawnRz")]
    //class CommonSpawnRzPatch
    //{
    //    static bool Prefix(ref Vector3d universalPosition, ref float materialAmount)
    //    {
    //        AdvLogger.LogInfo("prefix for commonspawnrzpatch called.");
    //        if (ProfileManager.Instance.GetModule<AP_MConfig>().ResourceZoneBaseMaterial == 0)
    //        {
    //            AdvLogger.LogInfo("ResourceZoneBaseMaterial is set to 0, skipping resource zone creation.");
    //            return false;
    //        }
    //        if (!ProfileManager.Instance.GetModule<AP_MConfig>().ResourceZoneDiffScaling)
    //        {
    //            return true;
    //        }
    //        int materialGrowthMin = WorldSpecification.i.AdventureModeSettings.MaterialGrowthMin;
    //        int materialGrowthMax = WorldSpecification.i.AdventureModeSettings.MaterialGrowthMax;
    //        int rzradiusMin = WorldSpecification.i.AdventureModeSettings.RZRadiusMin;
    //        int rzradiusMax = WorldSpecification.i.AdventureModeSettings.RZRadiusMax;
    //        ResourceZone resourceZone = R_Gameplay.ResourceZone.InstantiateACopy(PlanetList.MainFrame.UniversalPositionToFramePosition(universalPosition), Quaternion.identity);
    //        resourceZone.MakeAResourceZoneIfWeDontHaveOne();
    //        uint clamptime = (uint)ProfileManager.Instance.GetModule<AP_MConfig>().ResourceZoneClampedDrainTime;
    //        float bonusMaterialPerDifficultyLevel = ProfileManager.Instance.GetModule<AP_MConfig>().BonusMaterialPerDifficultyLevel;
    //        resourceZone.MapResourceZone.Material.ReserveAmount = Mathf.Max(UnityEngine.Random.Range(1f, 1.25f) * (InstanceSpecification.i.Adventure.WarpPlaneDifficulty * bonusMaterialPerDifficultyLevel + ProfileManager.Instance.GetModule<AP_MConfig>().ResourceZoneBaseMaterial), materialAmount);
    //        if (clamptime == 0) clamptime = 1;
    //        int num = (int)Mathf.Max((float)(resourceZone.MapResourceZone.Material.ReserveAmount / (float)clamptime), Aux.Rnd.Next(materialGrowthMin, materialGrowthMax));
    //        resourceZone.MapResourceZone.Material.Growth = (float)num;
    //        int num2 = Aux.Rnd.Next(rzradiusMin, rzradiusMax);
    //        resourceZone.MapResourceZone.Radius = (float)num2;
    //        resourceZone.Radius = (float)num2;
    //        return false;
    //    }
    //} OLD PATCH

    using HarmonyLib;
    using NetInfrastructure;
    using System.Reflection;
    using UnityEngine;

    [HarmonyPatch(typeof(AdventureModeProgression))]
    [HarmonyPatch("SpawnAnRz")]
    class SpawnAnRzPatch
    {
        static bool Prefix()
        {
            AdvLogger.LogInfo("Prefix for SpawnAnRzPatch called.");

            var config = ProfileManager.Instance.GetModule<AP_MConfig>();
            if (config.ResourceZoneBaseMaterial == 0)
            {
                AdvLogger.LogInfo("ResourceZoneBaseMaterial is set to 0, skipping resource zone creation.");
                return false;
            }

            // --- Reflection call for private static method FindAPointInFrontOfPrimaryForce ---
            MethodInfo findPointMethod = AccessTools.Method(typeof(AdventureModeProgression), "FindAPointInFrontOfPrimaryForce");
            if (findPointMethod == null)
            {
                AdvLogger.LogInfo("Failed to find method: FindAPointInFrontOfPrimaryForce");
                return false;
            }

            object[] parameters = new object[]
            {
            AdventureModeProgression.EnemySpawnDistance(),
            null // placeholder for 'out Quaternion quaternion'
            };

            Vector3d universePosition = (Vector3d)findPointMethod.Invoke(null, parameters);
            Quaternion quaternion = (Quaternion)parameters[1];
            // -------------------------------------------------------------

            // Adjust position
            universePosition.y += 50.0;
            bool isWater = AdventureModeProgression.AdventuringType == AdventureType.Water;
            if (isWater)
            {
                universePosition.y = 50.0;
            }
            else
            {
                universePosition.y = StaticTerrainAltitude.AltitudeForUniversalPosition(universePosition) + 100.0;
            }

            // Retrieve settings
            int materialAmountMin = WorldSpecification.i.AdventureModeSettings.MaterialAmountMin;
            int materialAmountMax = WorldSpecification.i.AdventureModeSettings.MaterialAmountMax;
            int materialGrowthMin = WorldSpecification.i.AdventureModeSettings.MaterialGrowthMin;
            int materialGrowthMax = WorldSpecification.i.AdventureModeSettings.MaterialGrowthMax;
            int rzradiusMin = WorldSpecification.i.AdventureModeSettings.RZRadiusMin;
            int rzradiusMax = WorldSpecification.i.AdventureModeSettings.RZRadiusMax;
            int radiusSize = Aux.Rnd.Next(rzradiusMin, rzradiusMax);
            float materialAmount = Aux.Rnd.Next(materialAmountMin, materialAmountMax);

            // Apply difficulty-based scaling
            if (config.ResourceZoneDiffScaling)
            {
                float bonusMaterial = config.BonusMaterialPerDifficultyLevel;
                float baseMat = config.ResourceZoneBaseMaterial;
                materialAmount = Mathf.Max(
                    UnityEngine.Random.Range(1f, 1.25f) *
                    (InstanceSpecification.i.Adventure.WarpPlaneDifficulty * bonusMaterial + baseMat),
                    materialAmount
                );
            }

            uint clamptime = (uint)config.ResourceZoneClampedDrainTime;
            if (clamptime == 0) clamptime = 1;

            int growthSize = (int)Mathf.Max((float)(materialAmount / (float)clamptime), Aux.Rnd.Next(materialGrowthMin, materialGrowthMax));

            if (Net.IsServer)
            {
                Coms.AddRpc(new RpcRequest(delegate (INetworkIdentity n)
                {
                    ServerOutgoingRpcs.SpawnResourceZoneAdventure(n, universePosition, materialAmount, growthSize, radiusSize);
                }));
            }

            AdventureModeProgression.Common_SpawnRz(universePosition, materialAmount, growthSize, radiusSize);

            // Skip original method entirely
            return false;
        }
    }


}
