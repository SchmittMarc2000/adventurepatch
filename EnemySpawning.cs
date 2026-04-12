using BrilliantSkies.Core;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.Networking;
using BrilliantSkies.Core.Timing; // Assuming ITimeStep is from here
using BrilliantSkies.Ftd.AchievementsAndStats;
using BrilliantSkies.Ftd.Multiplayer.NetworkCommunication;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.PlayerProfiles;
using HarmonyLib;
using NetInfrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace AdventurePatch
{
    [HarmonyPatch(typeof(AdventureModeProgression), nameof(AdventureModeProgression.RunAdventureMode))]
    public static class AdventureModeProgression_RunAdventureMode_Patch
    {
        private static float timeSinceLastSpawn = 0f;
        private static float gracePeriodTimer = 1000f;
        public static bool gracePeriodTriggered = false;
        public static float timeIncrease = 0f;
        public static void updateSettings()
        {
            gracePeriodTimer = ProfileManager.Instance.GetModule<AP_MConfig>().GracePeriod;
        }
        
        public static void Postfix(ITimeStep dt)
        {
            if(ProfileManager.Instance.GetModule<AP_MConfig>().ForceEnemySpawns && !Net.IsClient)
            {
                timeSinceLastSpawn += dt.DeltaTime;
                if (gracePeriodTriggered || timeSinceLastSpawn > gracePeriodTimer || InstanceSpecification.i.Adventure.TimeInWarpPlane > gracePeriodTimer ) //inaccurate, since the player could have travelled through a portal before grace period TODO: More advanced checks
                { 
                    gracePeriodTriggered = true;
                    if (timeSinceLastSpawn >= ProfileManager.Instance.GetModule<AP_MConfig>().EnemySpawnDelay)
                    {
                        AdvLogger.LogInfo("Spawning a force due to ForceEnemySpawns setting.");
                        timeSinceLastSpawn = 0f;
                        AdventureModeProgression.SpawnAForce();
                    }
                }
            }
            
        }
        public static bool Prefix(ITimeStep dt)
        {
            CustomBindingManager.Update();
            if (ProfileManager.Instance.GetModule<AP_MConfig>().waveMode && !Net.IsClient) {
                InstanceSpecification.i.Adventure.TimeInWarpPlane += dt.DeltaTime;
                if(InstanceSpecification.i.Adventure.WarpPlaneDifficulty < 5 && InstanceSpecification.i.Adventure.TimeInWarpPlane > 0.5f)
                {
                    NetworkedInfoStore.Add($"Press {ProfileManager.Instance.GetModule<AP_MConfig>().StartWaveKey} to start the wave, and {ProfileManager.Instance.GetModule<AP_MConfig>().StopWaveKey} to stop the wave.");
                    AdventurePatchUtils.TakeRedPortal();
                }
                return false;
            }
            if (ProfileManager.Instance.GetModule<AP_MConfig>().autoIncreaseDifficulty && !Net.IsClient)
            {
                if (InstanceSpecification.i.Adventure.TimeInWarpPlane > ProfileManager.Instance.GetModule<AP_MConfig>().autoIncreaseTime)
                {
                    if (gracePeriodTriggered || timeSinceLastSpawn > gracePeriodTimer || InstanceSpecification.i.Adventure.TimeInWarpPlane > gracePeriodTimer || InstanceSpecification.i.Adventure.WarpPlaneDifficulty > 1)
                    {
                        gracePeriodTriggered = true;
                        if (Net.IsServer || Net.IsConnected)
                        {
                            if (AdventurePatchUtils.GetEnemyCount() >= 1 && timeIncrease < 30)
                            {
                                timeIncrease += 30;
                                ProfileManager.Instance.GetModule<AP_MConfig>().autoIncreaseTime += 30;
                                AdvLogger.LogInfo($"The warp has been delayed due to the presence of an enemy. starting {timeIncrease} of 60s overtime");
                                return true;
                            }
                            NetworkedInfoStore.Add("difficulty increased by 5", 15);
                            AdventureModeProgression_Common_DoWarp_Patch.shouldRecordAndRestore = true;
                            InstantComs.GetSingleton().SendRpc(new RpcRequest(delegate (INetworkIdentity n)
                            {
                                ServerOutgoingRpcs.ChangeWarpPlane(n, WarpGateType.Harder, InstanceSpecification.i.Adventure.PrimaryForceUniversePosition);
                            }));
                            AdventureModeProgression.Common_DoWarp(WarpGateType.Harder, InstanceSpecification.i.Adventure.PrimaryForceUniversePosition);
                            ProfileManager.Instance.GetModule<AP_MConfig>().autoIncreaseTime -= timeIncrease;
                            timeIncrease = 0;
                        }
                        else
                        {
                            NetworkedInfoStore.Add("difficulty increased by 5", 15);
                            float newDifficulty = InstanceSpecification.i.Adventure.WarpPlaneDifficulty;
                            newDifficulty = Rounding.RoundToNearestMultipleOf(Mathf.Max(newDifficulty, 0f) + 5f, 5f);
                            newDifficulty = Mathf.Clamp(newDifficulty, 0.1f, 100f);
                            InstanceSpecification.i.Adventure.WarpPlaneDifficulty = newDifficulty;
                            InstanceSpecification.i.Adventure.TimeInWarpPlane = 0;
                        }
                    }
                }

            }
            return true;
        }

    }

}
