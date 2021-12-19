using ItemChanger;
using RandomizerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RandomizerMod.RC
{
    public class RandoModItem : RandoItem
    {
        [field: JsonIgnore] public Action<RandoPlacement> onRandomizerFinish;
        [field: JsonIgnore] public Func<ICFactory, RandoPlacement, AbstractItem> realItemCreator;

        public void Apply(ItemRequestInfo info)
        {
            this.onRandomizerFinish = info.onRandomizerFinish;
            this.realItemCreator = info.realItemCreator;
        }
    }
}
