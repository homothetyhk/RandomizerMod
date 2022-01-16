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
            transitionLookup ??= TD.ctx.transitionPlacements?.ToDictionary(p => p.source.Name, p => p.target.Name) ?? new();
            
            OnItemObtained += TD.OnItemObtained;
            OnItemObtained += TD_WSB.OnItemObtained;
            OnPlacementPreviewed += TD.OnPlacementPreviewed;
            OnPlacementPreviewed += TD_WSB.OnPlacementPreviewed;
            OnPlacementCleared += TD.OnPlacementCleared;
            OnPlacementCleared += TD_WSB.OnPlacementCleared;
            OnTransitionVisited += TD.OnTransitionVisited;
            OnTransitionVisited += TD_WSB.OnTransitionVisited;
        }

        public override void Unload()
        {
            RandoItemTag.AfterRandoItemGive -= AfterRandoItemGive;
            RandoPlacementTag.OnRandoPlacementVisitStateChanged -= OnRandoPlacementVisitStateChanged;
            Events.OnTransitionOverride -= OnTransitionOverride;

            OnItemObtained -= TD.OnItemObtained;
            OnItemObtained -= TD_WSB.OnItemObtained;
            OnPlacementPreviewed -= TD.OnPlacementPreviewed;
            OnPlacementPreviewed -= TD_WSB.OnPlacementPreviewed;
            OnPlacementCleared -= TD.OnPlacementCleared;
            OnPlacementCleared -= TD_WSB.OnPlacementCleared;
            OnTransitionVisited -= TD.OnTransitionVisited;
            OnTransitionVisited -= TD_WSB.OnTransitionVisited;
        }

        public static event Action<string> OnPlacementPreviewed;
        public static event Action<string> OnPlacementCleared;
        public static event Action<int, string, string> OnItemObtained;
        public static event Action<string, string> OnTransitionVisited;
        public static event Action OnFinishedUpdate;

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
            
            if (args.Placement.AllObtained())
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

        private void OnTransitionOverride(Transition source, Transition origTarget, ITransition newTarget)
        {
            OnTransitionFound(source.ToString());
        }

        private void OnTransitionFound(string sourceName)
        {
            if (transitionLookup.TryGetValue(sourceName, out string targetName) && !TD.HasVisited(sourceName))
            {
                OnTransitionVisited?.Invoke(sourceName, targetName);
                if (RandomizerMod.RS.GenerationSettings.TransitionSettings.Coupled && transitionLookup.ContainsKey(targetName))
                {
                    OnTransitionVisited?.Invoke(targetName, sourceName);
                }

                OnFinishedUpdate?.Invoke();
            }
        }

    }
}
