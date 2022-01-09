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
            LogManager.Write(BuildHelper(), "HelperLog.txt");
            LogManager.Write(TD.pm.ToString(), "TrackerDataPM.txt");
            LogManager.Write(TD_WSB.pm.ToString(), "TrackerDataWithoutSequenceBreaksPM.txt");
        }

        private TrackerData TD => RandomizerMod.RS.TrackerData;
        private TrackerData TD_WSB => RandomizerMod.RS.TrackerDataWithoutSequenceBreaks;

        public override void Initialize()
        {
            SetUpLog();
            TrackerUpdate.OnFinishedUpdate += PrintHelper;
        }

        public override void Unload()
        {
            TrackerUpdate.OnFinishedUpdate -= PrintHelper;
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

        private AbstractPlacement GetPlacement(string name)
        {
            return ItemChanger.Internal.Ref.Settings.Placements[name];
        }

        private bool IsPersistent(int id)
        {
            return ItemChanger.Internal.Ref.Settings.Placements[TD.ctx.itemPlacements[id].location.Name]
                .Items
                .First(i => i.GetTag<RandoItemTag>()?.id == id)
                .GetTags<ItemChanger.Tags.IPersistenceTag>()
                .Any(t => t is not ItemChanger.Tags.PersistentItemTag { Persistence: Persistence.Single });
        }

        private void SetUpLog()
        {
            PrintHelper();
        }

        private string BuildHelper()
        {
            StringBuilder sb = new();
            TrackerData td = TD;
            TrackerData tdwsb = TD_WSB;


            sb.AppendLine("UNCHECKED REACHABLE LOCATIONS");
            foreach (string s in TD.uncheckedReachableLocations
                .Where(s => tdwsb.uncheckedReachableLocations.Contains(s)).OrderBy(s => s)) // alphabetical. The locations are permuted after rando, but order could still give info regarding multi loc frequencies
            {
                sb.Append(' ', 2);
                sb.AppendLine(s);
            }
            foreach (string s in TD.uncheckedReachableLocations
                .Where(s => !tdwsb.uncheckedReachableLocations.Contains(s)).OrderBy(s => s))
            {
                sb.Append(' ', 2);
                sb.Append('*'); // sequence broken
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
                    && !string.IsNullOrEmpty(prt.previewText))
                {
                    sb.Append(' ', 4);
                    sb.AppendLine(prt.previewText);
                }
            }
            sb.AppendLine();

            if (TD.ctx.transitionPlacements?.Any() ?? false)
            {
                sb.AppendLine("UNCHECKED REACHABLE TRANSITIONS");
                foreach (string s in TD.uncheckedReachableTransitions
                    .Where(s => TD_WSB.uncheckedReachableTransitions.Contains(s)))
                {
                    sb.Append(' ', 2);
                    sb.AppendLine(s);
                }
                foreach (string s in TD.uncheckedReachableTransitions
                    .Where(s => !TD_WSB.uncheckedReachableTransitions.Contains(s)))
                {
                    sb.Append(' ', 2);
                    sb.Append('*'); // sequence broken transition
                    sb.AppendLine(s);
                }
                sb.AppendLine();

                sb.AppendLine("CHECKED TRANSITIONS");
                foreach (var kvp in TD.visitedTransitions)
                {
                    sb.Append(' ', 2);
                    if (TD_WSB.outOfLogicVisitedTransitions.Contains(kvp.Key)) sb.Append('*'); // sequence broken transition
                    sb.AppendLine($"{kvp.Key}  -->  {kvp.Value}");
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

            return sb.ToString();
        }
    }
}
