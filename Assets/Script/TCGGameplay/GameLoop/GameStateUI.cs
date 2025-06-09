using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class GameStateUI : MonoBehaviour
{
    public static GameStateUI Instance;

    [Header("Player Health UI")]
    public TMP_Text yourHealthText;
    public TMP_Text opponentHealthText;

    [Header("Player Mana UI")]
    public TMP_Text yourManaText;
    public TMP_Text opponentManaText;

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
        if (yourHealthText == null || opponentHealthText == null) return;

        //string name = PhotonNetwork.CurrentRoom.GetPlayer(actorId)?.NickName ?? $"Player {actorId}";

        if (PhotonNetwork.LocalPlayer.ActorNumber == actorId)
        {
            yourHealthText.text = $"{health}";
        }
        else
        {
            opponentHealthText.text = $"{health}";
        }
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
    public void UpdateManaUI(int actorId, int amount)
    {
        //string name = PhotonNetwork.CurrentRoom.GetPlayer(actorId)?.NickName ?? $"Player {actorId}";

        if (PhotonNetwork.LocalPlayer.ActorNumber == actorId)
        {
            yourManaText.text = $"{amount}";
        }
        else
        {
            opponentManaText.text = $"{amount}";
        }
    }


}
