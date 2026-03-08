using BrilliantSkies.Core.Logger;
using BrilliantSkies.Ftd.Planets.Instances;
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
    public static class EnemyDropManager
    {
        public static void ApplyEnemyDropSettings()
        {
            var config = ProfileManager.Instance.GetModule<AP_MConfig>();
            //if (!config.EnemyDropChanges) return;

            float percentage = config.EnemyDropPercentage;
            if (InstanceSpecification.i != null && InstanceSpecification.i.Header != null)
            {
                InstanceSpecification.i.Header.CommonSettings.EnemyBlockDestroyedResourceDrop = percentage * 0.01f;
            }
            else
            {
                AdvLogger.LogInfo("Failed to modify enemy block drop amount");
            }
        }
    }
}