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
        public const string Prefix = "Placeholder-";

        public PlaceholderItem(RandoModItem innerItem, bool wrapped = true)
        {
            this.innerItem = innerItem;
            this.info = innerItem.info?.Clone() ?? new();
            this.info.realItemCreator ??= ((factory, next) => factory.MakeItemWithEvents(innerItem.Name, next));
            if (wrapped) Wrap();
            else Unwrap();
        }

        /// <summary>
        /// Hides the innerItem of the PlaceholderItem from logic.
        /// </summary>
        public void Wrap()
        {
            wrapped = true;
            item = new EmptyItem($"Placeholder-{innerItem.Name}");
        }

        /// <summary>
        /// Makes the innerItem of the PlaceholderItem visible to logic.
        /// </summary>
        public void Unwrap()
        {
            wrapped = false;
            item = innerItem.item;
        }

        public readonly RandoModItem innerItem;
        public bool wrapped;
    }
}
