using UnityEngine;
using DivineDrift.Config;
using DivineDrift.Core;
using DivineDrift.Data;

namespace DivineDrift.Simulation
{
    /// <summary>
    /// Owns the in-game clock. Converts real time into in-game years according to
    /// the current TimeScaleStep (1 real min = 1..100 in-game years) and raises a
    /// per-year tick that simulation systems subscribe to. Also advances eras.
    /// </summary>
    public class TimeManager
    {
        private readonly GameConfig _config;
        private readonly GameState _state;

        private float _yearAccumulator;
        public TimeScaleStep Scale { get; private set; } = TimeScaleStep.Years5PerMinute;

        /// <summary>Raised once per whole in-game year advanced.</summary>
        public event System.Action<int> OnYearTick;
        /// <summary>Raised when the global era changes.</summary>
        public event System.Action<Era> OnEraChanged;

        public TimeManager(GameConfig config, GameState state)
        {
            _config = config;
            _state = state;
        }

        public void SetScale(TimeScaleStep step) => Scale = step;

        public void CycleFaster()
        {
            int next = Mathf.Min((int)Scale + 1, (int)TimeScaleStep.Years100PerMinute);
            Scale = (TimeScaleStep)next;
        }

        public void CycleSlower()
        {
            int next = Mathf.Max((int)Scale - 1, (int)TimeScaleStep.Paused);
            Scale = (TimeScaleStep)next;
        }

        /// <summary>Call every frame with Time.deltaTime.</summary>
        public void Tick(float deltaSeconds)
        {
            if (_state.IsGameOver || Scale == TimeScaleStep.Paused) return;

            _yearAccumulator += _config.YearsPerSecond(Scale) * deltaSeconds;
            while (_yearAccumulator >= 1f)
            {
                _yearAccumulator -= 1f;
                _state.CurrentYear += 1f;
                OnYearTick?.Invoke(Mathf.FloorToInt(_state.CurrentYear));
                EvaluateEra();
            }
        }

        /// <summary>
        /// Global era is the highest era any population has reached (simple model).
        /// Per-population tech still gates individual abilities.
        /// </summary>
        private void EvaluateEra()
        {
            Era highest = Era.Bronze;
            foreach (var pop in _state.AlivePopulations())
                if ((int)pop.Era > (int)highest) highest = pop.Era;

            if (highest != _state.CurrentEra)
            {
                _state.CurrentEra = highest;
                OnEraChanged?.Invoke(highest);
            }
        }
    }
}
