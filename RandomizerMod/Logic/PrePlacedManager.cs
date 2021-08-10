using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomizerMod.RandomizerData;

namespace RandomizerMod.Logic
{
    public class PrePlacedManager
    {
        private List<RandoPlacement> placements;
        private List<bool> tracker;
        private List<(int index, RandoPlacement placement)> temp = new List<(int, RandoPlacement)>();

        public PrePlacedManager(Settings.GenerationSettings gs, LogicManager lm)
        {
            var waypoints = lm.Waypoints.Select(w => new RandoPlacement(w.item, new RandoLocation { logic = w.logic, }));
            var vanilla = Data.GetVanillaPlacements(gs, lm);
            vanilla.AddRange(waypoints);

            placements = vanilla;
            foreach (var p in placements.Where(p => p.location.logic == null)) Console.WriteLine("Bad logic at location tied to " + p.item.name);

            this.tracker = new List<bool>(placements.Count);
            tracker.AddRange(Enumerable.Repeat(false, placements.Count));
        }

        public PrePlacedManager(List<RandoPlacement> placements)
        {
            this.placements = placements;
            this.tracker = new List<bool>(placements.Count);
            tracker.AddRange(Enumerable.Repeat(false, placements.Count));
        }

        public void Add(IEnumerable<RandoPlacement> placements)
        {
            this.placements.AddRange(placements);
            tracker.AddRange(Enumerable.Repeat(false, this.placements.Count - tracker.Count));
        }

        public void OnUpdate(ProgressionManager pm)
        {
            UpdateLoop(pm);
        }

        public void OnRemove(ProgressionManager pm)
        {
            foreach ((int index, RandoPlacement placement) in temp)
            {
                pm.RemoveInternal(placement.item);
                tracker[index] = false;
            }
            UpdateTempLoop(pm);
            for (int i = temp.Count - 1; i >= 0; i--)
            {
                if (!tracker[temp[i].index]) temp.RemoveAt(i);
            }
        }

        public void OnEndTemp(ProgressionManager pm, bool savedTemp)
        {
            if (!savedTemp)
            {
                foreach (var (index, placement) in temp)
                {
                    tracker[index] = false;
                    pm.RemoveInternal(placement.item);
                }
            }
            else
            {
                Console.WriteLine($"Unlocked vanilla/waypoints: {string.Join(", ", temp.Select(p => p.placement.item.name).ToArray())}");
            }

            temp.Clear();
        }

        public void UpdateLoop(ProgressionManager pm)
        {
            bool update;
            do
            {
                update = false;

                // forward pass
                for (int i = 0; i < placements.Count; i++)
                {
                    if (!tracker[i] && placements[i].location.CanGet(pm))
                    {
                        pm.AddInternal(placements[i].item);
                        if (pm.Temp) temp.Add((i, placements[i]));
                        tracker[i] = true;
                        update = true;
                    }
                }

                if (!update) break;
                update = false;

                // reverse pass
                for (int i = placements.Count - 1; i >= 0; i--)
                {
                    if (!tracker[i] && placements[i].location.CanGet(pm))
                    {
                        pm.AddInternal(placements[i].item);
                        if (pm.Temp) temp.Add((i, placements[i]));
                        tracker[i] = true;
                        update = true;
                    }
                }
            }
            while (update);
        }

        public void UpdateTempLoop(ProgressionManager pm)
        {
            bool update;
            do
            {
                update = false;

                // forward pass
                for (int i = 0; i < temp.Count; i++)
                {
                    var (index, placement) = temp[i];
                    if (!tracker[index] && placement.location.CanGet(pm))
                    {
                        pm.AddInternal(placement.item);
                        tracker[index] = true;
                        update = true;
                    }
                }

                if (!update) break;
                update = false;

                // reverse pass
                for (int i = temp.Count - 1; i >= 0; i--)
                {
                    var (index, placement) = temp[i];
                    if (!tracker[index] && placement.location.CanGet(pm))
                    {
                        pm.AddInternal(placement.item);
                        tracker[index] = true;
                        update = true;
                    }
                }
            }
            while (update);
        }
    }
}
