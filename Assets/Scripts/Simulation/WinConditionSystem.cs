using DivineDrift.Core;
using DivineDrift.Data;

namespace DivineDrift.Simulation
{
    /// <summary>
    /// Evaluates victory each year for the player population. The player wins when
    /// EITHER:
    ///   (a) Conquest — they own the entire planet (no other population survives), OR
    ///   (b) Peace — every surviving population that borders them is allied or
    ///       cooperating, i.e. no neighbour threatens them.
    /// Also flags defeat if the player is eliminated.
    /// </summary>
    public class WinConditionSystem
    {
        private readonly GameState _state;

        public event System.Action<int> OnVictory; // winner population id
        public event System.Action OnPlayerDefeated;

        public WinConditionSystem(GameState state) => _state = state;

        public void Evaluate()
        {
            if (_state.IsGameOver) return;

            var player = _state.Player;
            if (player == null || !player.IsAlive)
            {
                _state.IsGameOver = true;
                OnPlayerDefeated?.Invoke();
                return;
            }

            // (a) Conquest: player is the only alive population.
            int aliveCount = 0;
            foreach (var _ in _state.AlivePopulations()) aliveCount++;
            if (aliveCount == 1)
            {
                Win(player.Id);
                return;
            }

            // (b) Peace: every bordering population is allied/cooperating.
            var borders = TerritoryUtils.BorderingPopulations(_state, player);
            if (borders.Count > 0)
            {
                bool allFriendly = true;
                foreach (int other in borders)
                {
                    var rel = _state.GetRelation(player.Id, other);
                    if (rel != DiplomacyState.Allied && rel != DiplomacyState.Cooperating)
                    {
                        allFriendly = false;
                        break;
                    }
                }
                if (allFriendly) Win(player.Id);
            }
        }

        private void Win(int popId)
        {
            _state.IsGameOver = true;
            _state.WinnerPopulationId = popId;
            OnVictory?.Invoke(popId);
        }
    }
}
