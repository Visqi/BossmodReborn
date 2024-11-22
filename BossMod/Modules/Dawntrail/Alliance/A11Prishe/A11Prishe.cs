﻿namespace BossMod.Dawntrail.Alliance.A11Prishe;

class Banishga(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Banishga));
class NullifyingDropkick(BossModule module) : Components.CastSharedTankbuster(module, ActionID.MakeSpell(AID.NullifyingDropkick), 6);
class Holy(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Holy), 6);
class BanishgaIV(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BanishgaIV));
class Explosion(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Explosion), new AOEShapeCircle(8));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13351)]
public class A11Prishe(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultBounds)
{
    public static readonly WPos ArenaCenter = new(800, 400);
    public static readonly ArenaBoundsSquare DefaultBounds = new(35);
}