using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Ui.Consoles;
using BrilliantSkies.Ui.Consoles.Examples;
using BrilliantSkies.Ui.Resources;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventurePatch
{
    [HarmonyPatch(typeof(OptionsMenuUi))]
    public class Patch_OptionsMenuUi
    {
        [HarmonyPatch("BuildInterface")]
        [HarmonyPostfix]
        static void BuildInterface(ref ConsoleWindow __result)
        {
            __result.AllScreens.Add(new AP_Ui(__result, ProfileManager.Instance.GetModule<AP_MConfig>()));
        }
    }
}
