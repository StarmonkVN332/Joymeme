using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections.Generic;

public class GameStateUI : MonoBehaviour
{
    public static GameStateUI Instance;

    [Header("Player Health UI")]
    [SerializeField] private TMP_Text yourHealthText;
    [SerializeField] private TMP_Text opponentHealthText;

    [Header("Player Mana UI")]
    [SerializeField] private TMP_Text yourManaText;
    [SerializeField] private TMP_Text opponentManaText;
    [SerializeField] private List<Image> yourManaCells;
    [SerializeField] private Color filledColor = Color.blue;
    [SerializeField] private Color emptyColor = Color.gray;

    // --- CHANGED: Now references a single Animator on this GameObject ---
    [Header("Phase Announcers")]
    [SerializeField] private Animator phaseAnimator;


    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverText;


    public PhotonView photonView { get; private set; }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // If the animator isn't assigned, try to get it from this object
        if (phaseAnimator == null)
        {
            phaseAnimator = GetComponent<Animator>();
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        photonView = GetComponent<PhotonView>();
    }

    // --- CHANGED: This method now sets different triggers on the single Animator ---
    [PunRPC]
    public void RPC_AnnouncePhase(int phaseValue, int currentTurnActorId)
    {
        if (phaseAnimator == null) return;
        Debug.Log($"RPC_AnnouncePhase called with phaseValue: {phaseValue}, currentTurnActorId: {currentTurnActorId}");
        GamePhase phase = (GamePhase)phaseValue; // Convert the integer back to an enum

        switch (phase)
        {
            case GamePhase.Draw:
                phaseAnimator.SetTrigger("DrawTurn");
                break;
            case GamePhase.Placement:
                if (currentTurnActorId == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    phaseAnimator.SetTrigger("YourTurn");
                }
                else
                {
                    phaseAnimator.SetTrigger("OpponentTurn");
                }
                break;
            case GamePhase.Combat:
                phaseAnimator.SetTrigger("CombatTurn");
                break;
        }
    }


    #region Existing UI Methods
    public void UpdatePlayerHealthUI(int actorId, int health)
    {
        if (yourHealthText == null || opponentHealthText == null) return;

        if (PhotonNetwork.LocalPlayer.ActorNumber == actorId)
            yourHealthText.text = $"{health}";
        else
            opponentHealthText.text = $"{health}";
    }

    public void UpdateManaUI(int actorId, int amount)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == actorId)
        {
            yourManaText.text = $"{amount}";
            UpdateManaBar(amount);
        }
        else
        {
            opponentManaText.text = $"{amount}";
        }
    }

    private void UpdateManaBar(int amount)
    {
        if (yourManaCells == null || yourManaCells.Count == 0) return;

        for (int i = 0; i < yourManaCells.Count; i++)
        {
            if (yourManaCells[i] != null)
                yourManaCells[i].color = i < amount ? filledColor : emptyColor;
        }
    }

    public void ShowGameOver(int defeatedPlayerActorId)
    {
        if (gameOverPanel == null || gameOverText == null) return;

        gameOverPanel.SetActive(true);

        gameOverText.text = (defeatedPlayerActorId == PhotonNetwork.LocalPlayer.ActorNumber)
            ? "You Lost!"
            : "You Won!";
    }
    #endregion
}
