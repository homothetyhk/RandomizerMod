using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $BENCHRESET
     * Required Parameters: none
     * Optiional Parameters: none
    */
    public class BenchResetVariable : StateModifyingVariable
    {
        public override string Name { get; }
        public const string Prefix = "$BENCHRESET";
        public bool fromBenchwarp;
        public bool canDreamgate;
        public StateInt spentSoul;
        public StateInt spentReserveSoul;
        public Term salubrasBlessing;
        public Term dreamnail;
        public Term essence;
        public Term vesselFragments;
        public StateBool usedShade;
        public State resetState;

        protected BenchResetVariable(string name)
        {
            Name = name;
        }

        public BenchResetVariable(string name, bool fromBenchwarp, bool canDreamgate, LogicManager lm)
        {
            Name = name;
            this.fromBenchwarp = fromBenchwarp;
            this.canDreamgate = canDreamgate;
            try
            {
                spentSoul = lm.StateManager.GetIntStrict("SPENTSOUL");
                spentReserveSoul = lm.StateManager.GetIntStrict("SPENTRESERVESOUL");
                salubrasBlessing = lm.GetTermStrict("Salubra's_Blessing");
                dreamnail = lm.GetTermStrict("DREAMNAIL");
                essence = lm.GetTermStrict("ESSENCE");
                vesselFragments = lm.GetTermStrict("VESSELFRAGMENTS");
                usedShade = lm.StateManager.GetBoolStrict("USEDSHADE");
                resetState = GetResetState(lm.StateManager);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing BenchResetVariable", e);
            }
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out string[] parameters))
            {
                variable = new BenchResetVariable(term, parameters.Contains("WARP"), !parameters.Contains("noDG"), lm);
                return true;
            }
            variable = default;
            return false;
        }

        public static State GetResetState(StateManager sm)
        {
            StateBuilder sb = new(sm.StartState);
            // TODO: bench resettable field tag?
            return new(sb);
        }

        public override IEnumerable<Term> GetTerms()
        {
            yield return salubrasBlessing;
            yield return dreamnail;
            yield return essence;
            yield return vesselFragments;
        }

        public override int GetValue(object sender, ProgressionManager pm, StateUnion? localState)
        {
            return TRUE;
        }

        public override bool ModifyState(object sender, ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (fromBenchwarp && !pm.Has(salubrasBlessing))
            {
                if (!canDreamgate || !pm.Has(dreamnail, 2) || !pm.Has(essence))
                {
                    state.SetInt(spentSoul, state.GetBool(usedShade) ? 66 : 99);
                    state.SetInt(spentReserveSoul, pm.Get(vesselFragments) / 3 * 33);
                }
            }

            if (!LazyStateBuilder.IsComparablyLE(state, resetState))
            {
                if (!pm.Has(salubrasBlessing))
                {
                    int soul = state.GetInt(spentSoul);
                    int rSoul = state.GetInt(spentReserveSoul);
                    state = new(resetState);
                    if (soul > 0) state.SetInt(spentSoul, soul);
                    if (rSoul > 0) state.SetInt(spentReserveSoul, rSoul);
                }
                else
                {
                    state = new(resetState);
                }
            }
            return true;
        }
    }
}
