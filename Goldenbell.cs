using BrilliantSkies.Common.StatusChecking;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Pooling;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Core.UiSounds;
using BrilliantSkies.Core;
using BrilliantSkies.Effects.SoundSystem;
using BrilliantSkies.FromTheDepths.Planets;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Modding.Types;
using BrilliantSkies.Modding;
using BrilliantSkies.Ui.Tips;
using UnityEngine;
using BrilliantSkies.Core.Serialisation.Parameters.Prototypes;
using System;
using BrilliantSkies.Core.CSharp;
using BrilliantSkies.Localisation.Widgets;
using BrilliantSkies.DataManagement.Vars;
using BrilliantSkies.Core.Networking;
using NetInfrastructure;
using BrilliantSkies.Ftd.Multiplayer;
using BrilliantSkies.Ftd.Multiplayer.NetworkCommunication;
using BrilliantSkies.Ftd.Multiplayer.Requests;
/*public class GoldenBellData : PrototypeSystem
{
    public GoldenBellData(UInt32 uniqueId) : base(uniqueId)
    {
    }
    // Time of last bell activation
    [LocalVariableScraped(0U, "##Golden_Bell.Attribute_GoldenBellActivationTime", "Time of last activation", "")]
    public Var<uint> LastActivationTime { get; set; } = new Var<uint>(uint.MaxValue);
}*/
public class GoldenBell : Block
{
    private bool AboveWaterAndCanFunction => base.AltitudeAboveWaves > 0f;
    private bool BelowSpaceAndCanFunction => base.AltitudeAboveWaves < 300f;

    private uint LastRingTime = uint.MaxValue;
    public override void BlockStart()
    {
        this.NotAdventure = !InstanceSpecification.i.Header.IsAdventure;
    }

    public void PlayBellActivate(Vector3 location)
    {
        AudioClipDefinition randomClipByCollectionName = Configured.i.AudioCollections.GetRandomClipByCollectionName("Bell Ring");
        if (randomClipByCollectionName != null)
        {
            Pooler.GetPool<AdvSoundManager>().PlaySound(new SoundRequest(randomClipByCollectionName, location)
            {
                Priority = SoundPriority.ShouldHear,
                Pitch = Aux.Rnd.NextFloat(0.9f, 1.1f),
                MinDistance = 50f,
                Volume = 0.6f
            });
        }
    }

    public override void Secondary(Transform T)
    {
        if (Net.IsClient)
        {
            this._clientIsWaitingForBellNoise = true;
            base.GetConstructableOrSubConstructable().AllMultiplayerRestricted.RPCRequest_SyncroniseBlock(this);
        }
        else
        {
            if (this.RegisterSpawnRequest())
            {
                this.PlayBellActivate(base.GameWorldPosition);
                if (Net.IsServer)
                {
                    base.GetConstructableOrSubConstructable().AllMultiplayerRestricted.RPCRequest_SyncroniseBlock(this, true);
                }
            }
            else
            {
                GUISoundManager.GetSingleton().PlayFailure();
            }
        }
    }

    public override void SyncroniseUpdate()
    {
        if (Net.IsServer && this.RegisterSpawnRequest())
        {
            this.PlayBellActivate(base.GameWorldPosition);
            base.GetConstructableOrSubConstructable().AllMultiplayerRestricted.RPCRequest_SyncroniseBlock(this, true);
        }
        else
        {
            base.GetConstructableOrSubConstructable().AllMultiplayerRestricted.RPCRequest_SyncroniseBlock(this, false);
        }
    }

    private static Vector3d FindAPointInFrontOfPrimaryForce(float range)
    {
        float y = EngineSplines.Instance.AdventureAngleCurve.Evaluate(UnityEngine.Random.Range(-1f, 1f));
        Quaternion rotation = InstanceSpecification.i.Adventure.PrimaryForce.C.myTransform.rotation;
        Angles.RemoveRollAndPitch(rotation);
        Vector3d vector3d = InstanceSpecification.i.Adventure.PrimaryForceUniversePosition + Quaternion.Euler(0f, y, 0f) * rotation * new Vector3(0f, 0f, range);
        vector3d.y = (AdventureModeProgression.AdventuringType == AdventureType.Land) ? 50 : 50.0;
        return vector3d;

    }

    private bool RegisterSpawnRequest()
    {
        if (!InstanceSpecification.i.Header.IsAdventure)
        {
            this.NotAdventure = true;
            return false;
        }

        bool flag2 = !this.BelowSpaceAndCanFunction || !this.AboveWaterAndCanFunction;
        int num = (int)GameTimer.Instance.GameTime;
        //bool canRingBell = num - this.GoldenBellData.LastActivationTime > GoldenBellRingPeriod | this.GoldenBellData.LastActivationTime.Us == uint.MaxValue;
        bool canRingBell = num - this.LastRingTime > GoldenBellRingPeriod | this.LastRingTime == uint.MaxValue;

        if (canRingBell)
        {
            Vector3d universePosition = FindAPointInFrontOfPrimaryForce(1000);
            float materialAmount = UnityEngine.Random.Range(1f, 2f) * InstanceSpecification.i.Adventure.WarpPlaneDifficulty * 1000 + 25000;
            bool isServer = Net.IsServer;
            if (isServer)
            {
                Coms.AddRpc(new RpcRequest(delegate (INetworkIdentity n)
                {
                    ServerOutgoingRpcs.SpawnResourceZoneAdventure(n, universePosition, (float)materialAmount);
                }));
            }
            AdventureModeProgression.Common_SpawnRz(universePosition, materialAmount);
            //GoldenBellData.LastActivationTime.Us = (uint)num;
            this.LastRingTime = (uint)num;
        }

        return !flag2 && canRingBell;
    }

    public override void CheckStatus(IStatusUpdate updater)
    {
        base.CheckStatus(updater);

        if (!this.AboveWaterAndCanFunction)
        {
            updater.FlagWarning(this, "Bell cannot ring underwater");
        }
        if (!this.BelowSpaceAndCanFunction)
        {
            updater.FlagWarning(this, "Bell cannot ring above 300m");
        }

        int num = (int)GameTimer.Instance.GameTime;
        bool canRingBell = num - this.LastRingTime > GoldenBellRingPeriod | this.LastRingTime == uint.MaxValue;
        if (!canRingBell)
        {
            updater.FlagWarning(this, "You must wait 30 minutes between activations");
        }
    }

    protected override void AppendToolTip(ProTip tip)
    {
        base.AppendToolTip(tip);
        tip.Add<ProTipSegment_TitleSubTitle>(new ProTipSegment_TitleSubTitle("Golden Bell", "Use the bell to summon a new Resource Zone."), Position.Middle);
        tip.InfoOnly = false;
        if (this.NotAdventure)
        {
            tip.Add<ProTipSegment_TextAdjustable>(new ProTipSegment_TextAdjustable(500, "<!Only usable in adventure!>"), Position.Middle);
        }
    }
    public static int GoldenBellRingPeriod = 1800;
    private bool NotAdventure = false;
    private bool _clientIsWaitingForBellNoise = false;
}
