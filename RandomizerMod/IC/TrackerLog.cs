using System.Text;
using ItemChanger;
using ItemChanger.Modules;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerMod.Logging;
using RandomizerMod.RC;
using RandomizerMod.Settings;

namespace RandomizerMod.IC
{
    public class TrackerLog : Module
    {
        private void PrintHelper()
        {
            LogManager.Write(BuildHelper(), "HelperLog.txt");
        }

        private void AppendToTracker(string contents)
        {
            LogManager.Append(contents, "TrackerLog.txt");
        }

        private TrackerData TD => RandomizerMod.RS.TrackerData;

        public override void Initialize()
        {
            SetUpLog();
            TrackerUpdate.OnItemObtained += TrackItemObtained;
            TrackerUpdate.OnPlacementPreviewed += TrackLocationPreviewed;
            TrackerUpdate.OnPlacementCleared += TrackLocationCleared;
            TrackerUpdate.OnTransitionVisited += TrackTransitionFound;
            TrackerUpdate.OnFinishedUpdate += PrintHelper;
        }

        public override void Unload()
        {
            TrackerUpdate.OnItemObtained -= TrackItemObtained;
            TrackerUpdate.OnPlacementPreviewed -= TrackLocationPreviewed;
            TrackerUpdate.OnPlacementCleared -= TrackLocationCleared;
            TrackerUpdate.OnTransitionVisited -= TrackTransitionFound;
            TrackerUpdate.OnFinishedUpdate -= PrintHelper;
        }

        private void TrackItemObtained(int id, string item, string location) 
        {
            string line = $"ITEM OBTAINED --- {{{item}}} at {{{location}}}{Environment.NewLine}";
            AppendToTracker(line);
        }

        private void TrackLocationPreviewed(string location)
        {
            string line = $"LOCATION PREVIEWED --- {{{location}}}{Environment.NewLine}";
            AppendToTracker(line);
        }

        private void TrackLocationCleared(string location)
        {
            string line = $"LOCATION CLEARED --- {{{location}}}{Environment.NewLine}";
            AppendToTracker(line);
        }

        private void TrackTransitionFound(string t1, string t2)
        {
            string line = $"TRANSITION --- {{{t1}}} --> {{{t2}}}{Environment.NewLine}";
            AppendToTracker(line);
        }

        private string GetItemPreviewName(int id, string placement)
        {
            // very good and normal code to print item names
            return ItemChanger.Internal.Ref.Settings.Placements[placement]
                .Items
                .First(i => i.GetTag<RandoItemTag>()?.id == id)
                .GetResolvedUIDef()
                .GetPreviewName();
        }

        private bool IsPersistent(int id)
        {
            return ItemChanger.Internal.Ref.Settings.Placements[TD.ctx.itemPlacements[id].location.Name]
                .Items
                .First(i => i.GetTag<RandoItemTag>()?.id == id)
                .GetTags<ItemChanger.Tags.PersistentItemTag>()
                .Any(t => t.Persistence != Persistence.Single);
        }

        private void SetUpLog()
        {
            if (!File.Exists(Path.Combine(LogManager.UserDirectory, "TrackerLog.txt")))
            {
                StringBuilder sb = new("Starting tracker log for new randomizer file.");
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine(RandomizerData.JsonUtil.Serialize(RandomizerMod.RS.GenerationSettings));
                sb.AppendLine();
                AppendToTracker(sb.ToString());
            }

            PrintHelper();
        }

        private string BuildHelper()
        {
            StringBuilder sb = new();

            sb.AppendLine("UNCHECKED REACHABLE LOCATIONS");
            foreach (string s in TD.uncheckedReachableLocations.OrderBy(s => s)) // alphabetical. The locations are permuted after rando, but order could still give info regarding multi loc frequencies
            {
                sb.Append(' ', 2);
                sb.AppendLine(s);
            }
            sb.AppendLine();


            var previewLookup = Enumerable.Range(0, TD.ctx.itemPlacements.Count)
                .Where(i => !TD.obtainedItems.Contains(i) && TD.previewedLocations.Contains(TD.ctx.itemPlacements[i].location.Name))
                .ToLookup(i => TD.ctx.itemPlacements[i].location.Name);
            sb.AppendLine("PREVIEWED LOCATIONS");
            foreach (string s in TD.previewedLocations)
            {
                sb.Append(' ', 2);
                sb.AppendLine(s);

                foreach (int i in previewLookup[s])
                {
                    (RandoItem ri, RandoLocation rl) = TD.ctx.itemPlacements[i];
                    sb.Append(' ', 4);
                    sb.Append(GetItemPreviewName(i, rl.Name));

                    if (rl.costs?.Any() ?? false)
                    {
                        sb.Append(' ', 4);
                        sb.Append(string.Join(", ", rl.costs.Select(c => "{Requires: " + c.ToString().Split('{').Last())));
                    }
                    sb.AppendLine();
                }
            }
            sb.AppendLine();

            if (TD.ctx.transitionPlacements?.Any() ?? false)
            {
                sb.AppendLine("UNCHECKED REACHABLE TRANSITIONS");
                foreach (string s in TD.uncheckedReachableTransitions)
                {
                    sb.Append(' ', 2);
                    sb.AppendLine(s);
                }
                sb.AppendLine();
            }

            List<int> persistentItems = TD.obtainedItems.Where(i => IsPersistent(i)).ToList();
            if (persistentItems.Count != 0)
            {
                sb.AppendLine("RESPAWNING ITEMS");
                foreach (int i in persistentItems)
                {
                    sb.Append(' ', 2);
                    sb.Append(TD.ctx.itemPlacements[i].location.Name);
                    sb.Append(" - ");
                    sb.AppendLine(GetItemPreviewName(i, TD.ctx.itemPlacements[i].location.Name));
                }
                sb.AppendLine();
            }

            sb.AppendLine("CURRENT PROGRESSION (INCLUDING REACHABLE VANILLA ITEMS)");
            sb.AppendLine(TD.pm.ToString());

            return sb.ToString();
        }
    }
}
