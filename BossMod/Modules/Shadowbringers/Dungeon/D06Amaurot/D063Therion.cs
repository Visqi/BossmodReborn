namespace BossMod.Shadowbringers.Dungeon.D06Amaurot.D063Therion;

public enum OID : uint
{
    Boss = 0x27C1, // R=25.84
    TheFaceOfTheBeast = 0x27C3, // R=2.1
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 15574, // Boss->player, no cast, single-target
    ShadowWreck = 15587, // Boss->self, 4.0s cast, range 100 circle
    ApokalypsisFirst = 15575, // Boss->self, 6.0s cast, range 76 width 20 rect
    ApokalypsisRest = 15577, // Helper->self, no cast, range 76 width 20 rect
    TherionCharge = 15578, // Boss->location, 7.0s cast, range 100 circle, damage fall off AOE

    DeathlyRayVisualFaces1 = 15579, // Boss->self, 3.0s cast, single-target
    DeathlyRayVisualFaces2 = 16786, // Boss->self, no cast, single-target
    DeathlyRayVisualThereion1 = 17107, // Helper->self, 5.0s cast, range 80 width 6 rect
    DeathlyRayVisualThereion2 = 15582, // Boss->self, 3.0s cast, single-target
    DeathlyRayVisualThereion3 = 16785, // Boss->self, no cast, single-target

    DeathlyRayFacesFirst = 15580, // TheFaceOfTheBeast->self, no cast, range 60 width 6 rect
    DeathlyRayFacesRest = 15581, // Helper->self, no cast, range 60 width 6 rect
    DeathlyRayThereionFirst = 15583, // Helper->self, no cast, range 60 width 6 rect
    DeathlyRayThereionRest = 15585, // Helper->self, no cast, range 60 width 6 rect
    Misfortune = 15586, // Helper->location, 3.0s cast, range 6 circle
}

class ShadowWreck(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ShadowWreck));
class Misfortune(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Misfortune), 6);

class Border(BossModule module) : Components.GenericAOEs(module, warningText: "Platform will be removed during next Apokalypsis!")
{
    private const int SquareHalfWidth = 2;
    private const float RectangleHalfWidth = 10.1f;
    private const int MaxError = 5;
    private static readonly AOEShapeRect _square = new(2, 2, 2);

    public readonly List<AOEInstance> BreakingPlatforms = [];

    public static readonly WPos[] positions = [new(-12, -71), new(12, -71), new(-12, -51),
    new(12, -51), new(-12, -31), new(12, -31), new(-12, -17), new(12, -17), new(0, -65), new(0, -45)];

    private static readonly Square[] shapes = [new(positions[0], SquareHalfWidth), new(positions[1], SquareHalfWidth), new(positions[2], SquareHalfWidth),
    new(positions[3], SquareHalfWidth), new(positions[4], SquareHalfWidth), new(positions[5], SquareHalfWidth), new(positions[6], SquareHalfWidth),
    new(positions[7], SquareHalfWidth), new(positions[8], RectangleHalfWidth), new(positions[9], RectangleHalfWidth)];

    private static readonly Rectangle[] rect = [new(new(0, -45), 10, 30)];
    public readonly List<Shape> unionRefresh = [.. rect.Concat(shapes.Take(8))];
    private readonly List<Shape> difference = [];
    public static readonly ArenaBoundsComplex DefaultArena = new([.. rect, .. shapes.Take(8)]);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = BreakingPlatforms.Count;
        if (count == 0)
            return [];
        List<AOEInstance> aoes = new(count);
        for (var i = 0; i < count; ++i)
        {
            var p = BreakingPlatforms[i];
            aoes.Add(new(_square, p.Origin, Color: Colors.FutureVulnerable, Risky: Module.FindComponent<Apokalypsis>()!.NumCasts == 0));
        }
        return aoes;
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008)
        {
            for (var i = 0; i < 8; ++i)
            {
                if (actor.Position.AlmostEqual(positions[i], MaxError))
                {
                    if (unionRefresh.Remove(shapes[i]))
                    {
                        if (unionRefresh.Count == 7)
                            difference.Add(shapes[8]);
                        else if (unionRefresh.Count == 5)
                            difference.Add(shapes[9]);
                        ArenaBoundsComplex arena = new([.. unionRefresh], [.. difference]);
                        Arena.Bounds = arena;
                        Arena.Center = arena.Center;
                    }
                    BreakingPlatforms.Remove(new(_square, positions[i], Color: Colors.FutureVulnerable));
                }
            }
        }
        if (state == 0x00100020)
        {
            for (var i = 0; i < 8; ++i)
            {
                if (actor.Position.AlmostEqual(positions[i], MaxError))
                    BreakingPlatforms.Add(new(_square, positions[i], Color: Colors.FutureVulnerable));
            }
        }
    }
}

