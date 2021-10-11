using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            Events.OnBeginSceneTransition += OnBeginSceneTransition;
            transitionInverse ??= TD.ctx.transitionPlacements?.ToDictionary(p => p.target.Name, p => p.source.Name) ?? new();
        }

        public override void Unload()
        {
            RandoItemTag.AfterRandoItemGive -= AfterRandoItemGive;
            RandoPlacementTag.OnRandoPlacementVisitStateChanged -= OnRandoPlacementVisitStateChanged;
            Events.OnBeginSceneTransition -= OnBeginSceneTransition;
        }

        public static event Action<string> OnPlacementPreviewed;
        public static event Action<string> OnPlacementCleared;
        public static event Action<int, string, string> OnItemObtained;
        public static event Action<string, string> OnTransitionVisited;
        public static event Action OnFinishedUpdate;

        private TrackerData TD => RandomizerMod.RS.TrackerData;
        private Dictionary<string, string> transitionInverse;

        private void OnRandoPlacementVisitStateChanged(VisitStateChangedEventArgs args)
        {
            if ((args.NewFlags & VisitState.Previewed) == VisitState.Previewed)
            {
                TD.previewedLocations.Add(args.Placement.Name);
                TD.uncheckedReachableLocations.Remove(args.Placement.Name);
                OnPlacementPreviewed?.Invoke(args.Placement.Name);
                OnFinishedUpdate?.Invoke();
            }
        }

        private void AfterRandoItemGive(int id, ReadOnlyGiveEventArgs args)
        {
            string itemName = args.Item.name; // the name of the item that was given (not necessarily the item placed)
            string placementName = args.Placement.Name;
            
            TD.obtainedItems.Add(id);
            TD.pm.Add(TD.ctx.itemPlacements[id].item);
            OnItemObtained?.Invoke(id, itemName, placementName);
            
            if (args.Placement.AllObtained())
            {
                TD.clearedLocations.Add(placementName);
                TD.previewedLocations.Remove(placementName);
                TD.uncheckedReachableLocations.Remove(placementName);
                OnPlacementCleared?.Invoke(placementName);
            }

            OnFinishedUpdate?.Invoke();
        }

        private void OnBeginSceneTransition(Transition t)
        {
            string target = t.ToString();
            if (transitionInverse.TryGetValue(target, out string source) && !TD.visitedTransitions.Contains(source))
            {
                TD.visitedTransitions.Add(source);
                TD.uncheckedReachableTransitions.Remove(source);
                TD.pm.Add(TD.lm.GetTransition(source));
                TD.pm.Add(TD.lm.GetTransition(target));
                OnTransitionVisited?.Invoke(source, target);

                if (RandomizerMod.RS.GenerationSettings.TransitionSettings.Coupled && transitionInverse.ContainsKey(target))
                {
                    TD.visitedTransitions.Add(target);
                    TD.uncheckedReachableTransitions.Remove(target);
                    OnTransitionVisited?.Invoke(target, source);
                }

                OnFinishedUpdate?.Invoke();
            }
        }
    }
}
