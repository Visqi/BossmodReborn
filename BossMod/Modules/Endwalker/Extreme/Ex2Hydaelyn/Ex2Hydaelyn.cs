﻿using BossMod.Endwalker.Trial.T02Hydaelyn;

namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

class HerosSundering(BossModule module) : Components.BaitAwayCast(module, (uint)AID.HerosSundering, new AOEShapeCone(40, 45.Degrees()));

// state related to mousa scorn mechanic (shared tankbuster)
class MousaScorn(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.MousaScorn, 4);

// cast counter for pre-intermission AOE
class PureCrystal(BossModule module) : Components.CastCounter(module, (uint)AID.PureCrystal);

// cast counter for post-intermission AOE
class Exodus(BossModule module) : Components.CastCounter(module, (uint)AID.Exodus);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 791, NameID = 10453, PlanLevel = 90)]
public class Ex2Hydaelyn(WorldState ws, Actor primary) : BossModule(ws, primary, T02Hydaelyn.ArenaCenter, T02Hydaelyn.ArenaBounds);
