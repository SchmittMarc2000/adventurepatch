using BrilliantSkies.Ftd.Avatar.Build;
using BrilliantSkies.PlayerProfiles;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventurePatch
{
    [HarmonyPatch(typeof(cBuild), nameof(cBuild.ToggleFreeze))]
    public static class Patch_ToggleFreeze
    {
        static bool Prefix(cBuild __instance)
        {
            if (!ProfileManager.Instance.GetModule<AP_MConfig>().AllowFreeze)
                return true;

            if (__instance.C == null)
                return false;

            MainConstruct main = __instance.C.Main;

            if (main.GetConstructableType() == enumConstructableTypes.vehicle)
            {
                if (main.State == enumConstructableState.frozen)
                {
                    ConstructableChangeSync.Instance.FreezeVehicle(main, false);
                }
                else if (main.State == enumConstructableState.normal)
                {
                    ConstructableChangeSync.Instance.FreezeVehicle(main, true);
                }
            }

            return false;
        }
    }

}
