using UnityEngine;
using DivineDrift.Core;
using DivineDrift.Data;

namespace DivineDrift.Simulation
{
    /// <summary>
    /// Resolves battles over contested frontier cells. Strength is derived from the
    /// four stat sources (population size, tech, religious fervour, terrain) and
    /// reduced by border perimeter length. The winner takes the contested cell; if
    /// a population loses all cells it is eliminated and the conqueror absorbs it.
    ///
    /// Territory contiguity is enforced: a defender never loses a cell whose loss
    /// would split its realm unless it is the only remaining option.
    /// </summary>
    public class CombatSystem
    {
        private readonly GameState _state;

        public CombatSystem(GameState state) => _state = state;

        /// <summary>
        /// Attempt to capture a single contested cell. Returns the outcome.
        /// 'cellId' must be owned by defender and adjacent to attacker territory.
        /// </summary>
        public BattleResult ResolveBattleForCell(Population attacker, Population defender, int cellId)
        {
            var cell = _state.GetCell(cellId);
            if (cell == null || cell.OwnerPopulationId != defender.Id)
                return BattleResult.Stalemate;

            float atk = attacker.EffectiveStrengthForCell(cell);
            // Defender benefits fully from terrain (it holds the ground).
            float def = defender.EffectiveStrengthForCell(cell) * 1.15f; // defender's advantage

            // Protect contiguity: defender will not cede a cell that splits it,
            // unless it's their last cell.
            bool wouldSplit = !TerritoryUtils.RemainsContiguousWithout(_state, defender, cellId)
                              && defender.OwnedCells.Count > 1;

            float roll = Random.value; // stochastic edge to avoid deterministic stalemates
            float pAttacker = atk / (atk + def + 0.0001f);

            if (!wouldSplit && roll < pAttacker)
            {
                TransferCell(attacker, defender, cellId);
                AftermathCheck(defender, attacker);
                return BattleResult.AttackerWins;
            }
            return BattleResult.DefenderWins;
        }

        private void TransferCell(Population attacker, Population defender, int cellId)
        {
            defender.OwnedCells.Remove(cellId);
            attacker.OwnedCells.Add(cellId);
            _state.GetCell(cellId).OwnerPopulationId = attacker.Id;

            // Spoils: a fraction of the cell's population transfers to the victor.
            float spoils = defender.Stats.PopulationSize * 0.05f;
            defender.Stats.PopulationSize -= spoils;
            attacker.Stats.PopulationSize += spoils * 0.5f;

            TerritoryUtils.RecomputeFrontier(_state, attacker);
            TerritoryUtils.RecomputeFrontier(_state, defender);
        }

        /// <summary>
        /// If the defender was wiped out, the attacker absorbs it fully and any
        /// diplomacy/agent state is cleared.
        /// </summary>
        private void AftermathCheck(Population defender, Population attacker)
        {
            if (defender.OwnedCells.Count == 0)
            {
                defender.ActiveAgent = null;
                Debug.Log($"[Combat] {defender.Name} eliminated by {attacker.Name}.");
                // WinConditionSystem will pick up the elimination on its next pass.
            }
        }
    }
}
