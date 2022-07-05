using ItemChanger;
using RandomizerMod.IC;
using RandomizerMod.RC;

namespace RandomizerMod.Extensions
{
    /// <summary>
    /// Extensions for accessing randomizer data from IC classes.
    /// </summary>
    public static class ICExtensions
    {
        /// <summary>
        /// Enumerates the ItemPlacements indicated by the placement's RandoPlacementTag.
        /// <br/>Returns an empty enumerable if the placement does not have a tag.
        /// </summary>
        public static IEnumerable<ItemPlacement> RandoPlacements(this AbstractPlacement placement)
        {
            if (placement.GetTag(out RandoPlacementTag tag))
            {
                return tag.ids.Select(id => RandomizerMod.RS.Context.itemPlacements[id]);
            }
            return Enumerable.Empty<ItemPlacement>();
        }

        /// <summary>
        /// Gets a RandoModLocation corresponding to the placement's RandoPlacementTag. Returns null if the placement does not have a tag.
        /// <br/>Warning: different RandoModLocations corresponding to the same placement may have different behavior due to costs.
        /// </summary>
        public static RandoModLocation? RandoLocation(this AbstractPlacement placement)
        {
            if (placement.GetTag(out RandoPlacementTag tag))
            {
                return RandomizerMod.RS.Context.itemPlacements[tag.ids.First()].Location;
            }
            return null;
        }

        /// <summary>
        /// Gets the ItemPlacement indicated by the item's RandoItemTag. Returns default if the item does not have a tag.
        /// </summary>
        public static ItemPlacement RandoPlacement(this AbstractItem item)
        {
            if (item.GetTag(out RandoItemTag tag))
            {
                return RandomizerMod.RS.Context.itemPlacements[tag.id];
            }
            return default;
        }

        /// <summary>
        /// Gets the RandoModItem corresponding to the item's RandoItemTag. Returns null if the item does not have a tag.
        /// </summary>
        public static RandoModItem? RandoItem(this AbstractItem item)
        {
            return item.RandoPlacement().Item;
        }

        /// <summary>
        /// Gets the RandoModLocation corresponding to the item's RandoItemTag. Returns null if the item does not have a tag.
        /// </summary>
        public static RandoModLocation? RandoLocation(this AbstractItem item)
        {
            return item.RandoPlacement().Location;
        }
    }
}
