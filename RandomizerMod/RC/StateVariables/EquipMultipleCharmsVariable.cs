using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    public class EquipMultipleCharmsVariable : StateModifyingVariable
    {
        public override string Name { get; }
        public EquipCharmVariable[] charms;
        public const string Prefix = "$EQUIPPEDCHARMS";

        public EquipMultipleCharmsVariable(string name)
        {
            Name = name;
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out string[] parameters) && parameters.Length > 0)
            {
                EquipCharmVariable[] charms = parameters.Select(p =>
                {
                    if (int.TryParse(p, out int id))
                    {
                        EquipCharmVariable.TryMatch(lm, EquipCharmVariable.GetName(id), out LogicVariable var1);
                        return var1 as EquipCharmVariable;
                    }
                    else
                    {
                        EquipCharmVariable.TryMatch(lm, EquipCharmVariable.GetName(id), out LogicVariable var1);
                        return var1 as EquipCharmVariable;
                    }
                }).ToArray();
                variable = new EquipMultipleCharmsVariable(term)
                {
                    charms = charms,
                };
                return true;
            }
            variable = default;
            return false;
        }

        public override IEnumerable<Term> GetTerms()
        {
            return charms.SelectMany(c => c.GetTerms());
        }

        public override int GetValue(object sender, ProgressionManager pm, StateUnion? localState)
        {
            for (int i = 0; i < charms.Length; i++)
            {
                if (charms[i].GetValue(sender, pm, localState) != TRUE) return FALSE;
            }
            int totalNotches = pm.Get(charms[0].notchesTerm);
            for (int i = 0; i < localState.Count; i++)
            {
                GetNotchCosts(pm, localState[i], out int nonovercharmCost, out int overcharmCost);
                if (totalNotches - localState[i].GetInt(charms[0].usedNotchesInt) <= nonovercharmCost)
                {
                    return TRUE;
                }
                else if (totalNotches - localState[i].GetInt(charms[0].usedNotchesInt) < overcharmCost && !localState[i].GetBool(charms[0].overcharmBool))
                {
                    return TRUE;
                }
            }
            return FALSE;
        }

        public override bool ModifyState(object sender, ProgressionManager pm, ref LazyStateBuilder state)
        {
            int argMax = -1;
            int maxCost = -1;
            for (int i = 0; i < charms.Length; i++)
            {
                int cost = charms[i].GetNotchCost(pm, state);
                if (cost > maxCost)
                {
                    argMax = i;
                    maxCost = cost;
                }
            }
            for (int i = 0; i < charms.Length; i++)
            {
                if (i == argMax) continue;
                if (!charms[i].ModifyState(sender, pm, ref state)) return false;
            }
            return charms[argMax].ModifyState(sender, pm, ref state);
        }

        public void GetNotchCosts<T>(ProgressionManager pm, T state, out int nonovercharmCost, out int overcharmCost) where T : IState
        {
            int maxCost = 0;
            int runningCost = 0;
            for (int i = 0; i < charms.Length; i++)
            {
                if (state.GetBool(charms[i].charmBool)) continue;
                int cost = charms[i].GetNotchCost(pm, state);
                if (cost > maxCost)
                {
                    runningCost += maxCost;
                    maxCost = cost;
                }
                else
                {
                    runningCost += cost;
                }
            }
            nonovercharmCost = runningCost + maxCost;
            overcharmCost = runningCost;
        }
    }
}
