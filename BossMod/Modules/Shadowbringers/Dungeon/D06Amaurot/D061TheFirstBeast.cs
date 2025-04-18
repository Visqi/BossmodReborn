namespace BossMod.Shadowbringers.Dungeon.D06Amaurot.D061FirstBeast;

public enum OID : uint
{
    Boss = 0x27B6, // R=5.4
    FallenStar = 0x29DC, // R=2.4
    FallingTower = 0x18D6, // R=0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    VenomousBreath = 15566, // Boss->self, 3.0s cast, range 9 120-degree cone
    MeteorRainVisual = 15556, // Boss->self, 3.0s cast, single-target
    MeteorRain = 15558, // Helper->location, 3.0s cast, range 6 circle

    TheFallingSkyVisual = 15561, // Boss->self, 3.0s cast, single-target
    TheFallingSky = 15562, // Helper->location, 4.5s cast, range 10 circle
    TheFinalSky = 15563, // Boss->self, 12.0s cast, range 70 circle, meteor if failed to LoS
    CosmicKiss = 17108, // FallenStar->self, 4.5s cast, range 50 circle, meteor, damage fall off AOE
    CosmicShrapnel = 17110, // FallenStar->self, no cast, range 8 circle, meteor explodes after final sky, can be ignored since it only does like 500 dmg

    Towerfall = 15564, // FallingTower->self, 8.0s cast, range 35 width 40 rect
    Earthquake = 15565, // Boss->self, 4.0s cast, range 10 circle
    TheBurningSkyVisual = 15559, // Boss->self, 5.2s cast, single-target
    TheBurningSky1 = 13642, // FallingTower->location, 3.5s cast, range 6 circle
    TheBurningSky2 = 15560 // Helper->player, 5.2s cast, range 6 circle, spread
}

public enum IconID : uint
{
    Meteor = 57 // player
}

class VenomousBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VenomousBreath, new AOEShapeCone(9f, 60f.Degrees()));
class MeteorRain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MeteorRain, 6f);
class TheFallingSky(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheFallingSky, 10f);
class CosmicKiss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CosmicKiss, 10f);
class Towerfall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Towerfall, new AOEShapeRect(35f, 20f));
class Earthquake(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Earthquake, 10f);
class TheBurningSky1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheBurningSky1, 6f);
class TheBurningSky2(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.TheBurningSky2, 6f);

class Meteors(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeCircle circle = new(10f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Meteor)
            CurrentBaits.Add(new(actor, actor, circle));
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.FallenStar)
            CurrentBaits.Clear();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (ActiveBaitsOn(actor).Count != 0)
            hints.Add("Place meteor!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (ActiveBaitsOn(actor).Count != 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(new(-80f, 97f), new(-80f, 67f), 15f));
    }
}

class TheFinalSky(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.TheFinalSky, 70f, safeInsideHitbox: false)
{
    public override ReadOnlySpan<Actor> BlockerActors() => CollectionsMarshal.AsSpan(Module.Enemies((uint)OID.FallenStar));
}

class D061FirstBeastStates : StateMachineBuilder
{
    public D061FirstBeastStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheFinalSky>()
            .ActivateOnEnter<Meteors>()
            .ActivateOnEnter<TheBurningSky1>()
            .ActivateOnEnter<TheBurningSky2>()
            .ActivateOnEnter<Earthquake>()
            .ActivateOnEnter<Towerfall>()
            .ActivateOnEnter<CosmicKiss>()
            .ActivateOnEnter<TheFallingSky>()
            .ActivateOnEnter<MeteorRain>()
            .ActivateOnEnter<VenomousBreath>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 652, NameID = 8201)]
public class D061FirstBeast(WorldState ws, Actor primary) : BossModule(ws, primary, new(-80f, 82f), new ArenaBoundsSquare(19.5f));
