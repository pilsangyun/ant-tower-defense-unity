using UnityEngine;
using UnityEngine.UI;
using AntTD.Managers;

namespace AntTD.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("UI Text References")]
        public Text sugarText;
        public Text proteinText;
        public Text queenHpText;
        public Image queenHpBarFill;
        public Text waveBadgeText;

        [Header("Buttons")]
        public Button waveStartButton;
        public Button speedToggleButton;
        private int currentSpeed = 1;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnResourcesChanged += UpdateResourceUI;
                GameManager.Instance.OnQueenHpChanged += UpdateQueenHpUI;
                UpdateResourceUI();
                UpdateQueenHpUI(GameManager.Instance.queenHp);
            }

            if (waveStartButton != null)
            {
                waveStartButton.onClick.AddListener(OnWaveStartClicked);
            }

            if (speedToggleButton != null)
            {
                speedToggleButton.onClick.AddListener(ToggleGameSpeed);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnResourcesChanged -= UpdateResourceUI;
                GameManager.Instance.OnQueenHpChanged -= UpdateQueenHpUI;
            }
        }

        private void UpdateResourceUI()
        {
            if (sugarText != null) sugarText.text = GameManager.Instance.sugar.ToString();
            if (proteinText != null) proteinText.text = GameManager.Instance.protein.ToString();
        }

        private void UpdateQueenHpUI(int currentHp)
        {
            if (queenHpText != null) queenHpText.text = $"{currentHp} / {GameManager.Instance.maxQueenHp}";
            if (queenHpBarFill != null)
            {
                queenHpBarFill.fillAmount = (float)currentHp / GameManager.Instance.maxQueenHp;
            }
        }

        private void OnWaveStartClicked()
        {
            WaveManager.Instance.StartNextWave();
            if (waveBadgeText != null)
            {
                waveBadgeText.text = $"WAVE {WaveManager.Instance.currentWaveIndex + 1}";
            }
        }

        private void ToggleGameSpeed()
        {
            currentSpeed = (currentSpeed == 1) ? 2 : 1;
            Time.timeScale = currentSpeed;
            if (speedToggleButton != null)
            {
                Text btnText = speedToggleButton.GetComponentInChildren<Text>();
                if (btnText != null) btnText.text = $"{currentSpeed}x";
            }
        }
    }
}
