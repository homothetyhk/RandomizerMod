using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $LIFEBLOOD
     * Required Parameters:
         - If any parameters are provided, the first parameter must parse to int to give the required number of blue masks (including Joni masks). 
           If absent, defaults to 1.
     * Optional Parameters: none
     * Filters to states with determined hp which have at least a certain number of blue masks, including Joni masks.
    */
    public class LifebloodCountVariable : StateModifier
    {
        public override string Name { get; }
        public const string Prefix = "$LIFEBLOOD";

        protected readonly int RequiredBlueMasks;
        protected readonly IHPStateManager HPSM;
        protected readonly EquipCharmVariable JonisBlessing;

        public LifebloodCountVariable(string name, LogicManager lm, int requiredBlueMasks)
        {
            this.Name = name;
            RequiredBlueMasks = requiredBlueMasks;
            try
            {
                HPSM = (IHPStateManager)lm.GetVariableStrict(HPStateManager.Prefix);
                JonisBlessing = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Joni's_Blessing"));
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing LifebloodCountVariable", e);
            }
        }

        public override IEnumerable<Term> GetTerms()
        {
            return HPSM.GetTerms(IHPStateManager.HPSMOperation.GetHPInfo);
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out string[] parameters))
            {
                int amount = parameters.Length == 0 ? 1 : int.Parse(parameters[0]);
                variable = new LifebloodCountVariable(term, lm, amount);
                return true;
            }
            variable = default;
            return false;
        }

        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            return HPSM.DetermineHP(pm, state).Where(s => HPSM.GetHPInfo(pm, s) is IHPStateManager.StrictHPInfo hp
            && hp.CurrentBlueHP + (JonisBlessing.IsEquipped(s) ? hp.CurrentWhiteHP : 0) >= RequiredBlueMasks);
        }
    }
}
