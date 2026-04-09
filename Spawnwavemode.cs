using BrilliantSkies.Core;
using BrilliantSkies.Core.Id;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.Networking;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Ftd.Constructs;
using BrilliantSkies.Ftd.Constructs.Modules.All.FireDamage;
using BrilliantSkies.Ftd.Constructs.Modules.Main.Storage;
using BrilliantSkies.Ftd.Multiplayer.NetworkCommunication;
using BrilliantSkies.Ftd.Planets;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ftd.Planets.Instances.Adventure;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Ui.Special.InfoStore;
using NetInfrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using UnityEngine;
using Random = System.Random;

namespace AdventurePatch
{
    public class SpawnWaveMode : MonoBehaviour
    {
        private static SpawnWaveMode _instance;

   
        private static float waveDuration = 120f;
        private static int maxConcurrentEnemies = 10;
        private static float difficultyFromCost = 0;
        private static float spawnDelay = 15f;
        private static int enemiesKilled = 0;
        private static int enemiesToSpawn = 20;
        private static int currentWave = 0;
        private static bool isWaveActive = false;
        private static int difficultyLevel = 1;
        public static bool detectedSubmarine = false;
        public static List<string> encounteredEnemies = new List<string>();
        private static float difficultyFactor = 1f;
        private static float waveCompletion = 0f;
        private static float timeInWave = 0f;
        private static float lowRangeMin = 0.25f;
        private static float highRangeMin = 0.15f;
        private static float rangeWidth = 0.2f;
        private static int baseDelay = 12;
        private static int bonusTimePerEnemy = 12;
        private static float doubleSpawnChance = 0f;
        private static WaveType waveType = WaveType.Normal;
        public static ObjectId factionToSpawn = null;
        public int lastMessageTime = -1;
        private static float playerTeamCost;
        private static float enemyTeamCost;
        private static float minCost;
        private static float maxCost;
        private static List<MainConstruct> activeEnemies = new List<MainConstruct>();
        private Coroutine waveCoroutine;
        private Coroutine spawnCoroutine;
        private static float waveStartTime;
        public enum WaveType
        {
            Normal,
            LargeEnemies,
            SmallEnemies
        }
        public delegate void WaveEventHandler(int waveNumber);
        public static event WaveEventHandler OnWaveStart;
        public static event WaveEventHandler OnWaveComplete;
        public static event WaveEventHandler OnWaveFailed;

        // Properties
        public static bool IsWaveActive => _instance != null && isWaveActive;
        public static int CurrentWave => _instance != null ? currentWave : 0;
        public static int EnemiesRemaining => _instance != null ? enemiesToSpawn - enemiesKilled : 0;
        public static float WaveTimeRemaining => _instance != null && isWaveActive ?
            waveDuration - (Time.time - waveStartTime) : 0f;
        public static float lastEnemySpawn;
        
        public static SpawnWaveMode Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SpawnWaveMode");
                    _instance = go.AddComponent<SpawnWaveMode>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            runID = (uint)UnityEngine.Random.Range(0, int.MaxValue);
        }

        private void OnDestroy()
        {
            CleanupWave();
        }

