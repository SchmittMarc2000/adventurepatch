using Assets.Scripts.Persistence;
using BrilliantSkies.Core;
using BrilliantSkies.Core.Constants;
using BrilliantSkies.Core.FilesAndFolders;
using BrilliantSkies.Core.FilesAndFolders.Depreciated;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Ftd.FleetAndForceSaveAndSynch;
using BrilliantSkies.Ftd.Persistence.PlanetFileSaving;
using BrilliantSkies.Ftd.Planets;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ftd.Planets.LoadingAndSaving;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Ui.uGui;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using IFileSource = BrilliantSkies.Core.FilesAndFolders.IFileSource;

namespace AdventurePatch
{
    public static class AdventureQuickSaveSystem
    {
        private static string QuickSaveFolder
        {
            get
            {
                string path = Path.Combine(Get.ProfilePaths.GamestateDir().ToString(), "AdventurePatch", "QuickSaves");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }

        private static string GetQuickSaveDir(int slot, string saveName)
        {
            return Path.Combine(QuickSaveFolder, $"QuickSave_{slot}_{saveName}");
        }

        public static bool QuickSave(string saveName = "QuickSave")
        {
            try
            {
                var instance = InstanceSpecification.i;
                if (instance == null || !instance.Header.IsAdventure)
                {
                    AdvLogger.LogInfo("[QuickSave] Cannot save - not in adventure mode");
                    return false;
                }

                int slot = instance.Header.Id.Id;
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string fullSaveName = $"{saveName}_{timestamp}";

                AdvLogger.LogInfo($"[QuickSave] Starting save. Slot: {slot}, Name: {fullSaveName}");

                var planetFile = Planet.i?.File;
                if (planetFile == null)
                {
                    AdvLogger.LogInfo("[QuickSave] planetFile is null, aborting");
                    return false;
                }

                instance.Header.CommonSettings.ReferenceFrame = instance.Adventure.PrimaryForce.GetUniversalPositionOfForce(allowConstruct: true);

                var saveLoad = new PlanetFileSaveLoad();

                AdvLogger.LogInfo("[QuickSave] Calling SaveToGameState...");
                var saveToGameStateMethod = typeof(PlanetFileSaveLoad).GetMethod("SaveToGameState",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (saveToGameStateMethod != null)
                {
                    saveToGameStateMethod.Invoke(saveLoad, new object[] { planetFile, SaveFileType.Adventure });
                    AdvLogger.LogInfo("[QuickSave] SaveToGameState completed");
                }
                else
                {
                    saveLoad.SaveToSlot(planetFile, slot, SaveFileType.Adventure, instance.Header.Name.ToString());
                    AdvLogger.LogInfo("[QuickSave] SaveToSlot completed");
                }

                string gameStateFolder = Get.ProfilePaths.GamestateDir().ToString();
                AdvLogger.LogInfo($"[QuickSave] GameState folder: {gameStateFolder}");

                string quicksaveDir = GetQuickSaveDir(slot, fullSaveName);
                AdvLogger.LogInfo($"[QuickSave] Destination dir: {quicksaveDir}");

                if (!Directory.Exists(quicksaveDir))
                    Directory.CreateDirectory(quicksaveDir);
                else
                {
                    foreach (string file in Directory.GetFiles(quicksaveDir))
                        File.Delete(file);
                }
                int fileCount = 0;
                foreach (string file in Directory.GetFiles(gameStateFolder))
                {
                    string fileName = Path.GetFileName(file);
                    string destFile = Path.Combine(quicksaveDir, fileName);

                    if (fileName.StartsWith("~") || fileName.EndsWith(".tmp"))
                        continue;

                    File.Copy(file, destFile, true);
                    AdvLogger.LogInfo($"[QuickSave] Copied from GameState: {fileName}");
                    fileCount++;
                }

                string slotFolder = planetFile.GetSaveSlotFolder(slot, SaveFileType.Adventure).FullName;
                if (Directory.Exists(slotFolder))
                {
                    AdvLogger.LogInfo($"[QuickSave] Also copying from slot folder: {slotFolder}");
                    foreach (string file in Directory.GetFiles(slotFolder))
                    {
                        string fileName = Path.GetFileName(file);
                        string destFile = Path.Combine(quicksaveDir, fileName);

                        // Only copy if not already copied from GameState
                        if (!File.Exists(destFile))
                        {
                            File.Copy(file, destFile, true);
                            AdvLogger.LogInfo($"[QuickSave] Copied from slot: {fileName}");
                            fileCount++;
                        }
                    }
                }

                var config = ProfileManager.Instance?.GetModule<AP_MConfig>();
                if (config != null)
                {
                    var saveData = AdventureSaveData.FromConfig(config);
                    string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
                    string settingsPath = Path.Combine(quicksaveDir, $"AdventurePatch_{slot}.json");
                    File.WriteAllText(settingsPath, json);
                    AdvLogger.LogInfo($"[QuickSave] Saved config: {settingsPath}");
                }

                AdvLogger.LogInfo($"[QuickSave] Save completed successfully: {fullSaveName} ({fileCount} files)");
                NetworkedInfoStore.Add($"Quick save completed: {fullSaveName} ({fileCount} files)");
                return true;
            }
            catch (Exception ex)
            {
                AdvLogger.LogInfo($"[QuickSave] Failed to save: {ex.Message}\n{ex.StackTrace}");
                NetworkedInfoStore.Add($"Quick save failed: {ex.Message}");
                return false;
            }
        }
        public static bool RestoreQuickSaveToSlot(string saveName)
        {
            try
            {
                string quicksaveDir = null;
                int sourceSlot = -1;

                string quicksaveRoot = QuickSaveFolder;
                foreach (string dir in Directory.GetDirectories(quicksaveRoot))
                {
                    string dirName = Path.GetFileName(dir);
                    if (dirName.StartsWith("QuickSave_"))
                    {
                        string[] parts = dirName.Split('_');
                        if (parts.Length >= 3)
                        {
                            int parsedSlot;
                            if (int.TryParse(parts[1], out parsedSlot))
                            {
                                string candidateName = string.Join("_", parts.Skip(2));
                                if (candidateName == saveName)
                                {
                                    quicksaveDir = dir;
                                    sourceSlot = parsedSlot;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (quicksaveDir == null)
                {
                    AdvLogger.LogInfo($"[Restore] Quicksave not found: {saveName}");
                    NetworkedInfoStore.Add($"Quicksave '{saveName}' not found");
                    return false;
                }

                var planetFile = Planet.i?.File;
                if (planetFile == null)
                {
                    AdvLogger.LogInfo("[Restore] planetFile is null");
                    NetworkedInfoStore.Add("Restore failed: planet not loaded");
                    return false;
                }

                var freeSlotInfo = NewAdventureStuff.GetUnusedAdventureSlot();
                if (freeSlotInfo == null)
                {
                    AdvLogger.LogInfo("[Restore] No free adventure slots available");
                    NetworkedInfoStore.Add("No free adventure slots available — delete an adventure first.");
                    return false;
                }

                int destSlot = freeSlotInfo.Slot;
                string destFolder = planetFile.GetSaveSlotFolder(destSlot, SaveFileType.Adventure).FullName;
                AdvLogger.LogInfo($"[Restore] Restoring '{saveName}' (slot {sourceSlot}) to slot {destSlot} at {destFolder}");

                if (!Directory.Exists(destFolder))
                    Directory.CreateDirectory(destFolder);

                foreach (string file in Directory.GetFiles(destFolder))
                {
                    try { File.Delete(file); } catch { }
                }

                foreach (string file in Directory.GetFiles(quicksaveDir))
                {
                    string fileName = Path.GetFileName(file);
                    if (fileName.StartsWith("AdventurePatch_")) continue;
                    File.Copy(file, Path.Combine(destFolder, fileName), true);
                    AdvLogger.LogInfo($"[Restore] Copied: {fileName}");
                }

                string newAdventureName = $"Restored_{saveName}_from_slot_{sourceSlot}";
                string newGuid = Guid.NewGuid().ToString();

                foreach (string file in Directory.GetFiles(destFolder, "*.state"))
                {
                    PatchStateFile(file, sourceSlot, destSlot, newAdventureName, newGuid);
                }

                AdvLogger.LogInfo($"[Restore] Successfully restored to slot {destSlot}");
                NetworkedInfoStore.Add($"Restored to slot {destSlot} as '{newAdventureName}'. Load it from the main menu.");
                return true;
            }
            catch (Exception ex)
            {
                AdvLogger.LogInfo($"[Restore] Failed: {ex.Message}\n{ex.StackTrace}");
                NetworkedInfoStore.Add($"Restore failed: {ex.Message}");
                return false;
            }
        }
        private static void PatchStateFile(string filePath, int oldSlot, int newSlot, string newName, string newGuid)
        {
            try
            {
                AdvLogger.LogInfo($"[Restore] Patching {Path.GetFileName(filePath)}");
                string content = File.ReadAllText(filePath);
                bool changed = false;

                try
                {
                    var json = JObject.Parse(content);
                    bool jsonChanged = false;

                    jsonChanged |= ReplaceSlotInToken(json, oldSlot, newSlot);
                    jsonChanged |= ReplaceGuidInToken(json, newGuid);
                    jsonChanged |= ReplaceNameInToken(json, newName);

                    if (jsonChanged)
                    {
                        File.WriteAllText(filePath, json.ToString(Formatting.Indented));
                        AdvLogger.LogInfo($"[Restore] Patched JSON in {Path.GetFileName(filePath)}");
                        return;
                    }
                }
                catch
                {
                    AdvLogger.LogInfo($"[Restore] {Path.GetFileName(filePath)} is not JSON, trying string replacement");
                }

                // String replacement for non-JSON files
                string newContent = content;

                // Replace slot numbers (look for patterns like "Id":3, "Slot":3, etc.)
                newContent = System.Text.RegularExpressions.Regex.Replace(newContent,
                    $"\"Id\"\\s*:\\s*{oldSlot}\\b",
                    $"\"Id\":{newSlot}");

                newContent = System.Text.RegularExpressions.Regex.Replace(newContent,
                    $"\"Slot\"\\s*:\\s*{oldSlot}\\b",
                    $"\"Slot\":{newSlot}");

                newContent = System.Text.RegularExpressions.Regex.Replace(newContent,
                    $"\"UniqueId\"\\s*:\\s*\"[^\"]+\"",
                    $"\"UniqueId\":\"{newGuid}\"");

                // Replace the adventure name
                if (Path.GetFileName(filePath) == "header.state")
                {
                    newContent = System.Text.RegularExpressions.Regex.Replace(newContent,
                        $"\"Name\"\\s*:\\s*\"[^\"]+\"",
                        $"\"Name\":\"{newName}\"");
                }

                if (content != newContent)
                {
                    File.WriteAllText(filePath, newContent);
                    AdvLogger.LogInfo($"[Restore] Patched text in {Path.GetFileName(filePath)}");
                }
                else
                {
                    AdvLogger.LogInfo($"[Restore] No changes needed in {Path.GetFileName(filePath)}");
                }
            }
            catch (Exception ex)
            {
                AdvLogger.LogInfo($"[Restore] Failed to patch {Path.GetFileName(filePath)}: {ex.Message}");
            }
        }

        private static bool ReplaceSlotInToken(JToken token, int oldSlot, int newSlot)
        {
            bool changed = false;

            if (token.Type == JTokenType.Object)
            {
                foreach (var prop in token.Children<JProperty>())
                {
                    if ((prop.Name == "Id" || prop.Name == "Slot") && prop.Value.Type == JTokenType.Integer)
                    {
                        int currentValue = prop.Value.Value<int>();
                        if (currentValue == oldSlot)
                        {
                            prop.Value = newSlot;
                            AdvLogger.LogInfo($"[Restore] Replaced {prop.Name}: {oldSlot} -> {newSlot}");
                            changed = true;
                        }
                    }
                    else
                    {
                        changed |= ReplaceSlotInToken(prop.Value, oldSlot, newSlot);
                    }
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                foreach (var child in token.Children())
                {
                    changed |= ReplaceSlotInToken(child, oldSlot, newSlot);
                }
            }

            return changed;
        }

        private static bool ReplaceGuidInToken(JToken token, string newGuid)
        {
            bool changed = false;

            if (token.Type == JTokenType.Object)
            {
                foreach (var prop in token.Children<JProperty>())
                {
                    if (prop.Name == "UniqueId" && prop.Value.Type == JTokenType.String)
                    {
                        prop.Value = newGuid;
                        AdvLogger.LogInfo($"[Restore] Replaced UniqueId with {newGuid}");
                        changed = true;
                    }
                    else
                    {
                        changed |= ReplaceGuidInToken(prop.Value, newGuid);
                    }
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                foreach (var child in token.Children())
                {
                    changed |= ReplaceGuidInToken(child, newGuid);
                }
            }

            return changed;
        }

        private static bool ReplaceNameInToken(JToken token, string newName)
        {
            bool changed = false;

            if (token.Type == JTokenType.Object)
            {
                foreach (var prop in token.Children<JProperty>())
                {
                    if (prop.Name == "Name" && prop.Value.Type == JTokenType.String)
                    {
                        prop.Value = newName;
                        AdvLogger.LogInfo($"[Restore] Replaced Name with {newName}");
                        changed = true;
                    }
                    else
                    {
                        changed |= ReplaceNameInToken(prop.Value, newName);
                    }
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                foreach (var child in token.Children())
                {
                    changed |= ReplaceNameInToken(child, newName);
                }
            }

            return changed;
        }

        // Quick load the most recent save
        public static bool QuickLoad()
        {
            try
            {
                var instance = InstanceSpecification.i;
                if (instance == null || !instance.Header.IsAdventure)
                {
                    AdvLogger.LogInfo("[QuickLoad] Cannot load - not in adventure mode");
                    return false;
                }

                int slot = instance.Header.Id.Id;

                string quicksaveDir = QuickSaveFolder;
                if (!Directory.Exists(quicksaveDir))
                    return false;

                var directories = Directory.GetDirectories(quicksaveDir);
                DateTime latest = DateTime.MinValue;
                string latestDir = null;

                foreach (string dir in directories)
                {
                    string dirName = Path.GetFileName(dir);
                    if (dirName.StartsWith($"QuickSave_{slot}_"))
                    {
                        DateTime dirTime;
                        string timeStr = dirName.Substring(($"QuickSave_{slot}_").Length);
                        if (DateTime.TryParseExact(timeStr, "yyyy-MM-dd_HH-mm-ss", null, System.Globalization.DateTimeStyles.None, out dirTime))
                        {
                            if (dirTime > latest)
                            {
                                latest = dirTime;
                                latestDir = dir;
                            }
                        }
                    }
                }

                if (latestDir != null)
                {
                    return LoadFromFolder(latestDir, slot);
                }

                AdvLogger.LogInfo("[QuickLoad] No quicksaves found");
                return false;
            }
            catch (Exception ex)
            {
                AdvLogger.LogInfo($"[QuickLoad] Failed to load: {ex.Message}");
                return false;
            }
        }

        // Load a specific quicksave by name
        public static bool LoadQuickSave(string saveName)
        {
            try
            {
                var instance = InstanceSpecification.i;
                if (instance == null || !instance.Header.IsAdventure)
                {
                    AdvLogger.LogInfo("[QuickLoad] Cannot load - not in adventure mode");
                    return false;
                }

                int slot = instance.Header.Id.Id;
                string quicksaveDir = GetQuickSaveDir(slot, saveName);

                if (Directory.Exists(quicksaveDir))
                {
                    return LoadFromFolder(quicksaveDir, slot);
                }

                AdvLogger.LogInfo($"[QuickLoad] Quicksave not found: {saveName}");
                return false;
            }
            catch (Exception ex)
            {
                AdvLogger.LogInfo($"[QuickLoad] Failed to load: {ex.Message}");
                return false;
            }
        }
        private static Type FindType(string typeName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return new Type[0]; }
                })
                .FirstOrDefault(t => t.Name == typeName);
        }
        private static object GetModeChanger()
        {
            try
            {
                var modeChangerType = FindType("ModeChanger");
                if (modeChangerType == null) return null;

                var instanceProperty = modeChangerType.GetProperty("Instance",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                return instanceProperty?.GetValue(null);
            }
            catch (Exception ex)
            {
                AdvLogger.LogInfo($"[GetModeChanger] Error: {ex.Message}");
                return null;
            }
        }

        // Load from a specific folder
        private static bool LoadFromFolder(string folderPath, int slot)
        {
            try
            {
                var planetFile = Planet.i?.File;
                if (planetFile == null)
                {
                    AdvLogger.LogInfo("[QuickLoad] planetFile is null");
                    return false;
                }

                // Overwrite the slot folder with quicksave files
                string destFolder = planetFile.GetSaveSlotFolder(slot, SaveFileType.Adventure).FullName;
                AdvLogger.LogInfo($"[QuickLoad] Copying to slot folder: {destFolder}");

                if (!Directory.Exists(destFolder))
                    Directory.CreateDirectory(destFolder);

                foreach (string file in Directory.GetFiles(folderPath))
                {
                    string fileName = Path.GetFileName(file);
                    if (fileName.StartsWith("AdventurePatch_")) continue;
                    File.Copy(file, Path.Combine(destFolder, fileName), true);
                    AdvLogger.LogInfo($"[QuickLoad] Copied: {fileName}");
                }

                // Find ModeChanger via reflection
                var modeChangerType = FindType("ModeChanger");

                if (modeChangerType == null)
                {
                    AdvLogger.LogInfo("[QuickLoad] Could not find ModeChanger type");
                    NetworkedInfoStore.Add("Files copied to slot " + slot + " - please load from main menu");
                    return true;
                }

                AdvLogger.LogInfo($"[QuickLoad] Found ModeChanger in: {modeChangerType.FullName}");

                // Try to initiate the load
                var modeChanger = GetModeChanger();
                if (modeChanger != null)
                {
                    bool isStreamingWorld = planetFile.IsInThisFolderOrOneOfItsSubFolders(Get.PermanentPaths.StreamingWorldsDir().ToString());

                    var loadOptionsType = FindType("LoadGameOptions");
                    if (loadOptionsType != null)
                    {
                        try
                        {
                            var loadOptions = Activator.CreateInstance(loadOptionsType, planetFile, SaveFileType.Adventure, slot, isStreamingWorld);
                            var initiateMethod = modeChangerType.GetMethod("InitiateChangeSequence");

                            if (initiateMethod != null)
                            {
                                initiateMethod.Invoke(modeChanger, new object[] { loadOptions });
                                AdvLogger.LogInfo($"[QuickLoad] Triggered reload to slot {slot}");
                                return true;
                            }
                        }
                        catch (Exception ex)
                        {
                            AdvLogger.LogInfo($"[QuickLoad] Failed to initiate load: {ex.Message}");
                        }
                    }
                }

                NetworkedInfoStore.Add("Files copied to slot " + slot + " - please load from main menu");
                return true;
            }
            catch (Exception ex)
            {
                AdvLogger.LogInfo($"[QuickLoad] Failed: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        // Delete a quicksave
        public static bool DeleteQuickSave(string saveName)
        {
            return DeleteQuickSave(saveName, -1);
        }

        public static bool DeleteQuickSave(string saveName, int slot)
        {
            try
            {
                string quicksaveDir;

                if (slot >= 0)
                {
                    // Delete from specific slot
                    quicksaveDir = GetQuickSaveDir(slot, saveName);
                }
                else
                {
                    // Try to find which slot this quicksave belongs to
                    quicksaveDir = FindQuickSaveDirectory(saveName);
                }

                if (string.IsNullOrEmpty(quicksaveDir) || !Directory.Exists(quicksaveDir))
                {
                    AdvLogger.LogInfo($"[QuickSave] Quicksave not found: {saveName}");
                    return false;
                }

                Directory.Delete(quicksaveDir, true);
                AdvLogger.LogInfo($"[QuickSave] Deleted: {saveName} from {quicksaveDir}");
                return true;
            }
            catch (Exception ex)
            {
                AdvLogger.LogInfo($"[QuickSave] Failed to delete {saveName}: {ex.Message}");
                return false;
            }
        }

        public static bool DeleteQuickSaveByFullPath(string fullPath)
        {
            try
            {
                if (Directory.Exists(fullPath))
                {
                    Directory.Delete(fullPath, true);
                    AdvLogger.LogInfo($"[QuickSave] Deleted by path: {fullPath}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                AdvLogger.LogInfo($"[QuickSave] Failed to delete by path {fullPath}: {ex.Message}");
                return false;
            }
        }

        public static void DeleteAllQuickSaves()
        {
            try
            {
                string quicksaveDir = QuickSaveFolder;
                if (Directory.Exists(quicksaveDir))
                {
                    foreach (string dir in Directory.GetDirectories(quicksaveDir))
                    {
                        try
                        {
                            Directory.Delete(dir, true);
                            AdvLogger.LogInfo($"[QuickSave] Deleted: {Path.GetFileName(dir)}");
                        }
                        catch (Exception ex)
                        {
                            AdvLogger.LogInfo($"[QuickSave] Failed to delete {Path.GetFileName(dir)}: {ex.Message}");
                        }
                    }
                }
                AdvLogger.LogInfo("[QuickSave] All quicksaves cleared");
            }
            catch (Exception ex)
            {
                AdvLogger.LogInfo($"[QuickSave] Failed to clear all quicksaves: {ex.Message}");
            }
        }

        private static string FindQuickSaveDirectory(string saveName)
        {
            try
            {
                string quicksaveRoot = QuickSaveFolder;
                if (!Directory.Exists(quicksaveRoot))
                    return null;

                // Search all subdirectories for a match
                foreach (string dir in Directory.GetDirectories(quicksaveRoot))
                {
                    string dirName = Path.GetFileName(dir);
                    // Extract the save name part (after the slot prefix)
                    int underscoreIndex = dirName.IndexOf('_');
                    if (underscoreIndex >= 0)
                    {
                        int secondUnderscore = dirName.IndexOf('_', underscoreIndex + 1);
                        string candidate;
                        if (secondUnderscore >= 0)
                        {
                            candidate = dirName.Substring(secondUnderscore + 1);
                        }
                        else
                        {
                            candidate = dirName.Substring(underscoreIndex + 1);
                        }

                        if (candidate == saveName)
                        {
                            return dir;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                AdvLogger.LogInfo($"[QuickSave] FindQuickSaveDirectory error: {ex.Message}");
                return null;
            }
        }
        public static List<QuickSaveInfo> GetQuickSaveListWithDetails()
        {
            var saves = new List<QuickSaveInfo>();

            try
            {
                string quicksaveRoot = QuickSaveFolder;
                if (!Directory.Exists(quicksaveRoot))
                    return saves;

                foreach (string dir in Directory.GetDirectories(quicksaveRoot))
                {
                    string dirName = Path.GetFileName(dir);
                    // Parse format: QuickSave_{slot}_{saveName}
                    if (dirName.StartsWith("QuickSave_"))
                    {
                        string[] parts = dirName.Split('_');
                        if (parts.Length >= 3)
                        {
                            int slot;
                            if (int.TryParse(parts[1], out slot))
                            {
                                string saveName = string.Join("_", parts.Skip(2));
                                var info = new QuickSaveInfo
                                {
                                    Slot = slot,
                                    SaveName = saveName,
                                    FullPath = dir,
                                    CreatedTime = Directory.GetCreationTime(dir),
                                    FileCount = Directory.GetFiles(dir).Length
                                };

                                // Try to read adventure name from header
                                string headerPath = Path.Combine(dir, "header.state");
                                if (File.Exists(headerPath))
                                {
                                    try
                                    {
                                        string headerContent = File.ReadAllText(headerPath);
                                        var headerJson = JObject.Parse(headerContent);
                                        info.AdventureName = headerJson["Name"]?.ToString();
                                    }
                                    catch { }
                                }

                                saves.Add(info);
                            }
                        }
                    }
                }

                saves = saves.OrderByDescending(s => s.CreatedTime).ToList();
            }
            catch (Exception ex)
            {
                AdvLogger.LogInfo($"[QuickSave] GetQuickSaveListWithDetails error: {ex.Message}");
            }

            return saves;
        }
        public static string[] GetQuickSaveList()
        {
            return GetQuickSaveListWithDetails().Select(s => s.SaveName).ToArray();
        }
        public class QuickSaveInfo
        {
            public int Slot { get; set; }
            public string SaveName { get; set; }
            public string FullPath { get; set; }
            public DateTime CreatedTime { get; set; }
            public int FileCount { get; set; }
            public string AdventureName { get; set; }

            public string DisplayName
            {
                get
                {
                    //string name = string.IsNullOrEmpty(AdventureName) ? SaveName : $"{AdventureName} - {SaveName}";
                    string name = SaveName;
                    //return $"[Slot {Slot}] {name} ({CreatedTime:yyyy-MM-dd HH:mm:ss})";
                    return $"[Slot {Slot}] {name}";
                }
            }
        }
        // Clear all quicksaves for current adventure
        public static void ClearAllQuickSaves()
        {
            try
            {
                var instance = InstanceSpecification.i;
                if (instance == null || !instance.Header.IsAdventure)
                    return;

                int slot = instance.Header.Id.Id;
                string quicksaveDir = QuickSaveFolder;

                if (Directory.Exists(quicksaveDir))
                {
                    foreach (string dir in Directory.GetDirectories(quicksaveDir))
                    {
                        string dirName = Path.GetFileName(dir);
                        if (dirName.StartsWith($"QuickSave_{slot}_"))
                        {
                            Directory.Delete(dir, true);
                        }
                    }
                }

                AdvLogger.LogInfo("[QuickSave] All quicksaves cleared");
            }
            catch (Exception ex)
            {
                AdvLogger.LogInfo($"[QuickSave] Failed to clear: {ex.Message}");
            }
        }
    }
}