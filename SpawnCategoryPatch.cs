using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using BrilliantSkies.Ftd.Planets.Factions.Designs;
using BrilliantSkies.FromTheDepths.Planets;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Ftd.Planets.Factions;
using BrilliantSkies.Core.Logger;

namespace AdventurePatch
{
    [HarmonyPatch(typeof(WorldSpecificationFactionDesign), nameof(WorldSpecificationFactionDesign.IsTypeIncludedInAdventure))]
    public static class Patch_IsTypeIncludedInAdventure
    {
        static bool Prefix(WorldSpecificationFactionDesign __instance, AdventureType type, ref bool __result)
        {
            ModSettings settings;
            settings = ModSettings.Reload();
            if (!settings.SpawnFortress) { return true; }

            if (__instance.AdventureModeChance <= 0f || __instance.BlueprintType == enumBlueprintType.Installation)
            {
                float firepower = __instance.TotalAdjustedFirepower;
                if ((__instance.BlueprintType == enumBlueprintType.Installation) && firepower > 10f) {   
                    string name = __instance.Name;
                    int cost = (int)__instance.MaterialCostToBuild();
                    __instance.AdventureModeDifficultyMean = (int)Math.Round(0.01648 * Math.Pow(cost, 0.6242));
                    __instance.AdventureModeDifficultySigma = 3;
                    __instance.AdventureModeChance = 5;
                    //AdvLogger.LogError(string.Format("A unit: \"{0}\" has been added which would previously be excluded. The cost of the unit is: {1} and the difficutly is {2}", name,cost,__instance.AdventureModeDifficultyMean), LogOptions.OnlyInDeveloperLog);
                    __result = true;
                    return false;
                }
                //AdvLogger.LogError(string.Format("A unit: \"{0}\" has not been added:", __instance.Name), LogOptions.OnlyInDeveloperLog);
                __result = false;
                return false;
            }
            return true;
        }
    }
}
