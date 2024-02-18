using static WaveFunctionGeneration.Generation;

namespace WaveFunctionGeneration
{
    internal class Generation
    {
        private Dictionary<string, HashSet<string>> tile_map = new Dictionary<string, HashSet<string>>
        {
            { "N", new HashSet<string>{ "S", "NS", "ES", "SW", "NES", "NSW", "ESW", "NESW" } },
            { "S", new HashSet<string>{ "N", "NS", "NE", "NW", "NES", "NSW", "NEW", "NESW" } },
            { "E", new HashSet<string>{ "W", "NW", "SW", "EW", "NEW", "NSW", "ESW", "NESW" } },
            { "W", new HashSet<string>{ "E", "NE", "ES", "EW", "NES", "NEW", "ESW", "NESW" } },
            { "NE", new HashSet<string>{ "S", "W", "NS", "NW", "ES", "EW", "SW", "NES", "NEW", "NSW", "ESW", "NESW" } },
            { "NS", new HashSet<string>{ "N", "S", "NE", "NS", "NW", "ES", "SW", "NES", "NEW", "NSW", "ESW", "NESW" } },
            { "NW", new HashSet<string>{ "S", "E", "NE", "NS", "ES", "EW", "SW", "NES", "NEW", "NSW", "ESW", "NESW" } },
            { "ES", new HashSet<string>{ "N", "W", "NE", "NS", "NW", "EW", "SW", "NES", "NEW", "NSW", "ESW", "NESW" } },
            { "EW", new HashSet<string>{ "E", "W", "NE", "NW", "ES", "EW", "SW", "NES", "NEW", "NSW", "ESW", "NESW" } },
            { "SW", new HashSet<string>{ "N", "E", "NE", "NS", "NW", "ES", "EW", "NES", "NEW", "NSW", "ESW", "NESW" } },
            { "NES", new HashSet<string>{ "N", "S", "W", "NE", "NS", "NW", "ES", "EW", "SW", "NES", "NEW", "NSW", "ESW", "NESW" } },
            { "NEW", new HashSet<string>{ "S", "E", "W", "NE", "NS", "NW", "ES", "EW", "SW", "NES", "NEW", "NSW", "ESW", "NESW" } },
            { "NSW", new HashSet<string>{ "N", "S", "E", "NE", "NS", "NW", "ES", "EW", "SW", "NES", "NEW", "NSW", "ESW", "NESW" } },
            { "ESW", new HashSet<string>{ "N", "E", "W", "NE", "NS", "NW", "ES", "EW", "SW", "NES", "NEW", "NSW", "ESW", "NESW" } },
            { "NESW", new HashSet<string>{ "N", "S", "E", "W", "NE", "NS", "NW", "ES", "EW", "SW", "NES", "NEW", "NSW", "ESW", "NESW" } }
        };

        private (int, int)[] cardinals = { (-1, 0), (1, 0), (0, -1), (0, 1) };

        public enum generation_states { finished, impossible, unfinished }
        public generation_states gen_status = generation_states.unfinished;
        private HashSet<string>[,] dungeon;
        private HashSet<string> tile_types = new HashSet<string> { "N", "S", "E", "W", "NE", "NS", "NW", "ES", "EW", "SW", "NES", "NEW", "NSW", "ESW", "NESW" };
        private Random rand = new Random();
        private uint width;
        private uint height;

        public Generation(uint width, uint height, HashSet<string> tile_types)
        {
            this.width = width;
            this.height = height;
            this.tile_types = tile_types;

            dungeon = new HashSet<string>[width, height];

            for (uint w = 0; w < width; w++)
            {
                for (uint h = 0; h < height; h++)
                {
                    dungeon[w, h] = tile_types;
                }
            }

            gen_status = CheckStatus();
        }

