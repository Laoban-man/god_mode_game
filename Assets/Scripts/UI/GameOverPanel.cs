using UnityEngine;
using UnityEngine.UI;

namespace DivineDrift.UI
{
    /// <summary>
    /// End-of-game modal shown on victory (conquest or peace) or defeat. Offers a
    /// restart hook. Wire 'restartButton' to GameManager.Restart in the scene.
    /// </summary>
    public class GameOverPanel : MonoBehaviour
    {
        public Text titleLabel;
        public Text bodyLabel;
        public Button restartButton;

        public event System.Action OnRestartRequested;

        private void Awake()
        {
            if (restartButton) restartButton.onClick.AddListener(() => OnRestartRequested?.Invoke());
            gameObject.SetActive(false);
        }

        public void ShowVictory(bool conquest)
        {
            if (titleLabel) titleLabel.text = "Victory";
            if (bodyLabel) bodyLabel.text = conquest
                ? "Your people inherited the whole planet."
                : "No neighbour threatens your people. Peace prevails.";
            gameObject.SetActive(true);
        }

        public void ShowDefeat()
        {
            if (titleLabel) titleLabel.text = "Defeat";
            if (bodyLabel) bodyLabel.text = "Your people are no more.";
            gameObject.SetActive(true);
        }
    }
}
