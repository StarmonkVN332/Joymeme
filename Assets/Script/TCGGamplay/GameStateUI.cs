using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class GameStateUI : MonoBehaviour
{
    public static GameStateUI Instance;

    [Header("Player Health UI")]
    public TMP_Text player1HealthText;
    public TMP_Text player2HealthText;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TMP_Text gameOverText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void UpdatePlayerHealthUI(int actorId, int health)
    {
        if (player1HealthText == null || player2HealthText == null) return;

        if (actorId == 1) player1HealthText.text = $"Player 1 Health: {health}";
        else if (actorId == 2) player2HealthText.text = $"Player 2 Health: {health}";
    }

    public void ShowGameOver(int defeatedPlayerActorId)
    {
        if (gameOverPanel == null || gameOverText == null) return;

        gameOverPanel.SetActive(true);

        if (defeatedPlayerActorId == PhotonNetwork.LocalPlayer.ActorNumber)
            gameOverText.text = "You Lost!";
        else
            gameOverText.text = "You Won!";
    }
}
