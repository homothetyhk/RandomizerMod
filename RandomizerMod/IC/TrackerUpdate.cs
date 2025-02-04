using ItemChanger;
using RandomizerMod.Settings;

namespace RandomizerMod.IC
{
    public class TrackerUpdate : ItemChanger.Modules.Module
    {
        public override void Initialize()
        {
            RandoItemTag.AfterRandoItemGive += AfterRandoItemGive;
            RandoPlacementTag.OnRandoPlacementVisitStateChanged += OnRandoPlacementVisitStateChanged;
            Events.OnTransitionOverride += OnTransitionOverride;
            transitionLookup ??= TD?.ctx?.transitionPlacements?.ToDictionary(p => p.Source.Name, p => p.Target.Name) ?? [];
        }

        public override void Unload()
        {
            RandoItemTag.AfterRandoItemGive -= AfterRandoItemGive;
            RandoPlacementTag.OnRandoPlacementVisitStateChanged -= OnRandoPlacementVisitStateChanged;
            Events.OnTransitionOverride -= OnTransitionOverride;
            OnUnload?.Invoke();
        }

        public static event Action<string> OnPlacementPreviewed;
        public static event Action<string> OnPlacementCleared;
        public static event Action<int, string, string> OnItemObtained;
        public static event Action<string, string> OnTransitionVisited;
        public static event Action OnFinishedUpdate;
        public static event Action OnFoundTransitionsCleared;
        public static event Action OnPreviewsCleared;
        public static event Action OnUnload;

        private TrackerData TD => RandomizerMod.RS.TrackerData;
        private TrackerData TD_WSB => RandomizerMod.RS.TrackerDataWithoutSequenceBreaks;
        private Dictionary<string, string> transitionLookup;

        private void OnRandoPlacementVisitStateChanged(VisitStateChangedEventArgs args)
        {
            if ((args.NewFlags & VisitState.Previewed) == VisitState.Previewed)
            {
                OnPlacementPreviewed?.Invoke(args.Placement.Name);
                OnFinishedUpdate?.Invoke();
            }
        }

        private void AfterRandoItemGive(int id, ReadOnlyGiveEventArgs args)
        {
            string itemName = args.Item.name; // the name of the item that was given (not necessarily the item placed)
            string placementName = args.Placement.Name;

            OnItemObtained?.Invoke(id, itemName, placementName);
            
            if (args.Placement.GetTag<RandoPlacementTag>() is RandoPlacementTag rpt && rpt.ids.All(i => RandomizerModule.Items[i].WasEverObtained()))
            {
                OnPlacementCleared?.Invoke(placementName);
            }

            OnFinishedUpdate?.Invoke();
        }

        /// <summary>
        /// Static method intended to allow updating visited source transitions by external callers.
        /// </summary>
        public static void SendTransitionFound(Transition source)
        {
            if (ItemChangerMod.Modules.Get<TrackerUpdate>() is TrackerUpdate instance) instance.OnTransitionFound(source.ToString());
        }

        public static void ClearFoundTransitions()
        {
            OnFoundTransitionsCleared?.Invoke();
            OnFinishedUpdate?.Invoke();
        }

        public static void ClearPreviewedPlacements() 
        {
            OnPreviewsCleared?.Invoke();
            OnFinishedUpdate?.Invoke();
        }

        private void OnTransitionOverride(Transition source, Transition origTarget, ITransition newTarget)
        {
            OnTransitionFound(source.ToString());
        }

        private void OnTransitionFound(string sourceName)
        {
            if (transitionLookup.TryGetValue(sourceName, out string targetName) && !TD.HasVisited(sourceName))
            {
                OnTransitionVisited?.Invoke(sourceName, targetName);
                if (RandomizerMod.RS.GenerationSettings is GenerationSettings gs && gs.TransitionSettings.Coupled && transitionLookup.ContainsKey(targetName))
                {
                    OnTransitionVisited?.Invoke(targetName, sourceName);
                }

                OnFinishedUpdate?.Invoke();
            }
        }

    }
}
