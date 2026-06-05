using UnityEngine;
using UnityEngine.UI;
using DivineDrift.Core;
using DivineDrift.Data;

namespace DivineDrift.UI
{
    /// <summary>
    /// Shows information about the currently selected cell: terrain, owner (and the
    /// owner's philosophy/era), and the player's diplomatic relation with that owner.
    /// Also lets the player set an agent target (for ExpandToward / Ally) from the
    /// selected cell's owner.
    /// </summary>
    public class CellInfoPanel : MonoBehaviour
    {
        public Text terrainLabel;
        public Text ownerLabel;
        public Text relationLabel;
        public Button targetButton;     // "Set as agent target"
        public AgentPanel agentPanel;   // to forward the chosen target

        private GameState _state;
        private int _cellId = -1;

        public void Initialize(GameState state, AgentPanel agentPanel)
        {
            _state = state;
            this.agentPanel = agentPanel;
            if (targetButton) targetButton.onClick.AddListener(ForwardTarget);
            gameObject.SetActive(false);
        }

        public void Show(int cellId)
        {
            _cellId = cellId;
            if (cellId < 0) { gameObject.SetActive(false); return; }

            var cell = _state.GetCell(cellId);
            if (terrainLabel) terrainLabel.text = $"Terrain: {cell.Terrain}";

            if (cell.IsOwned)
            {
                var owner = _state.GetPopulation(cell.OwnerPopulationId);
                if (ownerLabel) ownerLabel.text = $"{owner.Name} ({owner.Philosophy}, {owner.Era})";
                if (relationLabel)
                {
                    var player = _state.Player;
                    var rel = player != null ? _state.GetRelation(player.Id, owner.Id) : DiplomacyState.Neutral;
                    relationLabel.text = $"Relation: {rel}";
                }
                if (targetButton) targetButton.interactable = !owner.IsPlayer;
            }
            else
            {
                if (ownerLabel) ownerLabel.text = "Unclaimed";
                if (relationLabel) relationLabel.text = "";
                if (targetButton) targetButton.interactable = false;
            }
            gameObject.SetActive(true);
        }

        private void ForwardTarget()
        {
            var cell = _state.GetCell(_cellId);
            if (cell != null && cell.IsOwned && agentPanel != null)
            {
                agentPanel.SetTarget(cell.OwnerPopulationId);
                agentPanel.Open();
            }
        }
    }
}
