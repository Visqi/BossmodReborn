﻿namespace BossMod.Endwalker.Savage.P3SPhoinix;

class P3SStates : StateMachineBuilder
{
    public P3SStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        ScorchedExaltation(id, 8.1f);
        HeatOfCondemnation(id + 0x010000, 3.2f);
        FireplumeCinderwing(id + 0x020000, 5.1f);
        DarkenedFire(id + 0x030000, 8.2f);
        HeatOfCondemnation(id + 0x040000, 6.6f);
        ScorchedExaltation(id + 0x050000, 2.1f);
        DevouringBrandFireplumeBreezeCinderwing(id + 0x060000, 7.2f);
        HeatOfCondemnation(id + 0x070000, 3.2f);

        FlyAwayBirds(id + 0x100000, 2.1f);

        DeadRebirth(id + 0x200000, 9.2f);
        HeatOfCondemnation(id + 0x210000, 9.2f);
        FledglingFlight(id + 0x220000, 7.1f);
        GloryplumeMulti(id + 0x230000, 8);
        FountainOfFire(id + 0x240000, 12.1f);

        ScorchedExaltation(id + 0x300000, 2.1f);
        ScorchedExaltation(id + 0x310000, 2.1f);
        HeatOfCondemnation(id + 0x320000, 5.2f);
        FirestormsOfAsphodelos(id + 0x330000, 8.6f);
        ConesAshplume(id + 0x340000, 3.2f);
        ConesStorms(id + 0x350000, 2.1f);
        DarkblazeTwister(id + 0x360000, 2.2f);
        ScorchedExaltation(id + 0x370000, 2.1f);
        DeathToll(id + 0x380000, 7.2f);

