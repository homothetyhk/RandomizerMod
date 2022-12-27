using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $HOTSPRINGRESET
     * Required Parameters: none
     * Optiional Parameters: none
    */
    public class HotSpringResetVariable : StateResetter
    {
        public override string Name { get; }
        protected override State ResetState { get; }
        protected override string? ResetLogicProperty => "HotSpringResetCondition";
        protected override bool OptIn => true;
        public const string Prefix = "$HOTSPRINGRESET";

        public HotSpringResetVariable(string name, LogicManager lm) : base(lm)
        {
            Name = name;
            try
            {
                ResetState = lm.StateManager.GetNamedStateStrict("HotSpringResetState");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing HotSpringResetVariable", e);
            }
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (term == Prefix)
            {
                variable = new HotSpringResetVariable(term, lm);
                return true;
            }
            variable = default;
            return false;
        }
    }
}
