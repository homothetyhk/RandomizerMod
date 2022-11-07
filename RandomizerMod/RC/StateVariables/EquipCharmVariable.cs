using RandomizerCore.Extensions;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    public class FragileCharmVariable : EquipCharmVariable
    {
        public Term repairTerm;
        public StateBool breakBool;

        public FragileCharmVariable(string name) : base(name) { }

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
                        breakBool = charmID switch
                        {
                            23 => lm.StateManager.GetBool("BROKEHEART"),
                            24 => lm.StateManager.GetBool("BROKEGREED"),
                            _ => lm.StateManager.GetBool("BROKESTRENGTH"),
                        },
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

        public virtual int GetNotchCost<T>(ProgressionManager pm, T state) where T : IState
        {
            return ((RandoModContext)pm.ctx).notchCosts[charmID - 1];
        }

        public virtual bool HasCharmProgression(ProgressionManager pm) => pm.Has(charmTerm) && pm.Has(canBenchTerm);

        /// <summary>
        /// Given that pm.HasCharmProgression returned true, this should determine whether the particular state supports equipping the charm, ignoring notch cost.
        /// </summary>
        protected virtual bool HasStateRequirements<T>(ProgressionManager pm, T state) where T : IState
        {
            if (state.GetBool(anticharmBool)) return false; // cannotBenchBool is also required for midpath charm equips, which aren't covered here.
            return true;
        }

        public bool CanEquipNonovercharm<T>(ProgressionManager pm, T state) where T : IState
        {
            if (HasStateRequirements(pm, state))
            {
                if (state.GetBool(charmBool)) return !state.GetBool(overcharmBool);
                if (state.GetInt(usedNotchesInt) + GetNotchCost(pm, state) <= pm.Get(notchesTerm)) return true;
            }
            return false;
        }

        public bool CanEquipOvercharm<T>(ProgressionManager pm, T state) where T : IState
        {
            if (HasStateRequirements(pm, state))
            {
                if (state.GetBool(charmBool)) return true;
                if (state.GetInt(usedNotchesInt) < pm.Get(notchesTerm)) return true;
            }
            return false;
        }

        public EquipResult CanEquip(ProgressionManager pm, StateUnion? localState)
        {
            if (localState is null || !HasCharmProgression(pm)) return EquipResult.None;
            for (int i = 0; i < localState.Count; i++)
            {
                if (CanEquipNonovercharm(pm, localState[i])) return EquipResult.Nonovercharm;
            }
            for (int i = 0; i < localState.Count; i++)
            {
                if (CanEquipOvercharm(pm, localState[i])) return EquipResult.Overcharm;
            }

            return EquipResult.None;
        }

        /// <summary>
        /// Checks whether the charm can be equipped. Does not modify the state--for that, use <see cref="ModifyState(object, ProgressionManager, ref LazyStateBuilder)"/>.
        /// </summary>
        public EquipResult CanEquip<T>(ProgressionManager pm, T state) where T : IState
        {
            if (!HasCharmProgression(pm) || !HasStateRequirements(pm, state)) return EquipResult.None;
            if (CanEquipNonovercharm(pm, state)) return EquipResult.Nonovercharm;
            if (CanEquipOvercharm(pm, state)) return EquipResult.Overcharm;
            return EquipResult.None;
        }


        public override bool ModifyState(object sender, ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (state.GetBool(charmBool)) return true;

            if (!HasCharmProgression(pm) || !HasStateRequirements(pm, state))
            {
                return false;
            }

            int notchCost = GetNotchCost(pm, state);
            if (notchCost <= 0)
            {
                state.Increment(usedNotchesInt, notchCost);
                state.SetBool(charmBool, true);
                return true;
            }

            int netNotches = pm.Get(notchesTerm) - state.GetInt(usedNotchesInt);
            if (netNotches <= 0)
            {
                return false;
            }
            else 
            {
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
