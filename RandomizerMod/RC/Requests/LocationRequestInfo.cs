using ItemChanger;
using Newtonsoft.Json;
using RandomizerCore;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;

namespace RandomizerMod.RC
{
    public class LocationRequestInfo
    {
        public Func<RandoFactory, RandoModLocation>? randoLocationCreator;
        public Action<RandoFactory, RandoModLocation>? onRandoLocationCreation;
        public Action<RandoPlacement>? onRandomizerFinish;
        public Func<ICFactory, RandoPlacement, AbstractPlacement>? customPlacementFetch;
        public Action<ICFactory, RandoPlacement, AbstractPlacement>? onPlacementFetch;
        public Action<ICFactory, RandoPlacement, AbstractPlacement, AbstractItem>? customAddToPlacement;
        public Func<LocationDef>? getLocationDef;

        public void AddGetLocationDefModifier(string name, Func<LocationDef, LocationDef> modifier)
        {
            Func<LocationDef> get = getLocationDef ?? (() => Data.GetLocationDef(name));
            getLocationDef = () => modifier(get());
        }

        public LocationRequestInfo Clone()
        {
            return new LocationRequestInfo
            {
                randoLocationCreator = (Func<RandoFactory, RandoModLocation>)randoLocationCreator?.Clone(),
                onRandoLocationCreation = (Action<RandoFactory, RandoModLocation>)onRandoLocationCreation?.Clone(),
                onRandomizerFinish = (Action<RandoPlacement>)onRandomizerFinish?.Clone(),
                customPlacementFetch = (Func<ICFactory, RandoPlacement, AbstractPlacement>)customPlacementFetch?.Clone(),
                onPlacementFetch = (Action<ICFactory, RandoPlacement, AbstractPlacement>)onPlacementFetch?.Clone(),
                customAddToPlacement = (Action<ICFactory, RandoPlacement, AbstractPlacement, AbstractItem>)customAddToPlacement?.Clone(),
                getLocationDef = (Func<LocationDef>)getLocationDef?.Clone(),
            };
        }

        public void AppendTo(LocationRequestInfo info)
        {
            if (randoLocationCreator != null) info.randoLocationCreator = randoLocationCreator;
            info.onRandoLocationCreation += onRandoLocationCreation;
            info.onRandomizerFinish += onRandomizerFinish;
            if (customPlacementFetch != null) info.customPlacementFetch = customPlacementFetch;
            info.onPlacementFetch += onPlacementFetch;
            info.customAddToPlacement += customAddToPlacement;
            if (getLocationDef != null) info.getLocationDef = getLocationDef;
        }
    }
}
