using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.Timing; // Assuming ITimeStep is from here
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
    [HarmonyPatch(typeof(AdventureModeProgression), nameof(AdventureModeProgression.RunAdventureMode))]
    public static class AdventureModeProgression_RunAdventureMode_Patch
    {
        private static float timeSinceLastSpawn = 0f;
        private static float gracePeriodTimer = 1000f;
        private static int spawnSequenceNumber = 0;
        private static int maxEnemySpawns = 10;
        private static float spawnTimeout = 60f;
        private static bool inCooldown = false;
        public static bool gracePeriodTriggered = false;

        public static void updateSettings()
        {
            gracePeriodTimer = ProfileManager.Instance.GetModule<AP_MConfig>().GracePeriod;
            maxEnemySpawns = ProfileManager.Instance.GetModule<AP_MConfig>().maxEnemySpawns;
            spawnTimeout = ProfileManager.Instance.GetModule<AP_MConfig>().SpawnTimeout;
        }
        
        public static void Postfix(ITimeStep dt)
        {
            if(ProfileManager.Instance.GetModule<AP_MConfig>().ForceEnemySpawns)
            {
                timeSinceLastSpawn += dt.DeltaTime;
                if(inCooldown)
                {
                    if(timeSinceLastSpawn > spawnTimeout)
                    {
                        inCooldown = false;
                    }
                    else
                    {
                        return;
                    }
                }
                if (gracePeriodTriggered || timeSinceLastSpawn > gracePeriodTimer || InstanceSpecification.i.Adventure.TimeInWarpPlane > gracePeriodTimer ) //inaccurate, since the player could have travelled through a portal before grace period
                {
                    gracePeriodTriggered = true;
                    if(spawnSequenceNumber == maxEnemySpawns && maxEnemySpawns > 0)
                    {
                        timeSinceLastSpawn = 0f;
                        spawnSequenceNumber = 0;
                        inCooldown = true;
                        return;
                    }
                    if (timeSinceLastSpawn >= ProfileManager.Instance.GetModule<AP_MConfig>().EnemySpawnDelay)
                    {
                        spawnSequenceNumber++;
                        AdvLogger.LogInfo("Spawning a force due to ForceEnemySpawns setting.");
                        timeSinceLastSpawn = 0f;
                        AdventureModeProgression.SpawnAForce();
                    }
                }
            }
            
        }

    }

}
