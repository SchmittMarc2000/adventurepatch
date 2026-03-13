using BrilliantSkies.Core.Types;
using BrilliantSkies.PlayerProfiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventurePatch
{
    public class AP_MConfig : ProfileModule<AP_MConfig.InternalData>
    {
        public class InternalData
        {
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
            public bool IgnoreHeartstone { get; set; } = false;
            public bool AllowFreeze { get; set; } = false;
            public bool PreventDamage { get; set; } = false;
            public bool AllowEnemySpawnUI { get; set; } = false;
            public float EnemyDropPercentage { get; set; } = 10f;
            public float SpawnDelay { get; set; } = 2f;
            public float GracePeriod { get; set; } = 600f;
            public float SpawnTimeout { get; set; } = 600f;
            public bool AdjustWincon { get; set; } = true;
            public int maxEnemySpawns { get; set; } = 0;
            //public bool EnemyDropChanges { get; set; } = false;

        }
        public override ModuleType ModuleType => ModuleType.Options;
        protected override string FilenameAndExtension => "profile.APModConfig";
        public int AdventureBellDelay { get => Internal.AdventureBellDelay; set => Internal.AdventureBellDelay = value; }
        public bool EnemySpawnDistancePatch { get => Internal.EnemySpawnDistancePatch; set => Internal.EnemySpawnDistancePatch = value; }
        public float SpawnBonusDistance { get => Internal.SpawnBonusDistance; set => Internal.SpawnBonusDistance = value; }
        public float MinimumSpawnrange { get => Internal.MinimumSpawnrange; set => Internal.MinimumSpawnrange = value; }
        public bool ResourceZoneDiffScaling { get => Internal.ResourceZoneDiffScaling; set => Internal.ResourceZoneDiffScaling = value; }
        public int ResourceZoneClampedDrainTime { get => Internal.ResourceZoneClampedDrainTime; set => Internal.ResourceZoneClampedDrainTime = value; }
        public int ResourceZoneBaseMaterial { get => Internal.ResourceZoneBaseMaterial; set => Internal.ResourceZoneBaseMaterial = value; }
        public float BonusMaterialPerDifficultyLevel { get => Internal.BonusMaterialPerDifficultyLevel; set => Internal.BonusMaterialPerDifficultyLevel = value; }
        public bool SpawnFortress { get => Internal.SpawnFortress; set => Internal.SpawnFortress = value; }
        public bool IgnoreAltitude { get => Internal.IgnoreAltitude; set => Internal.IgnoreAltitude = value; }
        public bool BlockRandomSpawns { get => Internal.BlockRandomSpawns; set => Internal.BlockRandomSpawns = value; }
        public int SpawnDifficulty { get => Internal.SpawnDifficulty; set => Internal.SpawnDifficulty = value; }
        public bool OverrideSpawnDifficulty { get => Internal.OverrideSpawnDifficulty; set => Internal.OverrideSpawnDifficulty = value; }
        public bool AllowSandboxing { get => Internal.AllowSandboxing; set => Internal.AllowSandboxing = value; }
        public bool ForceEnemySpawns { get => Internal.ForceEnemySpawns; set => Internal.ForceEnemySpawns = value; }
        public uint EnemySpawnDelay { get => Internal.EnemySpawnDelay; set => Internal.EnemySpawnDelay = value; }
        public uint MaxEnemyVolume { get => Internal.MaxEnemyVolume; set => Internal.MaxEnemyVolume = value; }
        public uint MaxEnemyCount { get => Internal.MaxEnemyCount; set => Internal.MaxEnemyCount = value; }
        public bool EnableCustomEncounters { get => Internal.EnableCustomEncounters; set => Internal.EnableCustomEncounters = value; }
        public uint CustomEncounterSpawnChance { get => Internal.CustomEncounterSpawnChance; set => Internal.CustomEncounterSpawnChance = value; }
        public bool IgnoreHeartstone { get => Internal.IgnoreHeartstone; set => Internal.IgnoreHeartstone = value; }
        public bool AllowFreeze { get => Internal.AllowFreeze; set => Internal.AllowFreeze = value; }
        public bool PreventDamage { get => Internal.PreventDamage; set => Internal.PreventDamage = value; }
        public bool AllowEnemySpawnUI { get => Internal.AllowEnemySpawnUI; set => Internal.AllowEnemySpawnUI = value; }
        public float EnemyDropPercentage { get => Internal.EnemyDropPercentage; set => Internal.EnemyDropPercentage = value; }
        public float GracePeriod { get => Internal.GracePeriod; set => Internal.GracePeriod = value; }
        public float SpawnDelay { get => Internal.SpawnDelay; set => Internal.SpawnDelay = value; }
        //public bool EnemyDropChanges { get => Internal.EnemyDropChanges; set => Internal.EnemyDropChanges = value; }

        public bool AdjustWincon { get => Internal.AdjustWincon; set => Internal.AdjustWincon = value; }
        public float SpawnTimeout { get => Internal.SpawnTimeout; set => Internal.SpawnTimeout = value; }
        public int maxEnemySpawns { get => Internal.maxEnemySpawns; set => Internal.maxEnemySpawns = value; }
    }
}
