using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
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

        public BenchResetVariable(string name)
        {
            Name = name;
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out string[] parameters))
            {
                variable = new BenchResetVariable(term)
                {
                    fromBenchwarp = parameters.Contains("WARP"),
                    canDreamgate = !parameters.Contains("noDG"),
                    spentSoul = lm.StateManager.GetInt("SPENTSOUL"),
                    spentReserveSoul = lm.StateManager.GetInt("SPENTRESERVESOUL"),
                    salubrasBlessing = lm.GetTerm("Salubra's_Blessing"),
                    dreamnail = lm.GetTerm("DREAMNAIL"),
                    essence = lm.GetTerm("ESSENCE"),
                    vesselFragments = lm.GetTerm("VESSELFRAGMENTS"),
                    usedShade = lm.StateManager.GetBool("USEDSHADE"),
                };
                return true;
            }
            variable = default;
            return false;
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
            if (!state.IsZero)
            {
                if (!pm.Has(salubrasBlessing))
                {
                    int soul = state.GetInt(spentSoul);
                    int rSoul = state.GetInt(spentReserveSoul);
                    state = new(pm.lm.StateManager.Zero);
                    if (soul > 0) state.SetInt(spentSoul, soul);
                    if (rSoul > 0) state.SetInt(spentReserveSoul, rSoul);
                }
                else
                {
                    state = new(pm.lm.StateManager.Zero);
                }
            }
            return true;
        }
    }
}
