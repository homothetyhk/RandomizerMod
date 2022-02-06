using ItemChanger;
using Newtonsoft.Json;
using RandomizerCore;
using RandomizerMod.RandomizerData;

namespace RandomizerMod.RC
{
    public class RandoModLocation : RandoLocation
    {
        /// <summary>
        /// The LocationRequestInfo associated with the location. May be null if the location does not require modification.
        /// <br/>This field is not serialized and will be null upon reloading the game.
        /// </summary>
        [JsonIgnore] public LocationRequestInfo? info;
        /// <summary>
        /// The LocationDef associated with the location. Preferred over Data.GetLocationDef, since this preserves modified location data.
        /// <br/>This field is serialized, and is safe to use after reloading the game. May rarely be null for external locations which choose not to supply a value.
        /// </summary>
        public LocationDef LocationDef;
    }
}
