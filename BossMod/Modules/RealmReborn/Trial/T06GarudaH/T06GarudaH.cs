﻿namespace BossMod.RealmReborn.Trial.T06GarudaH;

public enum OID : uint
{
    Boss = 0xF2, // x1
    Monolith = 0xF0, // x4
    Whirlwind = 0x623, // x7
    EyeOfTheStorm = 0x624, // x1
    RazorPlume = 0xF1, // spawn during fight
    SatinPlume = 0x5FF, // spawn during fight
    Chirada = 0x61E, // spawn during fight
    Suparna = 0x61F, // spawn during fight
    Monolith1 = 0x1E8706, // x1, EventObj type
    Monolith2 = 0x1E8707, // x1, EventObj type
    Monolith3 = 0x1E8708, // x1, EventObj type
    Monolith4 = 0x1E8709 // x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/Chirada/Suparna->player, no cast, single-target

    Friction = 1379, // Boss/Chirada/Suparna->players, no cast, range 5 circle at random target
    Downburst = 1380, // Boss/Chirada/Suparna->self, no cast, range 10+R ?-degree cone cleave
    WickedWheel = 1381, // Boss/Chirada/Suparna->self, no cast, range 7+R circle cleave
    Slipstream = 1382, // Boss/Chirada/Suparna->self, 2.5s cast, range 10+R ?-degree cone aoe
    MistralShriek = 1384, // Boss/Chirada/Suparna->self, 3.0s cast, range 23+R LOSable raidwide (from center); adds start casting if not killed in ~44s
    MistralSong = 1390, // Boss->self, 3.0s cast, range 30+R circle LOSable raidwide (from side)
    AerialBlast = 1385, // Boss->self, 4.0s cast, raidwide
    GreatWhirlwind = 1386, // Whirlwind->location, 3.0s cast, range 8 circle aoe with knockback 15
    EyeOfTheStorm = 1387, // EyeOfTheStorm->self, 3.0s cast, range 12-25 donut
    Featherlance = 1388, // RazorPlume->self, no cast, range 8 circle, suicide attack if not killed in ~25s
    ThermalTumult = 1389 // SatinPlume->self, no cast, range 6 circle, suicide attack (applies sleep) if not killed in ~25s
}

public enum TetherID : uint
{
    Rehabilitation = 4, // Chirada/Suparna->Boss, green (heal)
    DamageUp = 11, // Chirada/Suparna->Boss, red (damage up)
}

// disallow clipping monoliths
class Friction(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.PrimaryActor.CastInfo == null) // don't forbid standing near monoliths while boss is casting to allow avoiding aoes
            foreach (var m in ((T06GarudaH)Module).ActiveMonoliths)
                hints.AddForbiddenZone(ShapeDistance.Circle(m.Position, 5f));
    }
}

class Downburst(BossModule module) : Components.Cleave(module, (uint)AID.Downburst, new AOEShapeCone(11.7f, 60f.Degrees()));
class Slipstream(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Slipstream, new AOEShapeCone(11.7f, 45f.Degrees()));

class MistralShriek(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.MistralShriek, 24.7f, true)
{
    public override ReadOnlySpan<Actor> BlockerActors() => CollectionsMarshal.AsSpan(((T06GarudaH)Module).ActiveMonoliths);
}

class MistralSong(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.MistralSong, 31.7f, true)
{
    public override ReadOnlySpan<Actor> BlockerActors() => CollectionsMarshal.AsSpan(((T06GarudaH)Module).ActiveMonoliths);
}

class AerialBlast(BossModule module) : Components.RaidwideCast(module, (uint)AID.AerialBlast);
class GreatWhirlwind(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GreatWhirlwind, 8f);
class EyeOfTheStorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EyeOfTheStorm, new AOEShapeDonut(12f, 25f));

class T06GarudaHStates : StateMachineBuilder
{
    public T06GarudaHStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Friction>()
            .ActivateOnEnter<Downburst>()
            .ActivateOnEnter<Slipstream>()
            .ActivateOnEnter<MistralShriek>()
            .ActivateOnEnter<MistralSong>()
            .ActivateOnEnter<AerialBlast>()
            .ActivateOnEnter<GreatWhirlwind>()
            .ActivateOnEnter<EyeOfTheStorm>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 61, NameID = 1644)]
public class T06GarudaH : BossModule
{
    public readonly List<Actor> ActiveMonoliths;

    public T06GarudaH(WorldState ws, Actor primary) : base(ws, primary, default, new ArenaBoundsCircle(22))
    {
        ActiveMonoliths = Enemies((uint)OID.Monolith);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Suparna or (uint)OID.Chirada => e.Actor.Tether.ID == (uint)TetherID.Rehabilitation ? 3 : 2,
                (uint)OID.SatinPlume => 3,
                (uint)OID.RazorPlume => 2,
                (uint)OID.Boss => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(ActiveMonoliths, Colors.Object, true);
        Arena.Actors(Enemies((uint)OID.Suparna));
        Arena.Actors(Enemies((uint)OID.Chirada));
    }
}
