using AdventurePatch;
using BrilliantSkies.Core;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.Networking;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Core.UiSounds;
using BrilliantSkies.Core.Units;
using BrilliantSkies.Ftd.Autosave;
using BrilliantSkies.Ftd.Constructs.Modules.All.FireDamage;
using BrilliantSkies.Ftd.Multiplayer.NetworkCommunication;
using BrilliantSkies.Ftd.Planets;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ftd.Planets.LoadingAndSaving;
using BrilliantSkies.Localisation;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.PlayerProfiles._Modules._KeyMapping;
using BrilliantSkies.Ui.Consoles;
using BrilliantSkies.Ui.Consoles.Builders;
using BrilliantSkies.Ui.Consoles.Examples;
using BrilliantSkies.Ui.Consoles.Getters;
using BrilliantSkies.Ui.Consoles.Interpretters;
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
using HarmonyLib;
using JetBrains.Annotations;
using NetInfrastructure;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Reflection;
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
    [HarmonyPatch(typeof(GameEvents), "__Update")]
    public static class GameEventsUpdatePatch
    {
        [HarmonyPostfix]
        static void Postfix()
        {
            // Check for rebinding input if UI is active
            if (AP_Ui.Instance != null)
            {
                AP_Ui.Instance.CheckRebinding();
            }
        }
    }

    public class AP_Ui : SuperScreen<AP_MConfig>
    {
        public AP_Ui(ConsoleWindow window, AP_MConfig config) : base(window, config)
        {
            Instance = this;
        }
        public static AP_Ui Instance { get; private set; }
        public void adventureWarp(string gatetype = "alone", int difficultyoffset = 0)
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

        public static void spawnResourceZone()
        {
            if (InstanceSpecification.i.Header.IsAdventure)
            {
                Vector3d position = InstanceSpecification.i.Adventure.PrimaryForceUniversePosition;
                var spawnAnRzMethod = typeof(AdventureModeProgression).GetMethod(
                    "SpawnAnRz",
                    BindingFlags.NonPublic | BindingFlags.Static
                );

                if (spawnAnRzMethod != null)
                {
                    spawnAnRzMethod.Invoke(null, null); // no parameters
                };
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
        private void requestRepair()
        {
            SpawnWaveMode.calculateRepaircostAndRepair();
        }
        private void requestWaveStop()
        {
            SpawnWaveMode.StopWaveMode();
        }
        private void repairAllAllied()
        {
            // Loop through all constructs
            for (int i = 0; i < StaticConstructablesManager.Constructables.Count; i++)
            {
                MainConstruct mainConstruct = StaticConstructablesManager.Constructables[i];

                // Check if it's an allied construct (same team as player)
                bool isAllied = mainConstruct != null && mainConstruct.GetTeam() == GAME_STATE.MyTeam;

                if (isAllied)
                {
                    // Repair this allied construct
                    RepairConstruct(mainConstruct);
                }
            }
        }

        private static void RepairConstruct(AllConstruct C)
        {
            if (C.State == enumConstructableState.scrapping)
            {
                return;
            }

            // Extinguish all fires
            for (int num = C.Main.FireRestricted.Fires.Count; num > 0; num--)
            {
                Fire fire = C.Main.FireRestricted.Fires[num - 1];
                fire.FuelInfo.Fuel = 0f;
            }

            // Repair all blocks
            List<Block> blocks = C.AllBasics.AliveAndDead.Blocks;
            for (int num2 = blocks.Count - 1; num2 >= 0; num2--)
            {
                Block block = blocks[num2];
                if (!block.IsAlive)
                {
                    block.RepairToBlock();
                    C.BlockRepairedSoPerformAllActions(block);
                }
                else if (block.GetCurrentHealth() != block.MaximumHealth)
                {
                    block.SetCurrentHealth(block.MaximumHealth);
                    block.ChunkStuff.RequestChunkColorOrVisibilityChange();
                    block.FireDamageFraction = 0f;
                }
            }

            // Recursively repair all subconstructs
            foreach (SubConstruct subConstruct in C.AllBasics.SubConstructList)
            {
                RepairConstruct(subConstruct);
            }
        }

        private void spawnEnemy()
        {
            AdventureModeProgression.SpawnAForce();
        }

        private bool _isRebinding = false;
        private string _rebindingAction = "";

        private void StartRebinding(string action)
        {
            _isRebinding = true;
            _rebindingAction = action;
        }

        public void CheckRebinding()
        {
            if (!_isRebinding) return;

            // Check for any key press
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (InputWrapper.GetKeyDown(key))
                {
                    var config = ProfileManager.Instance.GetModule<AP_MConfig>();

                    if (_rebindingAction == "StartWave")
                    {
                        config.StartWaveKey = key;
                    }
                    else if (_rebindingAction == "StopWave")
                    {
                        config.StopWaveKey = key;
                    }

                    _isRebinding = false;
                    _rebindingAction = "";

                    TriggerScreenRebuild();
                    break;
                }
            }
        }

        public override Content Name => new Content("Adventurepatch", new ToolTip("The options menu for the Adventurepatches Mod"));
        public override void Build()
        {

            AdvLogger.LogInfo("Building Adventurepatch options screen");
            ScreenSegmentTable mainSelection = CreateTableSegment(4, 2);
            mainSelection.SqueezeTable = false;
            mainSelection.NameWhereApplicable = "Mode Selection";
            mainSelection.SpaceBelow = 40f;
            mainSelection.BackgroundStyleWhereApplicable = ConsoleStyles.Instance.Styles.Segments.OptionalSegmentDarkBackgroundWithHeader.Style;

            mainSelection.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "QOL Only",
                "Only Enables Quality of life Patches.",
                (AP_MConfig I, bool b) =>
                {
                    if (b)
                    {
                        I.challengeMode = false;
                        I.waveMode = false;
                        I.BlockRandomSpawns = false;
                    }
                },
                (AP_MConfig I) => !I.challengeMode && !I.waveMode));

            mainSelection.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Challenge Runs",
                "When the base gamemode isnt enough for you",
                (AP_MConfig I, bool b) =>
                {
                    if (b)
                    {
                        I.waveMode = false;
                        I.challengeMode = true;
                        I.BlockRandomSpawns = false;
                    }
                    else
                    {
                        I.challengeMode = false;
                    }
                },
                (AP_MConfig I) => I.challengeMode));

            mainSelection.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Wave Defense Mode",
                "Enables the Wave Defense Gamemode.",
                (AP_MConfig I, bool b) =>
                {
                    if (b)
                    {
                        I.challengeMode = false;
                        I.waveMode = true;
                    }
                    else
                    {
                        I.waveMode = false;
                    }
                },
                (AP_MConfig I) => I.waveMode));

            var sandboxbutton = mainSelection.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Allow Cheating/Debugging",
                "Not intended to be balanced.",
                (AP_MConfig I, bool b) => I.AllowSandboxing = b,
                (AP_MConfig I) => I.AllowSandboxing));
            sandboxbutton.SetConditionalDisplayFunction(() => !Net.IsClient);

            ScreenSegmentTable QOLSettings = CreateTableSegment(4, 2);
            QOLSettings.SqueezeTable = false;
            QOLSettings.NameWhereApplicable = "QOL Settings";
            QOLSettings.SpaceBelow = 40f;
            QOLSettings.BackgroundStyleWhereApplicable = ConsoleStyles.Instance.Styles.Segments.OptionalSegmentDarkBackgroundWithHeader.Style;

            QOLSettings.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Allow fortress spawning",
                "Determines if Fortresses are allowed to spawn.",
                (AP_MConfig I, bool b) => I.SpawnFortress = b,
                (AP_MConfig I) => I.SpawnFortress));

            QOLSettings.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Ignore Heartstones",
                "Allows shooting and surviving without Heartstone",
                (AP_MConfig I, bool b) => I.IgnoreHeartstone = b,
                (AP_MConfig I) => I.IgnoreHeartstone));

            QOLSettings.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Allow Freezing in all modes",
                "Allows caps lock freeze of vehicles outside of designer mode",
                (AP_MConfig I, bool b) => I.AllowFreeze = b,
                (AP_MConfig I) => I.AllowFreeze));

            QOLSettings.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Allow god mode for Rambot",
                "The player will no longer take damage.",
                (AP_MConfig I, bool b) => I.PreventDamage = b,
                (AP_MConfig I) => I.PreventDamage));

            QOLSettings.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Adjust Losecondition",
                "You will only lose if all allied craft are downed, not just the main craft.",
                (AP_MConfig I, bool b) => I.AdjustWincon = b,
                (AP_MConfig I) => I.AdjustWincon));

            QOLSettings.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Ignore bell altitude checks",
                "Allows ringing the Bell regardless of Altitude.",
                (AP_MConfig I, bool b) => I.IgnoreAltitude = b,
                (AP_MConfig I) => I.IgnoreAltitude));

            QOLSettings.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 1, 300, 1f, 60f,
                M.m((AP_MConfig I) => I.AdventureBellDelay),
                "Bell cooldown (seconds)",
                (AP_MConfig I, float f) => I.AdventureBellDelay = (int)f,
                new ToolTip("Cooldown of the Adventurebell in seconds.")));

            var RZChangeToggle = QOLSettings.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
               "Resource zone modifications",
               "Enable scaling of resource zone material amounts based on warpdifficulty and modification of the base value.",
               (AP_MConfig I, bool b) => I.ResourceZoneDiffScaling = b,
               (AP_MConfig I) => I.ResourceZoneDiffScaling));
            RZChangeToggle.SetConditionalDisplayFunction(() => !_focus.waveMode);

            //gameplaySettings.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
            //    "Enable Custom encounters",
            //    "Enable Custom encounters, which are random enemy spawn events.",
            //    (AP_MConfig I, bool b) => I.EnableCustomEncounters = b,
            //    (AP_MConfig I) => I.EnableCustomEncounters));

            ScreenSegmentTable waveModeSettings = CreateTableSegment(3, 15);
            waveModeSettings.SqueezeTable = false;
            waveModeSettings.NameWhereApplicable = "Wave Mode Settings";
            waveModeSettings.SpaceBelow = 40f;
            waveModeSettings.BackgroundStyleWhereApplicable = ConsoleStyles.Instance.Styles.Segments.OptionalSegmentDarkBackgroundWithHeader.Style;
            waveModeSettings.SetConditionalDisplay(() => _focus.waveMode);

            waveModeSettings.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 120, 1200, 15, 240,
                M.m((AP_MConfig I) => I.waveDuration),
                "Wave duration",
                (AP_MConfig I, float f) => I.waveDuration = f,
                new ToolTip("Duration of waves in seconds.")));

            //waveModeSettings.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 0, 60, 1, 15,
            //    M.m((AP_MConfig I) => I.spawnDelay),
            //    "Delay between spawns",
            //    (AP_MConfig I, float f) => I.spawnDelay = f,
            //    new ToolTip("Delay between spawns during waves.")));

            waveModeSettings.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 1, 10, 1, 5,
                M.m((AP_MConfig I) => I.difficultyLevel),
                "Danger level",
                (AP_MConfig I, float f) => I.difficultyLevel = (int)f,
                new ToolTip("Lower values (1-4) is easier, 5 is even, above 5 is hard.")));

            waveModeSettings.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 0, 100000, 5000, 0,
                M.m((AP_MConfig I) => I.bonusStartingMaterial),
                "Bonus starting Material",
                (AP_MConfig I, float f) => I.bonusStartingMaterial = (int)f,
                new ToolTip("If you want to start with more material in Wavemode. Difficulty is unaffected.")));

            waveModeSettings.AddInterpretter(SubjectiveButton<AP_MConfig>.Quick(_focus, "Repair All Allied Craft", new ToolTip("This button will repair all allied vehicles."), (AP_MConfig ) => requestRepair()));

            waveModeSettings.AddInterpretter(SubjectiveButton<AP_MConfig>.Quick(_focus, "Stop Current wave", new ToolTip("Stops the current wave without handing out rewards."), (AP_MConfig) => requestWaveStop()));

            // Add rebind button for Start Wave
            waveModeSettings.AddInterpretter(SubjectiveButton<AP_MConfig>.Quick(_focus,
                "Rebind Start Wave",
                new ToolTip("Click then press a key to rebind Start Wave"),
                (AP_MConfig I) => StartRebinding("StartWave")
            ));

            waveModeSettings.AddInterpretter(SubjectiveButton<AP_MConfig>.Quick(_focus,
                "Rebind Stop Wave",
                new ToolTip("Click then press a key to rebind Stop Wave"),
                (AP_MConfig I) => StartRebinding("StopWave")
            ));
            // For displaying current keybindings (dynamic, updates when config changes)
            waveModeSettings.AddInterpretter(new SubjectiveDisplay<AP_MConfig>(
                _focus,
                M.m((AP_MConfig I) => $"Start Wave Key: {I.StartWaveKey}")
            ));

            waveModeSettings.AddInterpretter(new SubjectiveDisplay<AP_MConfig>(
                _focus,
                M.m((AP_MConfig I) => $"Stop Wave Key: {I.StopWaveKey}")
            ));

            // For rebinding status (dynamic)
            waveModeSettings.AddInterpretter(new SubjectiveDisplay<AP_MConfig>(
                _focus,
                M.m((AP_MConfig I) => _isRebinding ? $"Press any key for {_rebindingAction}..." : "")
            ));
            // Resource Zone Settings 
            ScreenSegmentTable resourceZoneSettings = CreateTableSegment(3, 2);
            resourceZoneSettings.SqueezeTable = false;
            resourceZoneSettings.NameWhereApplicable = "Resource Zone Settings";
            resourceZoneSettings.SpaceBelow = 40f;
            resourceZoneSettings.BackgroundStyleWhereApplicable = ConsoleStyles.Instance.Styles.Segments.OptionalSegmentDarkBackgroundWithHeader.Style;
            resourceZoneSettings.SetConditionalDisplay(() => !Net.IsClient && !_focus.waveMode && _focus.ResourceZoneDiffScaling);

            var resSlider0 = resourceZoneSettings.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 0, 100000, 5000, 40000,
                M.m((AP_MConfig I) => I.ResourceZoneBaseMaterial),
                "Resource Zone base material amount",
                (AP_MConfig I, float f) => I.ResourceZoneBaseMaterial = (int)f,
                new ToolTip("The base materials for a resource zone. Setting this to 0 will stop them from spawning entirely.")));
            resSlider0.SetConditionalDisplayFunction(() => _focus.ResourceZoneDiffScaling);

            var resSlider1 = resourceZoneSettings.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 1, 1800, 30f, 900f,
                M.m((AP_MConfig I) => I.ResourceZoneClampedDrainTime),
                "Resource Zone drain time clamp (seconds)",
                (AP_MConfig I, float f) => I.ResourceZoneClampedDrainTime = (int)f,
                new ToolTip("Spawned Resource zones gain increased material generation rates and will take at least this long to drain entirely.")));
            resSlider1.SetConditionalDisplayFunction(() => _focus.ResourceZoneDiffScaling);

            var resSlider2 = resourceZoneSettings.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 0, 2000, 50f, 500f,
                M.m((AP_MConfig I) => I.BonusMaterialPerDifficultyLevel),
                "Resource zone bonus materials per difficulty level",
                (AP_MConfig I, float f) => I.BonusMaterialPerDifficultyLevel = f,
                new ToolTip("Extra reserve material in resource zones added for each difficulty level.")));
            resSlider2.SetConditionalDisplayFunction(() => _focus.ResourceZoneDiffScaling);

            //ScreenSegmentTable customEncounters = CreateTableSegment(3, 2);
            //customEncounters.SqueezeTable = false;
            //customEncounters.NameWhereApplicable = "Custom Encounter Settings";
            //customEncounters.SpaceBelow = 40f;
            //customEncounters.BackgroundStyleWhereApplicable = ConsoleStyles.Instance.Styles.Segments.OptionalSegmentDarkBackgroundWithHeader.Style;
            //customEncounters.SetConditionalDisplay(() => !Net.IsClient && _focus.EnableCustomEncounters && !_focus.waveMode);

            //customEncounters.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 0, 100, 5, 20,
            //    M.m((AP_MConfig I) => I.CustomEncounterSpawnChance),
            //    "Custom Encounter Chance",
            //    (AP_MConfig I, float f) => I.CustomEncounterSpawnChance = (uint)f,
            //    new ToolTip("% Chance to select a custom encounter when spawning enemies.")));

            //var spawnDelaySlider = customEncounters.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 1, 10, 0.1f, 2f,
            //    M.m((AP_MConfig I) => I.SpawnDelay),
            //    "Delay between spawns(s)",
            //    (AP_MConfig I, float f) => I.SpawnDelay = (int)f,
            //    new ToolTip("Delay between encounter enemy spawns in seconds.")));

            // Challenge Mode Select 
            ScreenSegmentTable challengeSelection = CreateTableSegment(3, 2);
            challengeSelection.SqueezeTable = false;
            challengeSelection.NameWhereApplicable = "Challenge Mode Selection";
            challengeSelection.SpaceBelow = 40f;
            challengeSelection.BackgroundStyleWhereApplicable = ConsoleStyles.Instance.Styles.Segments.OptionalSegmentDarkBackgroundWithHeader.Style;
            challengeSelection.SetConditionalDisplay(() => !Net.IsClient && !_focus.waveMode && _focus.challengeMode);

            // Scale difficulty dynamically
            challengeSelection.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Scale difficulty dynamically",
                "The difficulty will scale according to material cost of allied craft.",
                (AP_MConfig I, bool b) =>
                {
                    if (b)
                    {
                        I.ForceEnemySpawns = false;
                        I.autoIncreaseDifficulty = false;
                        I.MaterialScaling = true;
                    }
                    else if (I.MaterialScaling)
                    {
                        I.MaterialScaling = false;
                    }
                },
                (AP_MConfig I) => I.MaterialScaling));

            // Force enemy spawns
            var forceSpawnToggle = challengeSelection.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Force enemy spawns",
                "Forces enemy spawns to happen, even if the bell is not rung.",
                (AP_MConfig I, bool b) =>
                {
                    if (b)
                    {
                        I.MaterialScaling = false;
                        I.autoIncreaseDifficulty = false;
                        I.ForceEnemySpawns = true;
                    }
                    else if (I.ForceEnemySpawns)
                    {
                        I.ForceEnemySpawns = false;
                    }
                },
                (AP_MConfig I) => I.ForceEnemySpawns));

            // Increase difficulty by 5 based on a timer
            var autoIncDifficultyToggle = challengeSelection.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Increase difficulty by 5 based on a timer",
                "The difficulty will increase every X seconds",
                (AP_MConfig I, bool b) =>
                {
                    if (b)
                    {
                        I.MaterialScaling = false;
                        I.ForceEnemySpawns = false;
                        I.autoIncreaseDifficulty = true;
                    }
                    else if (I.autoIncreaseDifficulty)
                    {
                        I.autoIncreaseDifficulty = false;
                    }
                },
                (AP_MConfig I) => I.autoIncreaseDifficulty));

            //Challenge mode settings
            ScreenSegmentTable diffSection = CreateTableSegment(3, 15);
            diffSection.SqueezeTable = false;
            diffSection.NameWhereApplicable = "Challenge Mode Settings";
            diffSection.SpaceBelow = 40f;
            diffSection.BackgroundStyleWhereApplicable = ConsoleStyles.Instance.Styles.Segments.OptionalSegmentDarkBackgroundWithHeader.Style;
            diffSection.SetConditionalDisplay(() => !_focus.waveMode && _focus.challengeMode && (_focus.MaterialScaling || _focus.ForceEnemySpawns || _focus.autoIncreaseDifficulty));

            var offsetSlider = diffSection.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, -20, 20, 5, -5,
                M.m((AP_MConfig I) => I.ScalingOffset),
                "Difficulty offset",
                (AP_MConfig I, float f) => I.ScalingOffset = (int)f,
                new ToolTip("Offset applied to the dynamical difficulty. equal cost is achieved around -5.")));
            offsetSlider.SetConditionalDisplayFunction(() => _focus.MaterialScaling);

            var autoIncTimer = diffSection.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 180, 1800, 15, 300,
                M.m((AP_MConfig I) => I.autoIncreaseTime),
                "Time between difficulty increases",
                (AP_MConfig I, float f) => I.autoIncreaseTime = (int)f,
                new ToolTip("The difficulty will increase by 5 on this timer.")));
            autoIncTimer.SetConditionalDisplayFunction(() => _focus.autoIncreaseDifficulty && !_focus.waveMode);

            var forceSpawnDelaySlider = diffSection.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 5, 600, 5, 60,
                M.m((AP_MConfig I) => I.EnemySpawnDelay),
                "Time between forced enemy spawns",
                (AP_MConfig I, float f) => I.EnemySpawnDelay = (uint)f,
                new ToolTip("Time between spawns in seconds.")));
            forceSpawnDelaySlider.SetConditionalDisplayFunction(() => _focus.ForceEnemySpawns && !_focus.waveMode);

            var gracePeriodSlider = diffSection.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 60, 1800, 30, 600,
                M.m((AP_MConfig I) => I.GracePeriod),
                "Grace period (s)",
                (AP_MConfig I, float f) =>
                {
                    I.GracePeriod = f;
                    AdventureModeProgression_RunAdventureMode_Patch.updateSettings();
                },
                new ToolTip("Increase of difficulty or the first spawn may happen after at least this much time passed.")));
            gracePeriodSlider.SetConditionalDisplayFunction(() => (_focus.ForceEnemySpawns || _focus.autoIncreaseDifficulty) && !_focus.waveMode);


            // Enemy Settings Section
            ScreenSegmentTable enemySection = CreateTableSegment(3, 15);
            enemySection.SqueezeTable = false;
            enemySection.NameWhereApplicable = "Enemy Spawn Settings";
            enemySection.SpaceBelow = 40f;
            enemySection.BackgroundStyleWhereApplicable = ConsoleStyles.Instance.Styles.Segments.OptionalSegmentDarkBackgroundWithHeader.Style;

            enemySection.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 10000, 250000, 5000, 30000,
                M.m((AP_MConfig I) => I.MaxEnemyVolume),
                "Maximum enemy volume",
                (AP_MConfig I, float f) => I.MaxEnemyVolume = (uint)f,
                new ToolTip("If the combined volume of all enemies exceeds this value, no new enemies will be spawned.")));

            enemySection.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 5, 100, 5, 10,
                M.m((AP_MConfig I) => I.MaxEnemyCount),
                "Maximum number of enemies",
                (AP_MConfig I, float f) => I.MaxEnemyCount = (uint)f,
                new ToolTip("If the total number of enemies reaches this limit, no additional enemies will be spawned.")));

            var harvestAmountSlider = enemySection.AddInterpretter(
                SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(
                    _focus, 0, 100, 1f, 10f,
                    M.m((AP_MConfig I) => I.EnemyDropPercentage),
                    "Enemy resource drop percentage",
                    (AP_MConfig I, float f) =>
                    {
                        I.EnemyDropPercentage = f;
                        EnemyDropManager.ApplyEnemyDropSettings();
                    },
                    new ToolTip("Changes the percentage of dropped resources on kills.")
                )
            );
            //harvestAmountSlider.SetConditionalDisplayFunction(() => !_focus.waveMode);

            var distanceToggle = enemySection.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Enemies spawn at a customiseable distance",
                "Allow enemies to spawn at their preferred engagement range, respecting the minimum and bonus distance.",
                (AP_MConfig I, bool b) => I.EnemySpawnDistancePatch = b,
                (AP_MConfig I) => I.EnemySpawnDistancePatch));
            distanceToggle.SetConditionalDisplayFunction(() => !_focus.waveMode);

            var bonusDistanceSlider = enemySection.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 1, 5000, 50f, 500f,
                M.m((AP_MConfig I) => I.SpawnBonusDistance),
                "Engagement range bonus distance",
                (AP_MConfig I, float f) => I.SpawnBonusDistance = f,
                new ToolTip("Extra distance added to the engagement range.")));
            bonusDistanceSlider.SetConditionalDisplayFunction(() => _focus.EnemySpawnDistancePatch && !_focus.waveMode);

            var minSpawnSlider = enemySection.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 0, 10000, 100f, 2000f,
                M.m((AP_MConfig I) => I.MinimumSpawnrange),
                "Enemy minimum spawn distance",
                (AP_MConfig I, float f) => I.MinimumSpawnrange = f,
                new ToolTip("Enemies spawn at least this far away.")));
            minSpawnSlider.SetConditionalDisplayFunction(() => _focus.EnemySpawnDistancePatch && !_focus.waveMode);

            // Sandbox buttons
            ScreenSegmentTable sandBoxSettings = CreateTableSegment(3, 15);
            sandBoxSettings.SqueezeTable = false;
            sandBoxSettings.NameWhereApplicable = "Debugging Section";
            sandBoxSettings.SpaceBelow = 40f;
            sandBoxSettings.BackgroundStyleWhereApplicable = ConsoleStyles.Instance.Styles.Segments.OptionalSegmentDarkBackgroundWithHeader.Style;
            sandBoxSettings.SetConditionalDisplay(() => _focus.AllowSandboxing && !Net.IsClient);

            sandBoxSettings.AddInterpretter(SubjectiveButton<AP_MConfig>.Quick(_focus, "Enter a Blue Portal", new ToolTip("This button will send you through a Blue Portal."), (AP_MConfig I) => adventureWarp()));
            sandBoxSettings.AddInterpretter(SubjectiveButton<AP_MConfig>.Quick(_focus, "Enter a Green Portal", new ToolTip("This button will send you through a Green Portal."), (AP_MConfig I) => adventureWarp("easier")));
            sandBoxSettings.AddInterpretter(SubjectiveButton<AP_MConfig>.Quick(_focus, "Enter a Red Portal", new ToolTip("This button will send you through a Red Portal."), (AP_MConfig I) => adventureWarp("harder")));
            sandBoxSettings.AddInterpretter(SubjectiveButton<AP_MConfig>.Quick(_focus, "Spawn a Resource Zone", new ToolTip("This button will spawn a resource zone ontop of the main craft."), (AP_MConfig I) => spawnResourceZone()));
            sandBoxSettings.AddInterpretter(SubjectiveButton<AP_MConfig>.Quick(_focus, "Spawn an Enemy", new ToolTip("This button will instantly spawn an enemy."), (AP_MConfig I) => spawnEnemy()));
            sandBoxSettings.AddInterpretter(SubjectiveButton<AP_MConfig>.Quick(_focus, "Destroy All Enemies", new ToolTip("This button will instantly remove all enemies."), (AP_MConfig I) => destroyEnemies()));
            sandBoxSettings.AddInterpretter(SubjectiveButton<AP_MConfig>.Quick(_focus, "Repair All Allied Craft", new ToolTip("This button will repair all allied vehicles."), (AP_MConfig I) => repairAllAllied()));

            sandBoxSettings.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Allow enemy spawning menu",
                "Allows the x-menu to open in all game modes",
                (AP_MConfig I, bool b) => I.AllowEnemySpawnUI = b,
                (AP_MConfig I) => I.AllowEnemySpawnUI));

            sandBoxSettings.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Block enemy spawns",
                "Blocks naturally occuring enemy spawns. the adventurebell and forced spawns still work.",
                (AP_MConfig I, bool b) => I.BlockRandomSpawns = b,
                (AP_MConfig I) => I.BlockRandomSpawns));

            var sliderToggle = sandBoxSettings.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus,
                "Override difficulty with custom slider value",
                "Spawning enemies will use the custom value instead of the warp difficulty.",
                (AP_MConfig I, bool b) => I.OverrideSpawnDifficulty = b,
                (AP_MConfig I) => I.OverrideSpawnDifficulty));
            sliderToggle.SetConditionalDisplayFunction(() => !_focus.MaterialScaling && !_focus.waveMode);

            var difficultySlider = sandBoxSettings.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 0, 100, 5, 50,
                M.m((AP_MConfig I) => I.SpawnDifficulty),
                "Enemy Spawn difficulty",
                (AP_MConfig I, float f) => I.SpawnDifficulty = (int)f,
                new ToolTip("Spawned enemies will spawn according to this difficulty instead.")));
            difficultySlider.SetConditionalDisplayFunction(() => _focus.OverrideSpawnDifficulty && !_focus.MaterialScaling && !_focus.waveMode);

            AdvLogger.LogInfo("Adventurepatch options screen built successfully");
        }
    }
}
