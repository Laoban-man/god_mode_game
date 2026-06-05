using System.Collections.Generic;
using UnityEngine;
using DivineDrift.Agents;
using DivineDrift.AI;
using DivineDrift.Config;
using DivineDrift.Data;
using DivineDrift.InputControl;
using DivineDrift.Planet;
using DivineDrift.Rendering;
using DivineDrift.Simulation;
using DivineDrift.UI;

namespace DivineDrift.Core
{
    /// <summary>
    /// The single MonoBehaviour that boots and runs a match. It builds the planet,
    /// spawns populations after the player picks a philosophy, constructs every
    /// system, and drives the per-frame + per-year update loop. Scene references are
    /// wired in the inspector.
    ///
    /// Update order each year tick:
    ///   Agents -> Growth -> AI decisions -> Diplomacy benefits -> Win check -> Render refresh
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Config")]
        public GameConfig config;

        [Header("Scene refs")]
        public PlanetRenderer planetRenderer;
        public PlanetCameraController cameraController;
        public CellPicker cellPicker;

        [Header("UI")]
        public PhilosophySelectionPanel philosophyPanel;
        public HUDController hud;
        public AgentPanel agentPanel;
        public CellInfoPanel cellInfoPanel;
        public GameOverPanel gameOverPanel;

        // ---- Runtime ----
        private GameState _state;
        private TimeManager _time;
        private GrowthSystem _growth;
        private ExpansionSystem _expansion;
        private CombatSystem _combat;
        private DiplomacySystem _diplomacy;
        private WinConditionSystem _win;
        private PopulationAI _ai;
        private AgentSystem _agents;

        private bool _started;

        private void Start()
        {
            if (config == null)
            {
                Debug.LogError("[GameManager] No GameConfig assigned.");
                enabled = false;
                return;
            }
            BuildWorld();
            ShowPhilosophyChoice();
        }

        private void BuildWorld()
        {
            // 1. Generate the planet (pure data).
            var planet = PlanetBuilder.Build(config);

            // 2. Create the game state and attach cells.
            _state = new GameState { Cells = planet.Cells, CurrentEra = config.startingEra };

            // 3. Build the visible mesh + collider.
            planetRenderer.Build(_state, planet);

            // 4. Hook up input.
            cellPicker.Initialize(_state, planetRenderer);
            cellPicker.OnCellSelected += OnCellSelected;
        }

        private void ShowPhilosophyChoice()
        {
            philosophyPanel.OnPhilosophyChosen += OnPhilosophyChosen;
            philosophyPanel.Open();
        }

        private void OnPhilosophyChosen(Philosophy philosophy)
        {
            // 5. Spawn populations now that the player's philosophy is known.
            PopulationSpawner.Spawn(_state, config, philosophy);

            // 6. Construct systems.
            _time      = new TimeManager(config, _state);
            _expansion = new ExpansionSystem(_state);
            _combat    = new CombatSystem(_state);
            _diplomacy = new DiplomacySystem(_state);
            _growth    = new GrowthSystem(config, _state);
            _ai        = new PopulationAI(_state, _expansion, _combat, _diplomacy);
            _agents    = new AgentSystem(config, _state, _expansion, _combat, _diplomacy);
            _win       = new WinConditionSystem(_state);

            // 7. Wire per-year tick + events.
            _time.OnYearTick += OnYearTick;
            _win.OnVictory += OnVictory;
            _win.OnPlayerDefeated += OnDefeat;

            // 8. Initialize UI controllers.
            agentPanel.Initialize(_agents, config.maxAgentPerks);
            cellInfoPanel.Initialize(_state, agentPanel);
            hud.Initialize(_state, _time, agentPanel, cellInfoPanel);

            // 9. Paint initial ownership and start the clock.
            planetRenderer.RefreshAllColors();
            _time.SetScale(TimeScaleStep.Years5PerMinute);
            _started = true;
        }

        private void Update()
        {
            if (!_started || _state.IsGameOver) return;
            _time.Tick(Time.deltaTime);
        }

        /// <summary>Runs once per simulated in-game year.</summary>
        private void OnYearTick(int year)
        {
            _agents.TickYear();      // player's living agent executes directive
            _growth.TickYear();      // growth, research, tech, era
            _ai.TickYear();          // AI populations decide & act
            _diplomacy.TickYear();   // cooperation benefits flow
            _win.Evaluate();         // victory / defeat check

            // Repaint ownership (could be optimized to dirty cells only).
            planetRenderer.RefreshAllColors();
        }

        private void OnCellSelected(int cellId)
        {
            planetRenderer.SetSelectedCell(cellId);
            cellInfoPanel.Show(cellId);
        }

        private void OnVictory(int winnerId)
        {
            bool conquest = CountAlive() == 1;
            gameOverPanel.ShowVictory(conquest);
        }

        private void OnDefeat() => gameOverPanel.ShowDefeat();

        private int CountAlive()
        {
            int n = 0;
            foreach (var _ in _state.AlivePopulations()) n++;
            return n;
        }

        /// <summary>Hook for GameOverPanel restart button.</summary>
        public void Restart()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
}
