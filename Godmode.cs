using BrilliantSkies.Ftd.Avatar.Health;
using BrilliantSkies.PlayerProfiles;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventurePatch
{
    //[HarmonyPatch(typeof(sHealth), "BrilliantSkies.Ftd.Avatar.Health.IDamageProcessor.ApplyDamage")]
    //public static class Patch_ApplyDamage
    //{
    //    static bool Prefix()
    //    {
    //        if (ProfileManager.Instance.GetModule<AP_MConfig>().PreventDamage)
    //        {
    //            return false;
    //        }

    //        return true;
    //    }
    //} Did not work out :/

    [HarmonyPatch(typeof(sHealth), "Damage")]
    public static class Patch_Damage
    {
        static bool Prefix(float damage)
        {
            if (ProfileManager.Instance.GetModule<AP_MConfig>().PreventDamage)
            {
                return false;
            }

            return true;
        }
    }
    

}
