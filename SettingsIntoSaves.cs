using Assets.Scripts.Persistence;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Ftd.Modes.MainMenu.Ui;
using BrilliantSkies.Ftd.Persistence.PlanetFileSaving;
using BrilliantSkies.Ftd.Planets;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Ui.Special.PopUps;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace AdventurePatch
{
    public class HeaderInfo
    {
        public int Id { get; set; }
    }

    //public class AdventureSaveData
    //{
    //    public int AdventureBellDelay { get; set; } = 60;
    //    public bool EnemySpawnDistancePatch { get; set; } = true;
    //    public float SpawnBonusDistance { get; set; } = 500f;
    //    public float MinimumSpawnrange { get; set; } = 2000f;
    //    public bool ResourceZoneDiffScaling { get; set; } = true;
    //    public int ResourceZoneClampedDrainTime { get; set; } = 900;
    //    public int ResourceZoneBaseMaterial { get; set; } = 30000;
    //    public float BonusMaterialPerDifficultyLevel { get; set; } = 500f;
    //    public bool SpawnFortress { get; set; } = false;
    //    public bool IgnoreAltitude { get; set; } = true;
    //    public bool BlockRandomSpawns { get; set; } = false;
    //    public bool OverrideSpawnDifficulty { get; set; } = false;
    //    public int SpawnDifficulty { get; set; } = 0;
    //    public bool AllowSandboxing { get; set; } = false;
    //    public bool ForceEnemySpawns { get; set; } = false;
    //    public uint EnemySpawnDelay { get; set; } = 60;
    //    public uint MaxEnemyVolume { get; set; } = 30000;
    //    public uint MaxEnemyCount { get; set; } = 30;
    //    public bool EnableCustomEncounters { get; set; } = false;
    //    public uint CustomEncounterSpawnChance { get; set; } = 20;
    //    public bool IgnoreHeartstone { get; set; } = false;
    //    public bool AllowFreeze { get; set; } = false;
    //    public bool PreventDamage { get; set; } = false;
    //    public bool AllowEnemySpawnUI { get; set; } = false;
    //    public float EnemyDropPercentage { get; set; } = 10f;
    //    public float SpawnDelay { get; set; } = 2f;
    //    public float GracePeriod { get; set; } = 600f;
    //    public float SpawnTimeout { get; set; } = 600f;
    //    public bool AdjustWincon { get; set; } = true;
    //    public int maxEnemySpawns { get; set; } = 0;

    //    public static AdventureSaveData FromConfig(AP_MConfig config)
    //    {
    //        return new AdventureSaveData
    //        {
    //            AdventureBellDelay = config.AdventureBellDelay,
    //            EnemySpawnDistancePatch = config.EnemySpawnDistancePatch,
    //            SpawnBonusDistance = config.SpawnBonusDistance,
    //            MinimumSpawnrange = config.MinimumSpawnrange,
    //            ResourceZoneDiffScaling = config.ResourceZoneDiffScaling,
    //            ResourceZoneClampedDrainTime = config.ResourceZoneClampedDrainTime,
    //            ResourceZoneBaseMaterial = config.ResourceZoneBaseMaterial,
    //            BonusMaterialPerDifficultyLevel = config.BonusMaterialPerDifficultyLevel,
    //            SpawnFortress = config.SpawnFortress,
    //            IgnoreAltitude = config.IgnoreAltitude,
    //            BlockRandomSpawns = config.BlockRandomSpawns,
    //            OverrideSpawnDifficulty = config.OverrideSpawnDifficulty,
    //            SpawnDifficulty = config.SpawnDifficulty,
    //            AllowSandboxing = config.AllowSandboxing,
    //            ForceEnemySpawns = config.ForceEnemySpawns,
    //            EnemySpawnDelay = config.EnemySpawnDelay,
    //            MaxEnemyVolume = config.MaxEnemyVolume,
    //            MaxEnemyCount = config.MaxEnemyCount,
    //            EnableCustomEncounters = config.EnableCustomEncounters,
    //            CustomEncounterSpawnChance = config.CustomEncounterSpawnChance,
    //            IgnoreHeartstone = config.IgnoreHeartstone,
    //            AllowFreeze = config.AllowFreeze,
    //            PreventDamage = config.PreventDamage,
    //            AllowEnemySpawnUI = config.AllowEnemySpawnUI,
    //            EnemyDropPercentage = config.EnemyDropPercentage,
    //            SpawnDelay = config.SpawnDelay,
    //            GracePeriod = config.GracePeriod,
    //            SpawnTimeout = config.SpawnTimeout,
    //            AdjustWincon = config.AdjustWincon,
    //            maxEnemySpawns = config.maxEnemySpawns
    //        };
    //    }

    //    // Apply save data to config
    //    public void ApplyToConfig(AP_MConfig config)
    //    {
    //        config.AdventureBellDelay = this.AdventureBellDelay;
    //        config.EnemySpawnDistancePatch = this.EnemySpawnDistancePatch;
    //        config.SpawnBonusDistance = this.SpawnBonusDistance;
    //        config.MinimumSpawnrange = this.MinimumSpawnrange;
    //        config.ResourceZoneDiffScaling = this.ResourceZoneDiffScaling;
    //        config.ResourceZoneClampedDrainTime = this.ResourceZoneClampedDrainTime;
    //        config.ResourceZoneBaseMaterial = this.ResourceZoneBaseMaterial;
    //        config.BonusMaterialPerDifficultyLevel = this.BonusMaterialPerDifficultyLevel;
    //        config.SpawnFortress = this.SpawnFortress;
    //        config.IgnoreAltitude = this.IgnoreAltitude;
    //        config.BlockRandomSpawns = this.BlockRandomSpawns;
    //        config.OverrideSpawnDifficulty = this.OverrideSpawnDifficulty;
    //        config.SpawnDifficulty = this.SpawnDifficulty;
    //        config.AllowSandboxing = this.AllowSandboxing;
    //        config.ForceEnemySpawns = this.ForceEnemySpawns;
    //        config.EnemySpawnDelay = this.EnemySpawnDelay;
    //        config.MaxEnemyVolume = this.MaxEnemyVolume;
    //        config.MaxEnemyCount = this.MaxEnemyCount;
    //        config.EnableCustomEncounters = this.EnableCustomEncounters;
    //        config.CustomEncounterSpawnChance = this.CustomEncounterSpawnChance;
    //        config.IgnoreHeartstone = this.IgnoreHeartstone;
    //        config.AllowFreeze = this.AllowFreeze;
    //        config.PreventDamage = this.PreventDamage;
    //        config.AllowEnemySpawnUI = this.AllowEnemySpawnUI;
    //        config.EnemyDropPercentage = this.EnemyDropPercentage;
    //        config.SpawnDelay = this.SpawnDelay;
    //        config.GracePeriod = this.GracePeriod;
    //        config.SpawnTimeout = this.SpawnTimeout;
    //        config.AdjustWincon = this.AdjustWincon;
    //        config.maxEnemySpawns = this.maxEnemySpawns;
    //    }
    //}
    public class AdventureSaveData
    {
        // Mirror all properties from AP_MConfig.InternalData
        public int AdventureBellDelay { get; set; } = 60;
        public bool EnemySpawnDistancePatch { get; set; } = true;
        public float SpawnBonusDistance { get; set; } = 500f;
        public float MinimumSpawnrange { get; set; } = 2000f;
        public bool ResourceZoneDiffScaling { get; set; } = true;
        public int ResourceZoneClampedDrainTime { get; set; } = 900;
        public int ResourceZoneBaseMaterial { get; set; } = 30000;
        public float BonusMaterialPerDifficultyLevel { get; set; } = 500f;
        public bool SpawnFortress { get; set; } = false;
        public bool IgnoreAltitude { get; set; } = true;
        public bool BlockRandomSpawns { get; set; } = false;
        public bool OverrideSpawnDifficulty { get; set; } = false;
        public int SpawnDifficulty { get; set; } = 0;
        public bool AllowSandboxing { get; set; } = false;
        public bool ForceEnemySpawns { get; set; } = false;
        public uint EnemySpawnDelay { get; set; } = 60;
        public uint MaxEnemyVolume { get; set; } = 30000;
        public uint MaxEnemyCount { get; set; } = 30;
        public bool EnableCustomEncounters { get; set; } = false;
        public uint CustomEncounterSpawnChance { get; set; } = 20;
        public bool IgnoreHeartstone { get; set; } = true;
        public bool AllowFreeze { get; set; } = true;
        public bool PreventDamage { get; set; } = true;
        public bool AllowEnemySpawnUI { get; set; } = false;
        public float EnemyDropPercentage { get; set; } = 10f;
        public float SpawnDelay { get; set; } = 2f;
        public float GracePeriod { get; set; } = 600f;
        public float SpawnTimeout { get; set; } = 600f;
        public bool AdjustWincon { get; set; } = true;
        public bool MaterialScaling { get; set; } = false;
        public int ScalingOffset { get; set; } = -5;
        public int maxEnemySpawns { get; set; } = 0;

        // Wave mode settings
        public bool waveMode { get; set; } = false;
        public bool challengeMode { get; set; } = false;
        public float waveDuration { get; set; } = 360f;
        public float spawnDelayWave { get; set; } = 15f;
        public int difficultyLevel { get; set; } = 0;
        public bool autoIncreaseDifficulty { get; set; } = false;
        public float autoIncreaseTime { get; set; } = 30f;
        public int bonusStartingMaterial { get; set; } = 0;

        // Wave runtime state - just the current wave number
        public int CurrentWaveNumber { get; set; } = 0;

        // Convert from AP_MConfig to save data
        public static AdventureSaveData FromConfig(AP_MConfig config)
        {
            return new AdventureSaveData
            {
                AdventureBellDelay = config.AdventureBellDelay,
                EnemySpawnDistancePatch = config.EnemySpawnDistancePatch,
                SpawnBonusDistance = config.SpawnBonusDistance,
                MinimumSpawnrange = config.MinimumSpawnrange,
                ResourceZoneDiffScaling = config.ResourceZoneDiffScaling,
                ResourceZoneClampedDrainTime = config.ResourceZoneClampedDrainTime,
                ResourceZoneBaseMaterial = config.ResourceZoneBaseMaterial,
                BonusMaterialPerDifficultyLevel = config.BonusMaterialPerDifficultyLevel,
                SpawnFortress = config.SpawnFortress,
                IgnoreAltitude = config.IgnoreAltitude,
                BlockRandomSpawns = config.BlockRandomSpawns,
                OverrideSpawnDifficulty = config.OverrideSpawnDifficulty,
                SpawnDifficulty = config.SpawnDifficulty,
                AllowSandboxing = config.AllowSandboxing,
                ForceEnemySpawns = config.ForceEnemySpawns,
                EnemySpawnDelay = config.EnemySpawnDelay,
                MaxEnemyVolume = config.MaxEnemyVolume,
                MaxEnemyCount = config.MaxEnemyCount,
                EnableCustomEncounters = config.EnableCustomEncounters,
                CustomEncounterSpawnChance = config.CustomEncounterSpawnChance,
                IgnoreHeartstone = config.IgnoreHeartstone,
                AllowFreeze = config.AllowFreeze,
                PreventDamage = config.PreventDamage,
                AllowEnemySpawnUI = config.AllowEnemySpawnUI,
                EnemyDropPercentage = config.EnemyDropPercentage,
                SpawnDelay = config.SpawnDelay,
                GracePeriod = config.GracePeriod,
                SpawnTimeout = config.SpawnTimeout,
                AdjustWincon = config.AdjustWincon,
                MaterialScaling = config.MaterialScaling,
                ScalingOffset = config.ScalingOffset,
                maxEnemySpawns = config.maxEnemySpawns,
                waveMode = config.waveMode,
                challengeMode = config.challengeMode,
                waveDuration = config.waveDuration,
                spawnDelayWave = config.spawnDelay,
                difficultyLevel = config.difficultyLevel,
                autoIncreaseDifficulty = config.autoIncreaseDifficulty,
                autoIncreaseTime = config.autoIncreaseTime,
                bonusStartingMaterial = config.bonusStartingMaterial,
                CurrentWaveNumber = SpawnWaveMode.getWaveCount()
            };
        }

        public void ApplyToConfig(AP_MConfig config)
        {
            config.AdventureBellDelay = this.AdventureBellDelay;
            config.EnemySpawnDistancePatch = this.EnemySpawnDistancePatch;
            config.SpawnBonusDistance = this.SpawnBonusDistance;
            config.MinimumSpawnrange = this.MinimumSpawnrange;
            config.ResourceZoneDiffScaling = this.ResourceZoneDiffScaling;
            config.ResourceZoneClampedDrainTime = this.ResourceZoneClampedDrainTime;
            config.ResourceZoneBaseMaterial = this.ResourceZoneBaseMaterial;
            config.BonusMaterialPerDifficultyLevel = this.BonusMaterialPerDifficultyLevel;
            config.SpawnFortress = this.SpawnFortress;
            config.IgnoreAltitude = this.IgnoreAltitude;
            config.BlockRandomSpawns = this.BlockRandomSpawns;
            config.OverrideSpawnDifficulty = this.OverrideSpawnDifficulty;
            config.SpawnDifficulty = this.SpawnDifficulty;
            config.AllowSandboxing = this.AllowSandboxing;
            config.ForceEnemySpawns = this.ForceEnemySpawns;
            config.EnemySpawnDelay = this.EnemySpawnDelay;
            config.MaxEnemyVolume = this.MaxEnemyVolume;
            config.MaxEnemyCount = this.MaxEnemyCount;
            config.EnableCustomEncounters = this.EnableCustomEncounters;
            config.CustomEncounterSpawnChance = this.CustomEncounterSpawnChance;
            config.IgnoreHeartstone = this.IgnoreHeartstone;
            config.AllowFreeze = this.AllowFreeze;
            config.PreventDamage = this.PreventDamage;
            config.AllowEnemySpawnUI = this.AllowEnemySpawnUI;
            config.EnemyDropPercentage = this.EnemyDropPercentage;
            config.SpawnDelay = this.SpawnDelay;
            config.GracePeriod = this.GracePeriod;
            config.SpawnTimeout = this.SpawnTimeout;
            config.AdjustWincon = this.AdjustWincon;
            config.MaterialScaling = this.MaterialScaling;
            config.ScalingOffset = this.ScalingOffset;
            config.maxEnemySpawns = this.maxEnemySpawns;
            config.waveMode = this.waveMode;
            config.challengeMode = this.challengeMode;
            config.waveDuration = this.waveDuration;
            config.spawnDelay = this.spawnDelayWave;
            config.difficultyLevel = this.difficultyLevel;
            config.autoIncreaseDifficulty = this.autoIncreaseDifficulty;
            config.autoIncreaseTime = this.autoIncreaseTime;
            config.bonusStartingMaterial = this.bonusStartingMaterial;
            AdvLogger.LogInfo("Restored settings, trying to restore wave number now.");
            // Restore the wave count
            SpawnWaveMode.setWaveCount(this.CurrentWaveNumber);
        }
    }

    // Helper class for save/load operations
    public static class AdventureSaveManager
    {
        private static AdventureSaveData _backupSettings;
        private static bool _hasBackup = false;

        // Save current config to file
        public static void SaveModData(PlanetFile planetFile, int slot, int adventureId, AP_MConfig config)
        {
            try
            {
                string saveFolder = planetFile.GetSaveSlotFolder(slot, SaveFileType.Adventure).FullName;
                string modDataFolder = Path.Combine(saveFolder, "ModData");

                if (!Directory.Exists(modDataFolder))
                {
                    Directory.CreateDirectory(modDataFolder);
                }

                var saveData = AdventureSaveData.FromConfig(config);
                string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
                string savePath = Path.Combine(modDataFolder, $"AdventurePatch_{adventureId}.json");

                File.WriteAllText(savePath, json, Encoding.UTF8);
                AdvLogger.LogInfo($"[AdventurePatch] Saved mod settings for adventure {adventureId}");
            }
            catch (Exception ex)
            {
                AdvLogger.LogInfo($"[AdventurePatch] Failed to save: {ex.Message}");
            }
        }

        // Load settings from file if they exist
        public static void LoadModDataIfExists(PlanetFile planetFile, int slot, int adventureId, AP_MConfig config)
        {
            try
            {
                string saveFolder = planetFile.GetSaveSlotFolder(slot, SaveFileType.Adventure).FullName;
                string modDataPath = Path.Combine(saveFolder, "ModData", $"AdventurePatch_{adventureId}.json");

                if (File.Exists(modDataPath))
                {
                    // Backup current settings before loading
                    _backupSettings = AdventureSaveData.FromConfig(config);
                    _hasBackup = true;

                    // Load and apply saved settings
                    string json = File.ReadAllText(modDataPath, Encoding.UTF8);
                    var saveData = JsonConvert.DeserializeObject<AdventureSaveData>(json);

                    if (saveData != null)
                    {
                        saveData.ApplyToConfig(config);
                        AdvLogger.LogInfo($"[AdventurePatch] Loaded settings for adventure {adventureId}");

                        // Notify user that settings were loaded
                        //GuiPopUp.Instance.Add(new PopupInfo(
                        //    "AdventurePatch Settings Loaded",
                        //    "Mod settings have been loaded for this adventure."
                        //));
                    }
                }
            }
            catch (Exception ex)
            {
                AdvLogger.LogInfo($"[AdventurePatch] Failed to load: {ex.Message}");
            }
        }

        // Restore backup settings (call when returning to main menu)
        public static void RestoreBackupSettings(AP_MConfig config)
        {
            if (_hasBackup && _backupSettings != null)
            {
                _backupSettings.ApplyToConfig(config);
                _hasBackup = false;
                _backupSettings = null;
                AdvLogger.LogInfo("[AdventurePatch] Restored backup settings on main menu return");
            }
        }
    }

    // Save patch - works when player manually saves
    [HarmonyPatch(typeof(BrilliantSkies.Ftd.Modes.MainMenu.Ui.MainMenu))]
    [HarmonyPatch("SaveAdventure")]
    public class SaveAdventurePatch
    {
        static void Postfix()
        {
            try
            {
                var instance = InstanceSpecification.i;
                if (instance == null || !instance.Header.IsAdventure) return;

                int slot = instance.Header.Id.Id;
                int adventureId = instance.Header.Id.Id;

                var planetFile = Planet.i?.File;
                if (planetFile == null) return;

                var config = ProfileManager.Instance?.GetModule<AP_MConfig>();
                if (config == null) return;

                AdventureSaveManager.SaveModData(planetFile, slot, adventureId, config);
            }
            catch (Exception ex)
            {
                AdvLogger.LogInfo($"[AdventurePatch] Save failed: {ex.Message}");
            }
        }
    }

    // Patch the OpenSaveSlot method - FIXED parameter names to match the original method
    [HarmonyPatch(typeof(PlanetFileSaveLoad))]
    [HarmonyPatch("OpenSaveSlot")]
    public class LoadAdventurePatch
    {
        static void Postfix(PlanetFile file, int i, SaveFileType type, ref bool __result)
        {
            try
            {
                if (type != SaveFileType.Adventure || !__result) return;

                // After successful load, the instance should be available
                var instance = InstanceSpecification.i;
                if (instance == null || !instance.Header.IsAdventure) return;

                int adventureId = instance.Header.Id.Id;
                var config = ProfileManager.Instance?.GetModule<AP_MConfig>();
                if (config == null) return;

                // Load settings for this adventure - use 'i' as the slot number
                AdventureSaveManager.LoadModDataIfExists(file, i, adventureId, config);
            }
            catch (Exception ex)
            {
                AdvLogger.LogInfo($"[AdventurePatch] Load check failed: {ex.Message}");
            }
        }
    }

    // Patch the ReturnToMainMenu method to restore backup settings
    [HarmonyPatch(typeof(BrilliantSkies.Ftd.Modes.MainMenu.Ui.MainMenu))]
    [HarmonyPatch("ReturnToMainMenu")]
    public class ReturnToMainMenuPatch
    {
        static void Prefix()
        {
            try
            {
                var config = ProfileManager.Instance?.GetModule<AP_MConfig>();
                if (config != null)
                {
                    if (config.waveMode) SpawnWaveMode.StopWaveMode();
                    AdventureSaveManager.RestoreBackupSettings(config);
                }
            }
            catch (Exception ex)
            {
                AdvLogger.LogInfo($"[AdventurePatch] Backup restore failed: {ex.Message}");
            }
        }
    }

    // Also patch ReturnToMainMenuAsClient and ReturnToMultiplayerLobby for completeness
    [HarmonyPatch(typeof(BrilliantSkies.Ftd.Modes.MainMenu.Ui.MainMenu))]
    [HarmonyPatch("ReturnToMainMenuAsClient")]
    public class ReturnToMainMenuAsClientPatch
    {
        static void Prefix()
        {
            try
            {
                var config = ProfileManager.Instance?.GetModule<AP_MConfig>();
                if (config != null)
                {
                    AdventureSaveManager.RestoreBackupSettings(config);
                }
            }
            catch (Exception ex)
            {
                AdvLogger.LogInfo($"[AdventurePatch] Backup restore failed: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(BrilliantSkies.Ftd.Modes.MainMenu.Ui.MainMenu))]
    [HarmonyPatch("ReturnToMultiplayerLobby")]
    public class ReturnToMultiplayerLobbyPatch
    {
        static void Prefix()
        {
            try
            {
                var config = ProfileManager.Instance?.GetModule<AP_MConfig>();
                if (config != null)
                {
                    AdventureSaveManager.RestoreBackupSettings(config);
                }
            }
            catch (Exception ex)
            {
                AdvLogger.LogInfo($"[AdventurePatch] Backup restore failed: {ex.Message}");
            }
        }
    }
}