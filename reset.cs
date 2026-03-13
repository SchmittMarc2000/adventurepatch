using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Ftd.Modes.MainMenu.Ui;
using HarmonyLib;
using System;

namespace AdventurePatch
{
    // Patch the New Adventure button
    [HarmonyPatch(typeof(MainMenu))]
    [HarmonyPatch("NewAdventure")]
    public static class MainMenu_NewAdventure_Patch
    {
        public static void Prefix()
        {
            AdventureModeProgression_RunAdventureMode_Patch.gracePeriodTriggered = false;
            AdvLogger.LogInfo("New adventure started - reset forced spawn timers");
        }
    }

    // Patch the Load Adventure button
    [HarmonyPatch(typeof(MainMenu))]
    [HarmonyPatch("LoadAdventure")]
    public static class MainMenu_LoadAdventure_Patch
    {
        public static void Prefix()
        {
            AdventureModeProgression_RunAdventureMode_Patch.gracePeriodTriggered = false;
            AdvLogger.LogInfo("Loading adventure - reset forced spawn timers");
        }
    }
}