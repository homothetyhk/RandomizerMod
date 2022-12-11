using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $CASTSPELL
     * Required Parameters: none
     * Optional Parameters:
     *   - any integer parameters: parses to an array of ints, which represent number of spell casts, where time passes between different entries of the array (i.e. soul reserves can refill, etc).
     *                             If missing, number of casts is new int[]{1}
     *   - a parameter beginning with "before:": tries to convert the tail of the parameter to the NearbySoul enum (either by string or int parsing). Represents soul available before any spells are cast.
     *   - a parameter beginning with "after:": tries to convert the tail of the parameter to the NearbySoul enum (either by string or int parsing). Represents soul available after all spells are cast.
     *   - a parameter equal to "noDG": indicates that dream gate is not possible after the cast.
    */
    public class CastSpellVariable : StateModifier
    {
        public enum NearbySoul
        {
            NONE,
            ITEMSOUL,
            MAPAREASOUL,
            AREASOUL, 
            ROOMSOUL,
        }

        public override string Name { get; }
        public int[] spellCasts;
        public bool canDreamgate;
        public NearbySoul beforeSoul;
        public NearbySoul afterSoul;
        public StateInt spentSoul;
        public StateInt spentReserveSoul;
        public StateInt soulLimiter;
        public StateInt maxRequiredSoul;
        public StateInt usedNotches;
        public StateBool usedShade;
        public StateBool cannotRegainSoul;
        public StateBool noFlower;
        public Term vesselFragments;
        public Term dreamnail;
        public Term essence;
        public Term itemRando;
        public Term mapAreaRando;
        public Term areaRando;
        public Term roomRando;
        public EquipCharmVariable equipSpellTwister;
        public State dgState;
        public const string Prefix = "$CASTSPELL";

        protected CastSpellVariable(string name)
        {
            Name = name;
        }

        public CastSpellVariable(string name, LogicManager lm, int[] spellCasts, bool canDreamgate, NearbySoul beforeSoul, NearbySoul afterSoul)
        {
            Name = name;
            this.spellCasts = spellCasts;
            this.canDreamgate = canDreamgate;
            this.beforeSoul = beforeSoul;
            this.afterSoul = afterSoul;
            try
            {
                spentSoul = lm.StateManager.GetIntStrict("SPENTSOUL");
                spentReserveSoul = lm.StateManager.GetIntStrict("SPENTRESERVESOUL");
                soulLimiter = lm.StateManager.GetIntStrict("SOULLIMITER");
                maxRequiredSoul = lm.StateManager.GetIntStrict("REQUIREDMAXSOUL");
                usedNotches = lm.StateManager.GetIntStrict("USEDNOTCHES");
                usedShade = lm.StateManager.GetBoolStrict("USEDSHADE");
                cannotRegainSoul = lm.StateManager.GetBoolStrict("CANNOTREGAINSOUL");
                noFlower = lm.StateManager.GetBoolStrict("NOFLOWER");
                vesselFragments = lm.GetTermStrict("VESSELFRAGMENTS");
                dreamnail = lm.GetTermStrict("DREAMNAIL");
                essence = lm.GetTermStrict("ESSENCE");
                itemRando = lm.GetTermStrict("ITEMRANDO");
                mapAreaRando = lm.GetTermStrict("MAPAREARANDO");
                areaRando = lm.GetTermStrict("FULLAREARANDO");
                roomRando = lm.GetTermStrict("ROOMRANDO");
                equipSpellTwister = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Spell_Twister"));
                dgState = GetDGState(lm.StateManager);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing CastSpellVariable", e);
            }
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out string[] parameters))
            {
                List<int> spellCasts = new();
                bool canDreamgate = true;
                NearbySoul beforeSoul = NearbySoul.NONE;
                NearbySoul afterSoul = NearbySoul.NONE;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (int.TryParse(parameters[i], out int castNum)) spellCasts.Add(castNum);
                    else if (parameters[i] == "noDG") canDreamgate = false;
                    else if (parameters[i].StartsWith("before:"))
                    {
                        string arg = parameters[i].Substring(7);
                        if (!Enum.TryParse(arg, out beforeSoul))
                        {
                            if (int.TryParse(arg, out int k)) beforeSoul = (NearbySoul)k;
                            else throw new ArgumentException($"Could not parse {parameters[i]} to CastSpellVariable argument.");
                        }
                    }
                    else if (parameters[i].StartsWith("after:"))
                    {
                        string arg = parameters[i].Substring(6);
                        if (!Enum.TryParse(arg, out afterSoul))
                        {
                            if (int.TryParse(arg, out int k)) afterSoul = (NearbySoul)k;
                            else throw new ArgumentException($"Could not parse {parameters[i]} to CastSpellVariable argument.");
                        }
                    }
                    else throw new ArgumentException($"Could not parse {parameters[i]} to CastSpellVariable argument.");
                }
                if (spellCasts.Count == 0) spellCasts.Add(1);
                variable = new CastSpellVariable(term, lm, spellCasts.ToArray(), canDreamgate, beforeSoul, afterSoul);
                return true;
            }
            variable = default;
            return false;
        }

        private static State GetDGState(StateManager sm)
        {
            LazyStateBuilder lsb = new(sm.StartState);

            if (lsb.GetBool(sm.GetBoolStrict("NOFLOWER")))
            {
                lsb.SetBool(sm.GetBoolStrict("NOFLOWER"), false);
            }
            // TODO: dg-resetable field tag?
            return lsb.GetState();
        }

        public override IEnumerable<Term> GetTerms()
        {
            yield return vesselFragments;
            yield return dreamnail;
            yield return essence;
            yield return itemRando;
            yield return mapAreaRando;
            yield return areaRando;
            yield return roomRando;
            foreach (Term t in equipSpellTwister.GetTerms()) yield return t;
        }

        public override IEnumerable<LazyStateBuilder>? ProvideState(object? sender, ProgressionManager pm)
        {
            return base.ProvideState(sender, pm);
        }

        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            if (canDreamgate && pm.Has(dreamnail, 2) && pm.Has(essence) && CheckDGState())
            {
                // TODO: check if dgState satisfies soul requirement, for interop
                yield return new(dgState);
                if (LazyStateBuilder.IsComparablyLE(dgState, state)) yield break;
            }

            int soul;
            int reserves = GetReserves(pm, state);
            int maxSoul = GetMaxSoul(state);
            if (!state.GetBool(cannotRegainSoul) && NearbySoulToBool(beforeSoul, pm))
            {
                soul = GetMaxSoul(state);
            }
            else
            {
                soul = GetSoul(state);
            }

            if (!state.GetBool(equipSpellTwister.charmBool) && TryCastAll(33, maxSoul, reserves, soul))
            {
                LazyStateBuilder state33 = new(state);
                DoAllCasts(33, reserves, ref state33);
                if (!state33.GetBool(cannotRegainSoul) && NearbySoulToBool(afterSoul, pm))
                {
                    RecoverSoul(spellCasts.Sum() * 33, state33);
                }
                yield return state33;
            }

            if (TryCastAll(25, maxSoul, reserves, soul))
            {
                foreach (LazyStateBuilder STstate in equipSpellTwister.ModifyState(sender, pm, state))
                {
                    DoAllCasts(25, reserves, ref state);
                    if (!state.GetBool(cannotRegainSoul) && NearbySoulToBool(afterSoul, pm))
                    {
                        RecoverSoul(spellCasts.Sum() * 33, state); // recover the same amount of soul as in the normal path
                    }
                    yield return state;
                }
            }
        }

        public void DoAllCasts(int costPerCast, int currentReserve, ref LazyStateBuilder state)
        {
            for (int i = 0; i < spellCasts.Length; i++) SpendSoul(costPerCast * spellCasts[i], ref currentReserve, ref state);
        }

        public void SpendSoul(int amount, ref int currentReserve, ref LazyStateBuilder state)
        {
            if (currentReserve >= amount)
            {
                state.Increment(spentReserveSoul, amount);
                currentReserve -= amount;
            }
            else if (currentReserve > 0)
            {
                state.Increment(spentReserveSoul, currentReserve);
                state.Increment(spentSoul, amount - currentReserve);
                currentReserve = 0;
            }
            else
            {
                state.Increment(spentSoul, amount);
            }
            if (amount > state.GetInt(maxRequiredSoul))
            {
                state.SetInt(maxRequiredSoul, amount);
            }
        }

        private bool TrySpendSoul(int amount, int maxSoul, ref int reserves, ref int soul)
        {
            if (soul < amount) return false;
            soul -= amount;
            int transferAmt = Math.Min(reserves, maxSoul - soul);
            soul += transferAmt;
            reserves -= transferAmt;
            return true;
        }

        private bool TryCastAll(int costPerCast, int maxSoul, int reserves, int soul)
        {
            for (int i = 0; i < spellCasts.Length; i++)
            {
                if (!TrySpendSoul(costPerCast * spellCasts[i], maxSoul, ref reserves, ref soul)) return false;
            }
            return true;
        }

        public void RecoverSoul(int amount, LazyStateBuilder state)
        {
            int soulDiff = state.GetInt(spentSoul);
            if (soulDiff >= amount)
            {
                state.Increment(spentSoul, -amount);
            }
            else if (soulDiff > 0)
            {
                state.SetInt(spentSoul, 0);
                amount -= soulDiff;
            }
            int reserveDiff = state.GetInt(spentReserveSoul);
            if (reserveDiff >= amount)
            {
                state.Increment(spentReserveSoul, -amount);
            }
            else if (reserveDiff > 0)
            {
                state.SetInt(spentReserveSoul, 0);
                amount -= reserveDiff;
            }
        }

        private bool NearbySoulToBool(NearbySoul soul, ProgressionManager pm)
        {
            if (soul > NearbySoul.NONE && soul <= NearbySoul.ROOMSOUL)
            {
                NearbySoul mode = GetMode(pm);
                return mode > NearbySoul.NONE && mode <= soul;
            }
            return false;
        }

        private NearbySoul GetMode(ProgressionManager pm)
        {
            if (pm.Has(roomRando)) return NearbySoul.ROOMSOUL;
            else if (pm.Has(areaRando)) return NearbySoul.AREASOUL;
            else if (pm.Has(mapAreaRando)) return NearbySoul.MAPAREASOUL;
            else if (pm.Has(itemRando)) return NearbySoul.ITEMSOUL;
            else return NearbySoul.NONE;
        }

        private int GetMaxSoul<T>(T state) where T : IState
        {
            return 99 - state.GetInt(soulLimiter);
        }

        private int GetSoul<T>(T state) where T : IState
        {
            return GetMaxSoul(state) - state.GetInt(spentSoul);
        }

        private int GetMaxReserves(ProgressionManager pm)
        {
            return (pm.Get(vesselFragments) / 3) * 33;
        }

        private int GetReserves<T>(ProgressionManager pm, T state) where T : IState
        {
            return GetMaxReserves(pm) - state.GetInt(spentReserveSoul);
        }

        private bool CheckDGState()
        {
            int soul = GetSoul(dgState);
            if (soul >= spellCasts.Max() * 33) return true;
            if (dgState.GetBool(equipSpellTwister.charmBool) && soul >= spellCasts.Max() * 25) return true;
            return false; // too much work to consider bringing soul via dgate if the start doesn't have soul
        }

    }
}