        private void Update()
        {
            if (!isWaveActive) return;

            List<MainConstruct> deadEnemies = new List<MainConstruct>();
            foreach (var enemy in activeEnemies)
            {
                if (enemy == null) continue;
               
                bool stillExists = false;
                // Check if this enemy is still in the global constructs list
                for (int i = 0; i < StaticConstructablesManager.Constructables.Count; i++)
                {
                    var construct = StaticConstructablesManager.Constructables[i];
                    if (construct == enemy)
                    {
                        stillExists = true;
                        break;
                    }
                }

                if (!stillExists)
                {
                    deadEnemies.Add(enemy);
                }
            }
            int waveremain = (int)WaveTimeRemaining;
            int displayTime = (waveremain / 10) * 10 + 10;
            waveCompletion = waveDuration / (Time.time - waveStartTime);
            if ((waveremain < 30) && (waveremain % 5 == 0) && (lastMessageTime != displayTime))
            {
                NetworkedInfoStore.Add($"Remaining Wave Time: {displayTime }s",5);
                lastMessageTime = displayTime;
            } else if (waveremain % 10 == 0 && (lastMessageTime != displayTime))
            {
                NetworkedInfoStore.Add($"Remaining Wave Time: {displayTime}s",10);
                lastMessageTime = displayTime;
            }
            

            foreach (var deadEnemy in deadEnemies)
            {
                activeEnemies.Remove(deadEnemy);
                enemiesKilled++;
                AdvLogger.LogInfo($"[AdventurePatch Wavemode] Enemy destroyed! {deadEnemy.GetName()}");

                if (enemiesKilled % 5 == 0) // Every 5 kills
                {
                    NetworkedInfoStore.Add($"{enemiesKilled} enemies defeated! Waveprogress: {(int)(waveCompletion*100)}%", 2f);
                }
            }
            checkSubmarine();
            int enemyCount = AdventurePatchUtils.GetEnemyCount();
            float t = Mathf.Clamp01(difficultyFromCost / 100f);
            float difficultyMultiplier = Mathf.Lerp(1f, 2f, t);
            if (enemyCount == 0)
            {
                spawnDelay = 10;
            } else
            {
                spawnDelay = (baseDelay + bonusTimePerEnemy * enemyCount * (float)Math.Sqrt(enemyCount)) * difficultyMultiplier * (float)Math.Sqrt(difficultyFactor);
            }

            timeInWave = (int)(Time.time - waveStartTime);
            if (timeInWave >= waveDuration)
            {
                CompleteWave();
            }
        }

        // Public API
        public static void StartWaveMode()
        {
            if(!Net.IsClient) Instance.StartWave();
        }

        public static void StopWaveMode()
        {
            if (_instance != null)
                _instance.StopWave();
        }

        public static void setWaveCount(int waveCount) {
            currentWave = waveCount;
            AdvLogger.LogInfo($"[AdventurePatch Wavemode] Wave count set to: {currentWave}");
        } 
        public static int getWaveCount() {
            AdvLogger.LogInfo("[AdventurePatch Wavemode] Retrieving wave count");
            return currentWave; 
        }

