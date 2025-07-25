using Newtonsoft.Json;
using System;
using System.IO;
using HarmonyLib;
using BrilliantSkies.Common;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.UniverseRepresentation;
using BrilliantSkies.Ftd.Planets.Instances.Resources;
using BrilliantSkies.Ftd.Planets.World;
using UnityEngine;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Common.StatusChecking;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.PlayerProfiles;
namespace AdventurePatch
{
    

    [HarmonyPatch(typeof(AdventureModeProgression))]
    [HarmonyPatch("Common_SpawnRz")]
    class CommonSpawnRzPatch
    {
        static bool Prefix(ref Vector3d universalPosition, ref float materialAmount)
        {
            //ModSettings settings;
            //settings = ModSettings.Reload();
            //if (!settings.ResourceZoneDiffScaling) { return true; }
            if (ProfileManager.Instance.GetModule<AP_MConfig>().ResourceZoneDiffScaling) { return true; }
            int materialGrowthMin = WorldSpecification.i.AdventureModeSettings.MaterialGrowthMin;
            int materialGrowthMax = WorldSpecification.i.AdventureModeSettings.MaterialGrowthMax;
            int rzradiusMin = WorldSpecification.i.AdventureModeSettings.RZRadiusMin;
            int rzradiusMax = WorldSpecification.i.AdventureModeSettings.RZRadiusMax;
            ResourceZone resourceZone = R_Gameplay.ResourceZone.InstantiateACopy(PlanetList.MainFrame.UniversalPositionToFramePosition(universalPosition), Quaternion.identity);
            resourceZone.MakeAResourceZoneIfWeDontHaveOne();
            uint clamptime = (uint)ProfileManager.Instance.GetModule<AP_MConfig>().ResourceZoneClampedDrainTime;
            float bonusMaterialPerDifficultyLevel = ProfileManager.Instance.GetModule<AP_MConfig>().BonusMaterialPerDifficultyLevel;
            resourceZone.MapResourceZone.Material.ReserveAmount = Mathf.Max(UnityEngine.Random.Range(1f, 1.5f)*(InstanceSpecification.i.Adventure.WarpPlaneDifficulty * bonusMaterialPerDifficultyLevel + 30000), materialAmount);
            //uint clamptime = settings.ResourceZoneClampedDrainTime;
            if (clamptime == 0) clamptime = 1;

            int num = (int)Mathf.Max((float)(resourceZone.MapResourceZone.Material.ReserveAmount / (float)clamptime), Aux.Rnd.Next(materialGrowthMin, materialGrowthMax));
            resourceZone.MapResourceZone.Material.Growth = (float)num;
            int num2 = Aux.Rnd.Next(rzradiusMin, rzradiusMax);
            resourceZone.MapResourceZone.Radius = (float)num2;
            resourceZone.Radius = (float)num2;
            return false;
        }
    }
}
