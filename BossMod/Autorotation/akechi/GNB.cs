﻿using BossMod.GNB;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace BossMod.Autorotation.akechi;
public sealed class GNB(RotationModuleManager manager, Actor player) : bace<AID, TraitID>(manager, player)
{
    public enum Track { AOE, Targeting, Buffs, SonicBreak, DoubleDown, GnashingFang, BurstStrike, NoMercy, Bloodfest, Zone, BowShock, ForceST, ForceAOE }
    public enum SonicBreakStrategy { Force, Delay, Automatic }
    public enum DoubleDownStrategy { Force, Delay, Automatic }
    public enum GnashingFangStrategy { Force, Delay, Automatic }
    public enum BurstStrikeStrategy { Force, Delay, Automatic }
    public enum NoMercyStrategy { Force, Delay, Automatic }
    public enum BloodfestStrategy { Force, Delay, Automatic }
    public enum ZoneStrategy { Force, Delay, Automatic }
    public enum BowShockStrategy { Force, Delay, Automatic }
    public enum FSTstrategy { Force, Automatic } // force ST
    public enum FAOEstrategy { Force, Automatic } // force AOE

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("GNB", "Gunbreaker", "Akechi-kun", RotationModuleQuality.Ok, BitMask.Build(Class.GNB), 100);

        def.DefineAOE(Track.AOE);
        def.DefineTargeting(Track.Targeting);
        def.DefineSimple(Track.Buffs, "Buffs").AddAssociatedActions(AID.NoMercy, AID.Bloodfest);

        def.Define(Track.SonicBreak).As<SonicBreakStrategy>("SB")
            .AddOption(SonicBreakStrategy.Force, "Force", "Force use of Sonic Break")
            .AddOption(SonicBreakStrategy.Delay, "Delay", "Delay use of Sonic break")
            .AddOption(SonicBreakStrategy.Automatic, "Auto", "Normal use of Sonic break");
        def.Define(Track.DoubleDown).As<DoubleDownStrategy>("DD")
            .AddOption(DoubleDownStrategy.Force, "Force", "Force use of Double Down")
            .AddOption(DoubleDownStrategy.Delay, "Delay", "Delay use of Double Down")
            .AddOption(DoubleDownStrategy.Automatic, "Auto", "Normal use of Double Down");
        def.Define(Track.GnashingFang).As<GnashingFangStrategy>("GF")
            .AddOption(GnashingFangStrategy.Force, "Force", "Force use of Gnashing Fang")
            .AddOption(GnashingFangStrategy.Delay, "Delay", "Delay use of Gnashing Fang")
            .AddOption(GnashingFangStrategy.Automatic, "Auto", "Normal use of Gnashing Fang");
        def.Define(Track.BurstStrike).As<BurstStrikeStrategy>("BStrike")
            .AddOption(BurstStrikeStrategy.Force, "Force", "Force use of Burst Strike")
            .AddOption(BurstStrikeStrategy.Delay, "Delay", "Delay use of Burst Strike")
            .AddOption(BurstStrikeStrategy.Automatic, "Auto", "Normal use of Burst Strike");
        def.Define(Track.NoMercy).As<NoMercyStrategy>("NM")
            .AddOption(NoMercyStrategy.Force, "Force", "Force use of No Mercy")
            .AddOption(NoMercyStrategy.Delay, "Delay", "Delay use of No Mercy")
            .AddOption(NoMercyStrategy.Automatic, "Auto", "Normal use of No Mercy");
        def.Define(Track.Bloodfest).As<BloodfestStrategy>("BF")
            .AddOption(BloodfestStrategy.Force, "Force", "Force use of Bloodfest")
            .AddOption(BloodfestStrategy.Delay, "Delay", "Delay use of Bloodfest")
            .AddOption(BloodfestStrategy.Automatic, "Auto", "Normal use of Bloodfest");
        def.Define(Track.Zone).As<ZoneStrategy>("Zone")
            .AddOption(ZoneStrategy.Force, "Force", "Force use of Danger/Blasting Zone")
            .AddOption(ZoneStrategy.Delay, "Delay", "Delay use of Danger/Blasting Zone")
            .AddOption(ZoneStrategy.Automatic, "Auto", "Normal use of Danger/Blasting Zone");
        def.Define(Track.BowShock).As<BowShockStrategy>("BShock")
            .AddOption(BowShockStrategy.Force, "Force", "Force use of Bow Shock")
            .AddOption(BowShockStrategy.Delay, "Delay", "Delay use of Bow Shock")
            .AddOption(BowShockStrategy.Automatic, "Auto", "Normal use of Bow Shock");
        def.Define(Track.ForceST).As<FSTstrategy>("Force ST")
            .AddOption(FSTstrategy.Force, "Force", "Force use of Single Target rotation")
            .AddOption(FSTstrategy.Automatic, "Auto", "Use original rotation");
        def.Define(Track.ForceAOE).As<FAOEstrategy>("Force AOE")
            .AddOption(FAOEstrategy.Force, "Force", "Force use of AOE rotation")
            .AddOption(FAOEstrategy.Automatic, "Auto", "Use original rotation");

