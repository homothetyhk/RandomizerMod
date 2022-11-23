using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * See documentation for EquipCharmVariable.
    */
    public class FragileCharmVariable : EquipCharmVariable
    {
        public Term repairTerm;
        public StateBool breakBool;

        protected FragileCharmVariable(string name) : base(name) { }

        public FragileCharmVariable(string name, string charmName, int charmID, LogicManager lm) : this(name, charmName, charmID, lm, "Can_Repair_Fragile_Charms", charmID switch
        {
            23 => "BROKEHEART",
            24 => "BROKEGREED",
            25 => "BROKESTRENGTH",
            _ => throw new ArgumentException($"Error constructing FCV from {name}: Unknown fragile charm id {charmID}.")
        }) { }

        public FragileCharmVariable(string name, string charmName, int charmID, LogicManager lm, string repairTermName, string breakBoolName) : base(name, charmName, charmID, lm) 
        {
            repairTerm = lm.GetTermStrict(repairTermName) ?? throw new ArgumentException($"Error constructing ECV from {name}: {repairTermName} term does not exist?");
            breakBool = lm.StateManager.GetBoolStrict(breakBoolName) ?? throw new ArgumentException($"Error constructing ECV from {name}: could not find {breakBoolName} state bool.");
        }

        public override IEnumerable<Term> GetTerms()
        {
            return base.GetTerms().Append(repairTerm);
        }

        protected override bool HasStateRequirements<T>(ProgressionManager pm, T state)
        {
            return base.HasStateRequirements<T>(pm, state) && (pm.Has(charmTerm, 2) || !state.GetBool(breakBool) && pm.Has(repairTerm));
        }

        public void BreakCharm(ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (pm.Has(charmTerm, 2)) return;
            if (state.GetBool(charmBool))
            {
                state.SetBool(charmBool, false);
                state.Increment(usedNotchesInt, -((RandoModContext)pm.ctx).notchCosts[charmID - 1]);
                if (state.GetBool(overcharmBool)) state.SetBool(overcharmBool, false);
            }
            state.SetBool(anticharmBool, true);
            state.SetBool(breakBool, true);
        }

    }
}
