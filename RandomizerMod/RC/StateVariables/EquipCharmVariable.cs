using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $EQUIPPEDCHARM
     * Required Parameters:
     *   - First parameter MUST be either: the name of the charm term (e.g. Gathering_Swarm) or the 1-based charm ID (for Gathering Swarm, 1).
     * Optional Parameters: none
    */
    public class EquipCharmVariable : StateModifier
    {
        public override string Name { get; }
        protected int CharmID;
        protected Term CharmTerm;
        protected StateBool AnticharmBool;
        protected StateBool OvercharmBool;

        protected readonly StateBool NoPassedCharmEquip;
        protected readonly Term NotchesTerm;
        protected readonly StateBool CharmBool;
        protected readonly StateBool HasTakenDamage;
        protected readonly StateInt UsedNotchesInt;
        protected readonly StateInt MaxNotchCost;

        public const string Prefix = "$EQUIPPEDCHARM";

        protected EquipCharmVariable(string name, LogicManager lm)
        {
            Name = name;
            try
            {
                NotchesTerm = lm.GetTermStrict("NOTCHES");
                NoPassedCharmEquip = lm.StateManager.GetBoolStrict("NOPASSEDCHARMEQUIP");
                HasTakenDamage = lm.StateManager.GetBoolStrict("HASTAKENDAMAGE");
                OvercharmBool = lm.StateManager.GetBoolStrict("OVERCHARMED");
                UsedNotchesInt = lm.StateManager.GetIntStrict("USEDNOTCHES");
                MaxNotchCost = lm.StateManager.GetIntStrict("MAXNOTCHCOST");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing EquipCharmVariable", e);
            }
        }

        public EquipCharmVariable(string name, string charmName, int charmID, LogicManager lm) : this(name, lm)
        {
            this.CharmID = charmID;
            try
            {
                CharmTerm = lm.GetTermStrict(charmName);
                CharmBool = lm.StateManager.GetBoolStrict("CHARM" + charmID);
                AnticharmBool = lm.StateManager.GetBoolStrict("noCHARM" + charmID);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing EquipCharmVariable", e);
            }
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
                int charmID;
                string charmName;

                if (!int.TryParse(parameters[0], out charmID))
                {
                    charmID = LogicConstUtil.GetCharmID(charmName = parameters[0]);
                }
                else
                {
                    charmName = LogicConstUtil.GetCharmTerm(charmID);
                }

                EquipCharmVariable ecv;
                if (23 <= charmID && charmID <= 25)
                {
                    ecv = new FragileCharmVariable(term, charmName, charmID, lm);
                }
                else if (charmID == 36)
                {
                    ecv = new WhiteFragmentEquipVariable(term, lm);
                }
                else 
                {
                    ecv = new EquipCharmVariable(term, charmName, charmID, lm);
                }

                variable = ecv;
                return true;
            }
            variable = default;
            return false;
        }


        public override IEnumerable<Term> GetTerms()
        {
            yield return CharmTerm;
        }

        public enum EquipResult
        {
            None,
            Overcharm,
            Nonovercharm
        }

        public virtual int GetNotchCost<T>(ProgressionManager pm, T state) where T : IState
        {
            return ((RandoModContext)pm.ctx).notchCosts[CharmID - 1];
        }

        public virtual bool HasCharmProgression(ProgressionManager pm) => pm.Has(CharmTerm);

        /// <summary>
        /// Given that pm.HasCharmProgression returned true, this should determine whether the particular state supports equipping the charm, ignoring notch cost.
        /// </summary>
        protected virtual bool HasStateRequirements<T>(ProgressionManager pm, T state) where T : IState
        {
            if (state.GetBool(NoPassedCharmEquip) || state.GetBool(AnticharmBool)) return false;
            return true;
        }

        public bool CanEquipNonovercharm<T>(ProgressionManager pm, T state) where T : IState
        {
            if (HasStateRequirements(pm, state))
            {
                if (state.GetBool(CharmBool)) return !state.GetBool(OvercharmBool);
                if (state.GetInt(UsedNotchesInt) + GetNotchCost(pm, state) <= pm.Get(NotchesTerm)) return true;
            }
            return false;
        }

        public bool CanEquipOvercharm<T>(ProgressionManager pm, T state) where T : IState
        {
            if (HasStateRequirements(pm, state))
            {
                if (state.GetBool(CharmBool)) return true;
                if (state.GetInt(UsedNotchesInt) < pm.Get(NotchesTerm)) return true;
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
        /// Checks whether the charm can be equipped. Does not modify the state--for that, use <see cref="ModifyState(object, ProgressionManager, LazyStateBuilder)"/>.
        /// </summary>
        public EquipResult CanEquip<T>(ProgressionManager pm, T state) where T : IState
        {
            if (!HasCharmProgression(pm) || !HasStateRequirements(pm, state)) return EquipResult.None;
            if (CanEquipNonovercharm(pm, state)) return EquipResult.Nonovercharm;
            if (CanEquipOvercharm(pm, state)) return EquipResult.Overcharm;
            return EquipResult.None;
        }

        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            if (TryEquip(sender, pm, ref state)) yield return state;
        }

        public bool TryEquip(object? sender, ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (IsEquipped(state))
            {
                return true;
            }

            if (!HasCharmProgression(pm) || !HasStateRequirements(pm, state))
            {
                return false;
            }

            int notchCost = GetNotchCost(pm, state);
            if (notchCost <= 0)
            {
                DoEquipCharm(pm, notchCost, ref state);
                return true;
            }

            int netNotches = pm.Get(NotchesTerm) - state.GetInt(UsedNotchesInt);

            if (netNotches <= 0)
            {
                int oldMaxNotch = state.GetInt(MaxNotchCost);
                if (oldMaxNotch > notchCost && netNotches + oldMaxNotch > 0)
                {
                    DoEquipCharm(pm, notchCost, ref state);
                }
                return false;
            }
            else
            {
                if (netNotches < notchCost && state.GetBool(HasTakenDamage))
                {
                    return false; // state cannot overcharm!
                }
                DoEquipCharm(pm, notchCost, ref state);
                return true;
            }
        }

        public bool TryEquip(object? sender, ProgressionManager pm, in LazyStateBuilder state, out LazyStateBuilder newState)
        {
            if (IsEquipped(state))
            {
                return true;
            }

            if (!HasCharmProgression(pm) || !HasStateRequirements(pm, state))
            {
                return false;
            }

            int notchCost = GetNotchCost(pm, state);
            if (notchCost <= 0)
            {
                newState = new(state);
                DoEquipCharm(pm, notchCost, ref newState);
                return true;
            }

            int netNotches = pm.Get(NotchesTerm) - state.GetInt(UsedNotchesInt);

            if (netNotches <= 0)
            {
                int oldMaxNotch = state.GetInt(MaxNotchCost);
                if (oldMaxNotch > notchCost && netNotches + oldMaxNotch > 0)
                {
                    newState = new(state);
                    DoEquipCharm(pm, notchCost, ref newState);
                }
                return false;
            }
            else
            {
                if (netNotches < notchCost && state.GetBool(HasTakenDamage))
                {
                    return false; // state cannot overcharm!
                }
                newState = new(state);
                DoEquipCharm(pm, notchCost, ref newState);
                return true;
            }
        }

        protected virtual void DoEquipCharm(ProgressionManager pm, int notchCost, ref LazyStateBuilder state)
        {
            state.Increment(UsedNotchesInt, notchCost);
            state.SetBool(CharmBool, true);
            state.SetInt(MaxNotchCost, Math.Max(state.GetInt(MaxNotchCost), notchCost));
            if (state.GetInt(UsedNotchesInt) > pm.Get(NotchesTerm)) state.SetBool(OvercharmBool, true);
        }

        public virtual bool IsEquipped(LazyStateBuilder state) => state.GetBool(CharmBool);
        public virtual void SetUnequippable(ref LazyStateBuilder state) => state.SetBool(AnticharmBool, true);
        public int GetAvailableNotches(ProgressionManager pm, LazyStateBuilder state)
        {
            return pm.Get(NotchesTerm) - state.GetInt(UsedNotchesInt);
        }
    }
}
