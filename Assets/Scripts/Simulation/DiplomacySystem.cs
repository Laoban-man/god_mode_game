using UnityEngine;
using DivineDrift.Core;
using DivineDrift.Data;

namespace DivineDrift.Simulation
{
    /// <summary>
    /// Manages relationship transitions (neutral / war / cooperating / allied) and
    /// the benefits of cooperation (shared science, trade growth). Mutual high
    /// cooperation capacity makes alliances likely; aggressive philosophies make
    /// war likely. The win condition treats "all neighbors allied/cooperating" as
    /// a victory path alongside total conquest.
    /// </summary>
    public class DiplomacySystem
    {
        private readonly GameState _state;

        public DiplomacySystem(GameState state) => _state = state;

        /// <summary>
        /// Propose a relationship change between two populations. Acceptance depends
        /// on both sides' cooperation capacity and current state. Returns the new state.
        /// </summary>
        public DiplomacyState Propose(Population a, Population b, DiplomacyState desired)
        {
            var current = _state.GetRelation(a.Id, b.Id);

            float willingness = (a.Stats.CooperationCapacity + b.Stats.CooperationCapacity) * 0.5f;

            switch (desired)
            {
                case DiplomacyState.Cooperating:
                case DiplomacyState.Allied:
                    // Easier to agree the more cooperative both sides are.
                    if (Random.value < Mathf.Clamp01(willingness * 0.4f))
                    {
                        _state.SetRelation(a.Id, b.Id, desired);
                        return desired;
                    }
                    return current;

                case DiplomacyState.AtWar:
                    _state.SetRelation(a.Id, b.Id, DiplomacyState.AtWar);
                    return DiplomacyState.AtWar;

                default:
                    _state.SetRelation(a.Id, b.Id, DiplomacyState.Neutral);
                    return DiplomacyState.Neutral;
            }
        }

        /// <summary>Per-year benefits flowing between cooperating/allied pairs.</summary>
        public void TickYear()
        {
            var pops = new System.Collections.Generic.List<Population>(_state.AlivePopulations());
            for (int i = 0; i < pops.Count; i++)
            for (int j = i + 1; j < pops.Count; j++)
            {
                var rel = _state.GetRelation(pops[i].Id, pops[j].Id);
                if (rel == DiplomacyState.Cooperating || rel == DiplomacyState.Allied)
                {
                    // Science sharing: nudge both toward the higher tech level.
                    float avgTech = (pops[i].Stats.TechnologicalDevelopment +
                                     pops[j].Stats.TechnologicalDevelopment) * 0.5f;
                    pops[i].Stats.TechnologicalDevelopment =
                        Mathf.Lerp(pops[i].Stats.TechnologicalDevelopment, avgTech, 0.02f);
                    pops[j].Stats.TechnologicalDevelopment =
                        Mathf.Lerp(pops[j].Stats.TechnologicalDevelopment, avgTech, 0.02f);

                    // Trade: small mutual population growth.
                    pops[i].Stats.PopulationSize *= 1.002f;
                    pops[j].Stats.PopulationSize *= 1.002f;
                }
            }
        }
    }
}
