using DivineDrift.Config;
using DivineDrift.Core;
using DivineDrift.Data;

namespace DivineDrift.Simulation
{
    /// <summary>
    /// Per-year internal growth: population increase, research accumulation, tech
    /// unlocking, era advancement, and applying the active agent's perk deltas.
    /// Does NOT move borders or fight — those are ExpansionSystem / CombatSystem.
    /// </summary>
    public class GrowthSystem
    {
        private readonly GameConfig _config;
        private readonly GameState _state;

        public GrowthSystem(GameConfig config, GameState state)
        {
            _config = config;
            _state = state;
        }

        public void TickYear()
        {
            foreach (var pop in _state.AlivePopulations())
            {
                ApplyActiveAgent(pop);
                Grow(pop);
                Research(pop);
                TryAdvanceEra(pop);
            }
        }

        private void ApplyActiveAgent(Population pop)
        {
            var agent = pop.ActiveAgent;
            if (agent == null || !agent.IsAlive) return;

            // Perk deltas are applied as a small per-year nudge while the agent lives.
            foreach (var perk in agent.Perks)
            {
                pop.Stats.AttackPower += perk.DeltaAttackPower * 0.05f;
                pop.Stats.WillToExpand += perk.DeltaWillToExpand * 0.05f;
                pop.Stats.CooperationCapacity += perk.DeltaCooperation * 0.05f;
                pop.Stats.ResearchRate += perk.DeltaResearchRate * 0.05f;
                pop.Stats.ReligiousFervour += perk.DeltaReligiousFervour * 0.05f;
            }
            pop.Stats = pop.Stats.Clamped();
        }

        private void Grow(Population pop)
        {
            // Logistic-ish growth proportional to territory size (carrying capacity).
            float capacity = pop.OwnedCells.Count * 200f;
            float growthRate = 0.03f;
            pop.Stats.PopulationSize +=
                growthRate * pop.Stats.PopulationSize * (1f - pop.Stats.PopulationSize / (capacity + 1f));
            pop.Stats = pop.Stats.Clamped();
        }

        private void Research(Population pop)
        {
            float gain = pop.Stats.ResearchRate * (1f + pop.Stats.TechnologicalDevelopment * 0.05f);
            pop.Tech.AccumulatedResearch += gain;
            pop.Stats.TechnologicalDevelopment += gain * 0.01f;

            // Attempt to unlock any now-available node (cheapest first).
            if (_config.techTree == null) return;
            TechNode best = null;
            foreach (var node in _config.techTree.AvailableNodes(pop.Tech, pop.Era))
                if (best == null || node.Cost < best.Cost) best = node;

            if (best != null)
                UnlockNode(pop, best);
        }

        private void UnlockNode(Population pop, TechNode node)
        {
            pop.Tech.UnlockedNodeIds.Add(node.Id);
            pop.Tech.AccumulatedResearch -= node.Cost;

            pop.Stats.AttackPower += node.DeltaAttackPower;
            pop.Stats.WillToExpand += node.DeltaWillToExpand;
            pop.Stats.CooperationCapacity += node.DeltaCooperation;
            pop.Stats.ResearchRate += node.DeltaResearchRate;
            pop.Stats.ReligiousFervour += node.DeltaReligiousFervour;
            pop.Stats = pop.Stats.Clamped();
            // UnlocksAgentPerks become available in the agent UI (checked there).
        }

        private void TryAdvanceEra(Population pop)
        {
            // Simple model: enough tech development pushes the population to the next era.
            float[] thresholds = { 0, 10, 25, 50, 90, 150, 230 }; // index = Era
            int next = (int)pop.Era + 1;
            if (next < thresholds.Length &&
                pop.Stats.TechnologicalDevelopment >= thresholds[next])
            {
                pop.Era = (Era)next;
            }
        }
    }
}