        public static void ForceNextWave()
        {
            if (_instance != null && isWaveActive)
                _instance.CompleteWave();
        }
        private void StartWave()
        {
            if (Net.IsClient) return;
            factionToSpawn = null; //clear faction
            detectedSubmarine = false;

            // Create a descriptive quicksave name
            string adventureName = GetAdventureDisplayName();
            string quicksaveName = $"{adventureName}_Wave{currentWave + 1}";
            AdventureQuickSaveSystem.QuickSave(quicksaveName);

            updateSettings();
            if (isWaveActive) return;
            currentWave++;
            enemiesKilled = 0;
            activeEnemies.Clear();
            isWaveActive = true;
            waveStartTime = Time.time;
            chooseWaveEncounter();

            ProfileManager.Instance.GetModule<AP_MConfig>().MaxEnemyCount = (uint)maxConcurrentEnemies;
            if (waveCoroutine != null) StopCoroutine(waveCoroutine);
            waveCoroutine = StartCoroutine(WaveCoroutine());
            OnWaveStart?.Invoke(currentWave);
            AdvLogger.LogInfo($"[AdventurePatch Wavemode] Wave {currentWave} started! Spawn {enemiesToSpawn} enemies over {waveDuration}s");
            NetworkedInfoStore.Add($"Wave {currentWave} Started! Defeat enemies and survive {waveDuration}s", 5f);
        }
        private static void setWaveType() {
            if (waveType == WaveType.Normal)
            {
                lowRangeMin = 0.3f;
                highRangeMin = 0.2f;
                rangeWidth = 0.2f;
                baseDelay = 12;
                bonusTimePerEnemy = 12;
                maxConcurrentEnemies = 10;
            }
            else if (waveType == WaveType.SmallEnemies)
            {
                lowRangeMin = 0.15f;
                highRangeMin = 0.1f;
                rangeWidth = 0.15f;
                baseDelay = 5;
                bonusTimePerEnemy = 5;
                maxConcurrentEnemies = 15;
            } 
            else if (waveType == WaveType.LargeEnemies)
            {
                lowRangeMin = 0.4f;
                highRangeMin = 0.3f;
                rangeWidth = 0.2f;
                baseDelay = 24;
                bonusTimePerEnemy = 24;
                maxConcurrentEnemies = 5;
            }
        }
        private static void chooseWaveEncounter()
        {
            playerTeamCost = AdventurePatchUtils.CalculatePlayerTeamCost();

            Array waveValues = Enum.GetValues(typeof(WaveType));
            Random random = new Random();
            //waveType = (WaveType)waveValues.GetValue(random.Next(waveValues.Length)); //Random Wavetype Selection

            AdvLogger.LogInfo($"[AdventurePatch Wavemode] Selected wave type: {waveType}");
            NetworkedInfoStore.Add("Chose wave type: " + waveType);
            setWaveType();
            CalculateRanges();
        }
        private static void CalculateRanges()
        {
            float difficulty = AdventurePatchUtils.CalculateDifficulty(playerTeamCost);
            difficultyFromCost = difficulty;
            float costFraction = Mathf.Clamp01(playerTeamCost / 75000);
            float difficultyFraction  = Mathf.Clamp01(difficulty / 100);
            float doubleSpawnFraction = Mathf.Clamp01(difficulty / 40);

            doubleSpawnChance = Mathf.Lerp(0.6f,1f,doubleSpawnFraction);
            float lowCostFactor = Mathf.Lerp(1.4f, 1f, costFraction);

            float planetFactor = 1f;

            if (Planet.i.File.Header.Name.ToString().ToLower() == "adventure randomizer".ToLower()) 
            {
                AdvLogger.LogInfo("Detected adventure randomiser planet successfully!");
                planetFactor = 0.8f;
            }
            float min = Mathf.Lerp(lowRangeMin, highRangeMin , difficultyFraction);
            float max = Mathf.Lerp(lowRangeMin + rangeWidth, highRangeMin + rangeWidth, difficultyFraction);
            

            if (playerTeamCost < 75000)
            {
                minCost = Mathf.Lerp(5000f, 20000f, costFraction);
                maxConcurrentEnemies = 15;
            } else
            {
                maxConcurrentEnemies = 10;
                minCost = min * playerTeamCost * (1 / difficultyFactor) * lowCostFactor * planetFactor;
            }
            maxCost = max * playerTeamCost * (1 / difficultyFactor) * lowCostFactor * planetFactor;
            if (maxCost < 150000) maxCost = 15000;
            AdvLogger.LogInfo($"[AdventurePatch Wavemode] calculating ranges. min: {min:F2}/{(min * (1 / difficultyFactor) * lowCostFactor * planetFactor):F2}, max: {max:F2}/{(max*(1 / difficultyFactor) * lowCostFactor * planetFactor):F2}, difficulty: {difficulty:F2}, cost: {playerTeamCost:F2}");
        }
        private string GetAdventureDisplayName()
        {
            try
            {
                var instance = InstanceSpecification.i;
                if (instance != null && instance.Header.IsAdventure)
                {
                    // Get the adventure name from the header
                    string adventureName = instance.Header.Name?.ToString() ?? "Adventure";

                    // Strip out the GUID part that looks like " [xxxx-xxxx-xxxx]"
                    int bracketIndex = adventureName.IndexOf('[');
                    if (bracketIndex > 0)
                    {
                        adventureName = adventureName.Substring(0, bracketIndex).Trim();
                    }

                    // Also remove any trailing spaces or special characters
                    adventureName = adventureName.Trim();

                    // Clean the name for filename use (remove invalid characters)
                    char[] invalidChars = Path.GetInvalidFileNameChars();
                    foreach (char c in invalidChars)
                    {
                        adventureName = adventureName.Replace(c.ToString(), "");
                    }

                    // Trim and limit length
                    if (adventureName.Length > 20)
                        adventureName = adventureName.Substring(0, 20);

                    return adventureName;
                }
            }
            catch (Exception ex)
            {
                AdvLogger.LogInfo($"[AdventurePatch Wavemode] [GetAdventureDisplayName] Error: {ex.Message}");
            }

            return "Adventure";
        }

