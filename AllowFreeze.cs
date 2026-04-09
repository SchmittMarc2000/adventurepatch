using BrilliantSkies.Core.Logger;
using BrilliantSkies.Ftd.Avatar.Build;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ftd.Planets.World.Distances;
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
            if (__instance.C == null)
                return false;
            if(InstanceSpecification.i.Header.IsDesignerOrCustomBattle) return true;

            MainConstruct main = __instance.C.Main;

            UnityEngine.Vector3 maincraftposition = main.GetPositionForForce();

            if (main.State == enumConstructableState.frozen)                
            {
                ConstructableChangeSync.Instance.FreezeVehicle(main, false);                //always allow unfreezing
                return false;
            }

            if (!ProfileManager.Instance.GetModule<AP_MConfig>().AllowFreeze)
                return true;

            if (main.GetConstructableType() == enumConstructableTypes.vehicle)
            {
                if (main.State == enumConstructableState.normal)
                {
                    //check if an enemy is within 2000m
                    int vehiclecount = StaticConstructablesManager.Constructables.Count;
                    for (int i = 0; i < vehiclecount; i++)
                    {
                        if (StaticConstructablesManager.Constructables[i].GetTeam() != GAME_STATE.MyTeam)
                        {   
                            MainConstruct vehicle = StaticConstructablesManager.Constructables[i];
                            UnityEngine.Vector3 position = vehicle.GetPositionForForce();

                            float distance = (maincraftposition - position).magnitude;
                            if (distance < 2000)
                            {
                                AdvLogger.LogInfo("Tried locking while enemy is nearby");
                                return false;
                            }
                        }
                    }
                    ConstructableChangeSync.Instance.FreezeVehicle(main, true);
                }
            }

            return false;
        }
    }

}
