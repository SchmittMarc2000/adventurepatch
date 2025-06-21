//using AdventurePatch;
//using BrilliantSkies.Common.StatusChecking;
//using BrilliantSkies.Core.UiSounds;
//using BrilliantSkies.Ftd.Planets.Instances;
//using BrilliantSkies.Ui.Tips;
//using UnityEngine;

//public class AdventureWarper : Block
//{   
//    public override void BlockStart()
//    {
//        this.NotAdventure = !InstanceSpecification.i.Header.IsAdventure;
//    }
//    private bool RegisterWarpRequest()
//    {
//        ModSettings settings;
//        settings = ModSettings.Reload();
//        bool flag = !InstanceSpecification.i.Header.IsAdventure;
//        bool result;
//        if (flag)
//        {
//            this.NotAdventure = true;
//            result = false;
//        }
//        else
//        {
//            bool flag2 = InstanceSpecification.i.Adventure.TimeInWarpPlane > settings.AdventureWarperDelay;

//            if (flag2)
//            {
//                InstanceSpecification.i.Adventure.TimeInWarpPlane = 0;
//                return true;
//            }
//        }
//        return false;
//    }
//    public override void Secondary(Transform T)
//    {
//        bool flag = this.RegisterWarpRequest();
//        if (flag)
//        {
//            AdventureModeProgression.Common_DoWarp(WarpGateType.Harder, InstanceSpecification.i.Adventure.PrimaryForceUniversePosition);
//        }
//        else
//        {
//            GUISoundManager.GetSingleton().PlayFailure();
//        }
//    }
//    public override void CheckStatus(IStatusUpdate updater)
//    {
//        base.CheckStatus(updater);
//    }
//    protected override void AppendToolTip(ProTip tip)
//    {
//        ModSettings settings;
//        settings = ModSettings.Reload();
//        base.AppendToolTip(tip);
//        tip.Add<ProTipSegment_TitleSubTitle>(new ProTipSegment_TitleSubTitle("Adventurewarper", "Use to warp to a higher difficulty."), Position.Middle);
//        tip.InfoOnly = false;
//        if (this.NotAdventure)
//        {
//            tip.Add<ProTipSegment_TextAdjustable>(new ProTipSegment_TextAdjustable(500, "<!Only usable in adventure!>"), Position.Middle);
//        }
//        bool flag2 = InstanceSpecification.i.Adventure.TimeInWarpPlane > settings.AdventureWarperDelay;
//        if (!flag2)
//        {
//            tip.Add<ProTipSegment_TextAdjustable>(new ProTipSegment_TextAdjustable(500, "<!Not ready yet.!>"), Position.Middle);
//        } 
//    }
//    private bool NotAdventure = false;
//}
