﻿namespace BossMod.RealmReborn.Extreme.Ex3Titan;

class Geocrush(BossModule module, float radius) : Components.CastCounter(module, (uint)AID.Geocrush)
{
    private readonly float _radius = radius;
    private const float _ringWidth = 2;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!actor.Position.InCircle(Arena.Center, _radius))
            hints.Add("Move closer to center!");
        else if (actor.Position.InCircle(Arena.Center, _radius - _ringWidth))
            hints.Add("Move closer to the edge!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var ring = ShapeDistance.Donut(Arena.Center, _radius - _ringWidth, _radius);
        hints.AddForbiddenZone(p => -ring(p));
        hints.AddPredictedDamage(Raid.WithSlot(false, true, true).Mask(), default);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        Arena.ZoneDonut(Arena.Center, _radius, 25, Colors.AOE);
        Arena.ZoneDonut(Arena.Center, _radius - _ringWidth, _radius, Colors.SafeFromAOE);
    }
}

class Geocrush1(BossModule module) : Geocrush(module, Radius)
{
    public const float Radius = 15;
}

class Geocrush2(BossModule module) : Geocrush(module, Radius)
{
    public const float Radius = 12;
}
