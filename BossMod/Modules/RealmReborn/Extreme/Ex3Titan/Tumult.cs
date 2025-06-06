﻿namespace BossMod.RealmReborn.Extreme.Ex3Titan;

class Tumult(BossModule module) : Components.CastCounter(module, (uint)AID.TumultBoss)
{
    private DateTime _nextExpected = module.StateMachine.NextTransitionWithFlag(StateMachine.StateHint.Raidwide);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.AddPredictedDamage(Raid.WithSlot(false, true, true).Mask(), _nextExpected);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == WatchedAction)
            _nextExpected = WorldState.FutureTime(1.2f);
    }
}
