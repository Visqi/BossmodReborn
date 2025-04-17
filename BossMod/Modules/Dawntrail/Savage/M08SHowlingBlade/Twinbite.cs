namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

class Twinbite(BossModule module) : Components.GenericBaitAway(module, (uint)AID.Twinbite, tankbuster: true)
{
    private static readonly AOEShapeCircle circle = new(8f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TwinbiteVisual)
        {
            var party = Raid.WithoutSlot(true, true, true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i];
                if (p.Role == Role.Tank)
                    CurrentBaits.Add(new(new Actor(default, default, default, default!, default, default, default, default, default), p, circle, Module.CastFinishAt(spell, 0.1f)));
            }
        }
    }

    public override void Update()
    {
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var len = baits.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            for (var j = 0; j < 5; ++j)
            {
                var center = ArenaChanges.EndArenaPlatforms[j].Center;
                if (b.Target.Position.InCircle(center, 8f))
                {
                    b.Source.PosRot = center.ToVec4();
                    break;
                }
            }
        }
    }
}
