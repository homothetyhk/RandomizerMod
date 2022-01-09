using ItemChanger;
using ItemChanger.UIDefs;
using UnityEngine;

namespace RandomizerMod.IC
{
    /// <summary>
    /// Class for converting an arbitrary UIDef on a redundant item to a MsgUIDef which displays the amount of geo replacing the item.
    /// </summary>
    public class DupeUIDef : MsgUIDef
    {
        public static MsgUIDef Convert(int geoAmount, UIDef orig)
        {
            if (orig is MsgUIDef msgDef)
            {
                return new SplitUIDef
                {
                    preview = new BoxedString(msgDef.GetPreviewName()),
                    name = new BoxedString($"{geoAmount} ({msgDef.GetPostviewName()})"),
                    shopDesc = msgDef.shopDesc?.Clone(),
                    sprite = msgDef.sprite?.Clone(),
                };
            }
            else return new DupeUIDef(geoAmount, orig);
        }

        private DupeUIDef(int geoAmount, UIDef inner)
        {
            GeoAmount = geoAmount;
            Inner = inner;

            if (inner is null)
            {
                base.name = new BoxedString($"{GeoAmount} (Dupe)");
                base.shopDesc = new BoxedString("");
                base.sprite = new ItemChangerSprite("ShopIcons.Geo");
            }
            else
            {
                // these fields will not be accessed normally, but should still have values for compatibility
                base.name = new BoxedString($"{GeoAmount} Geo ({inner?.GetPostviewName()})");
                base.shopDesc = new BoxedString(inner?.GetShopDesc());
                base.sprite = new EmptySprite();
            }
        }

        public int GeoAmount;
        public UIDef Inner;
        public override Sprite GetSprite() => Inner is not null
            ? Inner.GetSprite()
            : base.GetSprite();
        public override string GetPreviewName() => Inner is not null
            ? Inner.GetPreviewName()
            : base.GetPreviewName();
        public override string GetPostviewName() => Inner is not null 
            ? $"{GeoAmount} Geo ({Inner?.GetPostviewName()})"
            : base.GetPostviewName();
        public override string GetShopDesc() => Inner is not null
            ? Inner.GetShopDesc()
            : base.GetShopDesc();
        public override UIDef Clone()
        {
            return new DupeUIDef(GeoAmount, Inner);
        }
    }
}
