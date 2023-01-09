using ItemChanger;
using ItemChanger.Modules;
using RandomizerMod.Logging;
using RandomizerMod.Settings;
using System.Text;

namespace RandomizerMod.IC
{
    public class HelperLogModule : Module
    {
        private void PrintHelper()
        {
            if (!ready) return;

            LogManager.Write(BuildHelper(), "HelperLog.txt");
            LogManager.Write(TD.pm.Dump(), "TrackerDataPM.txt");
            LogManager.Write(TD_WSB.pm.Dump(), "TrackerDataWithoutSequenceBreaksPM.txt");
            LogManager.Write(RandomizerData.JsonUtil.Serialize(TD), "TrackerData.json");
            LogManager.Write(RandomizerData.JsonUtil.Serialize(TD_WSB), "TrackerDataWithoutSequenceBreaks.json");
        }

        private TrackerData TD => RandomizerMod.RS.TrackerData;
        private TrackerData TD_WSB => RandomizerMod.RS.TrackerDataWithoutSequenceBreaks;
        private bool ready = false;

        public override void Initialize()
        {
            RandomizerModule.OnLoadComplete += SetUpLog;
            TrackerUpdate.OnFinishedUpdate += PrintHelper;
        }

        public override void Unload()
        {
            RandomizerModule.OnLoadComplete -= SetUpLog;
            TrackerUpdate.OnFinishedUpdate -= PrintHelper;
        }

        private string GetItemPreviewName(int id)
        {
            return RandomizerModule.Items[id]
                .GetResolvedUIDef()
                .GetPreviewName();
        }

        private AbstractPlacement GetPlacement(string name)
        {
            return ItemChanger.Internal.Ref.Settings.Placements[name];
        }

        private bool IsPersistent(int id)
        {
            return RandomizerModule.Items[id]
                .GetTags<ItemChanger.Tags.IPersistenceTag>()
                .Any(t => t is not ItemChanger.Tags.PersistentItemTag { Persistence: Persistence.Single });
        }

        private void SetUpLog()
        {
            ready = true;
            PrintHelper();
        }

        private string BuildHelper()
        {
            StringBuilder sb = new();
            TrackerData td = TD;
            TrackerData tdwsb = TD_WSB;

            sb.AppendLine("UNCHECKED REACHABLE LOCATIONS");
            foreach (string s in td.uncheckedReachableLocations
                .Where(s => tdwsb.uncheckedReachableLocations.Contains(s)).OrderBy(s => s)) // alphabetical. The locations are permuted after rando, but order could still give info regarding multi loc frequencies
            {
                sb.Append(' ', 2);
                sb.AppendLine(s);
            }
            foreach (string s in td.uncheckedReachableLocations
                .Where(s => !tdwsb.uncheckedReachableLocations.Contains(s)).OrderBy(s => s))
            {
                sb.Append(' ', 2);
                sb.Append('*'); // sequence broken
                sb.AppendLine(s);
            }
            sb.AppendLine();

            var previewLookup = Enumerable.Range(0, td.ctx.itemPlacements.Count)
                .Where(i => !td.obtainedItems.Contains(i) && td.previewedLocations.Contains(td.ctx.itemPlacements[i].Location.Name))
                .ToLookup(i => td.ctx.itemPlacements[i].Location.Name);
            sb.AppendLine("PREVIEWED LOCATIONS");
            foreach (string s in td.previewedLocations)
            {
                sb.Append(' ', 2);
                sb.AppendLine(s);
                AbstractPlacement p = GetPlacement(s);
                if (p.GetTag<ItemChanger.Tags.MultiPreviewRecordTag>() is ItemChanger.Tags.MultiPreviewRecordTag mprt
                    && mprt.previewTexts != null)
                {
                    for (int i = 0; i < mprt.previewTexts.Length; i++)
                    {
                        string t = mprt.previewTexts[i];
                        if (!string.IsNullOrEmpty(t) && i < p.Items.Count && !p.Items[i].WasEverObtained())
                        {
                            sb.Append(' ', 4);
                            sb.AppendLine(t);
                        }
                    }
                }
                else if (p.GetTag<ItemChanger.Tags.PreviewRecordTag>() is ItemChanger.Tags.PreviewRecordTag prt
                    && !string.IsNullOrEmpty(prt.previewText) && !p.Items.All(i => i.WasEverObtained()))
                {
                    sb.Append(' ', 4);
                    sb.AppendLine(prt.previewText);
                }
            }
            sb.AppendLine();

            if (td.ctx.transitionPlacements?.Any() ?? false)
            {
                sb.AppendLine("UNCHECKED REACHABLE TRANSITIONS");
                foreach (string s in td.uncheckedReachableTransitions
                    .Where(s => tdwsb.uncheckedReachableTransitions.Contains(s)))
                {
                    sb.Append(' ', 2);
                    sb.AppendLine(s);
                }
                foreach (string s in td.uncheckedReachableTransitions
                    .Where(s => !tdwsb.uncheckedReachableTransitions.Contains(s)))
                {
                    sb.Append(' ', 2);
                    sb.Append('*'); // sequence broken transition
                    sb.AppendLine(s);
                }
                sb.AppendLine();

                sb.AppendLine("CHECKED TRANSITIONS");
                foreach (var kvp in td.visitedTransitions)
                {
                    sb.Append(' ', 2);
                    if (tdwsb.outOfLogicVisitedTransitions.Contains(kvp.Key)) sb.Append('*'); // sequence broken transition
                    sb.AppendLine($"{kvp.Key}  -->  {kvp.Value}");
                }
                sb.AppendLine();
            }

            List<int> persistentItems = td.obtainedItems.Where(i => IsPersistent(i)).ToList();
            if (persistentItems.Count != 0)
            {
                sb.AppendLine("RESPAWNING ITEMS");
                foreach (int i in persistentItems)
                {
                    sb.Append(' ', 2);
                    sb.Append(td.ctx.itemPlacements[i].Location.Name);
                    sb.Append(" - ");
                    sb.AppendLine(GetItemPreviewName(i));
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