        private static void PotentiallySpawnEnemy()
        {
            if (Time.time - lastEnemySpawn <= spawnDelay)
                return;

            int enemyCount = AdventurePatchUtils.GetEnemyCount();
            if (enemyCount >= maxConcurrentEnemies)
                return;

            float random = UnityEngine.Random.Range(0f, 1f);

            int spawnAmount = 1;

            if (random > doubleSpawnChance)
            {
                AdvLogger.LogInfo($"[AdventurePatch Wavemode] Double Spawn! Random/Threshold: {random}/{doubleSpawnChance}");

                if (random > Math.Sqrt(doubleSpawnChance) && enemyCount < 5)
                {
                    AdvLogger.LogInfo($"[AdventurePatch Wavemode] Triple Spawn! Random/Threshold: {random}/{Math.Sqrt(doubleSpawnChance)}");
                    NetworkedInfoStore.Add("spawning three enemies instead of one!");
                    spawnAmount = 3;
                }
                else
                {
                    NetworkedInfoStore.Add("spawning two enemies instead of one!");
                    spawnAmount = 2;
                }
            }
            else
            {
                AdvLogger.LogInfo($"[AdventurePatch Wavemode] Random below threshold for double spawn: {random}/{doubleSpawnChance}");
            }

            for (int i = 0; i < spawnAmount; i++)
            {
                SpawnEnemy();
            }
        }
        private IEnumerator WaveCoroutine()
        {
            float waveEndTime = Time.time + waveDuration;

            while (Time.time < waveEndTime && isWaveActive)
            {
                PotentiallySpawnEnemy();
                updateSettings();
                yield return new WaitForSeconds(1f);
                
            }
        }
        private static void SpawnEnemy()
        {
            if (Net.IsClient) return;
            try
            {
                AdvLogger.LogInfo("[AdventurePatch Wavemode] spawning enemy in wavedefense mode! Spawndelay: " + spawnDelay);
                var config = ProfileManager.Instance?.GetModule<AP_MConfig>();
                if (config == null) return;

                //Change settings for wave mode
                config.ForceEnemySpawns = false;
                config.BlockRandomSpawns = true;
                config.MaterialScaling = false;
                config.EnemySpawnDistancePatch = true;
                config.OverrideSpawnDifficulty = false;
                config.EnableCustomEncounters = false;
                config.EnemySpawnDistancePatch = true;

                int activeEnemyCount = AdventurePatchUtils.GetEnemyCount();
                config.MinimumSpawnrange = 2000 + (200 * activeEnemyCount);
                config.MinimumSpawnrange *= (float)Math.Sqrt(difficultyFactor);

                AdvLogger.LogInfo("[AdventurePatch Wavemode] starting spawn next");
                //AdventureModeProgression.SpawnAForce();
                AdventurePatchUtils.SpawnEnemyCostRange(minCost, maxCost);
                lastEnemySpawn = Time.time;
                TrackNewestEnemy();

                AdvLogger.LogInfo($"[AdventurePatch Wavemode] Spawned enemy (mincost: {minCost}, maxcost: {maxCost}, Wave {currentWave} Difficultyfactor: {difficultyFactor}) {timeInWave}s into the Wave while {activeEnemyCount} enemies were present");
                activeEnemyCount = AdventurePatchUtils.GetEnemyCount();
                NetworkedInfoStore.Add($"Spawned an enemy. {activeEnemyCount} out of {maxConcurrentEnemies} are present.");
            }
            catch (System.Exception ex)
            {
                AdvLogger.LogInfo($"[AdventurePatch Wavemode] Error spawning enemy: {ex.Message}");
            }
        }

