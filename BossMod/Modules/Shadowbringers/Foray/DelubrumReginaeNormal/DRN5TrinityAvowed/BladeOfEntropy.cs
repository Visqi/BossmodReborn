﻿namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN5TrinityAvowed;

// note: instead of trying to figure out cone intersections and shit, we use the fact that clones are always positioned on grid and just check each cell
class BladeOfEntropy(BossModule module) : TemperatureAOE(module)
{
    private readonly List<(Actor caster, WDir dir, int temperature)> _casters = [];

    private static readonly AOEShapeRect _shapeCell = new(5, 5, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var playerTemp = Math.Clamp(Temperature(actor), -2, +2);
        for (var x = -2; x <= +2; ++x)
        {
            for (var z = -2; z <= +2; ++z)
            {
                var cellCenter = Module.Center + 10 * new WDir(x, z);
                var temperature = 0;
                var numClips = 0;
                DateTime activation = new();
                foreach (var c in _casters)
                {
                    activation = Module.CastFinishAt(c.caster.CastInfo);
                    if (c.dir.Dot(cellCenter - c.caster.Position) > 0)
                    {
                        temperature = c.temperature;
                        if (++numClips > 1)
                            break;
                    }
                }

                if (numClips > 1)
                    yield return new(_shapeCell, cellCenter, new(), activation);
                else if (activation != default && temperature == -playerTemp)
                    yield return new(_shapeCell, cellCenter, new(), activation, Colors.SafeFromAOE, false);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BladeOfEntropyAC11:
            case AID.BladeOfEntropyBC11:
                _casters.Add((caster, spell.Rotation.ToDirection(), -1));
                break;
            case AID.BladeOfEntropyAH11:
            case AID.BladeOfEntropyBH11:
                _casters.Add((caster, spell.Rotation.ToDirection(), +1));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BladeOfEntropyAC11 or AID.BladeOfEntropyBC11 or AID.BladeOfEntropyAH11 or AID.BladeOfEntropyBH11)
            _casters.RemoveAll(c => c.caster == caster);
    }
}
