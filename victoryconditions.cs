using BrilliantSkies.Core;
using BrilliantSkies.Core.Enumerations;
using BrilliantSkies.Core.Id;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.FromTheDepths.Game;
using BrilliantSkies.Ftd.Constructs;
using BrilliantSkies.Ftd.Constructs.Modules.All.EMP;
using BrilliantSkies.Ftd.Modes.Common.Debriefing;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ftd.Planets.Instances.Adventure;
using BrilliantSkies.Ftd.Planets.Instances.VictoryConditions;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Ui.Displayer;
using HarmonyLib;
using System;
using System.Drawing;
using System.Reflection;

namespace AdventurePatch
{
    [HarmonyPatch(typeof(VictoryAssessment))]
    [HarmonyPatch("AssessVictory")]
    public class VictoryAssessmentPatch
    {
        //private static void findAndReassignPrimaryForce()
        //{
        //    float maxvolume = 0;
        //    Force nextPrimary = null;
        //    for (int i = 0; i < StaticConstructablesManager.Constructables.Count; i++)
        //    {
        //        MainConstruct mainConstruct = StaticConstructablesManager.Constructables[i];

        //        bool isAllied = mainConstruct != null && mainConstruct.GetTeam() == GAME_STATE.MyTeam;

        //        if (isAllied)
        //        {
        //            Force temp = mainConstruct.GetForce();
        //            float size = temp.GetSize().magnitude;
        //            if (size > maxvolume)
        //            {
        //                maxvolume = size;
        //                nextPrimary = temp;
        //            }
        //        }
        //        InstanceSpecification.i.Adventure.PrimaryForce = nextPrimary;
        //    }
        //} didnt work since primaryforce is read-only
        //  primaryforceid isnt though...

    private static void findAndReassignPrimaryForce()
    {
        float maxvolume = 0;
        Force nextPrimary = null;
        ObjectId primaryForceId = null;

        // Find the largest allied force
        for (int i = 0; i < StaticConstructablesManager.Constructables.Count; i++)
        {
            MainConstruct mainConstruct = StaticConstructablesManager.Constructables[i];

            bool isAllied = mainConstruct != null && mainConstruct.GetTeam() == GAME_STATE.MyTeam;

            if (isAllied)
            {
                Force temp = mainConstruct.GetForce();
                if (temp != null)
                {
                    float size = temp.GetSize().magnitude;
                    AdvLogger.LogInfo("Found allied force with size: " + size + " and name: " + temp.GetNameWithStatusColored());
                    if (size > maxvolume)
                    {
                        maxvolume = size;
                        nextPrimary = temp;
                        primaryForceId = temp.Id; // Store the ID
                    }
                }
            }
        }

        // Set the primary force ID
        if (nextPrimary != null && primaryForceId != null)
        {
            AdvLogger.LogInfo("Attempting to assign: " + nextPrimary.GetNameWithStatusColored() + " with ID: " + primaryForceId);

            var adventure = InstanceSpecification.i.Adventure as InstanceAdventure;
            if (adventure != null)
            {
                // Find the PrimaryForceId backing field
                var field = AccessTools.Field(typeof(InstanceAdventure), "<PrimaryForceId>k__BackingField");

                if (field != null)
                {
                    field.SetValue(adventure, primaryForceId);
                    AdvLogger.LogInfo($"PrimaryForceId set to: {primaryForceId}");

                    // Verify by reading it back
                    var verifyField = field.GetValue(adventure) as ObjectId;
                        if (verifyField != null) return;
                    AdvLogger.LogInfo($"Verification - PrimaryForceId is now: {verifyField}");
                }
                else
                {
                    AdvLogger.LogError("Could not find PrimaryForceId backing field", LogOptions.StackTrace);
                }
            }
        }
    }
    static bool Prefix(VictoryAssessment __instance)
        {
            if (!ProfileManager.Instance.GetModule<AP_MConfig>().AdjustWincon || !InstanceSpecification.i.Header.IsAdventure)
            {
                AdvLogger.LogInfo("Victoryassessment continues as it should");
                return true;
            }
            // Skip if in world editor or victory already assigned
            if (GAME_STATE.GetGameType() == enumGameType.worldeditor ||
                (bool)AccessTools.Field(typeof(VictoryAssessment), "_victoryOrFailureAssigned").GetValue(__instance))
            {
                return true;
            }

            // Check if there are any allied craft
            bool hasAlliedCraft = false;
            int vehiclecount = StaticConstructablesManager.Constructables.Count;
            bool primaryForceExists = false;
            if (InstanceSpecification.i.Adventure != null)
            {
                var primaryForce = InstanceSpecification.i.Adventure.PrimaryForce;
                primaryForceExists = primaryForce != null && primaryForce.Exists;
            }

            if (!primaryForceExists)
            {
                AdvLogger.LogInfo("Primary force doesn't exist, attempting to reassign");
                findAndReassignPrimaryForce();
            }
            else
            {
                //AdvLogger.LogInfo("Primary force exists: " + InstanceSpecification.i.Adventure.PrimaryForce.ToString());
            }
            for (int i = 0; i < vehiclecount; i++)
            {
                MainConstruct construct = StaticConstructablesManager.Constructables[i];
                if (construct != null && construct.GetTeam() == GAME_STATE.MyTeam)
                {
                    hasAlliedCraft = true;
                    break;
                }
            }

            // If there are allied craft, don't trigger victory assessment
            if (hasAlliedCraft)
            {
                //AdvLogger.LogInfo("Victory assessment prevented - allied craft still exist");
                return false; // Skip original method
            }

            // No allied craft found, proceed with victory assessment
            AdvLogger.LogInfo("No allied craft found - proceeding with victory assessment");
            return true;
        }
    }
}