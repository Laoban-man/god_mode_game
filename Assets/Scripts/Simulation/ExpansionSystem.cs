using System.Collections.Generic;
using UnityEngine;
using DivineDrift.Core;
using DivineDrift.Data;

namespace DivineDrift.Simulation
{
    /// <summary>
    /// Handles peaceful expansion into UNCLAIMED adjacent cells. Contested cells
    /// (owned by another population) are routed to the CombatSystem or
    /// DiplomacySystem by the AI/agent layer, not here.
    ///
    /// Expansion pressure scales with WillToExpand and population size; terrain can
    /// slow it (forest/mountain harder to settle). New cells must keep territory
    /// contiguous (guaranteed since we only expand into neighbors).
    /// </summary>
    public class ExpansionSystem
    {
        private readonly GameState _state;

        public ExpansionSystem(GameState state) => _state = state;

        /// <summary>
        /// Try to claim up to 'budget' unclaimed frontier cells for the population
        /// this year, weighted by will-to-expand. Returns number of cells claimed.
        /// </summary>
        public int TickYear(Population pop, int budget = 1)
        {
            if (budget <= 0) return 0;

            // Probability gate from will-to-expand and population pressure.
            float pressure = pop.Stats.WillToExpand * Mathf.Log10(pop.Stats.PopulationSize + 10f);
            if (Random.value > Mathf.Clamp01(pressure * 0.15f)) return 0;

            var candidates = GatherUnclaimedFrontier(pop);
            if (candidates.Count == 0) return 0;

            // Prefer easy terrain (plains/desert settle faster than forest/mountain).
            candidates.Sort((a, b) => SettleCost(a).CompareTo(SettleCost(b)));

            int claimed = 0;
            for (int i = 0; i < candidates.Count && claimed < budget; i++)
            {
                Claim(pop, candidates[i]);
                claimed++;
            }
            if (claimed > 0) TerritoryUtils.RecomputeFrontier(_state, pop);
            return claimed;
        }

        private List<int> GatherUnclaimedFrontier(Population pop)
        {
            var set = new HashSet<int>();
            foreach (int cellId in pop.FrontierCells)
                foreach (int n in _state.GetCell(cellId).Neighbors)
                    if (!_state.GetCell(n).IsOwned) set.Add(n);
            return new List<int>(set);
        }

        private float SettleCost(int cellId)
        {
            switch (_state.GetCell(cellId).Terrain)
            {
                case TerrainType.Plains: return 1f;
                case TerrainType.Desert: return 1.4f;
                case TerrainType.Forest: return 1.8f;
                case TerrainType.Mountain: return 2.4f;
                default: return 1f;
            }
        }

        private void Claim(Population pop, int cellId)
        {
            pop.OwnedCells.Add(cellId);
            _state.GetCell(cellId).OwnerPopulationId = pop.Id;
        }
    }
}
