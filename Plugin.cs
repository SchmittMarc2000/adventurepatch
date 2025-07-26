using BrilliantSkies.Core.Timing;
using System.Reflection;
using System.Text;
using BrilliantSkies.Modding;
using HarmonyLib;
using BrilliantSkies.Modding.Helper;
using System;
using System.IO;
using BrilliantSkies.Core.Logger;


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
            get { return new Version(1, 0, 3); }
        }
    }
}
