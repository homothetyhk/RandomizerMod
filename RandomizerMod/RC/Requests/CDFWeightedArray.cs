namespace RandomizerMod.RC
{
    public struct CDFWeightedArray<T>
    {
        public readonly T[] values;
        public readonly double[] cumulativeDensities;

        /// <summary>
        /// Creates a new CDFWeightedArray with the given values. Densities should have the same positive length as values, and its entries should be increasing positive numbers, with last entry 1.
        /// </summary>
        public CDFWeightedArray(T[] values, double[] cumulativeDensities)
        {
            this.values = values;
            this.cumulativeDensities = cumulativeDensities;
        }

        /// <summary>
        /// Creates a new CDFWeightedArray with the given values.
        /// <br/> If cumulative, densities should have the same positive length as values, and its entries should be increasing positive numbers, with last entry 1.
        /// <br/> If noncumulative, densities should have the same positive length as values, and its entries should be nonnegative numbers.
        /// </summary>
        public CDFWeightedArray(T[] values, double[] densities, bool cumulative)
        {
            this.values = values;
            if (cumulative)
            {
                this.cumulativeDensities = densities;
            }
            else
            {
                this.cumulativeDensities = new double[densities.Length];
                double total = cumulativeDensities.Sum();
                double cdf = 0.0;
                for (int i = 0; i < densities.Length; i++)
                {
                    this.cumulativeDensities[i] = cdf += densities[i] / total;
                }
            }
        }

        /// <summary>
        /// Randomly selects a value from the array using the CDF weights.
        /// <br/>Chooses a random number between 0 and 1, and then returns the value with the least weight greater than the 
        /// </summary>
        public T Next(Random rng)
        {
            if (values.Length == 1) return values[0]; // don't burn rng samples unnecessarily

            double d = rng.NextDouble();
            for (int i = 0; i < values.Length; i++)
            {
                if (cumulativeDensities[i] > d) return values[i];
            }
            return values[values.Length - 1];
        }
    }
}
