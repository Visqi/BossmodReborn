namespace BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.GuttlerTheGutter;

public enum OID : uint
{
    GuttlerTheGutter = 0x4CAA,
    Helper = 0x233C,
    CombustingBlade = 0x4CAB, // R1.000, x6
    _Gen_ = 0x4E5D, // R1.400, x0 (spawn during fight)
    Fetters = 0x1EC0C3, // R0.500, x0 (spawn during fight), EventObj type
    MoltenBlade = 0x4CAC, // R1.000, x0 (spawn during fight)
    ThanatosPiece = 0x4CAD, // R2.000, x0 (spawn during fight)
}
public enum AID : uint
{
    AutoAttack = 49714, // GuttlerTheGutter->players, no cast, range 9 ?-degree cone
    _Ability_ = 48590, // GuttlerTheGutter->location, no cast, single-target
    _Weaponskill_Gyrocleave = 48604, // GuttlerTheGutter->self, 6.0s cast, single-target
    Gyrocleave = 48605, // Helper->self, 7.0s cast, range 80 width 20 rect
    _Weaponskill_CombustingBlades = 48598, // GuttlerTheGutter->self, no cast, single-target
    _Weaponskill_CombustingBlades1 = 48593, // 4CAB->GuttlerTheGutter, no cast, single-target
    _Weaponskill_CombustingBlades2 = 48592, // 4CAB->GuttlerTheGutter, no cast, single-target
    _Weaponskill_CombustingBlades3 = 48591, // 4CAB->GuttlerTheGutter, no cast, single-target
    _Weaponskill_CombustingBlades4 = 48594, // Helper->self, 0.5s cast, range 2 circle
    _Weaponskill_CombustingBlades5 = 48595, // Helper->self, 0.7s cast, range 2 circle
    _Weaponskill_CombustingBlades6 = 48596, // Helper->self, 0.9s cast, range 2 circle
    MagicalCombustion = 48597, // 4CAB->self, 5.0s cast, range 8 circle
    _Weaponskill_GluttonousGutting = 48599, // GuttlerTheGutter->self, 6.0+0.6s cast, single-target
    GluttonousGutting = 48600, // Helper->self, 11.6s cast, range 50 width 40 rect
    _Weaponskill_BeastlyAura = 48606, // GuttlerTheGutter->self, 6.0s cast, single-target
    BeastlyAura = 48607, // Helper->self, 7.0s cast, range 80 width 80 rect
    Thunderbolt = 48620, // GuttlerTheGutter->self/player, 5.0s cast, range 50 width 6 rect
    _Weaponskill_OverpoweringPoint = 48612, // GuttlerTheGutter->self, 4.9+3.5s cast, single-target
    _Weaponskill_OverpoweringPoint1 = 48613, // GuttlerTheGutter->self, no cast, single-target
    OverpoweringPoint = 48614, // Helper->self, 3.5s cast, range 60 width 6 rect
    _Weaponskill_DeadlyDemesne = 48608, // GuttlerTheGutter->self, 3.0s cast, single-target
    Fetters = 48609, // Helper->self, no cast, range 10 width 10 rect
    LifeClaim = 48611, // Helper->self, 3.0s cast, range 15 width 10 cross
    _Weaponskill_MoltenMetal = 48615, // GuttlerTheGutter->self, 6.2+2.1s cast, single-target
    _Weaponskill_MoltenMetal1 = 48616, // GuttlerTheGutter->self, no cast, single-target
    _Weaponskill_MoltenMetal2 = 48617, // 4CAC->GuttlerTheGutter, 2.5s cast, single-target
    MoltenMetal = 48618, // Helper->self, 3.0s cast, range 6 circle
    BeastlyFlare = 48619, // 4CAC->self, 8.0s cast, range 80 circle
    _Weaponskill_CombustingBlades7 = 48601, // GuttlerTheGutter->self, no cast, single-target
    _Weaponskill_GluttonousGoring = 48602, // GuttlerTheGutter->self, 6.0+0.6s cast, single-target
    GluttonousGoring = 48603, // Helper->self, 11.6s cast, range 40 circle
    _AutoAttack_Attack = 870, // 4CAD->player, no cast, single-target
}

public enum SID : uint
{
    _Gen_VulnerabilityUp = 1789, // Helper->player, extra=0x1
    _Gen_ = 2552, // none->GuttlerTheGutter, extra=0x477/0x483/0x482/0x478
    _Gen_Paralysis = 5388, // GuttlerTheGutter->player, extra=0x0
    _Gen_DamageDown = 4874, // Helper->player, extra=0x1
    _Gen_Poison = 5140, // Helper->player, extra=0x0

}

public enum IconID : uint
{
    TankbusterBait = 471, // player->self
    Lockon = 669, // player->self
}

public enum TetherID : uint
{
    ChainDark = 1, // GuttlerTheGutter->player
}

