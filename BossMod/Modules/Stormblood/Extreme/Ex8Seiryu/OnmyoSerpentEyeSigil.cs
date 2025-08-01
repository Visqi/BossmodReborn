﻿namespace BossMod.Stormblood.Extreme.Ex8Seiryu;

sealed class OnmyoSerpentEyeSigil(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeDonut donut = new(7f, 30f);
    private static readonly AOEShapeCircle circle = new(12f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        return CollectionsMarshal.AsSpan(_aoes)[..1];
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        void AddAOE(AOEShape shape, bool first = true) => _aoes.Add(new(shape, actor.Position.Quantized(), default, WorldState.FutureTime(first ? 5.6d : 8.7d)));
        if (modelState == 32u)
        {
            AddAOE(circle);
        }
        else if (modelState == 5u)
        {
            AddAOE(donut);
            AddAOE(circle, false);
        }
        else if (modelState == 6u)
        {
            AddAOE(circle);
            AddAOE(donut, false);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.OnmyoSigil or (uint)AID.OnmyoSigilFirst or (uint)AID.OnmyoSigilSecond or (uint)AID.SerpentEyeSigilFirst or (uint)AID.SerpentEyeSigilSecond)
        {
            ++NumCasts;
            _aoes.RemoveAt(0);
        }
    }
}
