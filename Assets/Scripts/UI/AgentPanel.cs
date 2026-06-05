using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DivineDrift.Agents;
using DivineDrift.Data;

namespace DivineDrift.UI
{
    /// <summary>
    /// The "guide your population" panel. Lets the player spawn an Agent by choosing
    /// a DIRECTIVE and up to GameConfig.maxAgentPerks PERK cards. Disabled while an
    /// agent is alive or during the spawn cooldown. Toggles for each available perk
    /// and a dropdown/buttons for the directive are wired in the scene.
    /// </summary>
    public class AgentPanel : MonoBehaviour
    {
        [Header("Directive buttons")]
        public Button expandButton;
        public Button expandTowardButton;
        public Button allyButton;
        public Button defendButton;
        public Button convertButton;

        [Header("Perk selection")]
        public Transform perkListRoot;       // container for perk toggle prefabs
        public Toggle perkTogglePrefab;
        public Text perkBudgetLabel;         // e.g. "Perks 1/2"

        [Header("Confirm / status")]
        public Button confirmButton;
        public Text statusLabel;

        private AgentSystem _agents;
        private int _maxPerks;
        private AgentDirective _directive = AgentDirective.Expand;
        private int _targetId = -1;

        private readonly List<(Toggle toggle, AgentPerk perk)> _perkToggles = new();

        public void Initialize(AgentSystem agents, int maxPerks)
        {
            _agents = agents;
            _maxPerks = maxPerks;

            if (expandButton) expandButton.onClick.AddListener(() => _directive = AgentDirective.Expand);
            if (expandTowardButton) expandTowardButton.onClick.AddListener(() => _directive = AgentDirective.ExpandToward);
            if (allyButton) allyButton.onClick.AddListener(() => _directive = AgentDirective.Ally);
            if (defendButton) defendButton.onClick.AddListener(() => _directive = AgentDirective.Defend);
            if (convertButton) convertButton.onClick.AddListener(() => _directive = AgentDirective.Convert);
            if (confirmButton) confirmButton.onClick.AddListener(Confirm);

            gameObject.SetActive(false);
        }

        /// <summary>For ExpandToward / Ally directives, set from a selected cell's owner.</summary>
        public void SetTarget(int populationId) => _targetId = populationId;

        public void Open()
        {
            RebuildPerkList();
            UpdateStatus();
            gameObject.SetActive(true);
        }

        public void Close() => gameObject.SetActive(false);

        private void RebuildPerkList()
        {
            foreach (Transform child in perkListRoot) Destroy(child.gameObject);
            _perkToggles.Clear();

            foreach (var perk in _agents.AvailablePerksForPlayer())
            {
                var toggle = Instantiate(perkTogglePrefab, perkListRoot);
                var label = toggle.GetComponentInChildren<Text>();
                if (label) label.text = $"{perk.DisplayName}";
                toggle.onValueChanged.AddListener(_ => EnforcePerkBudget());
                _perkToggles.Add((toggle, perk));
            }
            EnforcePerkBudget();
        }

        private void EnforcePerkBudget()
        {
            int selected = CountSelected();
            // Disable unchecked toggles once the budget is reached.
            foreach (var (toggle, _) in _perkToggles)
                toggle.interactable = toggle.isOn || selected < _maxPerks;
            if (perkBudgetLabel) perkBudgetLabel.text = $"Perks {selected}/{_maxPerks}";
        }

        private int CountSelected()
        {
            int n = 0;
            foreach (var (toggle, _) in _perkToggles) if (toggle.isOn) n++;
            return n;
        }

        private void Confirm()
        {
            if (!_agents.CanSpawnForPlayer())
            {
                if (statusLabel) statusLabel.text = "Agent unavailable (cooldown or one already active).";
                return;
            }

            var chosen = new List<AgentPerk>();
            foreach (var (toggle, perk) in _perkToggles)
                if (toggle.isOn) chosen.Add(perk);

            var agent = _agents.SpawnPlayerAgent(_directive, _targetId, chosen);
            if (agent != null) Close();
        }

        private void UpdateStatus()
        {
            if (statusLabel == null) return;
            statusLabel.text = _agents.CanSpawnForPlayer()
                ? "Choose a directive and perks."
                : "On cooldown or agent active.";
        }
    }
}