sealed class AutoAttack(BossModule module) : Components.Cleave(module, (uint)AID.AutoAttack, new AOEShapeCone(9f, 30f.Degrees()));
sealed class Gyrocleave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Gyrocleave, new AOEShapeRect(80f, 10f));
sealed class MagicalCombustion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagicalCombustion, 8f);
sealed class GluttonousGutting(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GluttonousGutting, new AOEShapeRect(50f, 20f));
sealed class Thunderbolt(BossModule module) : Components.BaitAwayCast(module, (uint)AID.Thunderbolt, new AOEShapeRect(50f, 3f));
sealed class OverpoweringPoint(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OverpoweringPoint, new AOEShapeRect(60f, 3f));
sealed class OverpoweringPointBait(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(60f, 3f), (uint)TetherID.ChainDark);
sealed class LifeClaim(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LifeClaim, new AOEShapeCross(15f, 5f));
sealed class MoltenMetalBait(BossModule module) : Components.BaitAwayIcon(module, 6f, (uint)IconID.Lockon);
sealed class MoltenMetal(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MoltenMetal, 6f);
sealed class BeastlyFlare(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BeastlyFlare, 30f);
sealed class GluttonousGoring(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GluttonousGoring, 40f);
sealed class ThanatosPiece(BossModule module) : Components.Adds(module, (uint)OID.ThanatosPiece, 1);
sealed class BeastlyAura(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.BeastlyAura, 20f, kind: Kind.DirForward)
{
    //public RelSimplifiedComplexPolygon Polygon;
    //public bool PolygonInit;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (Casters.Count == 0)
        {
            return [];
        }

        var kbs = new Knockback[1];
        ref var c = ref Casters.Ref(0);
        var direction = actor.Position.Z < -420f ? 180f.Degrees() : 0f.Degrees();
        kbs[0] = new(c.Origin, Distance, c.Activation, c.Shape, direction, c.Kind, c.MinDistance, c.SafeWalls, c.ActorID, c.IgnoreImmunes, c.ArenaProjectionLayer, c.RestrictToArenaProjectionLayer);
        return kbs;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var knockbacks = ActiveKnockbacks(slot, actor);
        if (knockbacks.Length == 0)
        {
            return;
        }
        /*
        if (!PolygonInit)
        {
            Polygon = Arena.Bounds.Shape.Offset(-1f); // pretend polygon is 1y smaller than real for less suspect knockbacks
            PolygonInit = true;
        }

        var kb = knockbacks[0];
        var direction = kb.Direction.ToDirection() * kb.Distance;
        var sd = new SDKnockbackInComplexPolygonFixedDirection(Arena.Center, direction, Polygon);
        hints.AddForbiddenZone(sd, kb.Activation, kb.ActorID, kb.ArenaProjectionLayer);
        */
        var kb = knockbacks[0];
        var rect = new AOEShapeRect(2f, 2f, 2f, default, true);
        hints.AddForbiddenZone(rect, Arena.Center, default, kb.Activation, kb.ActorID, kb.ArenaProjectionLayer);
    }
}
sealed class Fetters(BossModule module) : Components.GenericAOEs(module)
{
    // 3s for LifeClaim fast enough? should it display crosses on spawn to immediately go to safe spot?
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeRect _rect = new(5f, 5f, 5f);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Fetters)
        {
            _aoes.Add(new(_rect, actor.Position));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LifeClaim)
        {
            _aoes.Clear();
        }
    }
}

sealed class GuttlerTheGutterStates : StateMachineBuilder
{
    public GuttlerTheGutterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AutoAttack>()
            .ActivateOnEnter<Gyrocleave>()
            .ActivateOnEnter<MagicalCombustion>()
            .ActivateOnEnter<GluttonousGutting>()
            .ActivateOnEnter<BeastlyAura>()
            .ActivateOnEnter<OverpoweringPoint>()
            .ActivateOnEnter<OverpoweringPointBait>()
            .ActivateOnEnter<Thunderbolt>()
            .ActivateOnEnter<Fetters>()
            .ActivateOnEnter<LifeClaim>()
            .ActivateOnEnter<MoltenMetalBait>()
            .ActivateOnEnter<MoltenMetal>()
            .ActivateOnEnter<BeastlyFlare>()
            .ActivateOnEnter<GluttonousGoring>()
            .ActivateOnEnter<ThanatosPiece>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, PrimaryActorOID = (uint)OID.GuttlerTheGutter, Contributors = "gynorhino", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1090u, NameID = 14592u, SortOrder = 7)]
public sealed class GuttlerTheGutter(WorldState ws, Actor primary) : BossModule(ws, primary, new(520f, -420f), CustomArena)
{
    private static readonly Shape[] _arenashapes = [
        new Rectangle(new(520f, -420f), 10f, 20f),
        new Rectangle(new(520f, -398f), 2.5f, 2f),
        new Rectangle(new(520f, -442f), 2.5f, 2f),
        new Rectangle(new(532.5f, -427.5f), 2.5f, 2.5f),
        new Rectangle(new(507.5f, -422.5f), 2.5f, 2.5f),
        new Rectangle(new(532.5f, -417.5f), 2.5f, 2.5f),
        new Rectangle(new(507.5f, -412.5f), 2.5f, 2.5f)
        ];

    private static readonly ArenaBoundsCustom CustomArena = new(_arenashapes);
}
