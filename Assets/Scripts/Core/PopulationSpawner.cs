using System.Collections.Generic;
using UnityEngine;
using DivineDrift.Config;
using DivineDrift.Data;
using DivineDrift.Simulation;

namespace DivineDrift.Core
{
    /// <summary>
    /// Places the player and AI populations in their own corners of the planet at
    /// game start. Each population seeds on a well-separated cell and grabs a small
    /// contiguous starting territory. Most of the planet is left unclaimed so there
    /// is room to expand (per the brief).
    /// </summary>
    public static class PopulationSpawner
    {
        private static readonly Color[] PastelBanners =
        {
            new Color(0.95f, 0.70f, 0.72f), // rose
            new Color(0.70f, 0.82f, 0.95f), // sky
            new Color(0.78f, 0.92f, 0.74f), // mint
            new Color(0.95f, 0.88f, 0.68f), // butter
            new Color(0.86f, 0.74f, 0.95f), // lavender
            new Color(0.72f, 0.93f, 0.90f), // aqua
            new Color(0.96f, 0.80f, 0.66f), // peach
            new Color(0.80f, 0.80f, 0.86f), // slate
        };

        public static void Spawn(GameState state, GameConfig config, Philosophy playerPhilosophy)
        {
            int total = config.aiPopulationCount + 1;
            var seeds = PickSeparatedSeeds(state, total, config.randomSeed);

            for (int i = 0; i < total; i++)
            {
                bool isPlayer = (i == 0);
                var philosophy = isPlayer ? playerPhilosophy : RandomPhilosophy(config.randomSeed + i);

                var pop = new Population
                {
                    Id = i,
                    Name = isPlayer ? "Your People" : $"Population {i}",
                    BannerColor = PastelBanners[i % PastelBanners.Length],
                    IsPlayer = isPlayer,
                    Philosophy = philosophy,
                    Era = config.startingEra,
                    Stats = PopulationStats.Default
                };

                var def = config.GetPhilosophy(philosophy);
                if (def != null) pop.Stats.ApplyPhilosophyProfile(def.ToProfile());

                GrowStartingTerritory(state, pop, seeds[i], config.startingTerritorySize);
                TerritoryUtils.RecomputeFrontier(state, pop);

                state.Populations[i] = pop;
                if (isPlayer) state.PlayerPopulationId = i;
            }
        }

        private static Philosophy RandomPhilosophy(int seed)
        {
            var rng = new System.Random(seed);
            return (Philosophy)rng.Next(0, 3);
        }

        /// <summary>
        /// Greedily choose seed cells that are far apart (max-min angular distance),
        /// so populations start in separate corners.
        /// </summary>
        private static List<int> PickSeparatedSeeds(GameState state, int count, int seed)
        {
            var rng = new System.Random(seed);
            var seeds = new List<int> { rng.Next(0, state.Cells.Length) };

            while (seeds.Count < count)
            {
                int bestCell = -1;
                float bestMinDist = -1f;
                // Sample a subset for performance with thousands of cells.
                for (int s = 0; s < 512; s++)
                {
                    int candidate = rng.Next(0, state.Cells.Length);
                    if (state.Cells[candidate].IsOwned) continue;
                    float minDist = float.MaxValue;
                    foreach (int existing in seeds)
                    {
                        float d = 1f - Vector3.Dot(state.Cells[candidate].Center,
                                                   state.Cells[existing].Center);
                        if (d < minDist) minDist = d;
                    }
                    if (minDist > bestMinDist) { bestMinDist = minDist; bestCell = candidate; }
                }
                seeds.Add(bestCell >= 0 ? bestCell : rng.Next(0, state.Cells.Length));
            }
            return seeds;
        }

        private static void GrowStartingTerritory(GameState state, Population pop, int seedCell, int size)
        {
            var frontier = new Queue<int>();
            Claim(state, pop, seedCell);
            frontier.Enqueue(seedCell);

            while (pop.OwnedCells.Count < size && frontier.Count > 0)
            {
                int cur = frontier.Dequeue();
                foreach (int n in state.GetCell(cur).Neighbors)
                {
                    if (pop.OwnedCells.Count >= size) break;
                    if (!state.GetCell(n).IsOwned)
                    {
                        Claim(state, pop, n);
                        frontier.Enqueue(n);
                    }
                }
            }
        }

        private static void Claim(GameState state, Population pop, int cellId)
        {
            if (state.GetCell(cellId).IsOwned) return;
            pop.OwnedCells.Add(cellId);
            state.GetCell(cellId).OwnerPopulationId = pop.Id;
        }
    }
}
