using Newtonsoft.Json;
using System;
using System.IO;

//namespace AdventurePatch
//{
//    public class ModSettings
//    {  

//        public uint AdventureBellDelay { get; set; } = 60;
//        public bool EnemySpawnDistancePatch { get; set; } = true;
//        public float SpawnBonusDistance { get; set; } = 500.0f;
//        public float MinimumSpawnrange { get; set; } = 1500.0f;
//        public uint AdventureWarperDelay { get; set; } = 1800;
//        public bool ResourceZoneDiffScaling { get; set; } = true;
//        public uint ResourceZoneClampedDrainTime { get; set; } = 900;
//        public float BonusMaterialPerDifficultyLevel { get; set; } = 800.0f;
//        public bool SpawnFortress { get; set; } = true;

//        private static string settingsFilePath = Path.Combine(
//            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
//            "From The Depths", "Mods", "AdventurePatch", "settings.json");
//        public static ModSettings LoadSettings()
//        {
//            if (!File.Exists(settingsFilePath))
//            {
//                var defaultSettings = new ModSettings();
//                Directory.CreateDirectory(Path.GetDirectoryName(settingsFilePath));
//                File.WriteAllText(settingsFilePath, JsonConvert.SerializeObject(defaultSettings, Formatting.Indented));
//                return defaultSettings;
//            }
//            try
//            {
//                string json = File.ReadAllText(settingsFilePath);
//                ModSettings settings = JsonConvert.DeserializeObject<ModSettings>(json);
//                if (settings == null)
//                {
//                    settings = new ModSettings();
//                }
//                return settings;
//            }
//            catch (Exception ex)
//            {
//                return new ModSettings();
//            }
//        }

//        public static ModSettings Reload()
//        {
//            return LoadSettings();
//        }
//    }
//}
using System;
using System.IO;
using Newtonsoft.Json;
using BrilliantSkies.Core.Logger;

namespace AdventurePatch
{
    public class Setting<T>
    {
        public T Value { get; set; }
        public string Description { get; set; }

        public Setting() { }

        public Setting(T value, string description)
        {
            Value = value;
            Description = description;
        }

        public static implicit operator T(Setting<T> setting) => setting.Value;
    }

    public class ModSettings
    {
        public Setting<uint> AdventureBellDelay { get; set; } = new Setting<uint>(
            60,
            "Cooldown of the Adventurebell. Default: 60"
        );

        public Setting<bool> EnemySpawnDistancePatch { get; set; } = new Setting<bool>(
            true,
            "Allow enemies to spawn at their Prefered engagement range. Default: true"
        );

        public Setting<float> SpawnBonusDistance { get; set; } = new Setting<float>(
            500.0f,
            "Extra distance added to the engagement range. Default: 500"
        );

        public Setting<float> MinimumSpawnrange { get; set; } = new Setting<float>(
            1500.0f,
            "Enforces enemies are at least this far away when spawning them in. Default: 1500"
        );

        //public Setting<uint> AdventureWarperDelay { get; set; } = new Setting<uint>(
        //    1800,
        //    "Required time in current difficulty to allow warping. Default: 1800"
        //);

        public Setting<bool> ResourceZoneDiffScaling { get; set; } = new Setting<bool>(
            true,
            "Enable scaling of resource zone material amounts based on difficulty. Default: true"
        );

        public Setting<uint> ResourceZoneClampedDrainTime { get; set; } = new Setting<uint>(
            900,
            "Sets the materials gained per second so that it takes at most this time to drain it entirely. Default: 900"
        );

        public Setting<float> BonusMaterialPerDifficultyLevel { get; set; } = new Setting<float>(
            800.0f,
            "Extra reserve material gained per difficulty level in resource zones. Default: 800"
        );

        public Setting<bool> SpawnFortress { get; set; } = new Setting<bool>(
            true,
            "Determines if Fortresses are allowed to spawn. Default: true"
        );

        private static readonly string settingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "From The Depths", "Mods", "AdventurePatch", "settings.json"
        );

        public static ModSettings LoadSettings()
        {
            if (!File.Exists(settingsFilePath))
            {
                var defaultSettings = new ModSettings();
                Directory.CreateDirectory(Path.GetDirectoryName(settingsFilePath));
                File.WriteAllText(settingsFilePath, JsonConvert.SerializeObject(defaultSettings, Formatting.Indented));
                return defaultSettings;
            }

            try
            {
                string json = File.ReadAllText(settingsFilePath);
                ModSettings settings = JsonConvert.DeserializeObject<ModSettings>(json);
                //AdvLogger.LogEvent($"settings loaded: " +
                //$"AdventureBellDelay={settings.AdventureBellDelay}, " +
                //$"EnemySpawnDistancePatch={settings.EnemySpawnDistancePatch}, " +
                //$"SpawnBonusDistance={settings.SpawnBonusDistance}, " +
                //$"MinimumSpawnrange={settings.MinimumSpawnrange}, " +
                //$"AdventureWarperDelay={settings.AdventureWarperDelay}, " +
                //$"ResourceZoneDiffScaling={settings.ResourceZoneDiffScaling}, " +
                //$"ResourceZoneClampedDrainTime={settings.ResourceZoneClampedDrainTime}, " +
                //$"BonusMaterialPerDifficultyLevel={settings.BonusMaterialPerDifficultyLevel}, " +
                //$"SpawnFortress={settings.SpawnFortress}");
                return settings ?? new ModSettings();

            }
            catch (Exception)
            {
                return new ModSettings();
            }

        }

        public static ModSettings Reload()
        {
            return LoadSettings();
        }
    }
}
