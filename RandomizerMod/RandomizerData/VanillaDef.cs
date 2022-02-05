namespace RandomizerMod.RandomizerData
{
    public record VanillaDef(string Item, string Location, CostDef[]? Costs = null)
    {
        public virtual bool Equals(VanillaDef other)
        {
            return other != null && Item == other.Item && Location == other.Location &&
                (Costs == other.Costs || (Costs != null && other.Costs != null && Costs.SequenceEqual(other.Costs)));
        }

        public override int GetHashCode()
        {
            return Item.GetHashCode() ^ Location.GetHashCode() + (Costs != null ? Costs.Length : -1);
        }
    }
}
