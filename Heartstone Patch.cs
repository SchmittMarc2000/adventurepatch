using BrilliantSkies.Ftd.Avatar.Items;
using BrilliantSkies.PlayerProfiles;
using Ftd.Ftd.Avatar.Energy;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AdventurePatch
{
    //Blocks energy usage
    [HarmonyPatch(typeof(sItem), nameof(sItem.TryTakeEnergy))]
    public static class Heartstone_Patch
    {
        static bool Prefix(sItem __instance, float energyAmount, ref bool __result)
        {
            if (ProfileManager.Instance.GetModule<AP_MConfig>().IgnoreHeartstone)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    //Allows living without Heartstone
    [HarmonyPatch(typeof(EnergyCalculations), nameof(EnergyCalculations.EnergyLossEquation))]
    public static class Patch_EnergyLossEquation
    {
        static bool Prefix(ref float __result, float energy, bool onVehicle)
        {
            if (ProfileManager.Instance.GetModule<AP_MConfig>().IgnoreHeartstone)
            {
                __result = energy;
                return false;
            }

            return true;
        }
    }
}

