using BrilliantSkies.Common.StatusChecking;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ui.Tips;
using HarmonyLib;
using System;

namespace AdventurePatch
{
    [HarmonyPatch(typeof(AdventureModeProgression))]
    [HarmonyPatch("RequestDelayedSpawn")]
    class RequestDelayedSpawnPatch
    {
        static bool Prefix(ref uint delayInSeconds, ref bool __result)
        {
            ModSettings settings;
            settings = ModSettings.Reload();
            AdventureModeProgression.BellRingPeriod = settings.AdventureBellDelay;

            uint num = (uint)GameTimer.Instance.GameTime;
            bool flag = InstanceSpecification.i.Adventure.LastBellRingTimeInt == uint.MaxValue || (long)(num - InstanceSpecification.i.Adventure.LastBellRingTimeInt) >= (long)((ulong)AdventureModeProgression.BellRingPeriod);
            if (flag)
            {
                uint requestedSpawnTimeInt = InstanceSpecification.i.Adventure.RequestedSpawnTimeInt;
                bool flag2 = requestedSpawnTimeInt == uint.MaxValue;
                if (flag2)
                {
                    InstanceSpecification.i.Adventure.RequestedSpawnTimeInt = num + delayInSeconds;
                    InstanceSpecification.i.Adventure.LastBellRingTimeInt = num;
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
        public static void Postfix(AdventureBell __instance, ProTip tip)
        {
            ModSettings settings;
            settings = ModSettings.Reload();
            uint num = (uint)GameTimer.Instance.GameTime;
            float deltatime = (num - InstanceSpecification.i.Adventure.LastBellRingTimeInt);
            float remainder = settings.AdventureBellDelay - deltatime;
            if (deltatime > settings.AdventureBellDelay | (InstanceSpecification.i.Adventure.LastBellRingTimeInt == uint.MaxValue))
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
            ModSettings settings;
            settings = ModSettings.Reload();
            if (settings.IgnoreAltitude) return false;

            float altitude = __instance.AltitudeAboveWaves;

            if (altitude <= 0f)
            {
                updater.FlagWarning(__instance,
                    AdventureBell._locFile.Format("Tip_BellWaterWarning", "Bell cannot ring underwater", Array.Empty<object>()));
            }
            if (altitude >= 300f)
            {
                updater.FlagWarning(__instance,
                    AdventureBell._locFile.Format("Tip_BellSpaceWarning", "Bell cannot ring above 300m", Array.Empty<object>()));
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(AdventureBell))]
    [HarmonyPatch("RegisterSpawnRequest")]
    public static class AdventureBell_RegisterSpawnRequest_Patch
    {
        static bool Prefix(AdventureBell __instance, ref bool __result)
        {
            ModSettings settings = ModSettings.Reload();
            if (settings.IgnoreAltitude)
            {
                __result = AdventureModeProgression.RequestDelayedSpawn(5U);
                return false;
            }
            // Let original method run if IgnoreAltitude is false
            return true;
        }
    }
}