using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using RandomizerMod.Settings;
using RandomizerMod.RandomizerData;

namespace RandomizerMod.Logic
{
    public class ProgressionManager
    {
        public int[] obtained;
        public LogicManager LM { get; protected set; }
        private GenerationSettings GS;
        private PrePlacedManager ppm;

        public bool Temp { get; private set; }
        private List<LogicItem> tempItems = new List<LogicItem>();

        public int Index(string item) => LM.GetIndex(item);

        public ProgressionManager(LogicManager lm, GenerationSettings gs)
        {
            this.LM = lm;
            this.GS = gs;
            obtained = new int[lm.TermCount];
            ppm = new PrePlacedManager(gs, lm);

            foreach (string setting in Data.GetApplicableLogicSettings(GS))
            {
                obtained[Index(setting)]++;
            }
            obtained[Index("GRUBS")] -= gs.GrubCostRandomizerSettings.GrubTolerance;
            obtained[Index("ESSENCE")] -= gs.EssenceCostRandomizerSettings.EssenceTolerance;

            Update();
        }

        public int Get(string id)
        {
            return obtained[Index(id)];
        }

        public void Incr(string id, int incr)
        {
            obtained[Index(id)] += incr;
        }

        public bool Has(int index)
        {
            return obtained[index] > 0;
        }

        public bool Has(int index, int threshold)
        {
            return obtained[index] >= threshold;
        }

        public void AddInternal(LogicItem item)
        {
            item.AddTo(obtained);
        }

        public void AddInternal(IEnumerable<LogicItem> items)
        {
            foreach (var item in items) item.AddTo(obtained);
        }

        public void Add(LogicItem item)
        {
            if (Temp)
            {
                tempItems.Add(item);
            }
            item.AddTo(obtained);
            Update();
        }

        public void Add(IEnumerable<LogicItem> items)
        {
            if (Temp) tempItems.AddRange(items);
            foreach (var item in items)
            {
                item.AddTo(obtained);
            }
            Update();
        }

        public void AddTemp(LogicItem item)
        {
            Temp = true;
            tempItems.Add(item);
            item.AddTo(obtained);
            Update();
        }

        public void AddTemp(IEnumerable<LogicItem> items)
        {
            Temp = true;
            tempItems.AddRange(items);
            foreach (var item in items) item.AddTo(obtained);
            Update();
        }

        public void RemoveTempItems()
        {
            Temp = false;
            foreach (LogicItem item in tempItems)
            {
                item.RemoveFrom(obtained);
            }
            tempItems.Clear();
            ppm.OnEndTemp(this, false);
        }

        public void Remove(LogicItem removeItem)
        {
            if (!tempItems.Remove(removeItem))
            {
                throw new InvalidOperationException($"Tried to remove non-temp item {removeItem.name}!");
            }
            removeItem.RemoveFrom(obtained);
            ppm.OnRemove(this);
        }

        public void RestrictTempTo(LogicItem soleItem)
        {
            foreach (var item in tempItems) item.RemoveFrom(obtained);
            soleItem.AddTo(obtained);
            tempItems.Clear();
            tempItems.Add(soleItem);
            ppm.OnRemove(this);
        }

        public void RemoveInternal(LogicItem removeItem)
        {
            removeItem.RemoveFrom(obtained);
        }

        public void SaveTempItems()
        {
            Temp = false;
            tempItems.Clear();
            ppm.OnEndTemp(this, true);
        }

        public void Update()
        {
            ppm.OnUpdate(this);
        }
    }
}
