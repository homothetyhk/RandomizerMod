using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    public class FragileCharmVariable : EquipCharmVariable
    {
        public Term repairTerm;

        public FragileCharmVariable(string name) : base(name) { }

        public override IEnumerable<Term> GetTerms()
        {
            return base.GetTerms().Append(repairTerm);
        }

        public override bool HasCharmRequirements(ProgressionManager pm)
        {
            return base.HasCharmRequirements(pm) && (pm.Has(charmTerm, 2) || pm.Has(repairTerm));
        }
    }

    public class EquipCharmVariable : StateModifyingVariable
    {
        public override string Name { get; }
        public int charmID;
        public Term charmTerm;
        public Term canBenchTerm;
        public Term notchesTerm;
        public StateBool charmBool;
        public StateBool anticharmBool;
        public StateBool overcharmBool;
        public StateBool hasTakenDamage;
        public StateInt usedNotchesInt;

        public const string Prefix = "$EQUIPPEDCHARM";

        public EquipCharmVariable(string name)
        {
            Name = name;
        }

        public static string GetName(string charmTermName)
        {
            return $"{Prefix}[{charmTermName}]";
        }

        public static string GetName(int charmID)
        {
            return $"{Prefix}[{charmID}]";
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out string[] parameters))
            {
                if (!int.TryParse(parameters[0], out int charmID))
                {
                    charmID = LogicConstUtil.GetCharmID(parameters[0]);
                }

                EquipCharmVariable ecv;
                if (23 <= charmID && charmID <= 25)
                {
                    ecv = new FragileCharmVariable(term)
                    {
                        charmID = charmID,
                        charmTerm = lm.Terms.GetTerm(LogicConstUtil.GetCharmTerm(charmID)),
                        canBenchTerm = lm.GetTerm("Can_Bench"),
                        notchesTerm = lm.GetTerm("NOTCHES"),
                        charmBool = lm.StateManager.GetBool("CHARM" + charmID),
                        anticharmBool = lm.StateManager.GetBool("noCHARM" + charmID) ?? throw new NullReferenceException(),
                        overcharmBool = lm.StateManager.GetBool("OVERCHARMED"),
                        hasTakenDamage = lm.StateManager.GetBool("HASTAKENDAMAGE"),
                        usedNotchesInt = lm.StateManager.GetInt("USEDNOTCHES"),
                        repairTerm = lm.GetTerm("Can_Repair_Fragile_Charms"),
                    };
                }
                else 
                {
                    ecv = new EquipCharmVariable(term)
                    {
                        charmID = charmID,
                        charmTerm = lm.Terms.GetTerm(LogicConstUtil.GetCharmTerm(charmID)),
                        canBenchTerm = lm.GetTerm("Can_Bench"),
                        notchesTerm = lm.GetTerm("NOTCHES"),
                        charmBool = lm.StateManager.GetBool("CHARM" + charmID),
                        anticharmBool = lm.StateManager.GetBool("noCHARM" + charmID) ?? throw new NullReferenceException(),
                        hasTakenDamage = lm.StateManager.GetBool("HASTAKENDAMAGE"),
                        overcharmBool = lm.StateManager.GetBool("OVERCHARMED"),
                        usedNotchesInt = lm.StateManager.GetInt("USEDNOTCHES"),
                    };
                }

                variable = ecv;
                return true;
            }
            variable = default;
            return false;
        }


        public override IEnumerable<Term> GetTerms()
        {
            yield return charmTerm;
            yield return canBenchTerm;
        }

        public virtual bool HasCharmRequirements(ProgressionManager pm) => pm.Has(charmTerm) && pm.Has(canBenchTerm);

        public override int GetValue(object sender, ProgressionManager pm, StateUnion? localState)
        {
            return CanEquip(pm, localState) switch
            {
                EquipResult.Nonovercharm or EquipResult.Overcharm => TRUE,
                _ => FALSE,
            };
        }

        public enum EquipResult
        {
            None,
            Overcharm,
            Nonovercharm
        }

        public EquipResult CanEquip(ProgressionManager pm, StateUnion? localState)
        {
            if (localState is null || !HasCharmRequirements(pm)) return EquipResult.None;
            int notchCost = ((RandoModContext)pm.ctx).notchCosts[charmID - 1];
            for (int i = 0; i < localState.Count; i++)
            {
                if (localState[i].GetBool(anticharmBool)) continue;
                if (localState[i].GetBool(charmBool) && !localState[i].GetBool(overcharmBool)) return EquipResult.Nonovercharm;
                else if (localState[i].CouldSetIntToValue(usedNotchesInt, pm.Get(notchesTerm) + notchCost)) return EquipResult.Nonovercharm;
            }
            for (int i = 0; i < localState.Count; i++)
            {
                if (localState[i].GetBool(anticharmBool)) continue;
                if (localState[i].GetBool(charmBool)) return EquipResult.Overcharm;
                if (!localState[i].GetBool(hasTakenDamage) && !localState[i].GetBool(overcharmBool) && localState[i].GetInt(usedNotchesInt) < pm.Get(notchesTerm)) return EquipResult.Overcharm;
            }

            return EquipResult.None;
        }

        public EquipResult CanEquip(ProgressionManager pm, State state)
        {
            if (!HasCharmRequirements(pm) || state.GetBool(anticharmBool)) return EquipResult.None;
            int notchCost = ((RandoModContext)pm.ctx).notchCosts[charmID - 1];
            if (state.GetBool(charmBool)) return state.GetBool(overcharmBool) ? EquipResult.Overcharm : EquipResult.Nonovercharm;
            if (state.GetInt(usedNotchesInt) + notchCost <= pm.Get(notchesTerm)) return EquipResult.Nonovercharm;
            if (!state.GetBool(hasTakenDamage) && !state.GetBool(overcharmBool) && state.GetInt(usedNotchesInt) < pm.Get(notchesTerm)) return EquipResult.Overcharm;
            return EquipResult.None;
        }

        /// <summary>
        /// Checks whether the charm can be equipped. Does not modify the state--for that, use <see cref="ModifyState(object, ProgressionManager, ref LazyStateBuilder)"/>.
        /// </summary>
        public EquipResult CanEquip(ProgressionManager pm, LazyStateBuilder state)
        {
            if (!HasCharmRequirements(pm) || state.GetBool(anticharmBool)) return EquipResult.None;
            int notchCost = ((RandoModContext)pm.ctx).notchCosts[charmID - 1];
            if (state.GetBool(charmBool)) return state.GetBool(overcharmBool) ? EquipResult.Overcharm : EquipResult.Nonovercharm;
            if (state.GetInt(usedNotchesInt) + notchCost <= pm.Get(notchesTerm)) return EquipResult.Nonovercharm;
            if (!state.GetBool(hasTakenDamage) && !state.GetBool(overcharmBool) && state.GetInt(usedNotchesInt) < pm.Get(notchesTerm)) return EquipResult.Overcharm;
            return EquipResult.None;
        }


        public override bool ModifyState(object sender, ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (state.GetBool(charmBool)) return true;

            if (!HasCharmRequirements(pm) || state.GetBool(anticharmBool))
            {
                return false;
            }

            int netNotches = pm.Get(notchesTerm) - state.GetInt(usedNotchesInt);
            if (netNotches <= 0)
            {
                return false;
            }
            else 
            {
                int notchCost = ((RandoModContext)pm.ctx).notchCosts[charmID];
                if (netNotches < notchCost)
                {
                    if (state.GetBool(hasTakenDamage) || !state.TrySetBoolTrue(overcharmBool))
                    {
                        return false;
                    }
                }
                state.Increment(usedNotchesInt, notchCost);
                state.SetBool(charmBool, true);
                return true;
            }
        }
    }
}
