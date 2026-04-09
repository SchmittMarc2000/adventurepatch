using BrilliantSkies.Core.Types;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Ui.Consoles;
using BrilliantSkies.Ui.Consoles.Getters;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Buttons;
using BrilliantSkies.Ui.Consoles.Segments;
using BrilliantSkies.Ui.Consoles.Styles;
using BrilliantSkies.Ui.Tips;
using System;

namespace AdventurePatch
{
    public class QuickSaveUi : SuperScreen<AP_MConfig>
    {
        public QuickSaveUi(ConsoleWindow window, AP_MConfig config)
            : base(window, config)
        {
        }

        public override Content Name => new Content("Quick Saves", new ToolTip("Quick save and load your adventure progress"));

        public override void Build()
        {
            ScreenSegmentTable actions = CreateTableSegment(1, 2);
            actions.SqueezeTable = false;
            actions.NameWhereApplicable = "Quick Save";
            actions.SpaceBelow = 40f;
            actions.BackgroundStyleWhereApplicable = ConsoleStyles.Instance.Styles.Segments.OptionalSegmentDarkBackgroundWithHeader.Style;

            actions.AddInterpretter(SubjectiveButton<AP_MConfig>.Quick(
                _focus,
                "Quick Save Now",
                new ToolTip("Save the current adventure state to a new quick save slot."),
                _ =>
                {
                    bool ok = AdventureQuickSaveSystem.QuickSave();
                    NetworkedInfoStore.Add(ok ? "Quick save successful." : "Quick save failed – check the log.");
                    TriggerScreenRebuild();
                }));

            string[] saves = AdventureQuickSaveSystem.GetQuickSaveList();

            ScreenSegmentTable saveList = CreateTableSegment(3, Math.Max(saves.Length, 1));
            saveList.SqueezeTable = false;
            saveList.NameWhereApplicable = saves.Length == 0
                ? "No quick saves found"
                : $"Quick Saves ({saves.Length})";
            saveList.SpaceBelow = 40f;
            saveList.BackgroundStyleWhereApplicable = ConsoleStyles.Instance.Styles.Segments.OptionalSegmentDarkBackgroundWithHeader.Style;

            if (saves.Length == 0)
            {
                saveList.AddInterpretter(new SubjectiveDisplay<AP_MConfig>(
                    _focus,
                    M.m<AP_MConfig>(_ => "No quick saves yet. Use 'Quick Save Now' to create one."),
                    M.m<AP_MConfig>(new ToolTip(""))));
            }
            else
            {
                foreach (var save in AdventureQuickSaveSystem.GetQuickSaveListWithDetails())
                {
                    string display = save.DisplayName;

                    saveList.AddInterpretter(new SubjectiveDisplay<AP_MConfig>(
                        _focus,
                        M.m<AP_MConfig>(_ => display),
                        M.m<AP_MConfig>(new ToolTip($"Slot: {save.Slot}\nFiles: {save.FileCount}\nPath: {save.FullPath}"))));

                    // Restore button
                    saveList.AddInterpretter(SubjectiveButton<AP_MConfig>.Quick(
                        _focus,
                        "Restore to New Slot",
                        new ToolTip($"Restore this save to a free adventure slot"),
                        _ =>
                        {
                            AdventureQuickSaveSystem.RestoreQuickSaveToSlot(save.SaveName);
                            TriggerScreenRebuild();
                        }));

                    // Delete button
                    saveList.AddInterpretter(SubjectiveButton<AP_MConfig>.Quick(
                        _focus,
                        "Delete",
                        new ToolTip($"Delete save: {display}"),
                        _ =>
                        {
                            AdventureQuickSaveSystem.DeleteQuickSave(save.SaveName, save.Slot);
                            TriggerScreenRebuild();
                        }));
                }
            }
        }
    }
}