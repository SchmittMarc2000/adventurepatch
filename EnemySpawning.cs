using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrilliantSkies.Core.Timing; // Assuming ITimeStep is from here
using BrilliantSkies.Core.Logger;
using HarmonyLib;
using BrilliantSkies.PlayerProfiles;
namespace AdventurePatch
{
    [HarmonyPatch(typeof(AdventureModeProgression), nameof(AdventureModeProgression.RunAdventureMode))]
    public static class AdventureModeProgression_RunAdventureMode_Patch
    {
        private static float timeSinceLastSpawn = 0f;

        public static void Postfix(ITimeStep dt)
        {
            timeSinceLastSpawn += dt.DeltaTime;
            if (ProfileManager.Instance.GetModule<AP_MConfig>().ForceEnemySpawns && timeSinceLastSpawn >= ProfileManager.Instance.GetModule<AP_MConfig>().EnemySpawnDelay)
            {
                AdvLogger.LogInfo("Spawning a force due to ForceEnemySpawns setting.");
                timeSinceLastSpawn = 0f;
                AdventureModeProgression.SpawnAForce();
            }
        }

    }

}
