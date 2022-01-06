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
        [JsonIgnore] public ItemRequestInfo? info;

        public bool TryGetItemDef(out ItemDef def)
        {
            def = info?.getItemDef != null ? info.getItemDef() : Data.GetItemDef(Name);
            return def is not null;
        }
    }
}
