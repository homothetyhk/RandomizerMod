using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;

namespace RandomizerMod.Logic
{
    public readonly struct LogicItemEffect
    {
        public LogicItemEffect(ItemEffect effect, LogicManager lm)
        {
            this.id = lm.GetIndex(effect.id);
            this.incr = effect.incr;
        }

        public LogicItemEffect(int id, int incr)
        {
            this.id = id;
            this.incr = incr;
        }

        public readonly int id;
        public readonly int incr;
    }

    public abstract class LogicItem
    {
        public string name;

        public abstract void AddTo(int[] obtained);
        public abstract void RemoveFrom(int[] obtained);
    }

    public abstract class LogicItemTemplate
    {
        public string name;

        public static LogicItemTemplate Single(string id, int incr = 1)
        {
            return new SingleItemTemplate
            {
                effect = new ItemEffect(id, incr)
            };
        }

        public static LogicItemTemplate BoolLike(params string[] ids)
        {
            return new CappedItemTemplate
            {
                cap = new ItemEffect(ids[0], 1),
                effects = ids.Select(i => new ItemEffect(i, 1)).ToArray()
            };
        }

        public static LogicItemTemplate FromEffects(params (string, int)[] effects)
        {
            return new MassItemTemplate
            {
                effects = effects.Select(p => new ItemEffect(p.Item1, p.Item2)).ToArray()
            };
        }

        public static LogicItemTemplate FromEffects(params ItemEffect[] effects)
        {
            return new MassItemTemplate
            {
                effects = effects
            };
        }

        public abstract LogicItem ToLogicItem(LogicManager lm);
        public abstract IEnumerable<string> GetItemFlags();
    }

    public sealed class SingleItem : LogicItem
    {
        public LogicItemEffect effect;

        public override void AddTo(int[] obtained)
        {
            obtained[effect.id] += effect.incr;
        }

        public override void RemoveFrom(int[] obtained)
        {
            obtained[effect.id] -= effect.incr;
        }
    }

    public sealed class SingleItemTemplate : LogicItemTemplate
    {
        public ItemEffect effect;

        public override LogicItem ToLogicItem(LogicManager lm)
        {
            return new SingleItem
            {
                name = name,
                effect = new LogicItemEffect(effect, lm),
            };
        }

        public override IEnumerable<string> GetItemFlags()
        {
            yield return effect.id;
        }
    }

    public sealed class MassItem : LogicItem
    {
        public LogicItemEffect[] effects;

        public override void AddTo(int[] obtained)
        {
            for (int i = 0; i < effects.Length; i++)
            {
                obtained[effects[i].id] += effects[i].incr;
            }
        }
        public override void RemoveFrom(int[] obtained)
        {
            for (int i = 0; i < effects.Length; i++)
            {
                obtained[effects[i].id] -= effects[i].incr;
            }
        }
    }

    public sealed class MassItemTemplate : LogicItemTemplate
    {
        public ItemEffect[] effects;

        public override LogicItem ToLogicItem(LogicManager lm)
        {
            return new MassItem
            {
                name = name,
                effects = effects.Select(e => new LogicItemEffect(e, lm)).ToArray(),
            };
        }

        public override IEnumerable<string> GetItemFlags()
        {
            return effects.Select(e => e.id);
        }
    }

    public sealed class CappedItem : LogicItem
    {
        public LogicItemEffect[] effects;
        public LogicItemEffect cap;
        bool given;


        public override void AddTo(int[] obtained)
        {
            if (obtained[cap.id] < cap.incr)
            {
                given = true;
                for (int i = 0; i < effects.Length; i++)
                {
                    obtained[effects[i].id] += effects[i].incr;
                }
            }
            else given = false;
        }
        public override void RemoveFrom(int[] obtained)
        {
            if (given)
            {
                for (int i = 0; i < effects.Length; i++)
                {
                    obtained[effects[i].id] -= effects[i].incr;
                }
            }
        }
    }

    public sealed class CappedItemTemplate : LogicItemTemplate
    {
        public ItemEffect[] effects;
        public ItemEffect cap;

        public override LogicItem ToLogicItem(LogicManager lm)
        {
            return new CappedItem
            {
                name = name,
                effects = effects.Select(e => new LogicItemEffect(e, lm)).ToArray(),
                cap = new LogicItemEffect(cap, lm),
            };
        }

        public override IEnumerable<string> GetItemFlags()
        {
            return effects.Select(e => e.id);
        }
    }

    /*
    public sealed class MultiItem : LogicItem
    {
        public LogicItemEffect[] effects;
        public int[] realIncrs;

        public MultiItem(LogicItemEffect[] effects)
        {
            this.effects = effects;
            this.realIncrs = new int[effects.Length];
        }

        public override void AddTo(int[] obtained)
        {
            for (int i = 0; i < effects.Length; i++)
            {
                obtained[effects[i].id] += realIncrs[i] = effects[i].GetRealIncr(obtained);
            }
        }
        public override void RemoveFrom(int[] obtained)
        {
            for (int i = 0; i < effects.Length; i++) obtained[effects[i].id] -= realIncrs[i];
        }
    }

    public sealed class MultiItemTemplate : LogicItemTemplate
    {
        public ItemEffect[] effects;

        public override LogicItem ToLogicItem(LogicManager lm)
        {
            return new MultiItem(effects.Select(e => new LogicItemEffect(e, lm)).ToArray());
        }
    }
    */

    public sealed class BranchedItem : LogicItem
    {
        public LogicDef logic;
        public LogicItem trueItem;
        public LogicItem falseItem;
        bool branch;

        public override void AddTo(int[] obtained)
        {
            if (logic.CanGet(obtained))
            {
                trueItem?.AddTo(obtained);
                branch = true;
            }
            else
            {
                falseItem?.AddTo(obtained);
                branch = false;
            }
        }

        public override void RemoveFrom(int[] obtained)
        {
            if (branch)
            {
                trueItem?.RemoveFrom(obtained);
            }
            else
            {
                falseItem?.RemoveFrom(obtained);
            }
        }
    }
    

    public sealed class BranchedItemTemplate : LogicItemTemplate
    {
        public string logic;
        public LogicItemTemplate trueItem;
        public LogicItemTemplate falseItem;

        public override LogicItem ToLogicItem(LogicManager lm)
        {
            return new BranchedItem
            {
                name = name,
                logic = lm.FromString(logic),
                trueItem = trueItem.ToLogicItem(lm),
                falseItem = falseItem.ToLogicItem(lm),
            };
        }

        public override IEnumerable<string> GetItemFlags()
        {
            return (trueItem?.GetItemFlags() ?? Enumerable.Empty<string>()).Concat(falseItem?.GetItemFlags() ?? Enumerable.Empty<string>());
        }
    }
}
