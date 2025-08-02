using AdventurePatch;
using BrilliantSkies.Core;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.Networking;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Core.UiSounds;
using BrilliantSkies.Ftd.Autosave;
using BrilliantSkies.Ftd.Multiplayer.NetworkCommunication;
using BrilliantSkies.Ftd.Planets;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ftd.Planets.LoadingAndSaving;
using BrilliantSkies.Localisation;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Ui.Consoles;
using BrilliantSkies.Ui.Consoles.Builders;
using BrilliantSkies.Ui.Consoles.Examples;
using BrilliantSkies.Ui.Consoles.Getters;
using BrilliantSkies.Ui.Consoles.Interpretters.Simple;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Buttons;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Choices;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Numbers;
using BrilliantSkies.Ui.Consoles.Segments;
using BrilliantSkies.Ui.Consoles.Styles;
using BrilliantSkies.Ui.Examples.CampaignLaunching;
using BrilliantSkies.Ui.Examples.Credits;
using BrilliantSkies.Ui.Examples.OptionsMenu;
using BrilliantSkies.Ui.Layouts.DropDowns;
using BrilliantSkies.Ui.StyleEditing;
using BrilliantSkies.Ui.Tips;
using JetBrains.Annotations;
using NetInfrastructure;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using Ui.Consoles.Examples;
using Ui.Displayer.Types.Examples;
using UnityEngine;

namespace AdventurePatch
{
    
    public class AP_Ui : SuperScreen<AP_MConfig>
    {
        public AP_Ui(ConsoleWindow window, AP_MConfig config)
            : base(window, config)
        {
        }

        private void adventureWarp(string gatetype = "alone", int difficultyoffset = 0)
        {
            bool NotAdventure = !InstanceSpecification.i.Header.IsAdventure;
            if(NotAdventure)
            {
                AdvLogger.LogInfo("AdventureWarp called in non-adventure mode, aborting warp.");
                return;
            }
            if (Net.IsClient) return;
            switch (gatetype)
            {
                case "alone":
                    if (Net.IsServer)
                    {
                        InstantComs.GetSingleton().SendRpc(new RpcRequest(delegate (INetworkIdentity n)
                        {
                            ServerOutgoingRpcs.ChangeWarpPlane(n, WarpGateType.Alone, InstanceSpecification.i.Adventure.PrimaryForceUniversePosition);
                        }));
                    }
                    AdventureModeProgression.Common_DoWarp(WarpGateType.Alone, InstanceSpecification.i.Adventure.PrimaryForceUniversePosition);
                    break;
                case "easier":
                    if (Net.IsServer)
                    {
                        InstantComs.GetSingleton().SendRpc(new RpcRequest(delegate (INetworkIdentity n)
                        {
                            ServerOutgoingRpcs.ChangeWarpPlane(n, WarpGateType.Easier, InstanceSpecification.i.Adventure.PrimaryForceUniversePosition);
                        }));
                    }
                    AdventureModeProgression.Common_DoWarp(WarpGateType.Easier, InstanceSpecification.i.Adventure.PrimaryForceUniversePosition);
                    break;
                case "harder":
                    if (Net.IsServer)
                    {
                        InstantComs.GetSingleton().SendRpc(new RpcRequest(delegate (INetworkIdentity n)
                        {
                            ServerOutgoingRpcs.ChangeWarpPlane(n, WarpGateType.Harder, InstanceSpecification.i.Adventure.PrimaryForceUniversePosition);
                        }));
                    }
                    AdventureModeProgression.Common_DoWarp(WarpGateType.Harder, InstanceSpecification.i.Adventure.PrimaryForceUniversePosition);
                    break;
                default:
                    AdvLogger.LogInfo("Invalid warp gate type specified: " + gatetype);
                    return;
            }
        }


        private void spawnResourceZone()
        {
            if (InstanceSpecification.i.Header.IsAdventure)
            {
                if(!Net.IsServer)           //if we are not a host, we can just use the non-synced version
                {
                    Vector3d position = InstanceSpecification.i.Adventure.PrimaryForceUniversePosition;
                    AdventureModeProgression.Common_SpawnRz(position, 29990f);
                    return;
                }
                var method = typeof(AdventureModeProgression).GetMethod(
                    "SpawnAnRz",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic
                );

                if (method != null)
                {
                    method.Invoke(null, null);
                }
                else
                {
                    AdvLogger.LogInfo("Could not find method AdventureModeProgression.SpawnAnRz.");
                }
            }
            else
            {
                AdvLogger.LogInfo("Attempted to spawn resource zone outside of adventure mode.");
            }
        }

