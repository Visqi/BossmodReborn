﻿namespace BossMod.Endwalker.Ultimate.TOP;

sealed class P1ProgramLoop(BossModule module) : P1CommonAssignments(module)
{
    public int NumTowersDone;
    public int NumTethersDone;
    private readonly List<Actor> _towers = [];
    private BitMask _tethers;

    private const float _towerRadius = 3f;
    private const float _tetherRadius = 15f;

    protected override (GroupAssignmentUnique assignment, bool global) Assignments()
    {
        var config = Service.Config.Get<TOPConfig>();
        return (config.P1ProgramLoopAssignments, config.P1ProgramLoopGlobalPriority);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        var order = PlayerStates[slot].Order;
        if (order == 0)
            return;

        var nextTowers = _towers.Skip(NumTowersDone).Take(2);
        var soakingTower = nextTowers.InRadius(actor.Position, _towerRadius).Any();
        if (order == NextTowersOrder())
            hints.Add("Soak next tower", !soakingTower);
        else if (soakingTower)
            hints.Add("GTFO from tower!");

        if (order != NextTethersOrder())
        {
            if (_tethers[slot])
                hints.Add("Pass the tether!");
            if (Raid.WithSlot(false, true, true).IncludedInMask(_tethers).InRadiusExcluding(actor, _tetherRadius).Any())
                hints.Add("GTFO from tether targets!");
        }
        else if (_tethers.Any())
        {
            if (!_tethers[slot])
                hints.Add("Grab the tether!");
            else if (Raid.WithoutSlot(false, true, true).InRadiusExcluding(actor, _tetherRadius).Any())
                hints.Add("GTFO from raid!");
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => PlayerStates[playerSlot].Order == (PlayerStates[pcSlot].Order & 3) + 1 ? PlayerPriority.Interesting : base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var ps = PlayerStates[pcSlot];
        var soakTowers = ps.Order == NextTowersOrder();
        var towerToSoak = soakTowers ? SelectTowerForGroup(ps.Group) : null;
        foreach (var t in _towers.Skip(NumTowersDone).Take(2))
        {
            Arena.AddCircle(t.Position, _towerRadius, soakTowers && (towerToSoak == null || towerToSoak == t) ? Colors.Safe : default, 2f);
        }

        if (ps.Order == NextTowersOrder(1))
        {
            // show next tower to soak if possible
            var futureTowerToSoak = SelectTowerForGroup(ps.Group, 1);
            if (futureTowerToSoak != null)
                Arena.AddCircle(futureTowerToSoak.Position, _towerRadius, Colors.Safe);
        }

        var grabThisTether = ps.Order == NextTethersOrder();
        var grabNextTether = ps.Order == NextTethersOrder(1);
        foreach (var (s, t) in Raid.WithSlot(false, true, true).IncludedInMask(_tethers))
        {
            var ts = PlayerStates[s];
            var correctSoaker = ts.Order == NextTethersOrder();
            var tetherToGrab = ts.Group == ps.Group && (grabNextTether ? correctSoaker : grabThisTether && NumTethersDone > 0 && ts.Order == NextTethersOrder(-1));
            Arena.AddCircle(t.Position, _tetherRadius, t == pc ? Colors.Safe : default);
            Arena.AddLine(t.Position, Module.PrimaryActor.Position, correctSoaker ? Colors.Safe : default, tetherToGrab ? 2f : 1f);
        }

        if (grabThisTether && NumTethersDone == NumTowersDone)
        {
            // show hint for tether position
            var spot = GetTetherDropSpot(ps.Group);
            if (spot != null)
                Arena.AddCircle(spot.Value, 1f, Colors.Safe);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Tower2)
            _towers.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.StorageViolation1:
            case (uint)AID.StorageViolationObliteration:
                ++NumTowersDone;
                break;
            case (uint)AID.BlasterAOE:
                ++NumTethersDone;
                break;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Blaster)
            _tethers.Set(Raid.FindSlot(source.InstanceID));
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Blaster)
            _tethers.Clear(Raid.FindSlot(source.InstanceID));
    }

    private int NextTowersOrder(int skip = 0)
    {
        var index = NumTowersDone + skip * 2;
        return index < 8 ? (index >> 1) + 1 : 0;
    }

    private int NextTethersOrder(int skip = 0)
    {
        var index = NumTethersDone + skip * 2;
        return index < 8 ? (index >> 1) + (index < 4 ? 3 : -1) : 0;
    }

    // 0 = N, 1 = E, ... (CW)
    private int ClassifyTower(Actor tower)
    {
        var offset = tower.Position - Arena.Center;
        if (Math.Abs(offset.Z) > Math.Abs(offset.X))
            return offset.Z < 0 ? 0 : 2;
        else
            return offset.X > 0 ? 1 : 3;
    }

    private Actor? SelectTowerForGroup(int group, int skip = 0)
    {
        var firstIndex = NumTowersDone + skip * 2;
        if (group == 0 || _towers.Count < firstIndex + 2)
            return null;
        var t1 = _towers[firstIndex];
        var t2 = _towers[firstIndex + 1];
        if (ClassifyTower(t2) < ClassifyTower(t1))
            Utils.Swap(ref t1, ref t2);
        return group == 1 ? t1 : t2;
    }

    private WPos? GetTetherDropSpot(int group)
    {
        if (group == 0 || _towers.Count < NumTowersDone + 2)
            return null;

        var safeSpots = new BitMask(0xF);
        safeSpots.Clear(ClassifyTower(_towers[NumTowersDone]));
        safeSpots.Clear(ClassifyTower(_towers[NumTowersDone + 1]));
        var spot = group == 1 ? safeSpots.LowestSetBit() : safeSpots.HighestSetBit();
        return Arena.Center + 18 * (180.Degrees() - 90.Degrees() * spot).ToDirection();
    }
}
