using UnityEngine;
using UnityEngine.UI;
using DivineDrift.Data;

namespace DivineDrift.UI
{
    /// <summary>
    /// Start-of-game modal where the player picks one of the three philosophies
    /// (Attack / Cooperation / Defense). Each button shows the perk summary from the
    /// game brief. Raises OnPhilosophyChosen, which GameManager uses to finalize the
    /// player's population before the simulation starts.
    /// </summary>
    public class PhilosophySelectionPanel : MonoBehaviour
    {
        [Header("Buttons")]
        public Button attackButton;
        public Button cooperationButton;
        public Button defenseButton;

        [Header("Description labels (optional)")]
        public Text attackDesc;
        public Text cooperationDesc;
        public Text defenseDesc;

        public event System.Action<Philosophy> OnPhilosophyChosen;

        private void Awake()
        {
            if (attackButton) attackButton.onClick.AddListener(() => Choose(Philosophy.Attack));
            if (cooperationButton) cooperationButton.onClick.AddListener(() => Choose(Philosophy.Cooperation));
            if (defenseButton) defenseButton.onClick.AddListener(() => Choose(Philosophy.Defense));

            if (attackDesc) attackDesc.text =
                "Attack: stronger attack & expansion; lower tech & cooperation.";
            if (cooperationDesc) cooperationDesc.text =
                "Cooperation: high tech & cooperation; lower attack, slightly lower expansion.";
            if (defenseDesc) defenseDesc.text =
                "Defense: lower expansion; high attack & tech; medium cooperation.";
        }

        public void Open() => gameObject.SetActive(true);

        private void Choose(Philosophy p)
        {
            OnPhilosophyChosen?.Invoke(p);
            gameObject.SetActive(false);
        }
    }
}
