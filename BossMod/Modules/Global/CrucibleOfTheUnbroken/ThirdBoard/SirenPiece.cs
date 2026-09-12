namespace BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.SirenPiece;

public enum OID : uint
{
    SirenPiece = 0x4CA1,
    Helper = 0x233C,
    ShamblingPiece = 0x4CA3, // R0.750, x0 (spawn during fight)
    CrawlingPiece = 0x4CA4, // R0.750, x0 (spawn during fight)
    SweetSong = 0x4CA2, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 50395, // SirenPiece->players, no cast, range 9 ?-degree cone
    SongOfTorment = 48563, // SirenPiece->player, 5.0s cast, single-target
    Teleport = 48564, // SirenPiece->location, no cast, single-target
    _Weaponskill_UnmooringMelody = 48565, // SirenPiece->self, no cast, single-target
    UnmooringMelody = 48566, // Helper->self, no cast, range 50 45-degree cone
    _Weaponskill_FeralLunge = 48569, // SirenPiece->self, 3.8+0.2s cast, single-target
    FeralLunge = 48570, // Helper->self, 4.0s cast, range 50 width 16 rect
    Summon = 48567, // SirenPiece->self, 3.0s cast, single-target
    _Weaponskill_DeadMansDirge = 48574, // SirenPiece->self, 6.2+0.8s cast, single-target
    DeadMansDirge = 48575, // Helper->self, 7.0s cast, range 3-43 donut
    _Weaponskill_DistantTune = 48576, // SirenPiece->self, 3.0s cast, single-target
    Burst = 48577, // 4CA2->self, 1.0s cast, range 9 circle
    InvitingVerse = 48571, // SirenPiece->self, 5.0s cast, range 40 circle
}

public enum SID : uint
{
    WitsEnd = 5424, // Helper->player, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7/0x8/0x9/0xA/0xB
    ForwardMarch = 2161, // SirenPiece->player, extra=0x0
    AboutFace = 2162, // SirenPiece->player, extra=0x0
    LeftFace = 2163, // SirenPiece->player, extra=0x0
    RightFace = 2164, // SirenPiece->player, extra=0x0
    Confused = 1283, // Helper->player, extra=0x0

}

public enum IconID : uint
{
    Tankbuster = 218, // player->self
}

public enum TetherID : uint
{
    Tether = 17, // 4CA4->player
}

sealed class AutoAttack(BossModule module) : Components.Cleave(module, (uint)AID.AutoAttack, new AOEShapeCone(9f, 22.5f.Degrees()));
sealed class SongOfTorment(BossModule module) : Components.SingleTargetCast(module, (uint)AID.SongOfTorment);
sealed class FeralLunge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FeralLunge, new AOEShapeRect(50f, 8f));
sealed class ShamblingPiece(BossModule module) : Components.Adds(module, (uint)OID.ShamblingPiece, 1);
sealed class CrawlingPiece(BossModule module) : Components.Adds(module, (uint)OID.CrawlingPiece, 1);
sealed class DeadMansDirge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeadMansDirge, new AOEShapeDonut(3f, 43f));
sealed class InvitingVerse(BossModule module) : Components.StatusDrivenForcedMarch(module, 5f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace);
sealed class UnmooringMelody(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCone _cone = new(50f, 22.5f.Degrees());
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Teleport)
        {
            var position = spell.TargetXZ;
            var rotation = (Arena.Center - position).ToAngle();
            _aoes.Add(new(_cone, position, rotation));
        }
        else if (spell.Action.ID == (uint)AID.UnmooringMelody)
        {
            ++NumCasts;
            if (NumCasts != 0 && NumCasts % 12 == 0)
            {
                _aoes.Clear();
            }
        }
    }
}
sealed class Burst(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeCircle _circle = new(9f);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
        {
            return [];
        }

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var count = aoes.Length;
        var max = count > 6 ? 6 : count;
        return aoes[..max];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.SweetSong)
        {
            _aoes.Add(new(_circle, actor.Position));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.Burst)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class SirenPieceStates : StateMachineBuilder
{
    public SirenPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AutoAttack>()
            .ActivateOnEnter<SongOfTorment>()
            .ActivateOnEnter<UnmooringMelody>()
            .ActivateOnEnter<FeralLunge>()
            .ActivateOnEnter<ShamblingPiece>()
            .ActivateOnEnter<CrawlingPiece>()
            .ActivateOnEnter<DeadMansDirge>()
            .ActivateOnEnter<InvitingVerse>()
            .ActivateOnEnter<Burst>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, PrimaryActorOID = (uint)OID.SirenPiece, Contributors = "gynorhino", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1090u, NameID = 14583u, SortOrder = 6)]
public sealed class SirenPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f));
