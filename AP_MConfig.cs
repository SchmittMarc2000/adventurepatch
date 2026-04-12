using BrilliantSkies.Core.Types;
using BrilliantSkies.PlayerProfiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
            public int ScalingOffset {  get; set; } = -5;
            public int maxEnemySpawns { get; set; } = 0;
            public bool waveMode { get; set; } = false;

            public bool challengeMode { get; set; } = false;
            public float waveDuration { get; set; } = 360f;
            public float spawnDelay { get; set; } = 15f;
            public int difficultyLevel { get; set; } = 0;
            public bool autoIncreaseDifficulty { get; set; } = true;
            public float autoIncreaseTime { get; set; } = 30f;

            public int bonusStartingMaterial { get; set; } = 0;
            public KeyCode StartWaveKey { get; set; } = KeyCode.Return;
            public KeyCode StopWaveKey { get; set; } = (KeyCode)289;

            public bool testDistribution { get; set; } = false;

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
        public bool MaterialScaling { get => Internal.MaterialScaling; set => Internal.MaterialScaling = value; }
        public int ScalingOffset { get => Internal.ScalingOffset; set => Internal.ScalingOffset = value; }
        public float SpawnTimeout { get => Internal.SpawnTimeout; set => Internal.SpawnTimeout = value; }
        public int maxEnemySpawns { get => Internal.maxEnemySpawns; set => Internal.maxEnemySpawns = value; }

        public bool waveMode { get => Internal.waveMode; set => Internal.waveMode = value; }
        public bool challengeMode { get => Internal.challengeMode; set => Internal.challengeMode = value; }
        public float waveDuration { get => Internal.waveDuration; set => Internal.waveDuration = value; }
        public float spawnDelay { get => Internal.spawnDelay; set => Internal.spawnDelay = value; }
        public int difficultyLevel { get => Internal.difficultyLevel; set => Internal.difficultyLevel = value; }

        public int bonusStartingMaterial { get => Internal.bonusStartingMaterial; set => Internal.bonusStartingMaterial = value; }
        public bool autoIncreaseDifficulty { get => Internal.autoIncreaseDifficulty; set => Internal.autoIncreaseDifficulty = value; }

        public float autoIncreaseTime { get => Internal.autoIncreaseTime; set => Internal.autoIncreaseTime = value; }
        public KeyCode StartWaveKey { get => Internal.StartWaveKey; set => Internal.StartWaveKey = value; }
        public KeyCode StopWaveKey { get => Internal.StopWaveKey; set => Internal.StopWaveKey = value; }

        public bool testDistribution { get => Internal.testDistribution; set => Internal.testDistribution = value; }

    }
}
