using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItemChanger;

namespace RandomizerMod.IC
{
    public class RandoPlacementTag : Tag
    {
        public static event Action<VisitStateChangedEventArgs> OnRandoPlacementVisitStateChanged;
        public static event Action<AbstractPlacement, RandoPlacementTag> OnLoad;

        /// <summary>
        /// The ids of the item placements managed by this placement.
        /// </summary>
        public List<int> ids = new();

        public override void Load(object parent)
        {
            ((AbstractPlacement)parent).OnVisitStateChanged += OnVisitStateChanged;
            try
            {
                OnLoad?.Invoke((AbstractPlacement)parent, this);
            }
            catch (Exception e)
            {
                LogError($"Error invoking RandoPlacementTag.OnLoad:\n{e}");
            }
        }

        public override void Unload(object parent)
        {
            ((AbstractPlacement)parent).OnVisitStateChanged -= OnVisitStateChanged;
        }

        private void OnVisitStateChanged(VisitStateChangedEventArgs args)
        {
            OnRandoPlacementVisitStateChanged?.Invoke(args);
        }
    }
}
