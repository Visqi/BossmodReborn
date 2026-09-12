namespace BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.LakhamuPiece;

public enum OID : uint
{
    LakhamuPiece = 0x4C9E,
    Helper = 0x233C,
    GolemPiece = 0x4C9F,
    SandSphere = 0x4CA0, // R1.800, x0 (spawn during fight)
}

public enum AID : uint
{
    _Spell_Stone = 50792, // LakhamuPiece->player, no cast, single-target
    _Ability_Landslip = 48553, // LakhamuPiece->self, 7.5+1.0s cast, single-target
    Landslip = 48556, // Helper->self, 8.0s cast, range 45 width 10 rect
    _Weaponskill_Rockslide = 48554, // GolemPiece->self, 6.0s cast, single-target
    Rockslide = 48555, // Helper->self, 7.0s cast, range 45 width 10 rect
    _AutoAttack_ = 50398, // GolemPiece->player, no cast, single-target
    SandTempest = 48561, // LakhamuPiece->self, 5.0s cast, range 60 circle
    Burst = 48562, // 4CA0->self, 3.0s cast, range 12 circle
    EarthShakerCast = 48557, // LakhamuPiece->self, 4.0+0.2s cast, single-target
    EarthShaker = 48558, // Helper->self, no cast, range 60 ?-degree cone
    _Spell_Earthrender = 48559, // LakhamuPiece->self, 4.0s cast, single-target
    Earthrender = 48560, // Helper->location, 3.0s cast, range 6 circle
}

public enum SID : uint
{
    _Gen_Blind = 5389, // LakhamuPiece->player, extra=0x0
    _Gen_EarthResistanceDown = 5025, // Helper->player, extra=0x1
    _Gen_VulnerabilityUp = 1789, // Helper->player, extra=0x1

}

public enum IconID : uint
{
    Earthshaker = 40, // player/4A15->self
}

sealed class Adds(BossModule module) : Components.Adds(module, (uint)OID.GolemPiece, 1);
sealed class Rockslide(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Rockslide, new AOEShapeRect(45f, 5f));
sealed class SandTempest(BossModule module) : Components.RaidwideCast(module, (uint)AID.SandTempest);
sealed class Earthrender(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Earthrender, 6f);
sealed class Burst(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeCircle _circle = new(12f);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.SandSphere)
        {
            var activation = WorldState.CurrentTime.AddSeconds(6d);
            _aoes.Add(new(_circle, actor.Position, default, activation, actorID: actor.InstanceID));
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Burst)
        {
            _aoes.Clear();
        }
    }
}
sealed class EarthShaker(BossModule module) : Components.BaitAwayCast(module, (uint)AID.EarthShakerCast, new AOEShapeCone(60f, 30f.Degrees()), false, true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            //CurrentBaits.Add(new(caster, target, Shape, Module.CastFinishAt(spell), arenaProjectionLayer: ArenaProjectionLayer, restrictToArenaProjectionLayer: RestrictToArenaProjectionLayer));
            var player = Raid.Player();
            if (player != null)
            {
                CurrentBaits.Add(new(caster, player, Shape, Module.CastFinishAt(spell), arenaProjectionLayer: ArenaProjectionLayer, restrictToArenaProjectionLayer: RestrictToArenaProjectionLayer));
            }

            var pet = WorldState.Actors.Find(WorldState.Client.ActivePet.InstanceID);
            if (pet != null)
            {
                CurrentBaits.Add(new(caster, pet, Shape, Module.CastFinishAt(spell), arenaProjectionLayer: ArenaProjectionLayer, restrictToArenaProjectionLayer: RestrictToArenaProjectionLayer));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.EarthShaker && EndsOnCastEvent)
        {
            CurrentBaits.Clear();
        }
    }
}
sealed class Landslip(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Landslip, 20f, shape: new AOEShapeRect(45f, 5f), kind: Kind.DirForward)
{
    private readonly Rockslide _rockslide = module.FindComponent<Rockslide>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var knockbacks = ActiveKnockbacks(slot, actor);
            ref readonly var kb1 = ref Casters.Ref(0);
            var activation = kb1.Activation;

            if (IsImmune(slot, activation))
            {
                return;
            }

            var count = knockbacks.Length;
            Knockback? knockback = null;

            for (var i = 0; i < count; ++i)
            {
                ref readonly var kb = ref knockbacks[i];

                if (kb.Shape!.Check(actor.Position, kb.Origin, kb.Direction))
                {
                    knockback = kb;
                }
                else
                {
                    hints.AddForbiddenZone(kb.Shape, kb.Origin, kb.Direction, kb.Activation, kb.ActorID, kb.ArenaProjectionLayer);
                }
            }

            if (knockback == null)
            {
                return;
            }

            var direction = knockback.Value.Direction.ToDirection() * 20f;
            var aoes = _rockslide.ActiveAOEs(slot, actor).ToArray();
            var aoecount = aoes.Length;
            ShapeDistance sd = aoecount == 0 ? new SDKnockbackInAABBSquareFixedDirection(Arena.Center, direction, 20f) : new SDKnockbackInAABBSquareFixedDirectionPlusMixedAOEs(Arena.Center, direction, 20f, aoes, aoecount);
            hints.AddForbiddenZone(sd, activation, knockback.Value.ActorID);
        }
    }
}

sealed class LakhamuPieceStates : StateMachineBuilder
{
    public LakhamuPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<Rockslide>()
            .ActivateOnEnter<Landslip>()
            .ActivateOnEnter<SandTempest>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<EarthShaker>()
            .ActivateOnEnter<Earthrender>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, PrimaryActorOID = (uint)OID.LakhamuPiece, Contributors = "gynorhino", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1090u, NameID = 14580u, SortOrder = 5)]
public sealed class LakhamuPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, 0f), new ArenaBoundsRect(20f, 20f));
