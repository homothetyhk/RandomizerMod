using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomizerCore;
using RandomizerCore.LogicItems;

namespace RandomizerMod.RC
{
    /// <summary>
    /// A wrapper for a RandoModItem to allow it to be ignored by logic.
    /// <br/>Used mainly with duplicate progression, to avoid skewing.
    /// </summary>
    public class PlaceholderItem : RandoModItem
    {
        public PlaceholderItem(RandoModItem innerItem)
        {
            this.innerItem = innerItem;
            this.onRandomizerFinish = innerItem.onRandomizerFinish;
            this.realItemCreator = innerItem.realItemCreator;
            base.item = new EmptyItem($"Placeholder-{innerItem.Name}");
        }

        private readonly RandoModItem innerItem;

        public RandoModItem Unwrap()
        {
            innerItem.Priority = Priority;
            innerItem.Placed = Placed;
            return innerItem;
        }
    }
}