        // strategy.Option(Track.x).As<xStrategy>() == xStrategy.Force
        // strategy.Option(Track.x).As<xStrategy>() == xStrategy.Delay

        return def;
    }

    public int Ammo; // 0 to 3 (cartridges)
    public int GunComboStep; // 0 to 2
    public bool NoMercy; // 0s if buff not up, max 20s
    public bool ReadyToRip; // 0s if buff not up, max 10s
    public bool ReadyToTear; // 0s if buff not up, max 10s
    public bool ReadyToGouge; // 0s if buff not up, max 10s
    public bool ReadyToBlast; // 0s if buff not up, max 10s
    public bool ReadyToRaze; // 0s if buff not up, max 10s
    public bool ReadyToBreak; // 0s if buff not up, max 30s
    public bool ReadyToReign; // 0s if buff not up, max 30s
    public float NoMercyLeft; // 0s if buff not up, max 20s
    public float AuroraLeft; // 0s if mit not up, max 20s
    public float ReadyToRazeLeft; // 0 if buff not up, max 10
    public float ReadyToBreakLeft; // 0 if buff not up, max 30
    public float ReadyToReignLeft; // 0 if buff not up, max 30
    public int NumTargetsHitByAOE;
    public int MaxCartridges;
    public int NumAOETargets;

    // upgrade paths
    public AID BestZone => Unlocked(AID.BlastingZone) ? AID.BlastingZone : AID.DangerZone;
    public AID BestContinuation => ReadyToRip ? AID.JugularRip : ReadyToTear ? AID.AbdomenTear : ReadyToGouge ? AID.EyeGouge : ReadyToBlast ? AID.Hypervelocity : AID.Continuation;
    public AID ComboLastMove => (AID)World.Client.ComboState.Action;
    public AID ComboTime => (AID)World.Client.ComboState.Remaining;
    public AID ReignCombo => ComboLastMove == AID.ReignOfBeasts ? AID.NobleBlood : ComboLastMove == AID.NobleBlood ? AID.LionHeart : AID.Bloodfest;

    protected override float GetCastTime(AID aid) => 0;

    private void GetNextBestGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        // prepull
        if (!_state.TargetingEnemy || _state.CountdownRemaining > 0.7f)
            PushGCD(AID.None, Player);

        if (_state.RangeToTarget > 3)
            PushGCD(AID.LightningShot, primaryTarget);

        if (_state.CD(AID.NoMercy) is <= 60 or >= 40)
        {
            // GnashingFang
            if (Unlocked(AID.GnashingFang) && ShouldUseGnashingFang(strategy))
            {
                if (Ammo >= 2 && Unlocked(AID.SonicBreak) && Unlocked(AID.GnashingFang) &&
                    !Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown))
                {
                    PushGCD(AID.GnashingFang, primaryTarget); // Lv60 AOE GF fix
                }
                PushGCD(AID.GnashingFang, primaryTarget);
            }

            // DoubleDown
            if (Unlocked(AID.DoubleDown) && ShouldUseDoubleDown(strategy))
            {
                PushGCD(AID.DoubleDown, primaryTarget);
            }

            // Reign Combo
            if (Unlocked(AID.ReignOfBeasts))
            {
                if (ShouldUseBeasts(strategy) && _state.CD(AID.DoubleDown) > 0
                    && _state.CD(AID.GnashingFang) > 0)
                    PushGCD(AID.ReignOfBeasts, primaryTarget);
                if (ShouldUseNoble(strategy))
                    PushGCD(AID.NobleBlood, primaryTarget);
                if (ShouldUseLion(strategy))
                    PushGCD(AID.LionHeart, primaryTarget);
            }

            // SonicBreak
            if (Unlocked(AID.SonicBreak) && ShouldUseSonic(strategy))
            {
                PushGCD(AID.SonicBreak, primaryTarget);
            }

            // BurstStrike
            if (Unlocked(AID.BurstStrike) && ShouldUseBurstStrike(strategy))
            {
                PushGCD(AID.BurstStrike, primaryTarget);
            }
        }

        // ST Logic 80 & below
        if (NumAOETargets == 1)
        {
            if (Ammo >= 1 && !Unlocked(AID.DoubleDown) &&
                !Unlocked(AID.Bloodfest) && !Unlocked(AID.Continuation) && !Unlocked(AID.GnashingFang) &&
                !Unlocked(AID.SonicBreak))
            {
                PushGCD(AID.BurstStrike, primaryTarget);
            }
        }
        // AOE Logic 80 & below
        if (NumAOETargets >= 2)
        {
            if (Ammo >= 2 && GunComboStep == 0)
            {
                if (_state.CD(AID.GnashingFang) > _state.GCD && _state.CD(AID.DoubleDown) > _state.GCD &&
                    ReadyToBreakLeft >= 0 && Unlocked(AID.DoubleDown) && !Unlocked(AID.ReignOfBeasts))
                {
                    PushGCD(AID.FatedCircle, primaryTarget); // Lv90 AOE
                }
                if (_state.CD(AID.GnashingFang) > _state.GCD && Unlocked(AID.FatedCircle) &&
                    !Unlocked(AID.DoubleDown) && !Unlocked(AID.SonicBreak))
                {
                    PushGCD(AID.FatedCircle, primaryTarget); // Lv80 AOE
                }
                if (Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown) &&
                    !Unlocked(AID.SonicBreak) && !Unlocked(AID.GnashingFang))
                {
                    PushGCD(AID.FatedCircle, primaryTarget); // Lv80 AOE
                }
            }
        }

        if (Unlocked(AID.Continuation) && (ReadyToBlast || ReadyToRaze || ReadyToGouge || ReadyToTear || ReadyToRip))
            PushOGCD(BestContinuation, primaryTarget);

        // GF2&3
        if (GunComboStep > 0)
        {
            if (GunComboStep == 2)
                PushGCD(AID.WickedTalon, primaryTarget);
            if (GunComboStep == 1)
                PushGCD(AID.SavageClaw, primaryTarget);
        }

        // 123/12
        if (ComboLastMove == AID.DemonSlice)
        {
            if (Ammo == MaxCartridges)
                PushGCD(AID.FatedCircle, primaryTarget);
            if (Ammo != MaxCartridges)
                PushGCD(AID.DemonSlaughter, primaryTarget);
        }

        if (ComboLastMove == AID.BrutalShell)
        {
            if (Ammo == MaxCartridges)
                PushGCD(AID.BurstStrike, primaryTarget);
            if (Ammo != MaxCartridges)
                PushGCD(AID.SolidBarrel, primaryTarget);
        }
        if (ComboLastMove == AID.KeenEdge)
        {
            PushGCD(AID.BrutalShell, primaryTarget);
        }

        PushGCD(AID.KeenEdge, primaryTarget);
    }

    private void GetNextBestOGCD(StrategyValues strategy, Actor? primaryTarget, float deadline)
    {
        /*if (ShouldUsePotion(strategy) && _state.CanWeave(_state.PotionCD, 1.1f, deadline))
            return ActionDefinitions.IDPotionStr; */

        // No Mercy
        if (Unlocked(AID.NoMercy) && ShouldUseNoMercy(strategy, deadline))
        {
            if (_state.GCD < 0.8f)
            {
                if ((Unlocked(AID.Bloodfest) && Ammo == 1) // Opener conditions
                    || (!Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown) && !Unlocked(AID.Bloodfest) &&
                    !Unlocked(AID.Continuation) && !Unlocked(AID.GnashingFang) && !Unlocked(AID.SonicBreak) &&
                    Ammo == MaxCartridges) // subLv53
                    || (!Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown) && !Unlocked(AID.Bloodfest) &&
                    !Unlocked(AID.Continuation) && Unlocked(AID.GnashingFang) && Unlocked(AID.SonicBreak) &&
                    Ammo == MaxCartridges) // Lv60
                    || (!Unlocked(AID.DoubleDown) && !Unlocked(AID.FatedCircle) && !Unlocked(AID.Bloodfest) &&
                    Unlocked(AID.Continuation) &&
                    Ammo == MaxCartridges) // Lv70
                    || (!Unlocked(AID.DoubleDown) && Unlocked(AID.FatedCircle) && Unlocked(AID.Bloodfest) && _state.CD(AID.GnashingFang) < _state.AnimationLock &&
                    Ammo == MaxCartridges) // Lv80
                    || (Unlocked(AID.DoubleDown) && Ammo == MaxCartridges)) // Lv90+
                    PushOGCD(AID.NoMercy, Player);
            }
        }

        // Bloodfest
        if (Unlocked(AID.Bloodfest) && ShouldUseBloodfest(strategy, primaryTarget) && _state.CanWeave(AID.Bloodfest, 0.6f, deadline))
        {
            PushOGCD(AID.Bloodfest, primaryTarget);
        }

        // Zone
        if (Unlocked(AID.DangerZone) && ShouldUseZone(strategy, deadline) && _state.CanWeave(AID.DangerZone, 0.6f, deadline))
        {
            PushOGCD(BestZone, primaryTarget);
        }

        // Bow Shock
        if (Unlocked(AID.BowShock) && ShouldUseBowShock(strategy, deadline) && _state.CanWeave(AID.BowShock, 0.6f, deadline))
        {
            PushOGCD(AID.BowShock, primaryTarget);
        }

        // Continuation
        if ((Unlocked(AID.Continuation) || Unlocked(AID.Hypervelocity)) && (ReadyToBlast || ReadyToRaze || ReadyToGouge || ReadyToTear || ReadyToRip))
            PushOGCD(BestContinuation, primaryTarget);
    }

    // NM plan
    private bool ShouldUseNoMercy(StrategyValues strategy, float deadline)
    {
        if (!Unlocked(AID.NoMercy) || strategy.Option(Track.NoMercy).As<NoMercyStrategy>() == NoMercyStrategy.Delay)
        {
            return false;
        }
        if (strategy.Option(Track.NoMercy).As<NoMercyStrategy>() == NoMercyStrategy.Force)
        {
            return true;
        }

        return strategy.Option(Track.NoMercy).As<NoMercyStrategy>() == NoMercyStrategy.Automatic &&
            ((Unlocked(AID.Bloodfest) && Unlocked(AID.Bloodfest) && Ammo == 1 && _state.CD(AID.Bloodfest) == 0) // Opener/Reopener conditions
            || (!Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown) && !Unlocked(AID.Bloodfest) &&
            !Unlocked(AID.Continuation) && !Unlocked(AID.GnashingFang) && !Unlocked(AID.SonicBreak) &&
            Ammo == MaxCartridges) // subLv53
            || (!Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown) && !Unlocked(AID.Bloodfest) &&
            !Unlocked(AID.Continuation) && Unlocked(AID.GnashingFang) && Unlocked(AID.SonicBreak) &&
            Ammo == MaxCartridges) // Lv60
            || (!Unlocked(AID.DoubleDown) && !Unlocked(AID.FatedCircle) && !Unlocked(AID.Bloodfest) &&
            Unlocked(AID.Continuation) &&
            Ammo == MaxCartridges) // Lv70
            || (!Unlocked(AID.DoubleDown) && Unlocked(AID.FatedCircle) && Unlocked(AID.Bloodfest) &&
            Ammo == MaxCartridges) // Lv80
            || (Unlocked(AID.DoubleDown) && Ammo == MaxCartridges)); // Lv90+

    }

    // Bloodfest plan
    private bool ShouldUseBloodfest(StrategyValues strategy, Actor? primaryTarget)
    {
        if (!Unlocked(AID.Bloodfest) || strategy.Option(Track.Bloodfest).As<BloodfestStrategy>() == BloodfestStrategy.Delay)
        {
            return false;
        }
        if (strategy.Option(Track.Bloodfest).As<BloodfestStrategy>() == BloodfestStrategy.Force)
        {
            return true;
        }
        return strategy.Option(Track.Bloodfest).As<BloodfestStrategy>() == BloodfestStrategy.Automatic &&
            Ammo == 0 && NoMercy;
    }

    // Sonic plan
    private bool ShouldUseSonic(StrategyValues strategy)
    {
        if (!Unlocked(AID.SonicBreak) || !NoMercy || strategy.Option(Track.SonicBreak).As<SonicBreakStrategy>() == SonicBreakStrategy.Delay)
        {
            return false;
        }
        if (strategy.Option(Track.SonicBreak).As<SonicBreakStrategy>() == SonicBreakStrategy.Force)
        {
            return true;
        }
        return (strategy.Option(Track.SonicBreak).As<SonicBreakStrategy>() == SonicBreakStrategy.Automatic &&
            ReadyToBreak && _state.CD(AID.NoMercy) <= 42.45);
    }

    // DD plan
    private bool ShouldUseDoubleDown(StrategyValues strategy)
    {
        if (!Unlocked(AID.DoubleDown) || strategy.Option(Track.DoubleDown).As<DoubleDownStrategy>() == DoubleDownStrategy.Delay)
        {
            return false;
        }
        if (strategy.Option(Track.DoubleDown).As<DoubleDownStrategy>() == DoubleDownStrategy.Force)
        {
            return true;
        }
        return strategy.Option(Track.DoubleDown).As<DoubleDownStrategy>() == DoubleDownStrategy.Automatic &&
            _state.CD(AID.DoubleDown) < 0.6f && _state.CD(AID.NoMercy) < 58 && _state.CD(AID.GnashingFang) > 0 &&
            Ammo >= 2;
    }

    // RoB plan
    private bool ShouldUseBeasts(StrategyValues strategy)
    {
        if (!Unlocked(AID.ReignOfBeasts))
        {
            return false;
        }

        return (ReadyToReign && GunComboStep == 0);
    }

    // NB plan
    private bool ShouldUseNoble(StrategyValues strategy)
    {
        if (!Unlocked(AID.ReignOfBeasts))
        {
            return false;
        }

        return GunComboStep == 3;
    }

    // LH plan
    private bool ShouldUseLion(StrategyValues strategy)
    {
        if (!Unlocked(AID.ReignOfBeasts))
        {
            return false;
        }

        return GunComboStep == 4;
    }

    // BS plan
    private bool ShouldUseBurstStrike(StrategyValues strategy)
    {
        if (!Unlocked(AID.BurstStrike) || ComboLastMove == AID.ReignOfBeasts || ComboLastMove == AID.NobleBlood ||
            strategy.Option(Track.BurstStrike).As<BurstStrikeStrategy>() == BurstStrikeStrategy.Delay)
        {
            return false;
        }
        if (strategy.Option(Track.BurstStrike).As<BurstStrikeStrategy>() == BurstStrikeStrategy.Delay)
        {
            return true;
        }
        return strategy.Option(Track.BurstStrike).As<BurstStrikeStrategy>() == BurstStrikeStrategy.Automatic &&
            ((Ammo == MaxCartridges && ComboLastMove == AID.BrutalShell) // Overcap
            || (Ammo >= 1 & _state.CD(AID.DoubleDown) > 0 && _state.CD(AID.GnashingFang) > 0 && GunComboStep == 0 && !ReadyToReign && NoMercy)
            || (Ammo >= 2 && !Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown))
            || (!Unlocked(AID.DoubleDown) && !Unlocked(AID.Bloodfest) && !Unlocked(AID.Continuation) && !Unlocked(AID.GnashingFang) && !Unlocked(AID.SonicBreak))
            || (Ammo >= 2 && Unlocked(AID.SonicBreak) && Unlocked(AID.GnashingFang) && _state.CD(AID.GnashingFang) > _state.AnimationLock && !Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown))
            || (!Unlocked(AID.FatedCircle) && !Unlocked(AID.DoubleDown) && !Unlocked(AID.Bloodfest) && Unlocked(AID.Continuation)));
    }

    // GF plan
    private bool ShouldUseGnashingFang(StrategyValues strategy)
    {
        if (!Unlocked(AID.GnashingFang) || strategy.Option(Track.GnashingFang).As<GnashingFangStrategy>() == GnashingFangStrategy.Delay)
        {
            return false;
        }
        if (strategy.Option(Track.GnashingFang).As<GnashingFangStrategy>() == GnashingFangStrategy.Force)
        {
            return true;
        }
        return strategy.Option(Track.GnashingFang).As<GnashingFangStrategy>() == GnashingFangStrategy.Automatic && _state.TargetingEnemy && _state.CD(AID.GnashingFang) < 0.6f && Ammo >= 1 && (NoMercy || _state.CD(AID.NoMercy) > 17);
    }

    // Zone plan
    private bool ShouldUseZone(StrategyValues strategy, float deadline)
    {
        if (!Unlocked(AID.DangerZone) || strategy.Option(Track.Zone).As<ZoneStrategy>() == ZoneStrategy.Delay)
        {
            return false;
        }
        if (strategy.Option(Track.Zone).As<ZoneStrategy>() == ZoneStrategy.Force)
        {
            return true;
        }
        return strategy.Option(Track.Zone).As<ZoneStrategy>() == ZoneStrategy.Automatic &&
            _state.TargetingEnemy && (NoMercy || _state.CD(AID.NoMercy) > 17);
    }

    // BowShock plan
    private bool ShouldUseBowShock(StrategyValues strategy, float deadline)
    {
        if (!Unlocked(AID.BowShock) || strategy.Option(Track.BowShock).As<BowShockStrategy>() == BowShockStrategy.Delay)
        {
            return false;
        }
        if (strategy.Option(Track.BowShock).As<BowShockStrategy>() == BowShockStrategy.Force)
        {
            return true;
        }
        return strategy.Option(Track.BowShock).As<BowShockStrategy>() == BowShockStrategy.Automatic &&
            _state.TargetingEnemy && NoMercy;
    }

    public override void Exec(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay)
    {
        var targeting = strategy.Option(Track.Targeting).As<Targeting>();
        SelectPrimaryTarget(targeting, ref primaryTarget, range: 25);
        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);
        _state.HaveTankStance = Player.FindStatus(SID.RoyalGuard) != null;

        var gauge = GetGauge<GunbreakerGauge>();
        Ammo = gauge.Ammo;
        GunComboStep = gauge.AmmoComboStep;
        MaxCartridges = Unlocked(TraitID.CartridgeChargeII) ? 3 : 2;

        NoMercyLeft = StatusLeft(SID.NoMercy);
        NoMercy = Player.FindStatus(SID.NoMercy) != null;
        ReadyToRip = Player.FindStatus(SID.ReadyToRip) != null;
        ReadyToTear = Player.FindStatus(SID.ReadyToTear) != null;
        ReadyToGouge = Player.FindStatus(SID.ReadyToGouge) != null;
        ReadyToBreak = Player.FindStatus(SID.ReadyToBreak) != null;
        ReadyToBreakLeft = StatusLeft(SID.ReadyToBreak);
        ReadyToReign = Player.FindStatus(SID.ReadyToReign) != null;
        ReadyToReignLeft = StatusLeft(SID.ReadyToReign);
        ReadyToBlast = Player.FindStatus(SID.ReadyToBlast) != null;
        ReadyToRaze = Player.FindStatus(SID.ReadyToRaze) != null;
        ReadyToRazeLeft = StatusLeft(SID.ReadyToRaze);
        AuroraLeft = StatusLeft(SID.Aurora);

        NumAOETargets = strategy.Option(Track.AOE).As<AOEStrategy>() switch
        {
            AOEStrategy.AOE => NumMeleeAOETargets(),
            _ => 0
        };

        GetNextBestGCD(strategy, primaryTarget);
        QueueOGCD(deadline => GetNextBestOGCD(strategy, primaryTarget, deadline));
    }
}
