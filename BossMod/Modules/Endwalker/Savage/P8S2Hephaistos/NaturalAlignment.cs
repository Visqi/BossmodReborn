﻿using System.Linq;

namespace BossMod.Endwalker.Savage.P8S2
{
    class NaturalAlignment : Components.StackSpread
    {
        public enum Mechanic { None, StackSpread, FireIce }

        private BitMask _inverse;
        private Mechanic CurMechanic;
        private Actor? CurMechanicSource;
        private bool CurMechanicInverted;
        public int CurMechanicProgress { get; private set; }

        public NaturalAlignment() : base(0, 0) { }

        public override void Update(BossModule module)
        {
            StackMask.Reset();
            SpreadMask.Reset();
            if (CurMechanicProgress >= 2 || CurMechanicSource == null)
                return;

            bool firstPart = CurMechanicProgress == (CurMechanicInverted ? 1 : 0);
            switch (CurMechanic)
            {
                case Mechanic.StackSpread:
                    if (firstPart)
                    {
                        StackRadius = 6;
                        MinStackSize = MaxStackSize = 6;
                        // no idea how stack target is actually selected, assume it is closest...
                        var stackTarget = module.Raid.WithSlot().ExcludedFromMask(AvoidMask).Closest(CurMechanicSource.Position);
                        if (stackTarget.Item2 != null)
                            StackMask.Set(stackTarget.Item1);
                    }
                    else
                    {
                        SpreadRadius = 6;
                        SpreadMask = module.Raid.WithSlot().ExcludedFromMask(AvoidMask).Mask();
                    }
                    break;
                case Mechanic.FireIce:
                    if (firstPart)
                    {
                        StackRadius = 6;
                        MinStackSize = MaxStackSize = 2;
                        StackMask = module.Raid.WithSlot().ExcludedFromMask(AvoidMask).SortedByRange(CurMechanicSource.Position).TakeLast(3).Mask();
                    }
                    else
                    {
                        StackRadius = 5;
                        MinStackSize = MaxStackSize = 3;
                        StackMask = module.Raid.WithSlot().ExcludedFromMask(AvoidMask).SortedByRange(CurMechanicSource.Position).Take(2).Mask();
                    }
                    break;
            }
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (CurMechanicProgress >= 2 || CurMechanicSource == null)
                return;
            bool firstPart = CurMechanicProgress == (CurMechanicInverted ? 1 : 0);
            var hint = CurMechanic switch
            {
                Mechanic.StackSpread => firstPart ? "Stack" : "Spread",
                Mechanic.FireIce => firstPart ? "Fire" : "Ice",
                _ => ""
            };
            if (hint.Length > 0)
                hints.Add($"Next NA: {hint}");
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.InverseMagicks:
                    _inverse.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.NaturalAlignmentMechanic:
                    switch (status.Extra)
                    {
                        case 0x209: // initial application
                            AvoidMask.Set(module.Raid.FindSlot(actor.InstanceID));
                            break;
                        case 0x1E0: // stack->spread filling progress bars
                        case 0x1E1: // stack->spread empty progress bars
                            CurMechanic = Mechanic.StackSpread;
                            CurMechanicSource = actor;
                            CurMechanicInverted = _inverse[module.Raid.FindSlot(actor.InstanceID)];
                            CurMechanicProgress = 0;
                            break;
                        case 0x1E2: // spread->stack filling progress bars
                        case 0x1E3: // spread->stack empty progress bars
                            CurMechanic = Mechanic.StackSpread;
                            CurMechanicSource = actor;
                            CurMechanicInverted = !_inverse[module.Raid.FindSlot(actor.InstanceID)];
                            CurMechanicProgress = 0;
                            break;
                        case 0x1DC: // fire->ice filling progress bars
                        case 0x1DD: // fire->ice empty progress bars
                            CurMechanic = Mechanic.FireIce;
                            CurMechanicSource = actor;
                            CurMechanicInverted = _inverse[module.Raid.FindSlot(actor.InstanceID)];
                            CurMechanicProgress = 0;
                            break;
                        case 0x1DE: // ice->fire filling progress bars
                        case 0x1DF: // ice->fire empty progress bars
                            CurMechanic = Mechanic.FireIce;
                            CurMechanicSource = actor;
                            CurMechanicInverted = !_inverse[module.Raid.FindSlot(actor.InstanceID)];
                            CurMechanicProgress = 0;
                            break;
                    }
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.ForcibleTrifire:
                case AID.ForcibleFireStack:
                    if (CurMechanicProgress == (CurMechanicInverted ? 1 : 0))
                        ++CurMechanicProgress;
                    break;
                case AID.ForcibleDifreeze:
                case AID.ForcibleFireSpread:
                    if (CurMechanicProgress == (CurMechanicInverted ? 0 : 1))
                        ++CurMechanicProgress;
                    break;
            }
        }
    }
}