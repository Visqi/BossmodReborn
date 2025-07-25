namespace BossMod.Global.MaskedCarnivale.Stage25.Act1;

public enum OID : uint
{
    Boss = 0x2678, //R=1.2
    Helper = 0x233C
}

public enum AID : uint
{
    IceSpikes = 14762, // Boss->self, 2.0s cast, single-target, boss reflects all physical damage
    ApocalypticBolt = 14766, // Boss->self, 3.0s cast, range 50+R width 8 rect
    TheRamsVoice = 14763, // Boss->self, 3.5s cast, range 8 circle
    TheDragonsVoice = 14764, // Boss->self, 3.5s cast, range 6-30 donut
    Plaincracker = 14765, // Boss->self, 3.5s cast, range 6+R circle
    TremblingEarth1 = 14774, // Helper->self, 3.5s cast, range 10-20 donut
    TremblingEarth2 = 14775, // Helper->self, 3.5s cast, range 20-30 donut
    ApocalypticRoar = 14767 // Boss->self, 5.0s cast, range 35+R 120-degree cone
}

public enum SID : uint
{
    IceSpikes = 1307, // Boss->Boss, extra=0x64
    Doom = 910 // Boss->player, extra=0x0
}

sealed class ApocalypticBolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ApocalypticBolt, new AOEShapeRect(51.2f, 45f));
sealed class ApocalypticRoar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ApocalypticRoar, new AOEShapeCone(36.2f, 60f.Degrees()));
sealed class TheRamsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheRamsVoice, 8f);
sealed class TheDragonsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheDragonsVoice, new AOEShapeDonut(6f, 30f));
sealed class Plaincracker(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Plaincracker, 7.2f);
sealed class TremblingEarth1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TremblingEarth1, new AOEShapeDonut(10f, 20f));
sealed class TremblingEarth2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TremblingEarth2, new AOEShapeDonut(20f, 30f));

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} will reflect all physical damage in act 1, all magic damage in act 2\nand switch between both in act 3. Loom, Exuviation and Diamondback\nare recommended. In act 3 can start the Final Sting combination\nat about 50% health left. (Off-guard->Bristle->Moonflute->Final Sting)");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        hints.Add("Requirements for achievement: Take no damage, use all 6 magic elements,\nuse all 3 melee types and finish faster than ideal time", false);
    }
}

sealed class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Module.PrimaryActor.FindStatus((uint)SID.IceSpikes) != null)
        {
            hints.Add($"{Module.PrimaryActor.Name} will reflect all physical damage!");
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.FindStatus((uint)SID.Doom) != null)
        {
            hints.Add("You were doomed! Cleanse it with Exuviation or finish the act fast.");
        }
    }
}

sealed class Stage25Act1States : StateMachineBuilder
{
    public Stage25Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ApocalypticBolt>()
            .ActivateOnEnter<ApocalypticRoar>()
            .ActivateOnEnter<TheRamsVoice>()
            .ActivateOnEnter<TheDragonsVoice>()
            .ActivateOnEnter<Plaincracker>()
            .ActivateOnEnter<TremblingEarth1>()
            .ActivateOnEnter<TremblingEarth2>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 635, NameID = 8129, SortOrder = 1)]
public sealed class Stage25Act1 : BossModule
{
    public Stage25Act1(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        ActivateComponent<Hints>();
    }
}
