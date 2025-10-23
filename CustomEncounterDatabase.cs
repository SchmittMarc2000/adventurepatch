using BrilliantSkies.Core.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventurePatch
{
    public static class CustomEncounterDatabase
    {
        // Static readonly field initialized once when the class is loaded
        private static readonly List<CustomEncounter> _encounters = CreateEncounters();

        // Public accessor
        public static IReadOnlyList<CustomEncounter> Encounters => _encounters;

        private static List<CustomEncounter> CreateEncounters()
        {
            return new List<CustomEncounter>
        {
            new CustomEncounter(
                name: "Small fleet of ships",
                enemies: new List<CustomEncounterEnemy>
                {
                    new CustomEncounterEnemy(EnemyType.Ship),
                    new CustomEncounterEnemy(EnemyType.Ship),
                },
                primary: 0.8f,
                dropoff: 0.1f
            ),
            new CustomEncounter(
                name: "Sub, Boat and Plane",
                enemies: new List<CustomEncounterEnemy>
                {
                    new CustomEncounterEnemy(EnemyType.Submarine),
                    new CustomEncounterEnemy(EnemyType.Ship),
                    new CustomEncounterEnemy(EnemyType.Flying),
                },
                mindifficulty: 20,
                primary: 0.8f,
                dropoff: 0.1f
            ),
            new CustomEncounter(
                name: "Airforce",
                enemies: new List<CustomEncounterEnemy>
                {
                    new CustomEncounterEnemy(EnemyType.Flying),
                    new CustomEncounterEnemy(EnemyType.Flying),
                },
                primary: 0.8f,
                dropoff: 0.1f
            ),
            new CustomEncounter(
                name: "Crossbones & Tarpon",
                enemies: new List<CustomEncounterEnemy>
                {
                    new CustomEncounterEnemy(EnemyType.All,"Tarpon"),
                    new CustomEncounterEnemy(EnemyType.All,"Crossbones"),
                },
                mindifficulty: 60,
                forcesamefaction: false,
                filterduplicates: false,
                primary: 1f,
                dropoff: 0f
            ),
            new CustomEncounter(
                name: "Turtle Twin",
                enemies: new List<CustomEncounterEnemy>
                {
                    new CustomEncounterEnemy(EnemyType.All,"Timid"),
                    new CustomEncounterEnemy(EnemyType.All,"Gravitas"),
                },
                mindifficulty: 80,
                forcesamefaction: false,
                filterduplicates: false,
                primary: 1f,
                dropoff: 0f
            ),
            new CustomEncounter(
                name: "Sailing away",
                enemies: new List<CustomEncounterEnemy>
                {
                    new CustomEncounterEnemy(EnemyType.All,"Greenfield"),
                    new CustomEncounterEnemy(EnemyType.All,"Nightingale"),
                    new CustomEncounterEnemy(EnemyType.All,"Lunette"),
                },
                mindifficulty: 25,
                forcesamefaction: false,
                filterduplicates: false,
                primary: 1f,
                dropoff: 0f
            ),
            new CustomEncounter(
                name: "Eyrie + Bulwark",
                enemies: new List<CustomEncounterEnemy>
                {
                    new CustomEncounterEnemy(EnemyType.All,"Eyrie"),
                    new CustomEncounterEnemy(EnemyType.All,"Bulwark"),
                    new CustomEncounterEnemy(EnemyType.All,"Bulwark"),
                },
                mindifficulty: 80,
                forcesamefaction: false,
                filterduplicates: false,
                primary: 1f,
                dropoff: 0f
            ),
            new CustomEncounter(
                name: "Double Retaliation",
                enemies: new List<CustomEncounterEnemy>
                {
                    new CustomEncounterEnemy(EnemyType.All,"Retaliator"),
                    new CustomEncounterEnemy(EnemyType.All,"Retaliator"),
                },
                mindifficulty: 85,
                forcesamefaction: false,
                filterduplicates: false,
                primary: 1f,
                dropoff: 0f
            ),
            new CustomEncounter(
                name: "Cannon fodder",
                enemies: new List<CustomEncounterEnemy>
                {
                    new CustomEncounterEnemy(EnemyType.All,"Marauder"),
                    new CustomEncounterEnemy(EnemyType.All,"Marauder"),
                    new CustomEncounterEnemy(EnemyType.All,"Marauder"),
                    new CustomEncounterEnemy(EnemyType.All,"Marauder"),
                },
                mindifficulty: 20,
                forcesamefaction: false,
                filterduplicates: false,
                primary: 1f,
                dropoff: 0f
            ),
            new CustomEncounter(
                name: "Scorched Earth",
                enemies: new List<CustomEncounterEnemy>
                {
                    new CustomEncounterEnemy(EnemyType.All,"ICBM"),
                    new CustomEncounterEnemy(EnemyType.All,"ICBM"),
                    new CustomEncounterEnemy(EnemyType.All,"ICBM"),
                    new CustomEncounterEnemy(EnemyType.All,"ICBM"),
                    new CustomEncounterEnemy(EnemyType.All,"ICBM"),
                },
                mindifficulty: 60,
                forcesamefaction: false,
                filterduplicates: false,
                primary: 1f,
                dropoff: 0f
            ),
            new CustomEncounter(
                name: "Missile Threat",
                enemies: new List<CustomEncounterEnemy>
                {
                    new CustomEncounterEnemy(EnemyType.All,"Smoker"),
                    new CustomEncounterEnemy(EnemyType.All,"Duster"),
                },
                mindifficulty: 1,
                forcesamefaction: false,
                filterduplicates: false,
                primary: 1f,
                dropoff: 0f
            ),
            new CustomEncounter(
                name: "Double Trouble",
                enemies: new List<CustomEncounterEnemy>
                {
                    new CustomEncounterEnemy(EnemyType.All,"Sunfish"),
                    new CustomEncounterEnemy(EnemyType.All,"Swordfish"),
                },
                mindifficulty: 20,
                forcesamefaction: false,
                filterduplicates: false,
                primary: 1f,
                dropoff: 0f
            ),
            new CustomEncounter(
                name: "DWG Threats",
                enemies: new List<CustomEncounterEnemy>
                {
                    new CustomEncounterEnemy(EnemyType.All,"Shrike"),
                    new CustomEncounterEnemy(EnemyType.All,"Scuttlegun"),
                },
                mindifficulty: 10,
                forcesamefaction: false,
                filterduplicates: false,
                primary: 1f,
                dropoff: 0f
            ),
            new CustomEncounter(
                name: "Good Luck with those",
                enemies: new List<CustomEncounterEnemy>
                {
                    new CustomEncounterEnemy(EnemyType.All,"Buzzsaw"),
                    new CustomEncounterEnemy(EnemyType.All,"Pulverizer"),
                },
                mindifficulty: 25,
                forcesamefaction: false,
                filterduplicates: false,
                primary: 1f,
                dropoff: 0f
            ),
            new CustomEncounter(
                name: "Utter Carnage",
                enemies: new List<CustomEncounterEnemy>
                {
                    new CustomEncounterEnemy(EnemyType.All,"Velocity"),
                    new CustomEncounterEnemy(EnemyType.All,"Carnage"),
                },
                mindifficulty: 30,
                forcesamefaction: false,
                filterduplicates: false,
                primary: 1f,
                dropoff: 0f
            ),
            new CustomEncounter(
                name: "Galaxies Clashing",
                enemies: new List<CustomEncounterEnemy>
                {
                    new CustomEncounterEnemy(EnemyType.All,"Galaxy"),
                    new CustomEncounterEnemy(EnemyType.All,"Galaxy"),
                },
                mindifficulty: 40,
                forcesamefaction: false,
                filterduplicates: false,
                primary: 1f,
                dropoff: 0f
            ),
            new CustomEncounter(
                name: "Three times the charm",
                enemies: new List<CustomEncounterEnemy>
                {
                    new CustomEncounterEnemy(EnemyType.All,"Pequod"),
                    new CustomEncounterEnemy(EnemyType.All,"Loggerhead"),
                    new CustomEncounterEnemy(EnemyType.All,"Shrike"),
                },
                mindifficulty: 30,
                forcesamefaction: false,
                filterduplicates: false,
                primary: 1f,
                dropoff: 0f
            ),
            new CustomEncounter(
                name: "OW Triple threats",
                enemies: new List<CustomEncounterEnemy>
                {
                    new CustomEncounterEnemy(EnemyType.All,"Rook"),
                    new CustomEncounterEnemy(EnemyType.All,"Huskarl"),
                    new CustomEncounterEnemy(EnemyType.All,"Catapult"),
                },
                mindifficulty: 30,
                forcesamefaction: false,
                filterduplicates: false,
                primary: 1f,
                dropoff: 0f
            ),
            new CustomEncounter(
                name: "Striders of steel",
                enemies: new List<CustomEncounterEnemy>
                {
                    new CustomEncounterEnemy(EnemyType.All,"Tyr"),
                    new CustomEncounterEnemy(EnemyType.All,"Stralsund"),
                },
                mindifficulty: 80,
                forcesamefaction: false,
                filterduplicates: false,
                primary: 1f,
                dropoff: 0f
            ),
            new CustomEncounter(
                name: "Beast Swarm",
                enemies: new List<CustomEncounterEnemy>
                {
                    new CustomEncounterEnemy(EnemyType.All,"Curiosity"),
                    new CustomEncounterEnemy(EnemyType.All,"Curiosity"),
                    new CustomEncounterEnemy(EnemyType.All,"Vengeful"),
                    new CustomEncounterEnemy(EnemyType.All,"Spite"),
                },
                mindifficulty: 80,
                forcesamefaction: false,
                filterduplicates: false,
                primary: 1f,
                dropoff: 0f
            ),
            new CustomEncounter(
                name: "Ships of War",
                enemies: new List<CustomEncounterEnemy>
                {
                    new CustomEncounterEnemy(EnemyType.All,"Balmung"),
                    new CustomEncounterEnemy(EnemyType.All,"Nothung"),
                },
                mindifficulty: 50,
                forcesamefaction: false,
                filterduplicates: false,
                primary: 1f,
                dropoff: 0f
            ),


        };
        }

        public static CustomEncounter SelectRandomEncounter(float adventureDifficulty)
        {
            if (_encounters.Count == 0)
                throw new InvalidOperationException("No custom encounters available.");

            // Filter valid encounters
            var validEncounters = _encounters
                .Where(e => e.MinDifficutly <= adventureDifficulty && (adventureDifficulty - e.MinDifficutly) <= 15)
                .ToList();

            if (validEncounters.Count == 0)
            {
                AdvLogger.LogInfo("No valid encounters found for difficulty: " + adventureDifficulty);
                return null;
            }

            Random random = new Random();
            CustomEncounter selected = validEncounters[random.Next(validEncounters.Count)];

            AdvLogger.LogInfo("Selected the following encounter: " + selected.Name + " with difficulty: " + adventureDifficulty);
            return selected;
        }
    }
}
