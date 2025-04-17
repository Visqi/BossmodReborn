﻿namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE11ShadowOfDeathHand;

public enum OID : uint
{
    Boss = 0x2DA7, // R4.320, x1
    TamedCarrionCrow = 0x2DA8, // R1.800, spawn during fight
    Beastmaster = 0x2DA9, // R0.500, x1
    Whirlwind = 0x2DAA, // R1.000, spawn during fight
    Deathwall = 0x2EE8, // R0.500, x1
    DeathwallEvent = 0x1EB02E, // R0.500, EventObj type, spawn during fight
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackBeastmaster = 6497, // Beastmaster->player, no cast, single-target
    AutoAttackBossCrow = 6498, // Boss/TamedCarrionCrow->player, no cast, single-target

    FastBlade = 20155, // Beastmaster->player, no cast, single-target, micro tankbuster
    SavageBlade = 20156, // Beastmaster->player, no cast, single-target, micro tankbuster
    RippingBlade = 20157, // Beastmaster->player, no cast, single-target, micro tankbuster
    BestialLoyalty = 20163, // Beastmaster->self, 3.0s cast, single-target, visual (summon crows)
    BestialLoyaltyAOE = 20164, // Helper->location, no cast, range 5 circle (aoe where crows appear)
    RunWild = 20166, // Beastmaster->self, 4.0s cast, interruptible, buffs enemies with status effect Running Wild, seems to be some kind of damage buff
    Reward = 20169, // Beastmaster->Boss, 3.0s cast, single-target, heal
    WrathOfTheForsaken = 20170, // Boss->self, 3.0s cast, single-target, damage up after beastmaster death
    HardBeak = 20171, // Boss->player, 4.0s cast, single-target, tankbuster
    PiercingBarrageBoss = 20172, // Boss->self, 3.0s cast, range 40 width 8 rect aoe
    Helldive = 20173, // Boss->players, 5.0s cast, range 6 circle stack
    BroadsideBarrage = 20174, // Boss->self, 5.0s cast, range 40 width 40 rect aoe, 'knock-forward' 50 on failure
    BlindsideBarrage = 20175, // Boss->self, 4.0s cast, single-target, visual (raidwide + deathwall)
    BlindsideBarrageAOE = 20182, // Helper->self, 4.5s cast, range 30 circle, raidwide
    StrongWind = 20181, // Deathwall->self, no cast, range 20-30 donut, deathwall
    RollingBarrage = 20180, // Boss->self, 5.0s cast, single-target, visual
    RollingBarrageAOE = 20188, // Helper->self, 5.0s cast, range 8 circle aoe
    GaleForce = 20189, // Whirlwind->self, no cast, range 4 circle aoe around whirlwind, knockback 8 + dot

    NorthWind = 20176, // Boss->self, 7.5s cast, single-target, visual
    SouthWind = 20177, // Boss->self, 7.5s cast, single-target, visual
    EastWind = 20178, // Boss->self, 7.5s cast, single-target, visual
    WestWind = 20179, // Boss->self, 7.5s cast, single-target, visual
    WindVisual = 20183, // Helper->self, 7.5s cast, range 60 width 60 rect, visual (knockback across arena)
    NorthWindAOE = 20184, // Helper->self, no cast, range 10 width 60 rect 'knock-forward' 30
    SouthWindAOE = 20185, // Helper->self, no cast, range 10 width 60 rect 'knock-forward' 30
    EastWindAOE = 20186, // Helper->self, no cast, range 10 width 60 rect 'knock-forward' 30
    WestWindAOE = 20187, // Helper->self, no cast, range 10 width 60 rect 'knock-forward' 30

    HardBeakCrow = 20190, // TamedCarrionCrow->player, 4.0s cast, single-target, micro tankbuster
    PiercingBarrageCrow = 20191 // TamedCarrionCrow->self, 3.0s cast, range 40 width 8 rect
}

class BestialLoyalty(BossModule module) : Components.CastHint(module, (uint)AID.BestialLoyalty, "Summon crows");
class RunWild(BossModule module) : Components.CastInterruptHint(module, (uint)AID.RunWild, showNameInHint: true);
class HardBeak(BossModule module) : Components.SingleTargetCast(module, (uint)AID.HardBeak);
class Helldive(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Helldive, 6f);
class BroadsideBarrage(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BroadsideBarrage, new AOEShapeRect(40f, 20f));
class BlindsideBarrage(BossModule module) : Components.RaidwideCast(module, (uint)AID.BlindsideBarrage, "Raidwide + deathwall appears");
class RollingBarrage(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RollingBarrageAOE, 8f);
class Whirlwind(BossModule module) : Components.Voidzone(module, 4f, GetWhirlwind)
{
    private static List<Actor> GetWhirlwind(BossModule module) => module.Enemies((uint)OID.Whirlwind);
}
class Wind(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.WindVisual, 30f, kind: Kind.DirForward);

abstract class PiercingBarrage(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(40f, 4f));
class PiercingBarrageBoss(BossModule module) : PiercingBarrage(module, (uint)AID.PiercingBarrageBoss);
class PiercingBarrageCrow(BossModule module) : PiercingBarrage(module, (uint)AID.PiercingBarrageCrow);

class CE11ShadowOfDeathHandStates : StateMachineBuilder
{
    public CE11ShadowOfDeathHandStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BestialLoyalty>()
            .ActivateOnEnter<RunWild>()
            .ActivateOnEnter<HardBeak>()
            .ActivateOnEnter<PiercingBarrageBoss>()
            .ActivateOnEnter<Helldive>()
            .ActivateOnEnter<BroadsideBarrage>()
            .ActivateOnEnter<BlindsideBarrage>()
            .ActivateOnEnter<RollingBarrage>()
            .ActivateOnEnter<Whirlwind>()
            .ActivateOnEnter<Wind>()
            .ActivateOnEnter<PiercingBarrageCrow>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 735, NameID = 5)] // bnpcname=9400
public class CE11ShadowOfDeathHand(WorldState ws, Actor primary) : BossModule(ws, primary, new(825, 640), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies(OID.Beastmaster));
        Arena.Actors(Enemies(OID.TamedCarrionCrow));
    }
}
