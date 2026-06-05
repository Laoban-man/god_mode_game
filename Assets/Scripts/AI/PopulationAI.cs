using System.Collections.Generic;
using UnityEngine;
using DivineDrift.Core;
using DivineDrift.Data;
using DivineDrift.Simulation;

namespace DivineDrift.AI
{
    /// <summary>
    /// Philosophy-driven utility AI for non-player populations. Each year it scores
    /// the three macro-actions — EXPAND (into unclaimed land), BATTLE (a bordering
    /// rival), ALLY/COOPERATE (a bordering rival) — using the population's stats and
    /// philosophy, then executes the highest-scoring action via the shared systems.
    ///
    /// The player's population is skipped here; the player acts via Agents + UI.
    /// </summary>
    public class PopulationAI
    {
        private readonly GameState _state;
        private readonly ExpansionSystem _expansion;
        private readonly CombatSystem _combat;
        private readonly DiplomacySystem _diplomacy;

        public PopulationAI(GameState state, ExpansionSystem expansion,
                            CombatSystem combat, DiplomacySystem diplomacy)
        {
            _state = state;
            _expansion = expansion;
            _combat = combat;
            _diplomacy = diplomacy;
        }

        public void TickYear()
        {
            foreach (var pop in _state.AlivePopulations())
            {
                if (pop.IsPlayer) continue;
                DecideAndAct(pop);
            }
        }

        private void DecideAndAct(Population pop)
        {
            var rivals = TerritoryUtils.BorderingPopulations(_state, pop);

            float expandScore = ScoreExpand(pop);
            float battleScore = ScoreBattle(pop, rivals, out int targetToAttack);
            float allyScore = ScoreAlly(pop, rivals, out int targetToAlly);

            // Highest utility wins.
            if (expandScore >= battleScore && expandScore >= allyScore)
            {
                _expansion.TickYear(pop, budget: 1);
            }
            else if (battleScore >= allyScore && targetToAttack >= 0)
            {
                AttackBorderCellOf(pop, targetToAttack);
            }
            else if (targetToAlly >= 0)
            {
                var target = _state.GetPopulation(targetToAlly);
                _diplomacy.Propose(pop, target,
                    pop.Stats.CooperationCapacity > 1.2f ? DiplomacyState.Allied : DiplomacyState.Cooperating);
            }
        }

        // ---- Utility scoring (philosophy biases baked into stats already) ----

        private float ScoreExpand(Population pop)
        {
            // More open land + high will-to-expand => higher score.
            int openNeighbors = CountUnclaimedFrontier(pop);
            return openNeighbors * 0.5f * pop.Stats.WillToExpand;
        }

        private float ScoreBattle(Population pop, HashSet<int> rivals, out int target)
        {
            target = -1;
            float best = 0f;
            foreach (int rivalId in rivals)
            {
                var rival = _state.GetPopulation(rivalId);
                var rel = _state.GetRelation(pop.Id, rivalId);
                if (rel == DiplomacyState.Allied) continue; // don't attack allies

                // Attack appealing when we're stronger and aggressive.
                float ratio = pop.BaseStrength() / (rival.BaseStrength() + 0.001f);
                float score = ratio * pop.Stats.AttackPower;
                if (score > best) { best = score; target = rivalId; }
            }
            return best;
        }

        private float ScoreAlly(Population pop, HashSet<int> rivals, out int target)
        {
            target = -1;
            float best = 0f;
            foreach (int rivalId in rivals)
            {
                var rel = _state.GetRelation(pop.Id, rivalId);
                if (rel == DiplomacyState.Allied || rel == DiplomacyState.Cooperating) continue;
                var rival = _state.GetPopulation(rivalId);

                // Cooperation appealing when both cooperative; bonus if rival is strong (avoid losing war).
                float score = pop.Stats.CooperationCapacity
                              * (rival.BaseStrength() / (pop.BaseStrength() + 0.001f));
                if (score > best) { best = score; target = rivalId; }
            }
            return best;
        }

        // ---- Action helpers ----

        private int CountUnclaimedFrontier(Population pop)
        {
            var set = new HashSet<int>();
            foreach (int cellId in pop.FrontierCells)
                foreach (int n in _state.GetCell(cellId).Neighbors)
                    if (!_state.GetCell(n).IsOwned) set.Add(n);
            return set.Count;
        }

        private void AttackBorderCellOf(Population pop, int rivalId)
        {
            var rival = _state.GetPopulation(rivalId);
            // Find one contested cell of the rival adjacent to us, prefer best terrain for us.
            foreach (int cellId in pop.FrontierCells)
                foreach (int n in _state.GetCell(cellId).Neighbors)
                {
                    var nb = _state.GetCell(n);
                    if (nb.OwnerPopulationId == rivalId)
                    {
                        // Going to war flips relation.
                        _state.SetRelation(pop.Id, rivalId, DiplomacyState.AtWar);
                        _combat.ResolveBattleForCell(pop, rival, n);
                        return;
                    }
                }
        }
    }
}
