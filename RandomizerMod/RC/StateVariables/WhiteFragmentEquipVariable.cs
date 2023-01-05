using RandomizerCore.Logic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Equip logic for Kingsoul/Void Heart. See documentation for EquipCharmVariable for variable pattern.
    */
    public class WhiteFragmentEquipVariable : EquipCharmVariable
    {
        public WhiteFragmentEquipVariable(string name, LogicManager lm) : base(name, "WHITEFRAGMENTS", 36, lm)
        {
        }

        public override bool HasCharmProgression(ProgressionManager pm)
        {
            return pm.Has(CharmTerm, 3);
        }

        public override int GetNotchCost<T>(ProgressionManager pm, T state)
        {
            return pm.Get(CharmTerm) switch
            {
                <= 3 => base.GetNotchCost(pm, state),
                > 3 => 0,
            };
        }
    }
}
