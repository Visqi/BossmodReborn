﻿namespace BossMod.Dawntrail.Ultimate.FRU;

sealed class P5FulgentBlade : Components.Exaflare
{
    private readonly List<(Actor actor, WDir dir)> _lines = []; // before first line starts, it is sorted either in correct or reversed order - i don't think we can predict it?..
    private WDir _initialSafespot;
    private DateTime _nextBundle;

    public P5FulgentBlade(BossModule module) : base(module, new AOEShapeRect(5f, 40f))
    {
        ImminentColor = Colors.AOE;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        // add an extra hint to move to safe spot (TODO: reconsider? this can fuck up positionals for melee etc)
        if (Lines.Count != 0 && NumCasts <= 6 && SafeSpot() is var safespot && safespot != default)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(safespot, 1f), DateTime.MaxValue);
        //if (Lines.Count > 0 && NumCasts <= 6 && _lines.Count == 6)
        //{
        //    var shape = NumCasts switch
        //    {
        //        < 2 => ShapeDistance.InvertedCircle(SafeSpot(), 1),
        //        < 4 => LineIntersection(0, 1),
        //        < 6 => LineIntersection(2, 3),
        //        _ => LineIntersection(4, 5),
        //    };
        //    hints.AddForbiddenZone(shape, DateTime.MaxValue);
        //}
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var safespot = SafeSpot();
        if (safespot != default)
            Arena.AddCircle(safespot, 1f, Colors.Safe);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.FulgentBladeLine)
        {
            var dir = (actor.Position - Arena.Center).Normalized();
            _lines.Add((actor, dir));
            _initialSafespot -= dir;
            if (_lines.Count == 6)
            {
                // sort in arbitrary order (say, CW), until we know better
                _lines.Sort((a, b) => a.dir.Cross(_initialSafespot).CompareTo(b.dir.Cross(_initialSafespot)));
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.PathOfLightFirst or (uint)AID.PathOfDarknessFirst)
        {
            if (Lines.Count == 0)
                UpdateOrder(caster.Position); // first line - we should have all 6 line actors already created, and it should match position of first or last two

            var dir = spell.Rotation.ToDirection();
            var distanceToBorder = Intersect.RayCircle(caster.Position - Arena.Center, dir, 22f);
            Lines.Add(new(caster.Position, 5f * dir, Module.CastFinishAt(spell), 2d, (int)(distanceToBorder / 5) + 1, 1, spell.Rotation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.PathOfLightFirst or (uint)AID.PathOfDarknessFirst or (uint)AID.PathOfLightRest or (uint)AID.PathOfDarknessRest)
        {
            if (WorldState.CurrentTime > _nextBundle)
            {
                ++NumCasts;
                _nextBundle = WorldState.FutureTime(1d);
            }

            var count = Lines.Count;
            var pos = caster.Position;
            var rot = spell.Rotation;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f) && line.Rotation.AlmostEqual(rot, 0.1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
            ReportError($"Failed to find entry for {caster.InstanceID:X}");
        }
    }

    private void UpdateOrder(WPos firstPos)
    {
        var count = _lines.Count;
        if (count != 6)
        {
            ReportError($"Unexpected number of lines at cast start: {_lines.Count}");
        }
        else if (_lines[count - 1].actor.Position.AlmostEqual(firstPos, 1) || _lines[count - 2].actor.Position.AlmostEqual(firstPos, 1f))
        {
            _lines.Reverse(); // we guessed incorrectly, update the order
        }
        else if (!_lines[0].actor.Position.AlmostEqual(firstPos, 1) && !_lines[1].actor.Position.AlmostEqual(firstPos, 1f))
        {
            ReportError($"First cast at {firstPos} does not correspond to any of the first/last two lines");
        }
    }

    private WPos SafeSpot()
    {
        if (Lines.Count == 0 || _lines.Count != 6)
            return default; // not enough data

        return NumCasts switch
        {
            < 2 => SafeSpot(0, 1, 11), // prepare to dodge into third exaline of the first pair
            2 => SafeSpot(0, 1, 9), // dodge into third exaline of the first pair
            3 => SafeSpot(2, 3, 11), // prepare to dodge into third exaline of the second pair
            4 => SafeSpot(2, 3, 9), // dodge into third exaline of the second pair
            5 => SafeSpot(4, 5, 11), // prepare to dodge into third exaline of the third pair
            _ => SafeSpot(4, 5, 9), // dodge into third exaline of the third pair
        };
    }

    private WPos SafeSpot(int i1, int i2, float distance)
    {
        var l1 = _lines[i1];
        var l2 = _lines[i2];
        var p1 = l1.actor.Position - distance * l1.dir;
        var p2 = l2.actor.Position - distance * l2.dir;
        var d1 = l1.dir.OrthoL();
        var d2 = l2.dir.OrthoL();
        p1 -= 50f * d1; // rays are 0 to infinity, oh well
        p2 -= 50f * d2;
        var t = Intersect.RayLine(p1, d1, p2, d2);
        return t is > 0f and < 100f ? p1 + t * d1 : default;
    }

    //private Func<WPos, float> LineIntersection(int i1, int i2, float distance = 5, float cushion = 1)
    //{
    //    var l1 = _lines[i1];
    //    var l2 = _lines[i2];
    //    var r1 = ShapeDistance.Rect(l1.actor.Position - distance * l1.dir, -l1.dir, 5 - cushion, -cushion, 40);
    //    var r2 = ShapeDistance.Rect(l2.actor.Position - distance * l2.dir, -l2.dir, 5 - cushion, -cushion, 40);
    //    return p => -Math.Max(r1(p), r2(p));
    //}
}
