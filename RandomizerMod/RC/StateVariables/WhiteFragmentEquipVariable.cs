using RandomizerCore.Logic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Equip logic for Kingsoul/Void Heart. See documentation for EquipCharmVariable for variable pattern.
    */
    public class WhiteFragmentEquipVariable : EquipCharmVariable
    {
        protected readonly int Threshold;

        public WhiteFragmentEquipVariable(string name, string charmName, LogicManager lm) : base(name, "WHITEFRAGMENT", 36, lm)
        {
            Threshold = charmName switch
            {
                "Void_Heart" => 3,
                "Kingsoul" or _ => 2,
            };
        }

        public override bool HasCharmProgression(ProgressionManager pm)
        {
            return pm.Has(CharmTerm, Threshold);
        }

        public override int GetNotchCost<T>(ProgressionManager pm, T state)
        {
            return pm.Get(CharmTerm) switch
            {
                <= 2 => base.GetNotchCost(pm, state),
                > 2 => 0,
            };
        }
    }
}
