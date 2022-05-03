using System.Text;
using ItemChanger.Modules;
using RandomizerMod.Logging;

namespace RandomizerMod.IC
{
    public class TrackerLogModule : Module
    {
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
            string line = $"ITEM OBTAINED --- {{{item}}} at {{{location}}} with id {{{id}}}{Environment.NewLine}";
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
