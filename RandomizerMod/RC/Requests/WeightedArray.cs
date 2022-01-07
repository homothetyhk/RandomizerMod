namespace RandomizerMod.RC
{
    public struct WeightedArray<T>
    {
        public readonly T[] values;
        public readonly double[] weights;

        public WeightedArray(T[] values, double[] weights)
        {
            this.values = values;
            this.weights = weights;
        }

        public T Next(Random rng)
        {
            if (values.Length == 1) return values[0]; // don't burn rng samples unnecessarily

            double d = rng.NextDouble();
            for (int i = 0; i < values.Length; i++)
            {
                if (weights[i] < d) return values[i];
            }
            return values[values.Length - 1];
        }
    }
}
