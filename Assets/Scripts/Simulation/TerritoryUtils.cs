using System.Collections.Generic;
using DivineDrift.Core;
using DivineDrift.Data;

namespace DivineDrift.Simulation
{
    /// <summary>
    /// Stateless helpers for territory queries: contiguity checks (a territory must
    /// remain a single connected piece), frontier recomputation, and adjacency
    /// between populations.
    /// </summary>
    public static class TerritoryUtils
    {
        /// <summary>
        /// Recompute the frontier (border) cell list for a population and update the
        /// IsFrontier flag on cells. A cell is frontier if any neighbor is unowned
        /// or owned by a different population.
        /// </summary>
        public static void RecomputeFrontier(GameState state, Population pop)
        {
            pop.FrontierCells.Clear();
            foreach (int cellId in pop.OwnedCells)
            {
                var cell = state.GetCell(cellId);
                bool frontier = false;
                foreach (int n in cell.Neighbors)
                {
                    var nb = state.GetCell(n);
                    if (nb.OwnerPopulationId != pop.Id) { frontier = true; break; }
                }
                cell.IsFrontier = frontier;
                if (frontier) pop.FrontierCells.Add(cellId);
            }
        }

        /// <summary>
        /// Returns true if removing 'cellId' from the population would keep the
        /// remaining territory contiguous. Used to forbid losing a cell that would
        /// split the realm (territory must stay in one piece).
        /// </summary>
        public static bool RemainsContiguousWithout(GameState state, Population pop, int cellId)
        {
            if (pop.OwnedCells.Count <= 1) return true;

            int start = -1;
            foreach (int c in pop.OwnedCells)
                if (c != cellId) { start = c; break; }
            if (start < 0) return true;

            var visited = new HashSet<int> { start };
            var queue = new Queue<int>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                int cur = queue.Dequeue();
                foreach (int n in state.GetCell(cur).Neighbors)
                {
                    if (n == cellId) continue;
                    if (!pop.OwnedCells.Contains(n)) continue;
                    if (visited.Add(n)) queue.Enqueue(n);
                }
            }
            return visited.Count == pop.OwnedCells.Count - 1;
        }

        /// <summary>
        /// Returns true if adding 'cellId' keeps the territory contiguous, i.e. the
        /// cell is adjacent to at least one currently-owned cell.
        /// </summary>
        public static bool IsAdjacentToTerritory(GameState state, Population pop, int cellId)
        {
            foreach (int n in state.GetCell(cellId).Neighbors)
                if (pop.OwnedCells.Contains(n)) return true;
            return false;
        }

        /// <summary>Distinct population ids that border the given population.</summary>
        public static HashSet<int> BorderingPopulations(GameState state, Population pop)
        {
            var result = new HashSet<int>();
            foreach (int cellId in pop.FrontierCells)
                foreach (int n in state.GetCell(cellId).Neighbors)
                {
                    int owner = state.GetCell(n).OwnerPopulationId;
                    if (owner >= 0 && owner != pop.Id) result.Add(owner);
                }
            return result;
        }
    }
}