class Apokalypsis(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    private static readonly AOEShapeRect _rect = new(76, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(_rect, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation, _activation, Risky: (Module.FindComponent<Border>()!.unionRefresh.Count - Module.FindComponent<Border>()!.BreakingPlatforms.Count) > 1);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ApokalypsisFirst)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ApokalypsisFirst:
            case AID.ApokalypsisRest:
                if (++NumCasts == 5)
                {
                    _activation = default;
                    NumCasts = 0;
                }
                break;
        }
    }
}

class ThereionCharge(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _rect = new(10, 20, 100);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TherionCharge)
            _aoe = new(_rect, NumCasts == 0 ? Border.positions[8] : Border.positions[9], default, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TherionCharge)
        {
            ++NumCasts;
            _aoe = null;
        }
    }
}

class DeathlyRayThereion(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeRect rect = new(60, 3);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DeathlyRayVisualThereion1)
            _aoe = new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DeathlyRayThereionFirst:
            case AID.DeathlyRayThereionRest:
                if (++NumCasts == 5)
                {
                    _aoe = null;
                    NumCasts = 0;
                }
                break;
        }
    }
}

class DeathlyRayFaces(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _rect = new(60, 3);
    private readonly List<AOEInstance> _aoesFirst = new(5), _aoesRest = new(5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var countFirst = _aoesFirst.Count;
        var countRest = _aoesRest.Count;
        var total = countFirst + countRest;
        if (total == 0)
            return [];
        List<AOEInstance> aoes = new(total);
        for (var i = 0; i < countFirst; ++i)
            aoes.Add(_aoesFirst[i]);
        for (var i = 0; i < countRest; ++i)
            aoes.Add(_aoesRest[i] with { Color = countFirst > 0 ? Colors.AOE : Colors.Danger, Risky = countFirst == 0 });
        return aoes;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var countFirst = _aoesFirst.Count;
        var countRest = _aoesRest.Count;
        if ((AID)spell.Action.ID == AID.DeathlyRayFacesFirst && countFirst == 0 && countRest == 0)
        {
            foreach (var c in Module.Enemies(OID.TheFaceOfTheBeast).Where(x => x.Rotation.AlmostEqual(caster.Rotation, Angle.DegToRad)))
                _aoesFirst.Add(new(_rect, c.Position, c.Rotation, default, Colors.Danger));
            foreach (var c in Module.Enemies(OID.TheFaceOfTheBeast).Where(x => !x.Rotation.AlmostEqual(caster.Rotation, Angle.DegToRad)))
                _aoesRest.Add(new(_rect, c.Position, c.Rotation, WorldState.FutureTime(8.5f)));
        }
        if ((AID)spell.Action.ID is AID.DeathlyRayFacesFirst or AID.DeathlyRayFacesRest)
        {
            ++NumCasts;
            if (NumCasts == 5 * countFirst)
            {
                _aoesFirst.Clear();
                NumCasts = 0;
            }
            if (countFirst == 0 && NumCasts == 5 * countRest)
            {
                _aoesRest.Clear();
                NumCasts = 0;
            }
        }
    }
}

class D063TherionStates : StateMachineBuilder
{
    public D063TherionStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ThereionCharge>()
            .ActivateOnEnter<Misfortune>()
            .ActivateOnEnter<ShadowWreck>()
            .ActivateOnEnter<Apokalypsis>()
            .ActivateOnEnter<DeathlyRayFaces>()
            .ActivateOnEnter<DeathlyRayThereion>()
            .ActivateOnEnter<Border>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 652, NameID = 8210)]
public class D063Therion(WorldState ws, Actor primary) : BossModule(ws, primary, Border.DefaultArena.Center, Border.DefaultArena);
