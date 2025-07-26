using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.PlayerProfiles;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AdventurePatch
{
    [HarmonyPatch]
    public static class Patch_AdventureModeProgression_PotentiallySpawnForce
    {
        // Target the correct method
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AdventureModeProgression), "PotentiallySpawnForce")]
        public static bool Prefix_BlockRandomSpawns()
        {
            var config = ProfileManager.Instance.GetModule<AP_MConfig>();
            if (config != null && config.BlockRandomSpawns)
            {
                uint gametime = (uint)GameTimer.Instance.GameTime;
                if (gametime > InstanceSpecification.i.Adventure.RequestedSpawnTimeInt)     //bell requested a spawn, we let it through.
                {
                    InstanceSpecification.i.Adventure.RequestedSpawnTimeInt = uint.MaxValue;
                    AdventureModeProgression.SpawnAForce();
                } 
                else
                {
                    AdvLogger.LogInfo("PotentiallySpawnForce blocked due to BlockRandomSpawns setting.");
                }

                return false; // Skip original method
            }

            return true; // Continue with original method
        }
    }




}
