using System.Text;
using ItemChanger.Modules;
using RandomizerMod.Logging;
using RandomizerMod.Settings;

namespace RandomizerMod.IC
{
    public class TrackerLogModule : Module
    {
        private string oolRevisionMessage = string.Empty;

        private void AppendToTracker(string contents)
        {
            LogManager.Append(contents, "TrackerLog.txt");
        }

        public override void Initialize()
        {
            SetUpLog();
            TrackerUpdate.OnItemObtained += TrackItemObtained;
            TrackerUpdate.OnPlacementPreviewed += TrackLocationPreviewed;
            TrackerUpdate.OnPlacementCleared += TrackLocationCleared;
            TrackerUpdate.OnTransitionVisited += TrackTransitionFound;
            TrackerUpdate.OnFinishedUpdate += TrackOOLRevisions;
        }

        public override void Unload()
        {
            TrackerUpdate.OnItemObtained -= TrackItemObtained;
            TrackerUpdate.OnPlacementPreviewed -= TrackLocationPreviewed;
            TrackerUpdate.OnPlacementCleared -= TrackLocationCleared;
            TrackerUpdate.OnTransitionVisited -= TrackTransitionFound;
        }

        private void TrackItemObtained(int id, string item, string location)
        {
            bool ool = RandomizerMod.RS.TrackerDataWithoutSequenceBreaks is TrackerData tdwsb && tdwsb.outOfLogicObtainedItems.Contains(id);
            string line = $"ITEM OBTAINED{(ool ? "*" : "")} --- {{{item}}} at {{{location}}} with id {{{id}}}{Environment.NewLine}";
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
            bool ool = RandomizerMod.RS.TrackerDataWithoutSequenceBreaks is TrackerData tdwsb && tdwsb.outOfLogicVisitedTransitions.Contains(t1);
            string line = $"TRANSITION{(ool ? "*" : "")} --- {{{t1}}} --> {{{t2}}}{Environment.NewLine}";
            AppendToTracker(line);
        }

        internal void TrackItemOOLEnd(int id, string item, string location)
        {
            string line = $"REVISED --- obtained item no longer out of logic --- {{{item}}} at {{{location}}} with id {{{id}}}{Environment.NewLine}";
            oolRevisionMessage += line;
        }

        internal void TrackTransitionOOLEnd(string t1, string t2)
        {
            string line = $"REVISED --- found transition no longer out of logic --- {{{t1}}} --> {{{t2}}}{Environment.NewLine}";
            oolRevisionMessage += line;
        }

        private void TrackOOLRevisions()
        {
            // TrackerData manages whether placements are out of logic, and broadcasts to TrackerLogModule
            // Since TrackerData hooks with RM, before IC modules, we delay logging revisions to ensure the obtained item, etc goes through first.
            // If TrackerLogModule is present, but TrackerData is not, obtains will not be marked as OOL, and revisions will not be sent.

            if (!string.IsNullOrEmpty(oolRevisionMessage))
            {
                AppendToTracker(oolRevisionMessage);
            }
            oolRevisionMessage = string.Empty;
        }

        private void SetUpLog()
        {
            if (!File.Exists(Path.Combine(LogManager.UserDirectory, "TrackerLog.txt")))
            {
                StringBuilder sb = new("Starting tracker log for new randomizer file.");
                sb.AppendLine();
                sb.AppendLine();
                AppendToTracker(sb.ToString());
            }
        }
    }
}
