using Newtonsoft.Json;
using System;
using System.IO;

namespace AdventurePatch
{
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

    [HarmonyPatch(typeof(AdventureModeProgression))]
    [HarmonyPatch("Common_SpawnRz")]
    class CommonSpawnRzPatch
    {
        static bool Prefix(ref Vector3d universalPosition, ref float materialAmount)
        {
            ModSettings settings;
            settings = ModSettings.Reload();
            if (!settings.ResourceZoneDiffScaling) { return true; }
            int materialGrowthMin = WorldSpecification.i.AdventureModeSettings.MaterialGrowthMin;
            int materialGrowthMax = WorldSpecification.i.AdventureModeSettings.MaterialGrowthMax;
            int rzradiusMin = WorldSpecification.i.AdventureModeSettings.RZRadiusMin;
            int rzradiusMax = WorldSpecification.i.AdventureModeSettings.RZRadiusMax;
            ResourceZone resourceZone = R_Gameplay.ResourceZone.InstantiateACopy(PlanetList.MainFrame.UniversalPositionToFramePosition(universalPosition), Quaternion.identity);
            resourceZone.MakeAResourceZoneIfWeDontHaveOne();
            resourceZone.MapResourceZone.Material.ReserveAmount = Mathf.Max(UnityEngine.Random.Range(1f, 2f)*(InstanceSpecification.i.Adventure.WarpPlaneDifficulty * settings.BonusMaterialPerDifficultyLevel + 30000), materialAmount);
            int num = (int)Mathf.Max((float)(resourceZone.MapResourceZone.Material.ReserveAmount / (float)settings.ResourceZoneClampedDrainTime), Aux.Rnd.Next(materialGrowthMin, materialGrowthMax)); ;
            resourceZone.MapResourceZone.Material.Growth = (float)num;
            int num2 = Aux.Rnd.Next(rzradiusMin, rzradiusMax);
            resourceZone.MapResourceZone.Radius = (float)num2;
            resourceZone.Radius = (float)num2;
            return false;
        }
    }
}