        private void destroyEnemies()
        {
            int vehiclecount = StaticConstructablesManager.Constructables.Count;
            for (int i = 0; i < vehiclecount; i++)
            {
                bool flag4 = StaticConstructablesManager.Constructables[i].GetTeam() != GAME_STATE.MyTeam;
                if (flag4)
                {
                    StaticConstructablesManager.Constructables[i].DestroyCompletely(DestroyReason.Wiped, true);
                    i--;
                    vehiclecount--;
                }
            }
        }

        private void spawnEnemy()
        {
            AdventureModeProgression.SpawnAForce();
        }


        public override Content Name => new Content("Adventurepatch", new ToolTip("The options menu for the Adventurepatches Mod"));
        public override void Build()
        {
            ScreenSegmentTable gameplaySettings = CreateTableSegment(3, 2);
            gameplaySettings.SqueezeTable = false;
            gameplaySettings.NameWhereApplicable = "General Settings";
            gameplaySettings.SpaceBelow = 20f;
            gameplaySettings.BackgroundStyleWhereApplicable = ConsoleStyles.Instance.Styles.Segments.OptionalSegmentDarkBackgroundWithHeader.Style;
            gameplaySettings.SetConditionalDisplay(() => !Net.IsClient);
            AdvLogger.LogInfo("Building Adventurepatch options screen");

            // General toggles
            

            var sandboxbutton = gameplaySettings.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Allow Sandboxing",
                "Mainly for debugging/testing purposes, not intended to be balanced.",
                (AP_MConfig I, bool b) => I.AllowSandboxing = b,
                (AP_MConfig I) => I.AllowSandboxing));
            sandboxbutton.SetConditionalDisplayFunction(() => !Net.IsClient);


