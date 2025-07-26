using BrilliantSkies.Ftd.Autosave;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ftd.Planets.LoadingAndSaving;
using BrilliantSkies.Ftd.Planets;
using BrilliantSkies.Localisation;
using BrilliantSkies.Ui.Consoles;
using BrilliantSkies.Ui.Consoles.Examples;
using BrilliantSkies.Ui.Examples.CampaignLaunching;
using BrilliantSkies.Ui.Examples.OptionsMenu;
using BrilliantSkies.Ui.Tips;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Consoles.Examples;
using Ui.Displayer.Types.Examples;
using BrilliantSkies.Ui.Examples.Credits;
using BrilliantSkies.Ui.Consoles.Builders;
using BrilliantSkies.Ui.Consoles.Segments;
using BrilliantSkies.Ui.Consoles.Interpretters.Simple;
using UnityEngine;
using BrilliantSkies.Ui.Consoles.Getters;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective;
using BrilliantSkies.Ui.StyleEditing;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Numbers;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Choices;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Buttons;
using BrilliantSkies.Core.UiSounds;
using BrilliantSkies.Ui.Layouts.DropDowns;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Ui.Consoles.Styles;
using System.Security.Cryptography;
using AdventurePatch;
using BrilliantSkies.Core.Types;
using BrilliantSkies.PlayerProfiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrilliantSkies.Core.Logger;

namespace AdventurePatch
{
    public class AP_Ui : SuperScreen<AP_MConfig>
    {
        public AP_Ui(ConsoleWindow window, AP_MConfig config)
            : base(window, config)
        {
        }

        public override Content Name => new Content("Adventurepatch", new ToolTip("The options menu for the Adventurepatches Mod"));
        public override void Build()
        {
            ScreenSegmentTable gameplaySettings = CreateTableSegment(2, 15);
            gameplaySettings.SqueezeTable = false;
            gameplaySettings.NameWhereApplicable = "General Settings";
            gameplaySettings.SpaceBelow = 20f;
            gameplaySettings.BackgroundStyleWhereApplicable = ConsoleStyles.Instance.Styles.Segments.OptionalSegmentDarkBackgroundWithHeader.Style;
            AdvLogger.LogInfo("Building Adventurepatch options screen");
            // Toggle and sliders
            gameplaySettings.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus, "Enemy spawn at preferred distance", "Allow enemies to spawn at their preferred engagement range.",
                (AP_MConfig I, bool b) => I.EnemySpawnDistancePatch = b,
                (AP_MConfig I) => I.EnemySpawnDistancePatch));

            gameplaySettings.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus, "Difficulty-based resource scaling", "Enable scaling of resource zone material amounts based on difficulty.",
                (AP_MConfig I, bool b) => I.ResourceZoneDiffScaling = b,
                (AP_MConfig I) => I.ResourceZoneDiffScaling));

            gameplaySettings.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus, "Allow fortress spawning", "Determines if Fortresses are allowed to spawn.",
                (AP_MConfig I, bool b) => I.SpawnFortress = b,
                (AP_MConfig I) => I.SpawnFortress));

            gameplaySettings.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus, "Ignore bell altitude checks", "Allows ringing the Bell regardless of Altitude.",
                (AP_MConfig I, bool b) => I.IgnoreAltitude = b,
                (AP_MConfig I) => I.IgnoreAltitude));

            

            gameplaySettings.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 1, 300, 1f, 60f,
                M.m((AP_MConfig I) => I.AdventureBellDelay),
                "Bell cooldown (seconds)",
                (AP_MConfig I, float f) => I.AdventureBellDelay = (int)f,
                new ToolTip("Cooldown of the Adventurebell in seconds.")));

            gameplaySettings.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 1, 5000, 50f, 500f,
                M.m((AP_MConfig I) => I.SpawnBonusDistance),
                "Engagement range bonus distance",
                (AP_MConfig I, float f) => I.SpawnBonusDistance = f,
                new ToolTip("Extra distance added to the engagement range.")));

            gameplaySettings.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 0, 10000, 100f, 2000f,
                M.m((AP_MConfig I) => I.MinimumSpawnrange),
                "Enemy Minimum spawn distance",
                (AP_MConfig I, float f) => I.MinimumSpawnrange = f,
                new ToolTip("Enemies spawn at least this far away.")));

            gameplaySettings.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 1, 1800, 30f, 900f,
                M.m((AP_MConfig I) => I.ResourceZoneClampedDrainTime),
                "Resource Zone drain time clamp (seconds)",
                (AP_MConfig I, float f) => I.ResourceZoneClampedDrainTime = (int)f,
                new ToolTip("Scaled Resource zones will take at most this time to drain entirely.")));

            gameplaySettings.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 0, 2000, 50f, 500f,
                M.m((AP_MConfig I) => I.BonusMaterialPerDifficultyLevel),
                "Bonus materials per difficulty level",
                (AP_MConfig I, float f) => I.BonusMaterialPerDifficultyLevel = f,
                new ToolTip("Extra reserve material in resource zones added for each difficulty level.")));

            ScreenSegmentTable sandBoxSettings = CreateTableSegment(2, 15);
            sandBoxSettings.SqueezeTable = false;
            sandBoxSettings.NameWhereApplicable = "Sandboxing Settings";
            sandBoxSettings.SpaceBelow = 20f;
            sandBoxSettings.BackgroundStyleWhereApplicable = ConsoleStyles.Instance.Styles.Segments.OptionalSegmentDarkBackgroundWithHeader.Style;

            
            sandBoxSettings.AddInterpretter(SubjectiveFloatClampedWithBarFromMiddle<AP_MConfig>.Quick(_focus, 0, 100, 5, 10,
                M.m((AP_MConfig I) => I.SpawnDifficulty),
                "Enemys spawn according to this difficuly.",
                (AP_MConfig I, float f) => I.SpawnDifficulty = (int)f,
                new ToolTip("Spawned enemies will spawn according to this difficulty instead.")));
            sandBoxSettings.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus, "Override Difficulty with custom slider", "Allows to set the difficulty of spawned enemies to the sliders value.",
                (AP_MConfig I, bool b) => I.OverrideSpawnDifficulty = b,
                (AP_MConfig I) => I.OverrideSpawnDifficulty));
            sandBoxSettings.AddInterpretter(SubjectiveToggle<AP_MConfig>.Quick(_focus, "Block random spawns", "Blocks random time based enemy spawns. The Bell still works.",
                (AP_MConfig I, bool b) => I.BlockRandomSpawns = b,
                (AP_MConfig I) => I.BlockRandomSpawns));

            AdvLogger.LogInfo("Adventurepatch options screen built successfully");



        }
    }
}
