using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $TAKEDAMAGE
     * Required Parameters:
         - If any parameters are provided, the first parameter must parse to int to give the damage amount. If absent, defaults to 1.
     * Optional Parameters: none
     * Implements taking damage from a single hit. Assumes enough time to focus/hiveblood before and after the hit.
    */
    public class TakeDamageVariable : StateModifier
    {
        public override string Name { get; }
        public const string Prefix = "$TAKEDAMAGE";
        protected readonly int Amount;
        protected readonly StateBool Overcharmed;
        protected readonly StateBool HasTakenDamage;
        protected readonly StateBool HasTakenDoubleDamage;
        protected readonly StateBool HasAlmostDied;
        protected readonly StateBool NoFlower;
        protected readonly StateBool NoPassedCharmEquip;
        protected readonly StateInt SpentHP;
        protected readonly StateInt SpentBlueHP;
        protected readonly Term MaskShards;
        protected readonly Term Focus;
        protected readonly EquipCharmVariable LifebloodHeart;
        protected readonly EquipCharmVariable LifebloodCore;
        protected readonly EquipCharmVariable JonisBlessing;
        protected readonly EquipCharmVariable FragileHeart;
        protected readonly EquipCharmVariable Hiveblood;
        protected readonly EquipCharmVariable DeepFocus;
        protected readonly SpendSoulVariable SpendSoul33;
        // not supported: grubsong

        public TakeDamageVariable(string name, LogicManager lm, int amount)
        {
            Name = name;
            Amount = amount;
            try
            {
                Overcharmed = lm.StateManager.GetBoolStrict("OVERCHARMED");
                HasTakenDamage = lm.StateManager.GetBoolStrict("HASTAKENDAMAGE");
                HasTakenDoubleDamage = lm.StateManager.GetBoolStrict("HASTAKENDOUBLEDAMAGE");
                HasAlmostDied = lm.StateManager.GetBoolStrict("HASALMOSTDIED");
                NoFlower = lm.StateManager.GetBoolStrict("NOFLOWER");
                NoPassedCharmEquip = lm.StateManager.GetBoolStrict("NOPASSEDCHARMEQUIP");
                SpentHP = lm.StateManager.GetIntStrict("SPENTHP");
                SpentBlueHP = lm.StateManager.GetIntStrict("SPENTBLUEHP");
                MaskShards = lm.GetTermStrict("MASKSHARDS");
                Focus = lm.GetTermStrict("FOCUS");
                LifebloodHeart = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Lifeblood_Heart"));
                LifebloodCore = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Lifeblood_Core"));
                JonisBlessing = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Joni's_Blessing"));
                FragileHeart = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Fragile_Heart"));
                Hiveblood = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Hiveblood"));
                DeepFocus = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Deep_Focus"));
                SpendSoul33 = (SpendSoulVariable)lm.GetVariableStrict(SpendSoulVariable.Prefix + "[33]");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing TakeHitVariable", e);
            }
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out string[] parameters))
            {
                int amount = parameters.Length == 0 ? 1 : int.Parse(parameters[0]);
                variable = new TakeDamageVariable(term, lm, amount);
                return true;
            }
            variable = default;
            return false;
        }

        public override IEnumerable<Term> GetTerms()
        {
            yield return MaskShards;
            yield return Focus;
            foreach (Term t in LifebloodHeart.GetTerms()) yield return t;
            foreach (Term t in LifebloodCore.GetTerms()) yield return t;
            foreach (Term t in JonisBlessing.GetTerms()) yield return t;
            foreach (Term t in FragileHeart.GetTerms()) yield return t;
            foreach (Term t in Hiveblood.GetTerms()) yield return t;
            foreach (Term t in DeepFocus.GetTerms()) yield return t;
            foreach (Term t in SpendSoul33.GetTerms()) yield return t;
        }

        private int CalculateAmount(ProgressionManager pm, LazyStateBuilder state)
        {
            return state.GetBool(Overcharmed) ? 2 * Amount : Amount;
        }

        private int GetBlueHP(ProgressionManager pm, LazyStateBuilder state)
        {
            int blueHP = 0;
            if (LifebloodHeart.IsEquipped(state)) blueHP += 2;
            if (LifebloodCore.IsEquipped(state)) blueHP += 4;
            blueHP -= state.GetInt(SpentBlueHP);
            return blueHP;
        }


        private int GetHP(ProgressionManager pm, LazyStateBuilder state)
        {
            int hp = pm.Get(MaskShards) / 4;
            if (FragileHeart.IsEquipped(state)) hp += 2;
            if (JonisBlessing.IsEquipped(state)) hp = (int)(1.4f * hp);
            hp -= state.GetInt(SpentHP);
            return hp;
        }

        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            bool firstTime = !state.GetBool(HasTakenDamage);
            int amount = CalculateAmount(pm, state);
            if (firstTime)
            {
                return GenerateGreedyCharmLayouts(pm, state).SelectMany(state => amount == 1 ? ModifyStateSingleDamage(pm, state) : ModifyStateDoubleDamage(amount, pm, state));
            }
            else
            {
                return amount == 1 ? ModifyStateSingleDamage(pm, state) : ModifyStateDoubleDamage(amount, pm, state);
            }
        }

        private IEnumerable<LazyStateBuilder> ModifyStateSingleDamage(ProgressionManager pm, LazyStateBuilder state)
        {
            int blueHP = GetBlueHP(pm, state);
            int hp = GetHP(pm, state);

            if (blueHP <= 0 && hp <= 1)
            {
                foreach (LazyStateBuilder lsb in GenerateDesperationStates(pm, state))
                {
                    LazyStateBuilder lsb2 = lsb;
                    if (lsb2.GetBool(Overcharmed))
                    {
                        lsb2.SetBool(HasTakenDoubleDamage, true);
                        if (TrySurviveWithHealing(2, pm, ref lsb2))
                        {
                            yield return lsb;
                        }
                    }
                    else
                    {
                        if (TrySurviveWithHealing(1, pm, ref lsb2))
                        {
                            yield return lsb2;
                        }
                    }
                }
            }
            else
            {
                DoTakeDamage(1, blueHP, pm, ref state);
                yield return state;
            }
        }

        private IEnumerable<LazyStateBuilder> ModifyStateDoubleDamage(int amount, ProgressionManager pm, LazyStateBuilder state)
        {
            if (!state.GetBool(HasTakenDoubleDamage))
            {
                if (state.GetBool(HasAlmostDied)) // already handled in GenerateDesperationStates
                {
                    state.SetBool(HasTakenDoubleDamage, true);
                }
                else
                {
                    foreach (LazyStateBuilder lsb in GenerateBlueStates(pm, state))
                    {
                        lsb.SetBool(HasTakenDoubleDamage, true);
                        foreach (LazyStateBuilder lsb2 in ModifyStateDoubleDamage(CalculateAmount(pm, lsb), pm, lsb)) yield return lsb2;
                    }
                    yield break;
                }
            }

            int blueHP = GetBlueHP(pm, state);
            int hp = GetHP(pm, state);

            if (blueHP < amount && hp <= amount)
            {
                foreach (LazyStateBuilder lsb in GenerateDesperationStates(pm, state))
                {
                    LazyStateBuilder lsb2 = lsb;
                    if (TrySurviveWithHealing(CalculateAmount(pm, lsb2), pm, ref lsb2))
                    {
                        yield return lsb2;
                    }
                }
            }
            else
            {
                DoTakeDamage(amount, blueHP, pm, ref state);
                yield return state;
            }
        }

        private void DoTakeDamage(int amount, int blueHP, ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (blueHP >= amount)
            {
                state.Increment(SpentBlueHP, amount);
            }
            else
            {
                if (blueHP >= 0) state.Increment(SpentBlueHP, blueHP);
                state.Increment(SpentHP, amount);
            }
            state.SetBool(HasTakenDamage, true);
            if (state.GetInt(SpentHP) > 0 && Hiveblood.IsEquipped(state))
            {
                state.Increment(SpentHP, -1);
            }
        }

        private bool TrySurviveWithHealing(int amount, ProgressionManager pm, ref LazyStateBuilder state)
        {
            int blueHP = GetBlueHP(pm, state);
            int hp = GetHP(pm, state);
            if (blueHP >= amount || hp > amount)
            {
                DoTakeDamage(amount, blueHP, pm, ref state);
                return true;
            }

            int spentHP = state.GetInt(SpentHP);
            int maxHP = spentHP + hp;
            if (maxHP <= amount) return false;
            if (!pm.Has(Focus) || JonisBlessing.IsEquipped(state)) return false;
            JonisBlessing.SetUnequippable(ref state);
            int healAmt = DeepFocus.IsEquipped(state) ? 2 : 1;
            if (healAmt != 2) DeepFocus.SetUnequippable(ref state);

            while (maxHP - spentHP <= amount)
            {
                if (!SpendSoul33.TryModifySingle(33, pm, ref state)) return false;
                if (healAmt == 2 && spentHP == 1)
                {
                    spentHP -= 1;
                }
                else
                {
                    spentHP -= healAmt;
                }
            }
            state.SetInt(SpentHP, spentHP);

            DoTakeDamage(amount, blueHP, pm, ref state);
            return true;
        }

        private IEnumerable<LazyStateBuilder> GenerateGreedyCharmLayouts(ProgressionManager pm, LazyStateBuilder state)
        {
            if (!Hiveblood.IsEquipped(state) && Hiveblood.TryEquip(null, pm, in state, out LazyStateBuilder hbState))
            {
                Hiveblood.SetUnequippable(ref state);
                yield return hbState;
            }
            yield return state;
        }

        private IEnumerable<LazyStateBuilder> GenerateBlueStates(ProgressionManager pm, LazyStateBuilder state)
        {
            if (state.GetBool(NoPassedCharmEquip) || Hiveblood.IsEquipped(state) && state.GetBool(HasTakenDamage))
            {
                LifebloodHeart.SetUnequippable(ref state);
                LifebloodCore.SetUnequippable(ref state);
                yield return state;
                yield break;
            }

            LazyStateBuilder current = new(state);
            LifebloodHeart.SetUnequippable(ref state);
            LifebloodCore.SetUnequippable(ref state);
            yield return current;
            
            current = new(state);
            if (LifebloodHeart.TryEquip(null, pm, ref current))
            {
                if (LifebloodCore.TryEquip(null, pm, ref current))
                {
                    RebalanceHPAfterCharmChange(pm, ref current);
                    yield return current;

                    current = new(state);
                    LifebloodHeart.TryEquip(null, pm, ref current);
                    LifebloodCore.SetUnequippable(ref current);
                    RebalanceHPAfterCharmChange(pm, ref current);
                    yield return current;

                    current = state;
                    LifebloodCore.TryEquip(null, pm, ref current);
                    LifebloodHeart.SetUnequippable(ref current);
                    RebalanceHPAfterCharmChange(pm, ref current);
                    yield return current;
                }
                else
                {
                    LifebloodCore.SetUnequippable(ref current);
                    RebalanceHPAfterCharmChange(pm, ref current);
                    yield return current; 
                }
            }
            else if (LifebloodCore.TryEquip(null, pm, ref current))
            {
                LifebloodHeart.SetUnequippable(ref current);
                RebalanceHPAfterCharmChange(pm, ref current);
                yield return current;
            }
        }

        private IEnumerable<LazyStateBuilder> GenerateDesperationStates(ProgressionManager pm, LazyStateBuilder state)
        {
            bool previouslyEntered = !state.TrySetBoolTrue(HasAlmostDied);
            if (previouslyEntered)
            {
                yield return state;
                yield break;
            }

            List<int> steps = new();
            if (CanAdd(LifebloodHeart) && !Hiveblood.IsEquipped(state) && state.GetBool(HasTakenDamage))
            {
                steps.Add(0);
            }
            else if (!LifebloodHeart.IsEquipped(state))
            {
                LifebloodHeart.SetUnequippable(ref state); // set unequippable so that result states are incomparable and we can short circuit future GenerateDesperationStates calls
            }

            if (CanAdd(LifebloodCore) && !Hiveblood.IsEquipped(state) && state.GetBool(HasTakenDamage))
            {
                steps.Add(1);
            }
            else if (!LifebloodCore.IsEquipped(state))
            {
                LifebloodCore.SetUnequippable(ref state);
            }

            if (CanAdd(FragileHeart))
            {
                steps.Add(2);
            }
            else if (!FragileHeart.IsEquipped(state))
            {
                FragileHeart.SetUnequippable(ref state);
            }

            if (CanAdd(JonisBlessing))
            {
                steps.Add(3);
            }
            else if (!JonisBlessing.IsEquipped(state))
            {
                JonisBlessing.SetUnequippable(ref state);
            }

            if (pm.Has(Focus) && CanAdd(DeepFocus))
            {
                steps.Add(4);
            }
            else if (!DeepFocus.IsEquipped(state))
            {
                DeepFocus.SetUnequippable(ref state);
            }

            int pow = 1 << steps.Count;
            for (int i = 0; i < pow; i++)
            {
                LazyStateBuilder next = new(state);
                for (int j = 0; j < steps.Count; j++)
                {
                    if ((i & (1 << j)) == (1 << j))
                    {
                        switch (steps[j])
                        {
                            case 0:
                                if (!LifebloodHeart.TryEquip(null, pm, ref next)) goto CONTINUE_OUTER;
                                break;
                            case 1:
                                if (!LifebloodCore.TryEquip(null, pm, ref next)) goto CONTINUE_OUTER;
                                break;
                            case 2:
                                if (!FragileHeart.TryEquip(null, pm, ref next)) goto CONTINUE_OUTER;
                                break;
                            case 3:
                                if (!JonisBlessing.TryEquip(null, pm, ref next)) goto CONTINUE_OUTER;
                                break;
                            case 4:
                                if (!DeepFocus.TryEquip(null, pm, ref next)) goto CONTINUE_OUTER;
                                break;
                        }
                    }
                    else
                    {
                        switch (steps[j])
                        {
                            case 0:
                                LifebloodHeart.SetUnequippable(ref state);
                                break;
                            case 1:
                                LifebloodCore.SetUnequippable(ref state);
                                break;
                            case 2:
                                FragileHeart.SetUnequippable(ref state);
                                break;
                            case 3:
                                JonisBlessing.SetUnequippable(ref state);
                                break;
                            case 4:
                                DeepFocus.SetUnequippable(ref state);
                                break;
                        }
                    }
                }
                if (next.GetBool(Overcharmed)
                    && (next.GetBool(HasTakenDoubleDamage) || next.GetBool(HasTakenDamage) && Hiveblood.IsEquipped(next)))
                {
                    continue; // can't reconstruct damage history
                }
                RebalanceHPAfterCharmChange(pm, ref next);
                yield return next;
                CONTINUE_OUTER: continue;
            }

            bool CanAdd(EquipCharmVariable ecv)
            {
                return ecv.HasCharmProgression(pm) && !ecv.IsEquipped(state);
            }
        }

        private void RebalanceHPAfterCharmChange(ProgressionManager pm, ref LazyStateBuilder lsb)
        {
            int blueHP = GetBlueHP(pm, lsb);
            int spentHP = lsb.GetInt(SpentHP);

            if (spentHP > blueHP)
            {
                lsb.Increment(SpentHP, -blueHP);
                spentHP -= blueHP;
                lsb.Increment(SpentBlueHP, blueHP);
                blueHP += spentHP;
            }
            else
            {
                lsb.Increment(SpentHP, -spentHP);
                spentHP = 0;
                lsb.Increment(SpentBlueHP, spentHP);
                blueHP += spentHP;
            }
            if (lsb.GetBool(Overcharmed)) // we assume HasTakenDoubleDamage was false before entering, and that there has been no opportunities to heal through Focus/Hiveblood
            {
                int spentBlueHP = lsb.GetInt(SpentBlueHP);
                if (blueHP >= spentBlueHP)
                {
                    lsb.Increment(SpentBlueHP, spentBlueHP); // double blue damage taken
                }
                else
                {
                    lsb.Increment(SpentBlueHP, blueHP); // set blue health to 0
                    lsb.Increment(SpentHP, spentBlueHP - blueHP / 2); // take remainder in white health, next step will handle doubling damage
                }
                lsb.Increment(SpentHP, spentHP); // double white health damage taken
            }
        }
    }
}
