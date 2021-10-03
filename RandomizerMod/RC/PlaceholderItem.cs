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
    /// A wrapper for a RandoItem to allow it to be ignored by logic.
    /// <br/>Used mainly with duplicate progression, to avoid skewing 
    /// </summary>
    public class PlaceholderItem : RandoItem
    {
        public PlaceholderItem(RandoItem innerItem)
        {
            this.innerItem = innerItem;
            base.item = new EmptyItem($"Placeholder-{innerItem.Name}");
        }

        private readonly RandoItem innerItem;

        public RandoItem Unwrap()
        {
            innerItem.priority = priority;
            return innerItem;
        }
    }
}
