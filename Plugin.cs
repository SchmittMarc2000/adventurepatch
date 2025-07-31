using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Core.Widgets;
using BrilliantSkies.Ftd.Planets.Factions;
using BrilliantSkies.Ftd.Planets.Factions.Designs;
using BrilliantSkies.Modding;
using BrilliantSkies.Modding.Helper;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;


using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ftd.Planets.Instances.Factions;
using BrilliantSkies.Ftd.Terrain;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Ui.Tips;
using System.Linq;


namespace AdventurePatch { 
    public class AdventurePatch : GamePlugin
    {
        public void OnLoad()
        {
            string text = Assembly.GetExecutingAssembly().Location;
            string directoryName = Path.GetDirectoryName(text);
            while (Path.GetFileName(directoryName) != "Mods")
            {
                text = directoryName;
                directoryName = Path.GetDirectoryName(text);
            }
            string ModPath = text;
            ModProblems.AddModProblem($"Adventurepatch active, version {version}", ModPath, string.Empty, false);
            //ModSettings settings = ModSettings.LoadSettings();
            Harmony HarmonyPatches = new Harmony("Adventurepatch");
            HarmonyPatches.PatchAll();


        }
        public void OnStart()
        {

        }
        public void OnSave()
        {
        }

        public string name
        {
            get { return "Adventurepatch"; }
        }

        public Version version
        {
            get { return new Version(1, 0, 4); }
        }
    }
}
