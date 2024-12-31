using RandomizerCore.Logic;
using RandomizerMod.RC.LogicInts;
using RandomizerMod.RC.StateVariables;

namespace RandomizerMod.RC
{
    public class RandoVariableResolver : VariableResolver
    {
        public override bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (base.TryMatch(lm, term, out variable)) return true;

            if (StartLocationDelta.TryMatch(lm, term, out variable)) return true;
            if (BenchResetVariable.TryMatch(lm, term, out variable)) return true;
            if (CastSpellVariable.TryMatch(lm, term, out variable)) return true;
            if (SlopeballVariable.TryMatch(lm, term, out variable)) return true;
            if (ShriekPogoVariable.TryMatch(lm, term, out variable)) return true;
            if (SoulStateManager.TryMatch(lm, term, out variable)) return true;
            if (SpendSoulVariable.TryMatch(lm, term, out variable)) return true;
            if (RegainSoulVariable.TryMatch(lm, term, out variable)) return true;
            if (EquipCharmVariable.TryMatch(lm, term, out variable)) return true;
            if (HotSpringResetVariable.TryMatch(lm, term, out variable)) return true;
            if (ShadeStateVariable.TryMatch(lm, term, out variable)) return true;
            if (TakeDamageVariable.TryMatch(lm, term, out variable)) return true;
            if (HPStateManager.TryMatch(lm, term, out variable)) return true;
            if (LifebloodCountVariable.TryMatch(lm, term, out variable)) return true;
            if (StagStateVariable.TryMatch(lm, term, out variable)) return true;
            if (FlowerProviderVariable.TryMatch(lm, term, out variable)) return true;
            if (SaveQuitResetVariable.TryMatch(lm, term, out variable)) return true;
            if (StartRespawnResetVariable.TryMatch(lm, term, out variable)) return true;
            if (WarpToStartResetVariable.TryMatch(lm, term, out variable)) return true;
            if (WarpToBenchResetVariable.TryMatch(lm, term, out variable)) return true;

#pragma warning disable CS0618 // Type or member is obsolete
            if (NotchCostInt.TryMatch(lm, term, out variable)) return true;
            if (SafeNotchCostInt.TryMatch(lm, term, out variable)) return true;
#pragma warning restore CS0618 // Type or member is obsolete

            return false;
        }
    }
}
