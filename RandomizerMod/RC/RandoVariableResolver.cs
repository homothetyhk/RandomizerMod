using RandomizerCore.Logic;
using System.Text.RegularExpressions;

namespace RandomizerMod.RC
{
    public class RandoVariableResolver : VariableResolver
    {
        public override bool TryMatch(LogicManager lm, string term, out LogicInt variable)
        {
            if (base.TryMatch(lm, term, out variable)) return true;

            Match match = Regex.Match(term, @"^\$NotchCost\[(.+)\]$");
            if (match.Success)
            {
                variable = new NotchCostInt(match.Groups[1].Value.Split(',').Select(s => int.Parse(s)).ToArray());
                return true;
            }

            return false;
        }
    }
}
