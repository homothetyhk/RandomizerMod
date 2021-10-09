using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItemChanger;

namespace RandomizerMod.IC
{
    public class RandoItemTag : Tag
    {
        public static event Action<int, ReadOnlyGiveEventArgs> AfterRandoItemGive;
        public int id;
        public bool obtained = false;

        public override void Load(object parent)
        {
            ((AbstractItem)parent).AfterGive += Broadcast;
        }

        public override void Unload(object parent)
        {
            ((AbstractItem)parent).AfterGive -= Broadcast;
        }

        private void Broadcast(ReadOnlyGiveEventArgs args)
        {
            if (!obtained) AfterRandoItemGive?.Invoke(id, args);
            obtained = true;
        }
    }
}
