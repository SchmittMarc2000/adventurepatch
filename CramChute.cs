using BrilliantSkies.Core.Logger;
using HarmonyLib;

//namespace AdventurePatch
//{
//    [HarmonyPatch(typeof(CannonFiringPiece))]
//    [HarmonyPatch("MainThreadFire")]
//    public class BombChuteDamageBoostPatch
//    {
//        static void Prefix(CannonFiringPiece __instance, ref RailwayGunShellStats shell)
//        {
//            // Check if bomb chute is attached
//            if (__instance.Node != null && __instance.Node.BombChuteAttached)
//            {
//                float damagefactor = 2f;
//                // Double all damage stats
//                shell.KineticDamage *= damagefactor;
//                shell.HePower *= damagefactor;
//                shell.EmpDamage *= damagefactor;
//                shell.FragPower *= damagefactor;
//                shell.FireFuel *= damagefactor;
//                shell.FireOxidizer *= damagefactor;

//                // Optional: Log for debugging
//                AdvLogger.LogInfo($"[Bomb Chute] Firing boosted shell! Damage doubled.");
//            }
//        }
//    }
//    [HarmonyPatch(typeof(RailwayGunStats))]
//    [HarmonyPatch("DeriveStats")]
//    class Patch_RailwayGunStats_DeriveStats
//    {


//        static void Postfix(bool bombChuteAttached, ref RailwayGunStats __instance)
//        {
//            if (bombChuteAttached)
//            {
//                var muzzleVelocityField = AccessTools.Field(typeof(RailwayGunStats), "_muzzleVelocity");
//                muzzleVelocityField.SetValue(__instance, 30 * 2);
//            }
//        }
//    }
//} Cram Chute buffs, might separate this into another mod at some point