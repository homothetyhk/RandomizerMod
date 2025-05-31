using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $BENCHRESET
     * Required Parameters: none
     * Optional Parameters: none
     * Provides the effect of resting at a bench. Does not consider warping, etc.
    */
    public class BenchResetVariable : StateModifier
    {
        public override string Name { get; }
        protected readonly List<StateBool> CharmBools;
        protected readonly List<StateBool> AnticharmBools;
        protected readonly StateBool NoPassedCharmEquip;
        protected readonly StateBool UsedShade;
        protected readonly StateInt RequiredMaxSoul; 
        // technically, MaxRequiredSoul should be reset at the start of the path leading to where a soul limit is applied. Since currently the only consumer is ShadeStateVariable, paths start at benches.
        // we don't reset SoulLimiter, since the only current consumer is ShadeStateVariable, which sets and resets it already (and there is no natural reason to think benches should reset this)
        protected readonly ISoulStateManager SSM;
        protected readonly IHPStateManager HPSM;
        protected readonly Term SalubrasBlessing;

        public const string Prefix = "$BENCHRESET";

        public BenchResetVariable(string name, LogicManager lm)
        {
            Name = name;
            try
            {
                CharmBools = lm.StateManager.Bools.Where(sb => sb.Name.StartsWith("CHARM")).ToList();
                AnticharmBools = lm.StateManager.Bools.Where(sb => sb.Name.StartsWith("noCHARM")).ToList();
                NoPassedCharmEquip = lm.StateManager.GetBoolStrict("NOPASSEDCHARMEQUIP");
                UsedShade = lm.StateManager.GetBoolStrict("USEDSHADE");
                RequiredMaxSoul = lm.StateManager.GetIntStrict("REQUIREDMAXSOUL");
                SSM = (ISoulStateManager)lm.GetVariableStrict(SoulStateManager.Prefix);
                HPSM = (IHPStateManager)lm.GetVariableStrict(HPStateManager.Prefix);
                SalubrasBlessing = lm.GetTermStrict("Salubra's_Blessing");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing BenchResetVariable", e);
            }
        }

        public override IEnumerable<Term> GetTerms()
        {
            // TODO: enum version of GetTerms
            foreach (Term t in SSM.GetTerms(ISoulStateManager.SSMOperation.RestoreSoul)) yield return t;
            foreach (Term t in HPSM.GetTerms(IHPStateManager.HPSMOperation.RestoreAllHealth)) yield return t;
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out string[] parameters))
            {
                variable = new BenchResetVariable(term, lm);
                return true;
            }
            variable = default;
            return false;
        }

        public override IEnumerable<LazyStateBuilder>? ProvideState(object? sender, ProgressionManager pm)
        {
            return [];
        }

        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            foreach (StateBool sb in CharmBools)
            {
                state.SetBool(sb, false);
            }
            foreach (StateBool sb in AnticharmBools)
            {
                state.SetBool(sb, false);
            }
            if (pm.Has(SalubrasBlessing)) SSM.TryRestoreAllSoul(pm, ref state, true);
            state.SetInt(RequiredMaxSoul, 0);
            state.SetBool(NoPassedCharmEquip, false);
            state.SetBool(UsedShade, false);
            return HPSM.RestoreAllHealth(pm, state);
        }
    }
}
