using BrilliantSkies.Ftd.Modes.Designer;
using BrilliantSkies.Ftd.Planets.Instances.Headers;
using BrilliantSkies.PlayerProfiles;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventurePatch
{
    [HarmonyPatch(typeof(EnemySpawnUi), nameof(EnemySpawnUi.EnabledInThisMode))]
    public static class Patch_EnabledInThisMode
    {
        static bool Prefix(InstanceHeader m, ref bool __result)
        {
            var config = ProfileManager.Instance.GetModule<AP_MConfig>();
            if (config.AllowEnemySpawnUI)
            {
                __result = true;
                return false; 
            }

            return true;
        }
    }
}