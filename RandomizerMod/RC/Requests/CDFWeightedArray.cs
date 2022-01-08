namespace RandomizerMod.RC
{
    public struct CDFWeightedArray<T>
    {
        public readonly T[] values;
        public readonly double[] cumulativeDensities;

        /// <summary>
        /// Creates a new CDFWeightedArray with the given values. Densities should have the same positive length as values, and its entries should be increasing between 0 and 1.
        /// </summary>
        public CDFWeightedArray(T[] values, double[] cumulativeDensities)
        {
            this.values = values;
            this.cumulativeDensities = cumulativeDensities;
        }

        public T Next(Random rng)
        {
            if (values.Length == 1) return values[0]; // don't burn rng samples unnecessarily

            double d = rng.NextDouble();
            for (int i = 0; i < values.Length; i++)
            {
                if (cumulativeDensities[i] < d) return values[i];
            }
            return values[values.Length - 1];
        }
    }
}