        private generation_states CheckStatus()
        {
            for (uint w = 0; w < width; w++)
            {
                for (uint h = 0; h < height; h++)
                {
                    uint count = (uint) dungeon[w, h].Count;

                    if (count == 0)
                    {
                        return generation_states.impossible;
                    }

                    if (count > 1)
                    {
                        return generation_states.unfinished;
                    }
                }
            }

            return generation_states.finished;
        }

        private (uint, uint) FindLowestEntropy()
        {
            uint lowest_x = 0;
            uint lowest_y = 0;
            uint lowest_entropy = 99999999;

            for (uint w = 0; w < width; w++)
            {
                for (uint h = 0; h < height; h++)
                {
                    uint current_entropy = (uint) dungeon[w, h].Count;

                    if (current_entropy < lowest_entropy && current_entropy > 1)
                    {
                        lowest_entropy = current_entropy;
                        lowest_x = w;
                        lowest_y = h;
                    }
                }
            }

            Console.WriteLine("lowest entropy found");
            return (lowest_x, lowest_y);
        }

        private void CollapseWaveFunction(uint collapse_x, uint collapse_y)
        {
            ref HashSet<string> collapse_set = ref dungeon[collapse_x, collapse_y];
            collapse_set = new HashSet<string>{ collapse_set.ElementAt(rand.Next(collapse_set.Count)) };
            Console.WriteLine("function collapsed");
        }

        private int ResolveConflicts(ref HashSet<string> changed, ref HashSet<string> unchanged)
        {
            HashSet<string> new_unchanged = new HashSet<string>();
            foreach (string s in unchanged)
            {
                HashSet<string> compatibles = tile_map[s];
                if (changed.Any(x => compatibles.Contains(x)))
                {
                    new_unchanged.Add(s);
                }
            }

            unchanged = new_unchanged;
            return new_unchanged.Count;
        }

        private void PropogateCollapse(int collapse_x, int collapse_y)
        {
            Queue<(int, int)> locs = new Queue<(int, int)>();
            locs.Enqueue((collapse_x, collapse_y));

            while (locs.Count != 0)
            {
                Console.WriteLine(locs.Count);
                (int, int) cur = locs.Dequeue();
                foreach ((int, int) add in cardinals)
                {
                    (int, int) adjacent = (cur.Item1 + add.Item1, cur.Item2 + add.Item2);

                    if (adjacent.Item1 >= 0 && adjacent.Item2 >= 0 && adjacent.Item1 < width && adjacent.Item2 < height)
                    {
                        int cur_count = dungeon[adjacent.Item1, adjacent.Item2].Count;
                        int new_count = ResolveConflicts(ref dungeon[adjacent.Item1, adjacent.Item2], ref dungeon[cur.Item1, cur.Item2]);

                        if (cur_count != new_count)
                        {
                            locs.Enqueue(adjacent);
                        }
                    }
                }
            }

            Console.WriteLine("collapse propogated");
        }

        public void Generate()
        {
            int atempt_num = 0;
            while (AttemptGeneration() == false)
            {
                atempt_num++;

                dungeon = new HashSet<string>[width, height];

                for (uint w = 0; w < width; w++)
                {
                    for (uint h = 0; h < height; h++)
                    {
                        dungeon[w, h] = tile_types;
                    }
                }

                gen_status = CheckStatus();
            }
        }

        public bool AttemptGeneration()
        {
            while (gen_status == generation_states.unfinished)
            {
                (uint, uint) lowest = FindLowestEntropy();
                CollapseWaveFunction(lowest.Item1, lowest.Item2);
                PropogateCollapse((int) lowest.Item1, (int) lowest.Item2);

                gen_status = CheckStatus();
            }

            if (gen_status == generation_states.finished)
            {
                return true;
            }

            else
            {
                Console.WriteLine("failed");
                return false;
            }
        }
    }

    internal class Program
    {
        public static void Main()
        {
            Generation test = new Generation(5, 5, new HashSet<string> { "N", "S", "E", "W", "NE", "NS", "NW", "ES", "EW", "SW", "NES", "NEW", "NSW", "ESW", "NESW" });
            test.Generate();
        }
    }
}