        private static void TrackNewestEnemy()
        {
            for (int i = StaticConstructablesManager.Constructables.Count - 1; i >= 0; i--)
            {
                var construct = StaticConstructablesManager.Constructables[i];
                if (construct != null && construct.GetTeam() != GAME_STATE.MyTeam)
                {
                    if (!activeEnemies.Contains(construct))
                    {
                        activeEnemies.Add(construct);
                        break;
                    }
                }
            }
        }
        private static void checkSubmarine()
        {
            for (int i = 0; i < StaticConstructablesManager.Constructables.Count; i++)
            {
                var construct = StaticConstructablesManager.Constructables[i];
                if (construct.GetTeam() == GAME_STATE.MyTeam)
                {
                    if(construct.AltitudeOfComAboveMeanSeaLevel < -35)
                    {
                        AdvLogger.LogInfo("[AdventurePatch Wavemode] possibly detected a submarine!" + construct.GetName());
                        detectedSubmarine = true;
                        break;
                    }
                    else
                    {
                        detectedSubmarine = false;
                    }
                }
                else continue;

            }
        }
        public void updateSettings ()
        {
            var config = ProfileManager.Instance?.GetModule<AP_MConfig>();
            if (config != null)
            {
                waveDuration = config.waveDuration;
                if (difficultyLevel != config.difficultyLevel) {
                    difficultyLevel = config.difficultyLevel;
                    difficultyFactor = 1 + ((5 - difficultyLevel) * 0.05f);                        //difficultyfactor > 1 for easier, < 1 for harder; current min/max: 0.75...1.2
                    chooseWaveEncounter(); 
                }
            }
            
        }
        public bool isOngoing ()
        {
            return isWaveActive;
        }
        private void CompleteWave()
        {
            if (!isWaveActive) return;
            if (Net.IsClient) return;
            isWaveActive = false;
            if (waveCoroutine != null) StopCoroutine(waveCoroutine);
            CleanupRemainingEnemies();
            var config = ProfileManager.Instance?.GetModule<AP_MConfig>();
            if (config != null)
            {
                float killfactor = 1f + enemiesKilled * 0.15f;
                float rewardMaterial = 25000 * killfactor + (250 * difficultyFromCost * killfactor);
                rewardMaterial *= difficultyFactor;
                AdvLogger.LogInfo($"[AdventurePatch Wavemode] calculating reward for player. {enemiesKilled} enemies were killed, resulting in a killfactor of {killfactor}. used {difficultyFromCost} as difficulty.");
                //rewardMaterial = rewardMaterial * waveDuration / 300;
                var playerFaction = InstanceSpecification.i?.GetFirstActivePlayerFaction(true);
                if (playerFaction != null)
                {
                    playerFaction.ResourceStore.Material.Quantity += rewardMaterial;
                    if (Net.IsConnected)
                    {
                        InstantComs.GetSingleton().SendRpc(new RpcRequest(delegate (INetworkIdentity n)
                        {
                            ServerOutgoingRpcs.SetResources(n, playerFaction);
                        }));
                    }

                    //NetworkedInfoStore.Add($"Wave reward: {rewardMaterial:F0} materials added!", 3f);
                    AdvLogger.LogInfo($"[AdventurePatch Wavemode] Added {rewardMaterial:F0} materials to player faction");
                }
                else
                {
                    config.ResourceZoneDiffScaling = true;
                    config.BonusMaterialPerDifficultyLevel = 300 * (1f + enemiesKilled * 0.15f);
                    config.ResourceZoneBaseMaterial = (int)(20000 * (1f + enemiesKilled * 0.15f));
                    config.ResourceZoneClampedDrainTime = 60;

                    AP_Ui.spawnResourceZone();
                }
                AdventurePatchUtils.TakeRedPortal();
            }


            OnWaveComplete?.Invoke(currentWave);
            AdvLogger.LogInfo($"[AdventurePatch Wavemode] Wave {currentWave} completed!");
            NetworkedInfoStore.Add($"Wave {currentWave} Complete! You defeated {enemiesKilled} enemies", 3f);
            calculateRepaircostAndRepair();
        }
        

        //private void FailWave()
        //{
        //    if (!isWaveActive) return;

        //    isWaveActive = false;
        //    if (waveCoroutine != null) StopCoroutine(waveCoroutine);

        //    CleanupRemainingEnemies();
        //    OnWaveFailed?.Invoke(currentWave);
        //    AdvLogger.LogInfo($"[WaveMode] Wave {currentWave} failed! Time ran out.");
        //    NetworkedInfoStore.Add($"Wave {currentWave} Failed!", 3f);

        //    NetworkedInfoStore.Add($"You didn't defeat enough enemies in time. " + enemiesKilled + " were defeated" , 5f);
        //    calculateRepaircostAndRepair();
        //}

