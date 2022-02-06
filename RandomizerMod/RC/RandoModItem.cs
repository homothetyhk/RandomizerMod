using ItemChanger;
using RandomizerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RandomizerMod.RandomizerData;

namespace RandomizerMod.RC
{
    public class RandoModItem : RandoItem
    {
        /// <summary>
        /// The ItemRequestInfo associated with the item. May be null if the item does not require modification.
        /// <br/>This field is not serialized and will be null upon reloading the game.
        /// </summary>
        [JsonIgnore] public ItemRequestInfo? info;
        /// <summary>
        /// The ItemDef associated with the location. Preferred over Data.GetItemDef, since this preserves modified item data.
        /// <br/>This field is serialized, and is safe to use after reloading the game. May rarely be null for external items which choose not to supply a value.
        /// </summary>
        public ItemDef ItemDef;
    }
}
