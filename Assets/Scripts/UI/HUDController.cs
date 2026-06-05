using UnityEngine;
using UnityEngine.UI;
using DivineDrift.Core;
using DivineDrift.Data;
using DivineDrift.Simulation;

namespace DivineDrift.UI
{
    /// <summary>
    /// Top-level on-screen HUD: edge buttons (menu, speed -, speed +, actions) plus
    /// status readouts (year, era, population/territory). Buttons live around the
    /// screen edges as required by the brief. Wire the references in the scene.
    ///
    /// This controller is intentionally thin: it forwards button events to the
    /// systems and refreshes labels each frame. Visual styling (pastel + black edge)
    /// is done on the UI prefabs, not here.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Edge buttons")]
        public Button menuButton;
        public Button speedUpButton;
        public Button speedDownButton;
        public Button actionsButton;     // opens the agent / action panel

        [Header("Status labels")]
        public Text yearLabel;
        public Text eraLabel;
        public Text speedLabel;
        public Text populationLabel;
        public Text territoryLabel;

        [Header("Panels")]
        public GameObject menuPanel;
        public AgentPanel agentPanel;
        public CellInfoPanel cellInfoPanel;

        private GameState _state;
        private TimeManager _time;

        public void Initialize(GameState state, TimeManager time, AgentPanel agent, CellInfoPanel cellInfo)
        {
            _state = state;
            _time = time;
            agentPanel = agent;
            cellInfoPanel = cellInfo;

            if (menuButton) menuButton.onClick.AddListener(ToggleMenu);
            if (speedUpButton) speedUpButton.onClick.AddListener(() => _time.CycleFaster());
            if (speedDownButton) speedDownButton.onClick.AddListener(() => _time.CycleSlower());
            if (actionsButton) actionsButton.onClick.AddListener(() => agentPanel?.Open());
        }

        private void Update()
        {
            if (_state == null) return;
            var p = _state.Player;
            if (yearLabel) yearLabel.text = $"Year {Mathf.FloorToInt(_state.CurrentYear)}";
            if (eraLabel) eraLabel.text = _state.CurrentEra.ToString();
            if (speedLabel) speedLabel.text = SpeedText(_time.Scale);
            if (p != null)
            {
                if (populationLabel) populationLabel.text = $"Pop {Mathf.RoundToInt(p.Stats.PopulationSize)}";
                if (territoryLabel) territoryLabel.text = $"Cells {p.OwnedCells.Count}";
            }
        }

        private void ToggleMenu()
        {
            if (menuPanel) menuPanel.SetActive(!menuPanel.activeSelf);
        }

        private string SpeedText(TimeScaleStep s)
        {
            switch (s)
            {
                case TimeScaleStep.Paused: return "Paused";
                case TimeScaleStep.Years1PerMinute: return "1 yr/min";
                case TimeScaleStep.Years5PerMinute: return "5 yr/min";
                case TimeScaleStep.Years20PerMinute: return "20 yr/min";
                case TimeScaleStep.Years50PerMinute: return "50 yr/min";
                case TimeScaleStep.Years100PerMinute: return "100 yr/min";
                default: return "";
            }
        }
    }
}
