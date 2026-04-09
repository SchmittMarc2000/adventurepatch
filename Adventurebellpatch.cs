using BrilliantSkies.Common.StatusChecking;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ui.Tips;
using HarmonyLib;
using System;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Core.Logger;
namespace AdventurePatch
{
    [HarmonyPatch(typeof(AdventureModeProgression))]
    [HarmonyPatch(nameof(AdventureModeProgression.RequestDelayedSpawn))]
    public static class RequestDelayedSpawnPatch
    {
        static bool Prefix(uint delayInSeconds, ref bool __result)
        {
            AdventureModeProgression.BellRingPeriod = (uint)ProfileManager.Instance.GetModule<AP_MConfig>().AdventureBellDelay;

            uint now = (uint)GameTimer.Instance.GameTime;
            uint last = InstanceSpecification.i.Adventure.LastBellRingTimeInt;

            bool bellCooldownPassed = last == uint.MaxValue || now - last >= AdventureModeProgression.BellRingPeriod;

            if (bellCooldownPassed)
            {
                if (InstanceSpecification.i.Adventure.RequestedSpawnTimeInt == uint.MaxValue)
                {
                    InstanceSpecification.i.Adventure.RequestedSpawnTimeInt = now + delayInSeconds;
                    InstanceSpecification.i.Adventure.LastBellRingTimeInt = now;
                    __result = true;
                    return false;
                }
            }

            __result = false;
            return false; 
        }
    }
    [HarmonyPatch(typeof(AdventureBell))]
    [HarmonyPatch("AppendToolTip")]
    public static class AdventureBell_AppendToolTip_Patch
    {
        public static void Postfix(AdventureBell __instance, ProTip tip) //if we prefix and return false the tooltiptext for what it does goes missing, but not the annoying 1 minute thingy.
        {
            if (ProfileManager.Instance.GetModule<AP_MConfig>().waveMode)
            {
                if(SpawnWaveMode.IsWaveActive)
                {
                    tip.Add<ProTipSegment_TextAdjustable>(
                   new ProTipSegment_TextAdjustable(500,
                   AdventureBell._locFile.Get("Tip_BellDelay", $"Wave is ongoing.", true)),
                   Position.Middle);
                } else
                {
                    tip.Add<ProTipSegment_TextAdjustable>(
                   new ProTipSegment_TextAdjustable(500,
                   AdventureBell._locFile.Get("Tip_BellDelay", $"Activate the bell to start the next wave.", true)),
                   Position.Middle);
                }
            }
            uint num = (uint)GameTimer.Instance.GameTime;
            float deltatime = (num - InstanceSpecification.i.Adventure.LastBellRingTimeInt);
            uint belldelay = (uint)ProfileManager.Instance.GetModule<AP_MConfig>().AdventureBellDelay;
            float remainder = belldelay - deltatime;
            if (deltatime > belldelay | (InstanceSpecification.i.Adventure.LastBellRingTimeInt == uint.MaxValue))
            {
                tip.Add<ProTipSegment_TextAdjustable>(
                   new ProTipSegment_TextAdjustable(500,
                   AdventureBell._locFile.Get("Tip_BellDelay", $"Bell is ready.", true)),
                   Position.Middle);
            }
            else
            {
                tip.Add<ProTipSegment_TextAdjustable>(
                    new ProTipSegment_TextAdjustable(500,
                    AdventureBell._locFile.Get("Tip_BellDelay", $"The remaining Cooldown of the bell is {remainder} seconds.", true)),
                    Position.Middle);
            }
        }
    }
    [HarmonyPatch(typeof(AdventureBell))]
    [HarmonyPatch("CheckStatus")]
    public static class AdventureBell_CheckStatus_Replace
    {
        public static bool Prefix(AdventureBell __instance, IStatusUpdate updater)
        {
            if (ProfileManager.Instance.GetModule<AP_MConfig>().IgnoreAltitude || ProfileManager.Instance.GetModule<AP_MConfig>().waveMode) {
                updater.FlagOkay(__instance);
                return false;
            };

            return true;
        }
    }
    [HarmonyPatch(typeof(AdventureBell))]
    [HarmonyPatch("RegisterSpawnRequest")]
    public static class AdventureBell_RegisterSpawnRequest_Patch
    {
        static bool Prefix(AdventureBell __instance, ref bool __result)
        {
            if(ProfileManager.Instance.GetModule<AP_MConfig>().IgnoreAltitude || ProfileManager.Instance.GetModule<AP_MConfig>().waveMode) // skips altitude check
            {
                if(!ProfileManager.Instance.GetModule<AP_MConfig>().waveMode) { 
                    __result = AdventureModeProgression.RequestDelayedSpawn(5U);
                    return false;
                }
                SpawnWaveMode.StartWaveMode();
                return false;
            }
            return true;
        }
    }
}