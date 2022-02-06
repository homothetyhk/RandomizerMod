using RandomizerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomizerMod.RC
{
    public readonly record struct ItemPlacement(RandoModItem Item, RandoModLocation Location)
    {
        /// <summary>
        /// The index of the item placement in the RandoModContext item placements. Initialized to -1 if the placement is not part of the ctx.
        /// </summary>
        public int Index { get; init; } = -1;

        public void Deconstruct(out RandoModItem item, out RandoModLocation location)
        {
            item = Item;
            location = Location;
        }

        public static implicit operator GeneralizedPlacement(ItemPlacement p) => new(p.Item, p.Location);
        public static explicit operator ItemPlacement(GeneralizedPlacement p) => new((RandoModItem)p.Item, (RandoModLocation)p.Location);
    }
}
