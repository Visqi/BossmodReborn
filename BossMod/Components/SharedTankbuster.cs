﻿namespace BossMod.Components;

// generic 'shared tankbuster' component; assumes only 1 concurrent cast is active
// TODO: revise and improve (track invuln, ai hints, num stacked tanks?)
public class GenericSharedTankbuster(BossModule module, uint aid, AOEShape shape, bool originAtTarget = false) : CastCounter(module, aid)
{
    public readonly AOEShape Shape = shape;
    public readonly bool OriginAtTarget = originAtTarget;
    protected Actor? Source;
    protected Actor? Target;
    protected DateTime Activation;

    public bool Active => Source != null;

    // circle shapes typically have origin at target
    public GenericSharedTankbuster(BossModule module, uint aid, float radius) : this(module, aid, new AOEShapeCircle(radius), true) { }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Target == null)
            return;
        if (Target == actor)
        {
            var otherTanksInAOE = false;
            var party = Raid.WithoutSlot();
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var a = ref party[i];
                if (a != actor && a.Role == Role.Tank && InAOE(a))
                {
                    otherTanksInAOE = true;
                    break;
                }
            }
            hints.Add("Stack with other tanks or press invuln!", !otherTanksInAOE);
        }
        else if (actor.Role == Role.Tank)
        {
            hints.Add("Stack with tank!", !InAOE(actor));
        }
        else
        {
            hints.Add("GTFO from tank!", InAOE(actor));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Source != null && Target != null && Target != actor)
        {
            var shape = OriginAtTarget ? Shape.Distance(Target.Position, Target.Rotation) : Shape.Distance(Source.Position, Angle.FromDirection(Target.Position - Source.Position));
            if (actor.Role == Role.Tank)
            {
                hints.AddForbiddenZone(p => -shape(p), Activation);
            }
            else
                hints.AddForbiddenZone(shape, Activation);
        }
        else if (Source != null && Target != null && Target == actor && Shape is AOEShapeCircle circle)
        {
            var shape = circle;
            foreach (var c in Raid.WithoutSlot().Where(x => x.Role != Role.Tank && x != Target))
                hints.AddForbiddenZone(ShapeDistance.Circle(c.Position, shape.Radius), Activation);
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => Target == player ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Source != null && Target != null && pc.Role == Role.Tank)
        {
            if (OriginAtTarget)
                Shape.Outline(Arena, Target, Target == pc ? 0 : Colors.Safe);
            else
                Shape.Outline(Arena, Source.Position, Angle.FromDirection(Target.Position - Source.Position), Target == pc ? default : Colors.Safe);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Source != null && Target != null && pc.Role != Role.Tank)
        {
            if (OriginAtTarget)
                Shape.Draw(Arena, Target);
            else
                Shape.Draw(Arena, Source.Position, Angle.FromDirection(Target.Position - Source.Position));
        }
    }

    private bool InAOE(Actor actor) => Source != null && Target != null && (OriginAtTarget ? Shape.Check(actor.Position, Target) : Shape.Check(actor.Position, Source.Position, Angle.FromDirection(Target.Position - Source.Position)));
}

// shared tankbuster at cast target
public class CastSharedTankbuster(BossModule module, uint aid, AOEShape shape, bool originAtTarget = false) : GenericSharedTankbuster(module, aid, shape, originAtTarget)
{
    public CastSharedTankbuster(BossModule module, uint aid, float radius) : this(module, aid, new AOEShapeCircle(radius), true) { }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Source = caster;
            Target = WorldState.Actors.Find(spell.TargetID);
            Activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (caster == Source)
            Source = Target = null;
    }
}

// shared tankbuster at icon
public class IconSharedTankbuster(BossModule module, uint iconId, uint aid, AOEShape shape, float activationDelay = 5.1f, bool originAtTarget = false) : GenericSharedTankbuster(module, aid, shape, originAtTarget)
{
    public IconSharedTankbuster(BossModule module, uint iconId, uint aid, float radius, float activationDelay = 5.1f) : this(module, iconId, aid, new AOEShapeCircle(radius), activationDelay, true) { }

    public virtual Actor? BaitSource(Actor target) => Module.PrimaryActor;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == iconId)
        {
            Source = BaitSource(actor);
            Target = actor;
            Activation = WorldState.FutureTime(activationDelay);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == WatchedAction)
            Source = Target = null;
    }
}
