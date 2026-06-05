using System.Collections.Generic;
using DivineDrift.Data;

namespace DivineDrift.Core
{
    /// <summary>
    /// The single mutable runtime container for a match. Holds the planet's cells,
    /// every population, diplomacy relations, the current year/era and the win state.
    /// Systems read and mutate this; it has no Unity dependencies so it is easy to
    /// unit-test and serialize for save/load.
    /// </summary>
    public class GameState
    {
        public Cell[] Cells;
        public Dictionary<int, Population> Populations = new Dictionary<int, Population>();

        public int PlayerPopulationId = -1;

        public float CurrentYear;        // in-game years elapsed (can be negative for BCE if desired)
        public Era CurrentEra = Era.Bronze;

        // Diplomacy stored as a symmetric pair-keyed map. Key = OrderedPair(a,b).
        private readonly Dictionary<long, DiplomacyState> _relations = new Dictionary<long, DiplomacyState>();

        public bool IsGameOver;
        public int WinnerPopulationId = -1;

        public Population Player => GetPopulation(PlayerPopulationId);

        public Population GetPopulation(int id)
            => Populations.TryGetValue(id, out var p) ? p : null;

        public IEnumerable<Population> AlivePopulations()
        {
            foreach (var p in Populations.Values)
                if (p.IsAlive) yield return p;
        }

        // ---- Diplomacy helpers ----
        private static long Key(int a, int b)
        {
            int lo = a < b ? a : b;
            int hi = a < b ? b : a;
            return ((long)lo << 32) | (uint)hi;
        }

        public DiplomacyState GetRelation(int a, int b)
        {
            if (a == b) return DiplomacyState.Allied; // self
            return _relations.TryGetValue(Key(a, b), out var s) ? s : DiplomacyState.Neutral;
        }

        public void SetRelation(int a, int b, DiplomacyState state)
        {
            if (a == b) return;
            _relations[Key(a, b)] = state;
        }

        public Cell GetCell(int id) => (id >= 0 && id < Cells.Length) ? Cells[id] : null;
    }
}
