﻿namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.DD180Dendainsonne;

public enum OID : uint
{
    Boss = 0x181F, // R11.6
    Tornado = 0x18F0 // R1.0
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    CharybdisCast = 7163, // Boss->location, 3.0s cast, range 6 circle
    CharybdisTornado = 7164, // TornadoVoidZones->self, no cast, range 6 circle
    EclipticMeteor = 7166, // Boss->self, 6.0s cast, range 50 circle
    Maelstrom = 7167, // TornadoVoidZones->self, 1.3s cast, range 10 circle
    Thunderbolt = 7162, // Boss->self, 2.5s cast, range 5+R 120-degree cone
    Trounce = 7165 // Boss->self, 2.5s cast, range 40+R 60-degree cone
}

class Charybdis(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CharybdisCast, 6f);
class Maelstrom(BossModule module) : Components.Voidzone(module, 10f, GetVoidzones)
{
    public static List<Actor> GetVoidzones(BossModule module) => module.Enemies((uint)OID.Tornado);
}
class Trounce(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Trounce, new AOEShapeCone(51.6f, 30f.Degrees()));
class EclipticMeteor(BossModule module) : Components.RaidwideCast(module, (uint)AID.EclipticMeteor, "Kill him before he kills you! 80% max HP damage incoming!");
class Thunderbolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Thunderbolt, new AOEShapeCone(16.6f, 60f.Degrees()));

class EncounterHints : BossComponent
{
    private int NumCast;

    public EncounterHints(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CharybdisCast or (uint)AID.Trounce or (uint)AID.Thunderbolt)
            ++NumCast;
        else if (spell.Action.ID == (uint)AID.EclipticMeteor)
            NumCast = 11;

        if (NumCast == 10)
        {
            // reset the rotation back to the beginning. This loops continuously until you hit the hp threshold (15.0% here)
            NumCast = 0;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        switch (NumCast)
        {
            case 0:
                hints.Add("Thunderbolt -> Charybdis x2 -> Thunderbolt -> Trounce from South Wall.");
                break;
            case 1:
                hints.Add("Charybdis x2 -> Thunderbolt -> Trounce from South Wall.");
                break;
            case 2:
                hints.Add("Charybdis x1 -> Thunderbolt -> Trounce from South Wall!");
                break;
            case 3:
                hints.Add("Thunderbolt -> Trounce from South Wall!");
                break;
            case 4:
                hints.Add("Boss is running to the South wall to cast Trounce!");
                break;
            case 5:
                hints.Add("Charybdis x2 -> Thunderbolt -> Charybdis -> Trounce from North Wall.");
                break;
            case 6:
                hints.Add("Charybdis x1 -> Thunderbolt -> Charybdis -> Trounce from North Wall.");
                break;
            case 7:
                hints.Add("Thunderbolt -> Charybdis -> Trounce from North Wall!");
                break;
            case 8:
                hints.Add("Charybdis -> Trounce from North Wall!");
                break;
            case 9:
                hints.Add("Boss is running to the North wall to cast Trounce!");
                break;
        }
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} will cast Trounce (Cone AOE) from the South and North wall. \nMake sure to stay near him to dodge the AOE. \n{Module.PrimaryActor.Name} will also cast Ecliptic Meteor at 15% HP, plan accordingly!");
    }
}

class ManualBurst(BossModule module) : BossComponent(module)
{
    private float HPRatio => Module.PrimaryActor.HPMP.CurHP / (float)Module.PrimaryActor.HPMP.MaxHP;
    private bool Hold => HPRatio is > 0.15f and < 0.158f;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Hold && hints.FindEnemy(Module.PrimaryActor) is { } e)
            e.Priority = AIHints.Enemy.PriorityForbidden;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (HPRatio is > 0.15f and < 0.20f)
            hints.Add("Autorotation will not attack if boss HP is between 15 and 16% - press buttons manually when ready to start burst");
    }
}

class DD180DendainsonneStates : StateMachineBuilder
{
    private const uint BeheMaxHP = 373545;
    private const uint Breakpoint1 = BeheMaxHP * 30 / 100;
    private const uint Breakpoint2 = BeheMaxHP * 21 / 100;
    private const uint Breakpoint3 = BeheMaxHP * 16 / 100;
    private const uint Breakpoint4 = BeheMaxHP * 15 / 100;

    public DD180DendainsonneStates(BossModule module) : base(module)
    {
        SimplePhase(0, StateCommon("30%", 600f), "p1")
            .ActivateOnEnter<EncounterHints>()
            .DeactivateOnEnter<Hints>()
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.HPMP.CurHP <= Breakpoint1;
        SimplePhase(1, StateCommon("21%", 120f), "p2")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.HPMP.CurHP <= Breakpoint2;
        SimplePhase(2, StateCommon("16%", 120f), "p3")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.HPMP.CurHP <= Breakpoint3;
        SimplePhase(3, StateCommon("15%", 120f), "p4")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.HPMP.CurHP <= Breakpoint4;
        DeathPhase(4, StateCommon("Enrage", 60f))
            .ActivateOnEnter<EclipticMeteor>()
            .DeactivateOnEnter<EncounterHints>();
    }

    private Action<uint> StateCommon(string name, float duration = 10000f)
    {
        return id => SimpleState(id, duration, name)
            .ActivateOnEnter<Thunderbolt>()
            .ActivateOnEnter<Charybdis>()
            .ActivateOnEnter<Maelstrom>()
            .ActivateOnEnter<Trounce>()
            .ActivateOnEnter<ManualBurst>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 216, NameID = 5461, PlanLevel = 60)]
public class DD180Dendainsonne : BossModule
{
    public DD180Dendainsonne(WorldState ws, Actor primary) : base(ws, primary, SharedBounds.ArenaBounds160170180190.Center, SharedBounds.ArenaBounds160170180190)
    {
        ActivateComponent<Hints>();
    }
}
