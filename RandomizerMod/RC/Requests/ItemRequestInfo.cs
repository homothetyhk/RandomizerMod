using ItemChanger;
using Newtonsoft.Json;
using RandomizerCore;
using RandomizerMod.RandomizerData;

namespace RandomizerMod.RC
{
    public class ItemRequestInfo
    {
        public Func<RandoFactory, RandoModItem>? randoItemCreator;
        public Action<RandoFactory, RandoModItem>? onRandoItemCreation;
        public Action<RandoPlacement>? onRandomizerFinish;
        public Func<ICFactory, RandoPlacement, AbstractItem>? realItemCreator;
        public Func<ItemDef>? getItemDef;

        public void AddGetItemDefModifier(string name, Func<ItemDef, ItemDef> modifier)
        {
            Func<ItemDef> get = getItemDef ?? (() => Data.GetItemDef(name));
            getItemDef = () => modifier(get());
        }

        public ItemRequestInfo Clone()
        {
            return new ItemRequestInfo
            {
                randoItemCreator = (Func<RandoFactory, RandoModItem>)randoItemCreator?.Clone(),
                onRandoItemCreation = (Action<RandoFactory, RandoModItem>)onRandoItemCreation?.Clone(),
                onRandomizerFinish = (Action<RandoPlacement>)onRandomizerFinish?.Clone(),
                realItemCreator = (Func<ICFactory, RandoPlacement, AbstractItem>)realItemCreator?.Clone(),
                getItemDef = (Func<ItemDef>)getItemDef?.Clone()
            };
        }

        public void AppendTo(ItemRequestInfo info)
        {
            if (randoItemCreator != null) info.randoItemCreator = randoItemCreator;
            info.onRandoItemCreation += onRandoItemCreation;
            info.onRandomizerFinish += onRandomizerFinish;
            if (realItemCreator != null) info.realItemCreator = realItemCreator;
            if (getItemDef != null) info.getItemDef = getItemDef;
        }
    }
}
