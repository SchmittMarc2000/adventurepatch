using BrilliantSkies.Core.Logger;
using BrilliantSkies.Ftd.Planets.Factions.Designs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventurePatch
{
    public enum EnemyType
    {
        Ship,
        Flying,
        Space,
        Submarine,
        Installation,
        All,
    }
    /*
    {
    None,
    Plane,
    Ship,
    Submarine,
    Spacecraft,
    Ballooncraft,
    Monster,
    Fleet,
    Installation,
    Thrustercraft,
    Helicopters,
    DroneOrTarget,
    Building,
    Experiment,
    Tanks,
    NumberOfEnums,
    AllVehicles
    }
    */
    public class CustomEncounterEnemy
    {
        public EnemyType Type { get; set; }
        public string Name { get; set; } // Optional: Name for the enemy type
        public CustomEncounterEnemy(EnemyType type, string name = "not set")
        {
            Type = type;
            Name = name;
        }
    }
    public class CustomEncounterRules
    {

    }
    public class CustomEncounter
    {
        /// <summary>
        /// The enemies that spawn in this encounter.
        /// </summary>
        public List<CustomEncounterEnemy> Enemies { get; set; } = new List<CustomEncounterEnemy>();
        /// <summary>
        /// The display name or identifier of the enemy group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// How many enemies to spawn. Generated via the length of the enemies list.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Multiplier applied to base difficulty when evaluating spawns.
        /// </summary>
        public float PrimaryDifficultyFactor { get; set; }

        /// <summary>
        /// How much the difficulty factor decreases for each subsequent spawn.
        /// </summary>
        public float DifficultyFactorDropoff { get; set; }

        /// <summary>
        /// Minimum allowed difficulty value for spawned encounters.
        /// </summary>
        public float MinDifficutly { get; set; } = 0;

        /// <summary>
        /// Whether to prevent spawning the same design multiple times.
        /// </summary>
        public bool FilterDuplicates { get; set; } = true;

        /// <summary>
        /// If true, all spawned enemies will come from the same faction.
        /// </summary>
        public bool ForceSameFaction { get; set; } = true;
        public static bool ShouldSpawn (EnemyType type, WorldSpecificationFactionDesign t)
        {
            switch (type)
            {
                case EnemyType.Ship:
                    return t.BlueprintType == enumBlueprintType.Ship;
                case EnemyType.Flying:
                    return new[] {
                        enumBlueprintType.Thrustercraft,
                        enumBlueprintType.Plane,
                        enumBlueprintType.Ballooncraft,
                        enumBlueprintType.Helicopters,
                        enumBlueprintType.DroneOrTarget
                    }.Contains(t.BlueprintType);
                case EnemyType.Space:
                    return t.BlueprintType == enumBlueprintType.Spacecraft;
                case EnemyType.Submarine:
                    return t.BlueprintType == enumBlueprintType.Submarine;
                case EnemyType.Installation:
                    return t.BlueprintType == enumBlueprintType.Installation;
                case EnemyType.All: return true;
            }
            AdvLogger.LogInfo("did not satisfy ruleset: " + t.Name + " for type: " + type.ToString());
            return false;
        }

        public CustomEncounter(string name, List<CustomEncounterEnemy> enemies, float primary = 1, float dropoff = 0, float mindifficulty = 0, bool forcesamefaction = true, bool filterduplicates = true)
        {
            Name = name;
            Count = enemies.Count;
            Enemies = enemies;
            PrimaryDifficultyFactor = primary;
            DifficultyFactorDropoff = dropoff;
            MinDifficutly = mindifficulty;
            ForceSameFaction = forcesamefaction;
            FilterDuplicates = filterduplicates;
        }
    }
}
