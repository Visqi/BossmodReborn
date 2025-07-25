﻿namespace BossMod.Heavensward.Extreme.Ex3Thordan;

abstract class SpiralThrust(BossModule module, float predictionDelay) : Components.GenericAOEs(module, (uint)AID.SpiralThrust)
{
    private float _predictionDelay = predictionDelay;
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(54.2f, 6f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            if (_predictionDelay > 0f)
            {
                _predictionDelay = default;
                _aoes.Clear();
            }
            _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.KnightAppear:
                if (caster.OID is (uint)OID.SerVellguine or (uint)OID.SerPaulecrain or (uint)OID.SerIgnasse && (caster.Position - Arena.Center).LengthSq() > 625f)
                {
                    // prediction
                    _aoes.Add(new(_shape, caster.Position.Quantized(), Angle.FromDirection(Arena.Center - caster.Position), WorldState.FutureTime(_predictionDelay), risky: false));
                }
                break;
            case (uint)AID.SpiralThrust:
                ++NumCasts;
                break;
        }
    }
}

sealed class SpiralThrust1(BossModule module) : SpiralThrust(module, 10f);
sealed class SpiralThrust2(BossModule module) : SpiralThrust(module, 12.1f);