        private void StopWave()
        {
            if (!isWaveActive) return;

            isWaveActive = false;
            if (waveCoroutine != null) StopCoroutine(waveCoroutine);
            CleanupRemainingEnemies();
            AdvLogger.LogInfo($"[AdventurePatch Wavemode] Wave {currentWave} stopped manually.");
            calculateRepaircostAndRepair();
        }
        public static void calculateRepaircostAndRepair()
        {
            if (Net.IsClient || Net.IsServer) return;
            float totalCurrentCost = 0f;
            float totalMaxCost = 0f;
            double totalPaid = 0f;

            var playerFaction = InstanceSpecification.i?.GetFirstActivePlayerFaction(true);
            
            for (int i = 0; i < StaticConstructablesManager.Constructables.Count; i++)
            {
                var construct = StaticConstructablesManager.Constructables[i];
                if (construct == null) continue;
                float current = construct.MainBasicsRestricted.GetResourceCost(ValueQueryType.AliveSubsAndDrones).Material; //currentlyalive
                float max = construct.MainBasicsRestricted.GetResourceCost(ValueQueryType.SubsAndDrones).Material;
                AdvLogger.LogInfo($"current cost of craft: {current}, max cost of craft: {max}");
                totalCurrentCost += current;
                totalMaxCost += max;
                var force = construct.GetForce();
                if (force == null) continue;
                if (force.FactionId != GAME_STATE.MyTeam) continue;

                if (force.ResourceStore != null)
                {
                    double materialQuantity = force.ResourceStore.iMaterial.Quantity;
                    double difference = max - current;
                    if(materialQuantity > difference)
                    {
                        force.ResourceStore.TakeIfAvailable((float)difference);
                        AdventurePatchUtils.RepairConstruct(construct);
                        totalPaid += difference;
                        AdvLogger.LogInfo($"Paid from Craft storage: {difference}");

                    } else if (playerFaction != null)
                    {
                        if(playerFaction.ResourceStore.Material.Quantity > difference)
                        {
                            playerFaction.ResourceStore.Material.Quantity -= difference;
                            AdventurePatchUtils.RepairConstruct(construct);
                            totalPaid += difference;
                            AdvLogger.LogInfo($"Paid from Faction storage: {difference}");
                        }

                    } else
                    {
                        AdvLogger.LogInfo($"cant afford repair: {difference}");
                    }
                }
                NetworkedInfoStore.Add($"Repair complete. Paid {totalPaid} Materials.");
                AdvLogger.LogInfo($"[AdventurePatch Wavemode] Repair complete. cost sum before: {totalCurrentCost}, max: {totalMaxCost} difference: {totalMaxCost - totalCurrentCost}, paid: {totalPaid}");

            }
        }
        private void CleanupWave()
        {
            if (waveCoroutine != null) StopCoroutine(waveCoroutine);
            isWaveActive = false;
            activeEnemies.Clear();
        }

        private void CleanupRemainingEnemies()
        {
            factionToSpawn = null;
            encounteredEnemies.Clear();
            detectedSubmarine = false;
            AdventurePatchUtils.clearEnemies();
            activeEnemies.Clear();
        }
        public static WaveInfo GetWaveInfo()
        {
            if (_instance == null || !isWaveActive)
                return null;

            return new WaveInfo
            {
                WaveNumber = currentWave,
                EnemiesKilled = enemiesKilled,
                EnemiesToSpawn = enemiesToSpawn,
                TimeRemaining = waveDuration - (Time.time - waveStartTime),
                IsActive = true
            };
        }
    }

    public class WaveInfo
    {
        public int WaveNumber;
        public int EnemiesKilled;
        public int EnemiesToSpawn;
        public float TimeRemaining;
        public bool IsActive;

        public float Progress => EnemiesToSpawn > 0 ? (float)EnemiesKilled / EnemiesToSpawn : 0f;
        public string FormattedTimeRemaining => $"{Mathf.Max(0, Mathf.FloorToInt(TimeRemaining / 60)):00}:{Mathf.Max(0, Mathf.FloorToInt(TimeRemaining % 60)):00}";
    }
}