namespace BossMod.Modules.RealmReborn.Dungeon.D27TheKeeperoftheLake.D273Midgardsormr;

public enum OID : uint
{
    Boss = 0x392A, // R14.0
    MirageDragon1 = 0x392C, // R5.0
    MirageDragon2 = 0x392B, // R5.0
    MirageDragon3 = 0x392E, // R5.0
    MirageDragon4 = 0x392D, // R5.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // MirageDragon3/MirageDragon4->player, no cast, single-target
    MirageDragonVisual = 30226, // MirageDragon3/MirageDragon4->self, no cast, single-target

    Admonishment = 29889, // Boss->self, 4.0s cast, range 40 width 12 rect
    InnerTurmoil = 29888, // Boss->self, 4.0s cast, range 22 circle
    PhantomOuterTurmoil = 29279, // Boss->self, 7.0s cast, range 20.5-39 180-degree donut segment
    PhantomInnerTurmoil = 29278, // Boss->self, 7.0s cast, range 22 circle

    PhantomKin = 29277, // Boss->self, 4.0s cast, single-target
    PhantomAdmonishment = 29280, // Boss->self, 7.0s cast, range 40 width 12 rect
    MirageAdmonishment = 29290, // MirageDragon1/MirageDragon2->self, 2.0s cast, range 40 width 12 rect

    DisgustVisual = 29891, // Boss->self, 4.0s cast, single-target
    Disgust = 29892, // Helper->self, no cast, range 20 circle, raidwide

    AntipathyVisual = 29285, // Boss->self, 4.0s cast, single-target
    Antipathy1 = 29286, // Helper->self, 4.0s cast, range 6 circle
    Antipathy2 = 29287, // Helper->self, 4.0s cast, range 6-12 donut
    Antipathy3 = 29288, // Helper->self, 4.0s cast, range 12-20 donut
    Animadversion = 29281, // Boss->self, 7.0s cast, range 50 circle
    AnimadversionEnrage = 29282, // Boss->self, 7.0s cast, range 50 circle
    AkhMornFirst = 29283, // Boss->players, 5.0s cast, range 6 circle
    AkhMornRest = 29284, // Boss->players, no cast, range 6 circle
    Condescension = 29602, // Boss->player, 5.0s cast, single-target
}

class InnerTurmoil(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.InnerTurmoil), new AOEShapeCircle(22));
class PhantomInnerTurmoil(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PhantomInnerTurmoil), new AOEShapeCircle(22));
class PhantomOuterTurmoil(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PhantomOuterTurmoil), new AOEShapeDonutSector(20.5f, 39, 90.Degrees()));
class Animadversion(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Animadversion));
class Condescension(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Condescension));
class Disgust(BossModule module) : Components.RaidwideCastDelay(module, ActionID.MakeSpell(AID.DisgustVisual), ActionID.MakeSpell(AID.Disgust), 0.5f);
class Admonishment(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Admonishment), new AOEShapeRect(40, 6));
class PhantomAdmonishment(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PhantomAdmonishment), new AOEShapeRect(40, 6));

class MirageAdmonishment(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(40, 6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D3 && (OID)actor.OID is OID.MirageDragon1 or OID.MirageDragon2)
            _aoes.Add(new(rect, actor.Position, actor.Rotation, WorldState.FutureTime(12)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.MirageAdmonishment)
            _aoes.Clear();
    }
}

class Antipathy(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(6), new AOEShapeDonut(6, 12), new AOEShapeDonut(12, 20)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Antipathy1)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.Antipathy1 => 0,
            AID.Antipathy2 => 1,
            AID.Antipathy3 => 2,
            _ => -1
        };
        AdvanceSequence(order, caster.Position, WorldState.FutureTime(2));
    }
}

class AhkMorn(BossModule module) : Components.UniformStackSpread(module, 6, 0, 4, 4)
{
    private int numCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AkhMornFirst)
            AddStack(WorldState.Actors.Find(spell.TargetID)!);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AkhMornFirst or AID.AkhMornRest)
        {
            ++numCasts;
            if (numCasts == 4)
            {
                Stacks.Clear();
                numCasts = 0;
            }
        }
    }
}

class D273MidgardsormrStates : StateMachineBuilder
{
    public D273MidgardsormrStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<InnerTurmoil>()
            .ActivateOnEnter<PhantomInnerTurmoil>()
            .ActivateOnEnter<PhantomOuterTurmoil>()
            .ActivateOnEnter<Admonishment>()
            .ActivateOnEnter<Animadversion>()
            .ActivateOnEnter<Condescension>()
            .ActivateOnEnter<Disgust>()
            .ActivateOnEnter<MirageAdmonishment>()
            .ActivateOnEnter<PhantomAdmonishment>()
            .ActivateOnEnter<Antipathy>()
            .ActivateOnEnter<AhkMorn>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 32, NameID = 3374, Category = BossModuleInfo.Category.Dungeon, Expansion = BossModuleInfo.Expansion.RealmReborn)]
public class D273Midgardsormr(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBounds arena = new ArenaBoundsComplex([new Circle(new(-40.8f, -78.2f), 18.8f)], [new Rectangle(new(-40.787f, -59.416f), 20, 1.25f)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.MirageDragon3));
        Arena.Actors(Enemies(OID.MirageDragon4));
    }
}