using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using static RandomizerMod.RC.StateVariables.IHPStateManager;

namespace RandomizerMod.RC.StateVariables
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>This interface only uses <see cref="IEnumerable{T}"/> return values, rather than the ref <see cref="LazyStateBuilder"/> pattern.
    /// This is because many hp operations may require branching to fully account for how their effects interact with the hp state model.</remarks>
    public interface IHPStateManager : ILogicVariable
    {
        /// <summary>
        /// Applies the effect of taking a single hit of the specified amount (preovercharm).
        /// </summary>
        IEnumerable<LazyStateBuilder> TakeDamage(ProgressionManager pm, LazyStateBuilder state, int amount);
        /// <summary>
        /// Applies the effect of taking a sequence of hits of the specified amounts (preovercharm), with insufficient time between hits to focus or proc hiveblood.
        /// </summary>
        IEnumerable<LazyStateBuilder> TakeDamageSequence(ProgressionManager pm, LazyStateBuilder state, params int[] amounts);
        /// <summary>
        /// Attempts to focus once. Returns true if successful. Deducts soul without healing if Joni is equipped. Does not modify the input on failure.
        /// <br/>Automatically fails if used when any focus charm (Joni, Deep Focus) is not determined.
        /// <br/>Automatically fails if used with indeterminate hp. See <see cref="IsHPDetermined(LazyStateBuilder)"/>.
        /// </summary>
        bool TryFocus(ProgressionManager pm, ref LazyStateBuilder state);
        /// <summary>
        /// Attempts to focus exactly the specified number of times, allowing use of soul vessels. Deducts soul without healing if Joni is equipped.
        /// <br/>If Joni and Deep Focus are not determined, determines both charms.
        /// </summary>
        IEnumerable<LazyStateBuilder> DoFocus(ProgressionManager pm, LazyStateBuilder state, int amount);
        /// <summary>
        /// Applies the effect of receiving the specified amount of lifeblood.
        /// </summary>
        IEnumerable<LazyStateBuilder> GiveBlueHealth(ProgressionManager pm, LazyStateBuilder state, int amount);
        /// <summary>
        /// Applies the effect of restoring the specified amount of ordinary health.
        /// </summary>
        IEnumerable<LazyStateBuilder> GiveHealth(ProgressionManager pm, LazyStateBuilder state, int amount);
        /// <summary>
        /// Applies the effect of restoring all white health.
        /// </summary>
        IEnumerable<LazyStateBuilder> RestoreWhiteHealth(ProgressionManager pm, LazyStateBuilder state);
        /// <summary>
        /// Applies the effect of restoring all health. Resets blue health similarly to a bench.
        /// </summary>
        IEnumerable<LazyStateBuilder> RestoreAllHealth(ProgressionManager pm, LazyStateBuilder state);
        /// <summary>
        /// Returns true if the state can be queried for specific hp info (hp remaining, etc).
        /// If false, <see cref="DetermineHP(ProgressionManager, LazyStateBuilder)"/> must be used before querying.
        /// </summary>
        bool IsHPDetermined(LazyStateBuilder state);
        /// <summary>
        /// Determines various questions which can affect calculated hp (overcharm, equipped charms, etc).
        /// </summary>
        IEnumerable<LazyStateBuilder> DetermineHP(ProgressionManager pm, LazyStateBuilder state);
        /// <summary>
        /// Returns the exact amount of each hp type for the state. See <see cref="IsHPDetermined(LazyStateBuilder)"/> for when this can be safely used.
        /// </summary>
        /// <exception cref="InvalidOperationException">The state has indeterminate HP info.</exception>
        StrictHPInfo GetHPInfo(ProgressionManager pm, LazyStateBuilder state);
        /// <summary>
        /// Data representing the exact amount of each hp type for a particular state.
        /// <br/>For the purpose of this data, Joni HP is treated as white, rather than blue.
        /// </summary>
        public readonly record struct StrictHPInfo(int CurrentWhiteHP, int CurrentBlueHP, int MaxWhiteHP);

        IEnumerable<Term> GetTerms(HPSMOperation operation);

        public enum HPSMOperation
        {
            TakeDamage,
            TakeDamageSequence,
            TryFocus,
            DoFocus,
            GiveBlueHealth,
            GiveHealth,
            RestoreWhiteHealth,
            RestoreAllHealth,
            IsHPDetermined,
            DetermineHP,
            GetHPInfo,
        }
    }

    /*
     * Goal:
     *   We want to optimize for the most common use case of single hit damage with no survivability concern
     *    while trying not to compromise on correctness and completeness.
     * Model:
     *   We will have two types of states which respectively track damage lazily and strictly.
     *   A lazy state can be converted to a collection of strict states at any time.
     *   A lazy state tracks the number of single damage hits it has received.
     *   A strict state tracks the amount of blue hp damage and white hp damage it has received.
     *   On conversion from lazy to strict, overcharm status and various charm equip status are determined so that exact hp is known.
     *   Conversion is forced by:
     *     Taking any sequence of consecutive hits without pause (a scenario where we would not be able to reconstruct hiveblood history)
     *     Taking any hit of base amount greater than 1 (a scenario where we would not be able to reconstruct overcharm damage)
     *     Taking a single damage hit that would kill an overcharmed state at base max hp.
     *     External call, by any consumer that requires exact hp data.
     *   On a hit that would otherwise kill, in strict mode, all remaining survivability charms are equipped, and an attempt is made to focus exactly enough to survive.
     *     This intentionally does not explore all possible focus strategies. 
     *     Thus, it allows false negatives, where a strategy of more aggressive focus use paired with soul refills would otherwise work.
     *   There are no assumptions regarding overcharm, charm equip for a lazy state. Those can be modified freely by other consumers, without needing conversion.
     *     DecideOvercharm was implemented in the interface with the idea that potentially it would need to "rebalance" on overcharm.
     *     However, that idea was discarded so that charm equip could remain a one-to-one state operation.
     * TODO:
     *   change other state modifiers (BenchReset, HotSpringsReset, etc) to use HPSM api.
     *   write documentation and extensive tests. most likely there are still several errors in the below implementation.
    */

    public class HPStateManager : LogicVariable, IHPStateManager
    {
        public override string Name { get; }
        public const string Prefix = "$HPSM";

        public override IEnumerable<Term> GetTerms()
        {
            yield return MaskShards;
            yield return Focus;
            foreach (ILogicVariable variable in 
                (ILogicVariable[])[Hiveblood, LifebloodHeart, LifebloodCore, FragileHeart, JonisBlessing, DeepFocus, SSM])
            {
                foreach (Term t in variable.GetTerms()) yield return t;
            }
        }

        public IEnumerable<Term> GetTerms(HPSMOperation op)
        {
            switch (op)
            {
                case HPSMOperation.TryFocus:
                    return FocusCharms.SelectMany(c => c.GetTerms()).Append(Focus).Concat(SSM.GetTerms());
                case HPSMOperation.RestoreAllHealth:
                case HPSMOperation.IsHPDetermined:
                    return [];
                case HPSMOperation.TakeDamage:
                case HPSMOperation.TakeDamageSequence:
                case HPSMOperation.DoFocus:
                case HPSMOperation.GiveBlueHealth:
                case HPSMOperation.GiveHealth:
                case HPSMOperation.RestoreWhiteHealth:
                case HPSMOperation.DetermineHP:
                case HPSMOperation.GetHPInfo:
                    return GetTerms();
                default:
                    throw new NotImplementedException();
            }
        }

        protected readonly StateBool Overcharmed;
        protected readonly StateBool CannotOvercharm;
        protected readonly StateBool NoFlower;
        protected readonly StateInt SpentHP;
        protected readonly StateInt SpentBlueHP;
        protected readonly StateInt LazySpentHP;
        protected readonly Term MaskShards;
        protected readonly Term Focus;
        protected readonly EquipCharmVariable LifebloodHeart;
        protected readonly EquipCharmVariable LifebloodCore;
        protected readonly EquipCharmVariable JonisBlessing;
        protected readonly EquipCharmVariable FragileHeart;
        protected readonly EquipCharmVariable Hiveblood;
        protected readonly EquipCharmVariable DeepFocus;
        // not supported: grubsong
        protected EquipCharmVariable[] DetermineHPCharms;
        protected EquipCharmVariable[] BeforeDeathCharms;
        protected EquipCharmVariable[] FocusCharms;
        protected readonly LogicManager lm;
        protected ISoulStateManager SSM { get => field ?? (ISoulStateManager)lm.GetVariableStrict(name: SoulStateManager.Prefix); set; } = null!;

        public HPStateManager(string name, LogicManager lm)
        {
            Name = name;
            this.lm = lm;
            try
            {
                Overcharmed = lm.StateManager.GetBoolStrict("OVERCHARMED");
                CannotOvercharm = lm.StateManager.GetBoolStrict("CANNOTOVERCHARM");
                NoFlower = lm.StateManager.GetBoolStrict("NOFLOWER");
                SpentHP = lm.StateManager.GetIntStrict("SPENTHP");
                SpentBlueHP = lm.StateManager.GetIntStrict("SPENTBLUEHP");
                LazySpentHP = lm.StateManager.GetIntStrict("LAZYSPENTHP");
                MaskShards = lm.GetTermStrict("MASKSHARDS");
                Focus = lm.GetTermStrict("FOCUS");
                LifebloodHeart = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Lifeblood_Heart"));
                LifebloodCore = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Lifeblood_Core"));
                JonisBlessing = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Joni's_Blessing"));
                FragileHeart = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Fragile_Heart"));
                Hiveblood = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Hiveblood"));
                DeepFocus = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Deep_Focus"));

                DetermineHPCharms = [Hiveblood, LifebloodHeart, LifebloodCore, FragileHeart, JonisBlessing];
                FocusCharms = [DeepFocus, JonisBlessing];
                BeforeDeathCharms = [Hiveblood, LifebloodHeart, LifebloodCore, FragileHeart, JonisBlessing, DeepFocus];
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing HPStateManager", e);
            }
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (term == Prefix)
            {
                variable = new HPStateManager(term, lm);
                return true;
            }
            variable = default;
            return false;
        }

        public IEnumerable<LazyStateBuilder> GiveBlueHealth(ProgressionManager pm, LazyStateBuilder state, int amount)
        {
            if (!IsHPDetermined(state))
            {
                return DetermineHP(pm, state).SelectMany(l => GiveBlueHealth(pm, l, amount));
            }
            else
            {
                state.SetInt(SpentBlueHP, state.GetInt(SpentBlueHP) - amount);
                return [state];
            }
        }

        public IEnumerable<LazyStateBuilder> GiveHealth(ProgressionManager pm, LazyStateBuilder state, int amount)
        {
            if (!IsHPDetermined(state))
            {
                if (state.GetInt(LazySpentHP) > 0)
                {
                    return DetermineHP(pm, state).SelectMany(l => GiveHealth(pm, l, amount));
                }
                else
                {
                    return [state];
                }
            }
            else
            {
                int spentHP = state.GetInt(SpentHP);
                state.SetInt(SpentHP, Math.Max(spentHP - amount, 0));
                return [state];
            }
        }

        public IEnumerable<LazyStateBuilder> RestoreWhiteHealth(ProgressionManager pm, LazyStateBuilder state)
        {
            if (!IsHPDetermined(state))
            {
                if (state.GetInt(LazySpentHP) == 0)
                {
                    return [state];
                }
                else
                {
                    return DetermineHP(pm, state).SelectMany(l => RestoreWhiteHealth(pm, l));
                }
            }
            else
            {
                state.SetInt(SpentHP, 0);
                return [state];
            }
        }

        public IEnumerable<LazyStateBuilder> RestoreAllHealth(ProgressionManager pm, LazyStateBuilder state)
        {
            if (!IsHPDetermined(state))
            {
                state.SetInt(LazySpentHP, 0);
                return [state];
            }
            else
            {
                state.SetInt(SpentHP, 0);
                state.SetInt(SpentBlueHP, 0);
                return [state];
            }
        }

        public bool TryFocus(ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (!pm.Has(Focus) || !IsHPDetermined(state)) return false;
            foreach (EquipCharmVariable ecv in FocusCharms) if (!ecv.IsDetermined(state)) return false;
            if (!SSM.TrySpendSoul(pm, ref state, 33)) return false;
            int healAmt = GetHealAmount(pm, state);

            if (state.GetInt(SpentHP) > 0) state.SetInt(SpentHP, Math.Max(0, state.GetInt(SpentHP) - healAmt));
            return true;
        }

        public IEnumerable<LazyStateBuilder> DoFocus(ProgressionManager pm, LazyStateBuilder state, int amount)
        {
            if (!pm.Has(Focus)) return [];
            if (!IsHPDetermined(state))
            {
                return DetermineHP(pm, state).SelectMany(s => DoFocus(pm, state, amount));
            }
            if (FocusCharms.Any(c => !c.IsDetermined(state)))
            {
                return EquipCharmVariable.GenerateCharmCombinations(pm, state, FocusCharms).SelectMany(s => DoFocus(pm, s, amount));
            }
            if (!SSM.TrySpendSoul(pm, ref state, 33)) return [];

            int spentHP = state.GetInt(SpentHP);
            int healAmt = GetHealAmount(pm, state);
            state.SetInt(SpentHP, Math.Max(spentHP - amount * healAmt, 0));
            return [state];
        }

        protected int GetHealAmount(ProgressionManager pm, LazyStateBuilder state)
        {
            return JonisBlessing.IsEquipped(state) ? 0 : DeepFocus.IsEquipped(state) ? 2 : 1;
        }

        public bool IsHPDetermined(LazyStateBuilder state)
        {
            return state.GetInt(LazySpentHP) == int.MaxValue;
        }

        public IEnumerable<LazyStateBuilder> DetermineHP(ProgressionManager pm, LazyStateBuilder state)
        {
            if (IsHPDetermined(state))
            {
                return [state];
            }

            int lazySpentHP = state.GetInt(LazySpentHP);
            state.SetInt(LazySpentHP, int.MaxValue);

            IEnumerable<LazyStateBuilder> lsbs = DecideOvercharm(pm, state)
                .SelectMany(l => EquipCharmVariable.GenerateCharmCombinations(pm, l, DetermineHPCharms));

            for (int i = 0; i < lazySpentHP; i++)
            {
                lsbs = lsbs.SelectMany(l => TakeDamage(pm, l, 1));
            }
            return lsbs;
        }

        protected IEnumerable<LazyStateBuilder> DecideOvercharm(ProgressionManager pm, LazyStateBuilder state)
        {
            if (state.GetBool(CannotOvercharm) || state.GetBool(Overcharmed))
            {
                yield return state;
                yield break;
            }
            else
            {
                LazyStateBuilder oc = new(state);
                state.SetBool(CannotOvercharm, true);
                oc.SetBool(Overcharmed, true);
                yield return state;
                yield return oc;
                yield break;
            }
        }

        public StrictHPInfo GetHPInfo(ProgressionManager pm, LazyStateBuilder state)
        {
            if (!IsHPDetermined(state))
            {
                throw new InvalidOperationException($"{nameof(GetHPInfo)} called on state {pm.lm.StateManager.PrettyPrint(state)} with indeterminate hp.");
            }

            int maxHp = pm.Get(MaskShards) / 4;
            if (FragileHeart.IsEquipped(state)) maxHp += 2;
            if (JonisBlessing.IsEquipped(state)) maxHp = (int)(1.4f * maxHp);
            int hp = maxHp - state.GetInt(SpentHP);
            int blueHP = -state.GetInt(SpentBlueHP);
            if (LifebloodHeart.IsEquipped(state)) blueHP += 2;
            if (LifebloodCore.IsEquipped(state)) blueHP += 4;
            return new(hp, blueHP, maxHp);
        }

        #region Damage

        protected readonly record struct HitInfo(int Amount, int BlueHPDamage, int WhiteHPDamage, bool WaitAfterHit)
        {
            public bool Survives { get; }

            public HitInfo(StrictHPInfo info, int Amount, bool WaitAfterHit) : this(Amount,
                info.CurrentBlueHP >= Amount ? Amount : info.CurrentBlueHP,
                info.CurrentBlueHP >= Amount ? 0 : Amount,
                WaitAfterHit)
            {
                Survives = info.CurrentBlueHP >= Amount || info.CurrentWhiteHP > Amount;
            }
        }

        protected virtual void DoHit(ref LazyStateBuilder state, HitInfo hit)
        {
            if (!hit.Survives) throw new InvalidOperationException("DoHit called on lethal hit.");

            if (hit.BlueHPDamage > 0)
            {
                state.Increment(SpentBlueHP, hit.BlueHPDamage);
            }
            if (hit.WhiteHPDamage > 0)
            {
                state.Increment(SpentHP, hit.WhiteHPDamage);
            }
            if (hit.WaitAfterHit)
            {
                if (Hiveblood.IsEquipped(state) && hit.WhiteHPDamage > 0)
                {
                    state.Increment(SpentHP, -1);
                }
            }
            state.SetBool(NoFlower, true);
        }

        public IEnumerable<LazyStateBuilder> TakeDamage(ProgressionManager pm, LazyStateBuilder state, int amount)
        {
            if (!IsHPDetermined(state))
            {
                if (amount > 1 || !CanTakeNextLazyHit(pm, state))
                {
                    return DetermineHP(pm, state).SelectMany(s => TakeDamageStrict(pm, s, amount, waitAfterHit: true));
                }
                else
                {
                    state.Increment(LazySpentHP, 1);
                    return [state];
                }
            }
            else
            {
                return TakeDamageStrict(pm, state, amount, waitAfterHit: true);
            }
        }

        public IEnumerable<LazyStateBuilder> TakeDamageSequence(ProgressionManager pm, LazyStateBuilder state, params int[] amounts)
        {
            if (!IsHPDetermined(state)) return DetermineHP(pm, state).SelectMany(s => TakeDamageSequence(pm, s, amounts));

            IEnumerable<LazyStateBuilder> states = [state];

            for (int i = 0; i < amounts.Length; i++)
            {
                int amount = amounts[i];
                bool waitAfterHit = i == amounts.Length - 1;
                states = states.SelectMany(s => TakeDamageStrict(pm, s, amount, waitAfterHit));
            }
            return states;
        }

        /// <summary>
        /// Test inside lazy path for whether to stay in lazy path.
        /// </summary>
        protected bool CanTakeNextLazyHit(ProgressionManager pm, LazyStateBuilder state)
        {
            // conservatively exit lazy mode when it would kill an overcharmed state with no useful charms or healing
            // we could hypothetically check for charms or cannotOvercharmed, but this risks monotonicity issues if not done carefully

            int hitsTaken = state.GetInt(LazySpentHP);
            int maxHP = pm.Get(MaskShards) / 4;
            int totalHits = (maxHP - 1) / 2;
            return totalHits - hitsTaken > 0;
        }

        /// <summary>
        /// Standard TakeDamage path for states with determined hp.
        /// </summary>
        protected IEnumerable<LazyStateBuilder> TakeDamageStrict(ProgressionManager pm, LazyStateBuilder state, int amount, bool waitAfterHit)
        {
            bool overcharmed = state.GetBool(Overcharmed);
            int adjAmount = overcharmed ? 2 * amount : amount;

            StrictHPInfo info = GetHPInfo(pm, state);
            HitInfo hit = new(info, adjAmount, waitAfterHit);
            if (hit.Survives)
            {
                DoHit(ref state, hit);
                return [state];
            }
            else
            {
                return EquipCharmVariable.GenerateCharmCombinations(pm, state, BeforeDeathCharms)
                    .SelectMany(s => TakeDamageDesperate(pm, s, amount, waitAfterHit: true));
            }
        }

        /// <summary>
        /// TakeDamage path for states with determined hp which will die on the next hit barring focus or charm interactions.
        /// </summary>
        protected virtual IEnumerable<LazyStateBuilder> TakeDamageDesperate(ProgressionManager pm, LazyStateBuilder state, int amount, bool waitAfterHit)
        {
            StrictHPInfo info = GetHPInfo(pm, state);
            bool overcharmed = state.GetBool(Overcharmed);
            int adjAmount = overcharmed ? 2 * amount : amount;

            int deficit = adjAmount - info.CurrentWhiteHP;
            IEnumerable<LazyStateBuilder> states;
            if (deficit >= 0)
            {
                if (waitAfterHit)
                {
                    int healAmt = GetHealAmount(pm, state);
                    int healAvail = info.MaxWhiteHP - info.CurrentWhiteHP;
                    int healReq = 1 + deficit;
                    if (healAmt > 0 && healAvail >= healReq)
                    {
                        states = DoFocus(pm, state, healReq);
                    }
                    else yield break;
                }
                else yield break;
            }
            else
            {
                states = [state];
            }
            
            foreach (LazyStateBuilder lsb in states)
            {
                state = lsb;
                info = GetHPInfo(pm, state);
                HitInfo hit = new(info, adjAmount, true);
                if (!hit.Survives) continue;
                DoHit(ref state, hit);
                yield return state;
            }
        }

        #endregion
    }
}
