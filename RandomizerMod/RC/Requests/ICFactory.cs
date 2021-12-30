using ItemChanger;
using RandomizerCore;
using RandomizerMod.IC;
using RandomizerMod.Settings;

namespace RandomizerMod.RC
{
    public class ICFactory
    {
        public readonly RequestBuilder rb;
        public readonly GenerationSettings gs;
        private readonly Dictionary<string, AbstractPlacement> _placements;

        public ICFactory(RequestBuilder rb, Dictionary<string, AbstractPlacement> placements)
        {
            this.rb = rb;
            this.gs = rb.gs;
            _placements = placements;
        }

        public void HandlePlacement(int index, RandoModItem ri, RandoModLocation rl)
        {
            AbstractPlacement placement;
            if (rl.customPlacementFetch != null)
            {
                placement = rl.customPlacementFetch(this, new(ri, rl));
                if (placement == null) throw new NullReferenceException("Placement cannot be null after custom placement fetch!");
                if (!_placements.ContainsKey(placement.Name)) AddPlacement(placement);
            }
            else
            {
                placement = FetchOrMakePlacement(rl.Name);
            }
            rl.onPlacementFetch?.Invoke(this, new(ri, rl), placement);

            AbstractItem item;
            if (ri.realItemCreator != null)
            {
                item = ri.realItemCreator(this, new(ri, rl));
            }
            else
            {
                item = MakeItem(ri.Name);
            }
            if (item == null) throw new InvalidOperationException($"Failed to instantiate item {ri.Name} at {rl.Name}.");

            if (rl.customAddToPlacement != null)
            {
                rl.customAddToPlacement(this, new(ri, rl), placement, item);
                if (item.GetTag<RandoItemTag>() is not RandoItemTag tag || tag.id != index) item.AddTag<RandoItemTag>().id = index;
            }
            else
            {
                if (rl.costs != null) CostConversion.HandleCosts(rl.costs, item, placement);
                item.AddTag<RandoItemTag>().id = index;
                placement.Add(item);
            }
        }

        /// <summary>
        /// Makes the item by name using the info in the RequestBuilder, or else the default method.
        /// <br/>This does not consider the events attached to the RandoModItem, if any.
        /// </summary>
        public AbstractItem MakeItemWithEvents(string itemName, RandoPlacement placement)
        {
            if (rb.TryGetItemDef(itemName, out ItemRequestInfo info) && info.realItemCreator != null)
            {
                return info.realItemCreator(this, placement);
            }
            else
            {
                return MakeItem(itemName);
            }
        }

        /// <summary>
        /// Makes the item by name, using ItemChanger.Finder.
        /// </summary>
        public AbstractItem MakeItem(string name)
        {
            AbstractItem item = Finder.GetItem(name);
            if (item == null) throw new ArgumentException($"Item {name} did not correspond to any ItemChanger item!");
            return item;
        }

        public AbstractLocation MakeLocation(string name)
        {
            AbstractLocation location = Finder.GetLocation(name);
            if (location == null) throw new ArgumentException($"Location {name} did not correspond to any ItemChanger location!");
            return location;
        }

        /// <summary>
        /// Looks up the placement by name. If the placement has not yet been added, does nothing and returns false.
        /// </summary>
        public bool TryFetchPlacement(string name, out AbstractPlacement placement)
        {
            return _placements.TryGetValue(name, out placement);
        }

        /// <summary>
        /// Looks up the placement by name. If the placmeent has not yet been added, makes and adds the placement using the default procedure.
        /// </summary>
        public AbstractPlacement FetchOrMakePlacement(string name)
        {
            if (_placements.TryGetValue(name, out AbstractPlacement placement)) return placement;
            return MakePlacement(name);
        }

        public AbstractPlacement FetchOrMakePlacementWithEvents(string placementName, RandoPlacement next)
        {
            bool hasInfo = rb.TryGetLocationDef(placementName, out LocationRequestInfo info);

            AbstractPlacement placement;
            if (hasInfo && info.customPlacementFetch != null)
            {
                placement = info.customPlacementFetch(this, next);
            }
            else
            {
                placement = FetchOrMakePlacement(placementName);
            }
            if (hasInfo && info.onPlacementFetch != null)
            {
                info.onPlacementFetch?.Invoke(this, next, placement);
            }
            return placement;
        }

        private AbstractPlacement MakePlacement(string name)
        {
            if (_placements.ContainsKey(name)) throw new ArgumentException($"Placement {name} already exists!");

            AbstractPlacement placement = MakeLocation(name).Wrap();
            AddPlacement(placement);
            return placement;
        }

        public void AddPlacement(AbstractPlacement placement)
        {
            if (_placements.TryGetValue(placement.Name, out AbstractPlacement p2))
            {
                if (p2 != placement) throw new ArgumentException($"Placement {placement.Name} already exists!");
            }
            else
            {
                _placements.Add(placement.Name, placement);
                placement.AddTag<RandoPlacementTag>();
            }
        }

        // TODO: how to handle rando item tag?
        public void AddToPlacement(RandoPlacement next, AbstractItem item, AbstractPlacement placement)
        {
            RandoLocation rl = (RandoLocation)next.Location;
            if (rl.costs != null)
            {
                CostConversion.HandleCosts(rl.costs, item, placement);
            }
            placement.Add(item);
        }
    }
}
