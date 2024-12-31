using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Equip logic for Fragile Heart, Greed, and Strength. See documentation for EquipCharmVariable for variable pattern.
    */
    public class FragileCharmVariable : EquipCharmVariable
    {
        protected readonly Term RepairTerm;
        protected readonly StateBool BreakBool;

        public FragileCharmVariable(string name, string charmName, int charmID, LogicManager lm) : this(name, charmName, charmID, lm, "Can_Repair_Fragile_Charms", charmID switch
        {
            23 => "BROKEHEART",
            24 => "BROKEGREED",
            25 => "BROKESTRENGTH",
            _ => throw new ArgumentException($"Error constructing FCV from {name}: Unknown fragile charm id {charmID}.")
        }) { }

        public FragileCharmVariable(string name, string charmName, int charmID, LogicManager lm, string repairTermName, string breakBoolName) : base(name, charmName, charmID, lm) 
        {
            RepairTerm = lm.GetTermStrict(repairTermName) ?? throw new ArgumentException($"Error constructing ECV from {name}: {repairTermName} term does not exist?");
            BreakBool = lm.StateManager.GetBoolStrict(breakBoolName) ?? throw new ArgumentException($"Error constructing ECV from {name}: could not find {breakBoolName} state bool.");
        }

        public override IEnumerable<Term> GetTerms()
        {
            return base.GetTerms().Append(RepairTerm);
        }

        public override bool HasStateRequirements<T>(ProgressionManager pm, T state)
        {
            return base.HasStateRequirements<T>(pm, state) && (pm.Has(CharmTerm, 2) || !state.GetBool(BreakBool) && pm.Has(RepairTerm));
        }

        public void BreakCharm(ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (pm.Has(CharmTerm, 2)) return;
            if (state.GetBool(CharmBool))
            {
                state.SetBool(CharmBool, false);
                state.Increment(UsedNotchesInt, -((RandoModContext)pm.ctx).notchCosts[CharmID - 1]);
                if (state.GetBool(Overcharmed)) state.SetBool(Overcharmed, false);
            }
            state.SetBool(AnticharmBool, true);
            state.SetBool(BreakBool, true);
        }

    }
}
