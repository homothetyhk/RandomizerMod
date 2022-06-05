namespace RandomizerMod.RC
{
    public static class CharmNotchCosts
    {
        public static int[] _vanillaCosts;

        public static int GetVanillaCost(int id) => _vanillaCosts[id - 1];

        public static int[] GetUniformlyRandomCosts(Random rng, int min, int max)
        {
            if (max > 240) throw new ArgumentOutOfRangeException(nameof(max));
            if (min < 0 || min > max) throw new ArgumentOutOfRangeException(nameof(min));

            int count = rng.Next(min, max + 1);
            int[] costs = new int[40];

            for (int i = 0; i < count; i++)
            {
                int index;
                do
                {
                    index = rng.Next(40);
                }
                while (costs[index] >= 6);

                costs[index]++;
            }

            return costs;
        }


        static CharmNotchCosts()
        {
            _vanillaCosts = new int[]
            {
                1,
                1,
                1,
                2,
                2,
                2,
                3,
                2,
                3,
                1,
                3,
                1,
                3,
                1,
                2,
                2,
                1,
                2,
                3,
                2,
                4,
                2,
                2,
                2,
                3,
                1,
                4,
                2,
                4,
                1,
                2,
                3,
                2,
                4,
                3,
                5,
                1,
                3,
                2,
                2
            };

        }
    }
}
