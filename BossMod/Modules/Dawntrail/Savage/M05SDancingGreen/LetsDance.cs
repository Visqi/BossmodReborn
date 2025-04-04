namespace BossMod.Dawntrail.Savage.M05SDancingGreen;

class LetsDance(BossModule module) : Components.GenericAOEs(module)
{
    protected readonly List<AOEInstance> _aoes = new(8);
    protected static readonly AOEShapeRect rect = new(25f, 25f);
    protected readonly GetDownBait _bait = module.FindComponent<GetDownBait>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _bait.CurrentBaits.Count == 0 && _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void AddGlobalHints(GlobalHints hints)
    {
        var count = _aoes.Count;
        if (count > 0)
        {
            var sb = new StringBuilder(4 * count - 1 + count * 5);
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; i++)
            {
                var roundedrot = MathF.Round(aoes[i].Rotation.Deg);
                var shapeHint = roundedrot switch
                {
                    -90 => "Left",
                    90 => "Right",
                    _ => ""
                };
                sb.Append(shapeHint);

                if (i < count - 1)
                    sb.Append(" -> ");
            }
            hints.Add(sb.ToString());
        }
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.Frogtourage && modelState is 5 or 7)
        {
            var count = _aoes.Count;
            var act = count != 0 ? _aoes[0].Activation.AddSeconds(count * 2.4d) : WorldState.FutureTime(26.1d);
            _aoes.Add(new(rect, WPos.ClampToGrid(Arena.Center), modelState == 5 ? Angle.AnglesCardinals[3] : Angle.AnglesCardinals[0], act));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LetsDance)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
                _aoes.RemoveAt(0);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_aoes.Count < 2)
            return;
        // make ai stay close to boss to ensure successfully dodging the combo
        var aoe = _aoes[0];
        hints.AddForbiddenZone(ShapeDistance.InvertedRect(Module.PrimaryActor.Position, aoe.Rotation, 2f, 2f, 40f), aoe.Activation);
    }
}

class LetsDanceRemix(BossModule module) : LetsDance(module)
{
    private static readonly Angle a180 = 180f.Degrees();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0 || _bait.CurrentBaits.Count != 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i == 0)
            {
                if (count > 1)
                    aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
            else
            {
                if (aoes[0].Rotation.AlmostEqual(aoe.Rotation + a180, Angle.DegToRad))
                    aoe.Risky = false;
            }
        }
        return aoes[..max];
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        var count = _aoes.Count;
        if (count > 0)
        {
            var sb = new StringBuilder(4 * count - 1 + count * 5);
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; i++)
            {
                var roundedrot = MathF.Round(aoes[i].Rotation.Deg);
                var shapeHint = roundedrot switch
                {
                    -90 => "Left",
                    90 => "Right",
                    180 => "Back",
                    0 => "Front",
                    _ => ""
                };
                sb.Append(shapeHint);

                if (i < count - 1)
                    sb.Append(" -> ");
            }
            hints.Add(sb.ToString());
        }
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.Frogtourage && modelState is 5 or 7 or 31 or 32)
        {
            var count = _aoes.Count;
            var act = count != 0 ? _aoes[0].Activation.AddSeconds(count * 1.5d) : WorldState.FutureTime(26d);
            _aoes.Add(new(rect, WPos.ClampToGrid(Arena.Center), modelState == 5 ? Angle.AnglesCardinals[3] : modelState == 31 ? Angle.AnglesCardinals[1]
            : modelState == 32 ? a180 : Angle.AnglesCardinals[0], act));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LetsDanceRemix)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
                _aoes.RemoveAt(0);
        }
    }
}