            gameplaySettings.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Allow fortress spawning",
                "Determines if Fortresses are allowed to spawn.",
                (AP_MConfig I, bool b) => I.SpawnFortress = b,
                (AP_MConfig I) => I.SpawnFortress));

            gameplaySettings.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Enable Custom encounters",
                "Enable Custom encounters, which are random enemy spawn events.",
                (AP_MConfig I, bool b) => I.EnableCustomEncounters = b,
                (AP_MConfig I) => I.EnableCustomEncounters));

            // Resource Zone Settings 
            ScreenSegmentTable resourceBellSettings = CreateTableSegment(3, 2);
            resourceBellSettings.SqueezeTable = false;
            resourceBellSettings.NameWhereApplicable = "Resource Zone Settings";
            resourceBellSettings.SpaceBelow = 20f;
            resourceBellSettings.BackgroundStyleWhereApplicable = ConsoleStyles.Instance.Styles.Segments.OptionalSegmentDarkBackgroundWithHeader.Style;
            resourceBellSettings.SetConditionalDisplay(() => !Net.IsClient);

            resourceBellSettings.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Resource zone modifications",
                "Enable scaling of resource zone material amounts based on warpdifficulty and modification of the base value.",
                (AP_MConfig I, bool b) => I.ResourceZoneDiffScaling = b,
                (AP_MConfig I) => I.ResourceZoneDiffScaling));

            var resSlider0 = resourceBellSettings.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 0, 100000, 5000, 40000,
                M.m((AP_MConfig I) => I.ResourceZoneBaseMaterial),
                "Resource Zone base material amount",
                (AP_MConfig I, float f) => I.ResourceZoneBaseMaterial = (int)f,
                new ToolTip("The base materials for a resource zone. Setting this to 0 will stop them from spawning entirely.")));
            resSlider0.SetConditionalDisplayFunction(() => _focus.ResourceZoneDiffScaling);
            var resSlider1 = resourceBellSettings.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 1, 1800, 30f, 900f,
                M.m((AP_MConfig I) => I.ResourceZoneClampedDrainTime),
                "Resource Zone drain time clamp (seconds)",
                (AP_MConfig I, float f) => I.ResourceZoneClampedDrainTime = (int)f,
                new ToolTip("Spawned Resource zones gain increased material generation rates and will take at least this long to drain entirely.")));
            resSlider1.SetConditionalDisplayFunction(() => _focus.ResourceZoneDiffScaling);
            var resSlider2 = resourceBellSettings.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 0, 2000, 50f, 500f,
                M.m((AP_MConfig I) => I.BonusMaterialPerDifficultyLevel),
                "Resource zone bonus materials per difficulty level",
                (AP_MConfig I, float f) => I.BonusMaterialPerDifficultyLevel = f,
                new ToolTip("Extra reserve material in resource zones added for each difficulty level.")));
            resSlider2.SetConditionalDisplayFunction(() => _focus.ResourceZoneDiffScaling);

            // Bell Settings
            ScreenSegmentTable bellSettings = CreateTableSegment(3, 2);
            bellSettings.SqueezeTable = false;
            bellSettings.NameWhereApplicable = "Adventure Bell Settings";
            bellSettings.SpaceBelow = 20f;
            bellSettings.BackgroundStyleWhereApplicable = ConsoleStyles.Instance.Styles.Segments.OptionalSegmentDarkBackgroundWithHeader.Style;
            bellSettings.SetConditionalDisplay(() => !Net.IsClient);
            bellSettings.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Ignore bell altitude checks",
                "Allows ringing the Bell regardless of Altitude.",
                (AP_MConfig I, bool b) => I.IgnoreAltitude = b,
                (AP_MConfig I) => I.IgnoreAltitude));

            bellSettings.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 1, 300, 1f, 60f,
                M.m((AP_MConfig I) => I.AdventureBellDelay),
                "Bell cooldown (seconds)",
                (AP_MConfig I, float f) => I.AdventureBellDelay = (int)f,
                new ToolTip("Cooldown of the Adventurebell in seconds.")));

            ScreenSegmentTable customEncounters = CreateTableSegment(3, 2);
            customEncounters.SqueezeTable = false;
            customEncounters.NameWhereApplicable = "Custom Encounter Settings";
            customEncounters.SpaceBelow = 20f;
            customEncounters.BackgroundStyleWhereApplicable = ConsoleStyles.Instance.Styles.Segments.OptionalSegmentDarkBackgroundWithHeader.Style;
            customEncounters.SetConditionalDisplay(() => !Net.IsClient && _focus.EnableCustomEncounters);

            customEncounters.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 0, 100, 5, 20,
                M.m((AP_MConfig I) => I.CustomEncounterSpawnChance),
                "Custom Encounter Chance",
                (AP_MConfig I, float f) => I.CustomEncounterSpawnChance = (uint)f,
                new ToolTip("% Chance to select a custom encounter when spawning enemies.")));


            // Enemy Settings Section
            ScreenSegmentTable enemySection = CreateTableSegment(2, 15);
            enemySection.SqueezeTable = false;
            enemySection.NameWhereApplicable = "Enemy Spawn Settings";
            enemySection.SpaceBelow = 20f;
            enemySection.BackgroundStyleWhereApplicable = ConsoleStyles.Instance.Styles.Segments.OptionalSegmentDarkBackgroundWithHeader.Style;

            //enemySection.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 10000, 250000, 5000, 30000,
            //    M.m((AP_MConfig I) => I.MaxEnemyVolume),
            //    "Maximum enemy volume",
            //    (AP_MConfig I, float f) => I.MaxEnemyVolume = (uint)f,
            //    new ToolTip("If the combined volume of all enemies exceeds this value, no new enemies will be spawned.")));

            //enemySection.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 5, 100, 5, 10,
            //    M.m((AP_MConfig I) => I.MaxEnemyCount),
            //    "Maximum number of enemies",
            //    (AP_MConfig I, float f) => I.MaxEnemyCount = (uint)f,
            //    new ToolTip("If the total number of enemies reaches this limit, no additional enemies will be spawned.")));

            enemySection.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Force enemy spawns",
                "Forces enemy spawns to happen, even if the bell is not rung.",
                (AP_MConfig I, bool b) => I.ForceEnemySpawns = b,
                (AP_MConfig I) => I.ForceEnemySpawns));

            var forceSpawnDelaySlider = enemySection.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 5, 600, 5, 60,
                M.m((AP_MConfig I) => I.EnemySpawnDelay),
                "Time between forced enemy spawns",
                (AP_MConfig I, float f) => I.EnemySpawnDelay = (uint)f,
                new ToolTip("Time between spawns in seconds.")));
            forceSpawnDelaySlider.SetConditionalDisplayFunction(() => _focus.ForceEnemySpawns);

            enemySection.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Override Difficulty with custom slider value",
                "Spawning enemies will use the custom value instead of the warp difficulty.",
                (AP_MConfig I, bool b) => I.OverrideSpawnDifficulty = b,
                (AP_MConfig I) => I.OverrideSpawnDifficulty));

            var difficultySlider = enemySection.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 0, 100, 5, 50,
                M.m((AP_MConfig I) => I.SpawnDifficulty),
                "Enemy Spawn difficulty",
                (AP_MConfig I, float f) => I.SpawnDifficulty = (int)f,
                new ToolTip("Spawned enemies will spawn according to this difficulty instead.")));
            difficultySlider.SetConditionalDisplayFunction(() => _focus.OverrideSpawnDifficulty);

            enemySection.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Block enemy spawns",
                "Blocks naturally occuring enemy spawns. the adventurebell and forced spawns still work.",
                (AP_MConfig I, bool b) => I.BlockRandomSpawns = b,
                (AP_MConfig I) => I.BlockRandomSpawns));

            

            ScreenSegmentTable distSection = CreateTableSegment(3, 15);
            distSection.SqueezeTable = false;
            distSection.NameWhereApplicable = "Spawning Distance Settings";
            distSection.SpaceBelow = 20f;
            distSection.BackgroundStyleWhereApplicable = ConsoleStyles.Instance.Styles.Segments.OptionalSegmentDarkBackgroundWithHeader.Style;

            distSection.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Enemies spawn at a customiseable distance",
                "Allow enemies to spawn at their preferred engagement range, respecting the minimum and bonus distance.",
                (AP_MConfig I, bool b) => I.EnemySpawnDistancePatch = b,
                (AP_MConfig I) => I.EnemySpawnDistancePatch));

            // Engagement range settings (conditionally visible)
            var bonusDistanceSlider = distSection.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 1, 5000, 50f, 500f,
                M.m((AP_MConfig I) => I.SpawnBonusDistance),
                "Engagement range bonus distance",
                (AP_MConfig I, float f) => I.SpawnBonusDistance = f,
                new ToolTip("Extra distance added to the engagement range.")));
            bonusDistanceSlider.SetConditionalDisplayFunction(() => _focus.EnemySpawnDistancePatch);

            var minSpawnSlider = distSection.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 0, 10000, 100f, 2000f,
                M.m((AP_MConfig I) => I.MinimumSpawnrange),
                "Enemy minimum spawn distance",
                (AP_MConfig I, float f) => I.MinimumSpawnrange = f,
                new ToolTip("Enemies spawn at least this far away.")));
            minSpawnSlider.SetConditionalDisplayFunction(() => _focus.EnemySpawnDistancePatch);


            // Sandbox buttons
            ScreenSegmentTable sandBoxSettings = CreateTableSegment(3, 15);
            sandBoxSettings.SqueezeTable = false;
            sandBoxSettings.NameWhereApplicable = "Sandboxing Section";
            sandBoxSettings.SpaceBelow = 20f;
            sandBoxSettings.BackgroundStyleWhereApplicable = ConsoleStyles.Instance.Styles.Segments.OptionalSegmentDarkBackgroundWithHeader.Style;
            sandBoxSettings.SetConditionalDisplay(() => _focus.AllowSandboxing && !Net.IsClient);

            sandBoxSettings.AddInterpretter(SubjectiveButton<AP_MConfig>.Quick(_focus, "Enter a Blue Portal", new ToolTip("This button will send you through a Blue Portal."), (AP_MConfig I) => adventureWarp()));
            sandBoxSettings.AddInterpretter(SubjectiveButton<AP_MConfig>.Quick(_focus, "Enter a Green Portal", new ToolTip("This button will send you through a Green Portal."), (AP_MConfig I) => adventureWarp("easier")));
            sandBoxSettings.AddInterpretter(SubjectiveButton<AP_MConfig>.Quick(_focus, "Enter a Red Portal", new ToolTip("This button will send you through a Red Portal."), (AP_MConfig I) => adventureWarp("harder")));
            sandBoxSettings.AddInterpretter(SubjectiveButton<AP_MConfig>.Quick(_focus, "Spawn a Resource Zone", new ToolTip("This button will spawn a resource zone ontop of the main craft."), (AP_MConfig I) => spawnResourceZone()));
            sandBoxSettings.AddInterpretter(SubjectiveButton<AP_MConfig>.Quick(_focus, "Spawn an Enemy", new ToolTip("This button will instantly spawn an enemy."), (AP_MConfig I) => spawnEnemy()));
            sandBoxSettings.AddInterpretter(SubjectiveButton<AP_MConfig>.Quick(_focus, "Destroy all Enemies", new ToolTip("This button will instantly remove all enemies."), (AP_MConfig I) => destroyEnemies()));
            
            AdvLogger.LogInfo("Adventurepatch options screen built successfully");


        }
    }
}