        GloryplumeSingle(id + 0x400000, 7.3f);
        FlyAwayNoBirds(id + 0x410000, 3);
        DevouringBrandFireplumeBreezeCinderwing(id + 0x420000, 5.1f);
        ScorchedExaltation(id + 0x430000, 6.2f);
        ScorchedExaltation(id + 0x440000, 2.2f);
        Cast(id + 0x450000, (uint)AID.FinalExaltation, 2.1f, 10, "Enrage");
    }

    private void ScorchedExaltation(uint id, float delay)
    {
        Cast(id, (uint)AID.ScorchedExaltation, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DeadRebirth(uint id, float delay)
    {
        Cast(id, (uint)AID.DeadRebirth, delay, 10, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void FirestormsOfAsphodelos(uint id, float delay)
    {
        Cast(id, (uint)AID.FirestormsOfAsphodelos, delay, 5, "Raidwide")
            .ActivateOnExit<FlamesOfAsphodelos>()
            .ActivateOnExit<TwisterVoidzone>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void HeatOfCondemnation(uint id, float delay)
    {
        Cast(id, (uint)AID.HeatOfCondemnation, delay, 6)
            .ActivateOnEnter<HeatOfCondemnation>();
        ComponentCondition<HeatOfCondemnation>(id + 2, 1.1f, comp => comp.NumCasts > 0, "Tankbuster tethers")
            .DeactivateOnExit<HeatOfCondemnation>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    // note - activates component, which should be later deactivated manually
    // note - positioning state is set at the end, make sure to clear later - this is because this mechanic overlaps with other stuff
    private State Fireplume(uint id, float delay)
    {
        // mechanics:
        // 1. single-plume version: immediately after cast end, 1 helper teleports to position and starts casting 26303, which takes 6s
        // 2. multi-plume version: immediately after cast end, 9 helpers teleport to positions and start casting 26305
        //    first pair starts cast almost immediately, then pairs 2-4 and finally central start their cast with 1 sec between them; each cast lasts 2 sec
        // so center (last/only) plume hits around 6s after cast end
        // note that our helpers rely on 233C casts rather than states
        CastStartMulti(id, [(uint)AID.ExperimentalFireplumeSingle, (uint)AID.ExperimentalFireplumeMulti], delay)
            .SetHint(StateMachine.StateHint.PositioningStart);
        return CastEnd(id + 1, 5, "Fireplume")
            .ActivateOnEnter<Fireplume>();
    }

    // note - no positioning flags, since this is part of mechanics that manage it themselves
    // note - since it resolves in a complex way, make sure to add a resolve state!
    private void AshplumeCast(uint id, float delay)
    {
        CastMulti(id, [(uint)AID.ExperimentalAshplumeStack, (uint)AID.ExperimentalAshplumeSpread], delay, 5, "Stack/Spread")
            .ActivateOnEnter<Ashplume>();
    }

    private State AshplumeResolve(uint id, float delay)
    {
        return ComponentCondition<Ashplume>(id, delay, comp => comp.CurState == Ashplume.State.Done, "Stack/Spread resolve")
            .DeactivateOnExit<Ashplume>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void GloryplumeMulti(uint id, float delay)
    {
        // first part for this mechanic always seems to be "multi-plume", works just like fireplume
        // 9 helpers teleport to position, first pair almost immediately starts casting 26315s, 1 sec stagger between pairs, 7 sec for each cast
        // ~3 sec after cast ends, boss makes an instant cast that determines stack/spread (26316/26312), ~10 sec after that hits with real AOE (26317/26313)
        Cast(id, (uint)AID.ExperimentalGloryplumeMulti, delay, 5, "Circles")
            .ActivateOnEnter<Ashplume>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<Ashplume>(id + 0x10, 13.2f, comp => comp.CurState == Ashplume.State.Done, "Circles resolve")
            .ActivateOnEnter<Fireplume>()
            .DeactivateOnExit<Fireplume>()
            .DeactivateOnExit<Ashplume>()
            .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd);
    }

    private void GloryplumeSingle(uint id, float delay)
    {
        // first part for this mechanic always seems to be "single-plume", works just like fireplume
        // helper teleports to position, almost immediately starts casting 26311, 6 sec for cast
        // ~3 sec after cast ends, boss makes an instant cast that determines stack/spread (26316/26312), ~4 sec after that hits with real AOE (26317/26313)
        // note that our helpers rely on casts rather than states
        Cast(id, (uint)AID.ExperimentalGloryplumeSingle, delay, 5, "Circle")
            .ActivateOnEnter<Ashplume>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<Ashplume>(id + 0x10, 7.2f, comp => comp.CurState == Ashplume.State.Done, "Circle resolves")
            .ActivateOnEnter<Fireplume>()
            .DeactivateOnExit<Fireplume>()
            .DeactivateOnExit<Ashplume>()
            .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd);
    }

    private State Cinderwing(uint id, float delay)
    {
        return CastMulti(id, [(uint)AID.RightCinderwing, (uint)AID.LeftCinderwing], delay, 5, "Side cleave")
            .ActivateOnEnter<LeftCinderwing>()
            .ActivateOnEnter<RightCinderwing>()
            .DeactivateOnExit<LeftCinderwing>()
            .DeactivateOnExit<RightCinderwing>();
    }

    private void FireplumeCinderwing(uint id, float delay)
    {
        Fireplume(id, delay); // pos-start
        Cinderwing(id + 0x1000, 5.7f)
            .DeactivateOnExit<Fireplume>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void DevouringBrandFireplumeBreezeCinderwing(uint id, float delay)
    {
        Cast(id, (uint)AID.DevouringBrand, delay, 3, "Devouring Brand");
        Fireplume(id + 0x1000, 2.1f); // pos-start
        CastStart(id + 0x2000, (uint)AID.SearingBreeze, 7.2f)
            .ActivateOnEnter<SearingBreeze>()
            .ActivateOnEnter<DevouringBrand>() // start showing brand aoe after fireplume cast is done
            .DeactivateOnExit<Fireplume>();
        CastEnd(id + 0x2001, 3, "Baited AOEs");
        Cinderwing(id + 0x3000, 3.2f)
            .SetHint(StateMachine.StateHint.PositioningEnd)
            .DeactivateOnExit<SearingBreeze>()
            .OnEnter(Module.DeactivateComponent<DevouringBrand>); // TODO: stop showing brand when aoes finish...
    }

    private void DarkenedFire(uint id, float delay)
    {
        // 3s after cast ends, adds start casting 26299
        CastStart(id, (uint)AID.DarkenedFire, delay)
            .SetHint(StateMachine.StateHint.PositioningStart);
        CastEnd(id + 0x1000, 6, "Darkened Fire phase")
            .ActivateOnEnter<DarkenedFire>()
            .DeactivateOnExit<DarkenedFire>();
        CastStart(id + 0x2000, (uint)AID.BrightenedFire, 5.2f)
            .ActivateOnEnter<BrightenedFire>(); // icons appear just before cast start
        CastEnd(id + 0x2001, 5, "Numbers") // at the end boss starts shooting 1-8
            .ActivateOnEnter<DarkenedFireAdd>();
        ComponentCondition<BrightenedFire>(id + 0x3000, 8.4f, comp => comp.NumCasts == 8)
            .DeactivateOnExit<BrightenedFire>();
        Timeout(id + 0x4000, 6.6f, "Darkened Fire resolve") // this timer is max time to kill adds before enrage, timeout is ok here
            .DeactivateOnExit<DarkenedFireAdd>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private State TrailOfCondemnation(uint id, float delay)
    {
        // at this point boss teleports to one of the cardinals
        // parallel to this one of the helpers casts 26365 (actual aoe fire trails)
        CastMulti(id, [(uint)AID.TrailOfCondemnationCenter, (uint)AID.TrailOfCondemnationSides], delay, 6)
            .ActivateOnEnter<TrailOfCondemnationAOE>()
            .ActivateOnEnter<TrailOfCondemnation>();
        return ComponentCondition<TrailOfCondemnation>(id + 2, 1.5f, comp => comp.Done, "Sides/Center AOE")
            .DeactivateOnExit<TrailOfCondemnation>()
            .DeactivateOnExit<TrailOfCondemnationAOE>();
    }

    // note: expects downtime at enter, clears when birds spawn, reset when birds die
    private void SmallBirdsPhase(uint id, float delay)
    {
        ComponentCondition<SunBirdSmall>(id, delay, comp => comp.ActiveActors.Count != 0, "Small birds", 10000)
            .ActivateOnEnter<SunBirdSmall>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ComponentCondition<SunBirdSmall>(id + 0x010, 25, comp => comp.ActiveActors.Count == 0, "Small birds enrage", 10000)
            .ActivateOnEnter<SmallBirdDistance>()
            .DeactivateOnExit<SmallBirdDistance>()
            .DeactivateOnExit<SunBirdSmall>()
            .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.DowntimeStart); // raidwide (26326) happens ~3sec after last bird death
    }

    // note: expects downtime at enter, clears when birds spawn, reset when birds die
    private void LargeBirdsPhase(uint id, float delay)
    {
        ComponentCondition<SunBirdLarge>(id, delay, comp => comp.ActiveActors.Count != 0, "Large birds", 10000)
            .ActivateOnEnter<SunBirdLarge>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ComponentCondition<SunBirdLarge>(id + 0x1000, 18.2f, comp => comp.FinishedTethers >= 4 || comp.ActiveActors.Count == 0, "", 10000)
            .ActivateOnEnter<BirdTether>() // note that first tethers appear ~5s after this
            .DeactivateOnExit<BirdTether>();
        ComponentCondition<SunBirdLarge>(id + 0x2000, 36.8f, comp => comp.ActiveActors.Count == 0, "Large birds enrage", 10000) // enrage is ~55sec after spawn
            .ActivateOnEnter<LargeBirdDistance>()
            .DeactivateOnExit<LargeBirdDistance>()
            .DeactivateOnExit<SunBirdLarge>()
            .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.DowntimeStart); // raidwide (26326) happens ~3sec after last bird death
    }

    private void FlyAwayBirds(uint id, float delay)
    {
        Fireplume(id, delay); // pos-start
        Targetable(id + 0x10000, false, 4.6f, "Boss disappears");
        TrailOfCondemnation(id + 0x20000, 3.8f)
            .DeactivateOnExit<Fireplume>();
        SmallBirdsPhase(id + 0x30000, 7.3f);
        LargeBirdsPhase(id + 0x40000, 4.3f);
        Targetable(id + 0x50000, true, 5.2f, "Boss reappears")
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void FlyAwayNoBirds(uint id, float delay)
    {
        Targetable(id, false, delay, "Boss disappears");
        TrailOfCondemnation(id + 0x1000, 3.8f);
        Targetable(id + 0x2000, true, 3.1f, "Boss reappears");
    }

    private void FledglingFlight(uint id, float delay)
    {
        // mechanic timeline:
        // 0s cast end
        // 2s icons appear
        // 8s 3540's teleport to players
        // 10s 3540's start casting 26342
        // 14s 3540's finish casting 26342
        // note that helper relies on icons and cast events rather than states
        Cast(id, (uint)AID.FledglingFlight, delay, 3)
            .ActivateOnEnter<FledglingFlight>();
        ComponentCondition<FledglingFlight>(id + 0x10, 10.3f, comp => comp.PlacementDone, "Eyes place");
        ComponentCondition<FledglingFlight>(id + 0x20, 4, comp => comp.CastsDone, "Eyes resolve")
            .DeactivateOnExit<FledglingFlight>();
    }

    private void DeathToll(uint id, float delay)
    {
        // notes on mechanics:
        // - on 26349 cast end, debuffs with 25sec appear
        // - 12-15sec after 26350 cast starts, eyes finish casting their cones - at this point, there's about 5sec left on debuffs
        Cast(id, (uint)AID.DeathToll, delay, 6, "Death Toll");
        Cast(id + 0x1000, (uint)AID.FledglingFlight, 3.2f, 3, "Eyes")
            .ActivateOnEnter<FledglingFlight>();
        Cast(id + 0x2000, (uint)AID.LifesAgonies, 2.1f, 24, "Life Agonies")
            .DeactivateOnExit<FledglingFlight>();
    }

    private void FountainOfFire(uint id, float delay)
    {
        // TODO: healer component - not even sure, mechanic looks so simple...
        Cast(id, (uint)AID.FountainOfFire, delay, 6, "Fountain of Fire")
            .SetHint(StateMachine.StateHint.PositioningStart);
        Cast(id + 0x1000, (uint)AID.SunsPinion, 2.1f, 6, "First birds");
        ComponentCondition<SunshadowTether>(id + 0x2000, 16.1f, comp => comp.NumCharges == 6, "Charges")
            .ActivateOnEnter<SunshadowTether>()
            .DeactivateOnExit<SunshadowTether>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void ConesAshplume(uint id, float delay)
    {
        Cast(id, (uint)AID.FlamesOfAsphodelos, delay, 3, "Cones")
            .SetHint(StateMachine.StateHint.PositioningStart);
        AshplumeCast(id + 0x1000, 2.1f);
        AshplumeResolve(id + 0x2000, 6.1f)
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void ConesStorms(uint id, float delay)
    {
        Cast(id, (uint)AID.FlamesOfAsphodelos, delay, 3, "Cones")
            .SetHint(StateMachine.StateHint.PositioningStart);
        Cast(id + 0x1000, (uint)AID.StormsOfAsphodelos, 10.2f, 8, "Storms")
            .ActivateOnEnter<StormsOfAsphodelos>()
            .DeactivateOnExit<StormsOfAsphodelos>()
            .DeactivateOnExit<FlamesOfAsphodelos>()
            .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.Tankbuster | StateMachine.StateHint.PositioningEnd);
    }

    private void DarkblazeTwister(uint id, float delay)
    {
        Cast(id, (uint)AID.DarkblazeTwister, delay, 4, "Twister")
            .ActivateOnEnter<DarkTwister>()
            .ActivateOnEnter<BurningTwister>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        Cast(id + 0x1000, (uint)AID.SearingBreeze, 4.1f, 3, "Searing Breeze")
            .ActivateOnEnter<SearingBreeze>();
        AshplumeCast(id + 0x2000, 4.1f);
        ComponentCondition<DarkTwister>(id + 0x3000, 2.8f, comp => comp.Casters.Count == 0, "Knockback")
            .DeactivateOnEnter<SearingBreeze>()
            .DeactivateOnExit<DarkTwister>()
            .SetHint(StateMachine.StateHint.Knockback);
        ComponentCondition<BurningTwister>(id + 0x4000, 2, comp => comp.NumCasts == 2, "AOE")
            .DeactivateOnExit<BurningTwister>();
        AshplumeResolve(id + 0x5000, 2.3f)
            .DeactivateOnExit<TwisterVoidzone>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }
}
