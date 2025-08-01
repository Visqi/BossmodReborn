﻿namespace BossMod.Endwalker.Ultimate.DSW2;

sealed class P6HPCheck(BossModule module) : BossComponent(module)
{
    private readonly Actor? _nidhogg = module.Enemies((uint)OID.NidhoggP6).FirstOrDefault();
    private readonly Actor? _hraesvelgr = module.Enemies((uint)OID.HraesvelgrP6).FirstOrDefault();

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_nidhogg != null && _hraesvelgr != null)
        {
            var diff = (int)(_nidhogg.HPMP.CurHP - _hraesvelgr.HPMP.CurHP) * 100.0f / _nidhogg.HPMP.MaxHP;
            hints.Add($"Nidhogg HP: {(diff > 0 ? "+" : "")}{diff:f1}%");
        }
    }
}

sealed class P6AkhAfah(BossModule module) : Components.UniformStackSpread(module, 4f, default, 4)
{
    public bool Done { get; private set; }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AkhAfahN)
            AddStacks(Raid.WithoutSlot(true, true, true).Where(p => p.Role == Role.Healer));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AkhAfahHAOE or (uint)AID.AkhAfahNAOE)
        {
            Stacks.Clear();
            Done = true;
        }
    }
}
