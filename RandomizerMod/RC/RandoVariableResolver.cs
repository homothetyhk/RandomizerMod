using RandomizerCore.Logic;
using RandomizerMod.RC.LogicInts;
using RandomizerMod.RC.StateVariables;
using System.Text.RegularExpressions;

namespace RandomizerMod.RC
{
    public class RandoVariableResolver : VariableResolver
    {
        public override bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (base.TryMatch(lm, term, out variable)) return true;

            Match match = Regex.Match(term, @"^\$NotchCost\[(.+)\]$");
            if (match.Success)
            {
                variable = new NotchCostInt(match.Groups[1].Value.Split(',').Select(s => int.Parse(s)).ToArray());
                return true;
            }

            match = Regex.Match(term, @"^\$SafeNotchCost\[(.+)\]$");
            if (match.Success)
            {
                variable = new SafeNotchCostInt(match.Groups[1].Value.Split(',').Select(s => int.Parse(s)).ToArray());
                return true;
            }

            match = Regex.Match(term, @"^\$StartLocation\[(.+)\]$");
            if (match.Success)
            {
                variable = new StartLocationDelta(match.Groups[1].Value);
                return true;
            }

            if (BenchResetVariable.TryMatch(lm, term, out variable)) return true;
            if (CastSpellVariable.TryMatch(lm, term, out variable)) return true;
            if (EquipCharmVariable.TryMatch(lm, term, out variable)) return true;
            if (HotSpringResetVariable.TryMatch(lm, term, out variable)) return true;
            if (ShadeStateVariable.TryMatch(lm, term, out variable)) return true;
            if (TakeDamageVariable.TryMatch(lm, term, out variable)) return true;
            if (StagStateVariable.TryMatch(lm, term, out variable)) return true;
            if (FlowerProviderVariable.TryMatch(lm, term, out variable)) return true;
            if (RequireFlowerVariable.TryMatch(lm, term, out variable)) return true;
            if (EquipMultipleCharmsVariable.TryMatch(lm, term, out variable)) return true;

            return false;
        }
    }
}
