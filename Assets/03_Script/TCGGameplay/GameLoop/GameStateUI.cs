using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections.Generic;

public class GameStateUI : MonoBehaviourPunCallbacks
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

    [Header("Phase Announcers")]
    [SerializeField] private Animator phaseAnimator;

    // --- CHANGED: These are now GameObject prefabs ---
    [Header("Game Over UI")]
    [SerializeField] private GameObject winPanelPrefab;
    [SerializeField] private GameObject lossPanelPrefab;
    [Tooltip("The main canvas to spawn the Game Over panels onto.")]
    [SerializeField] private Transform mainCanvas;


    public PhotonView photonView { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        photonView = GetComponent<PhotonView>();
        if (phaseAnimator == null) phaseAnimator = GetComponent<Animator>();

        // We no longer need to disable panels in Awake as they are prefabs.
    }

    // --- CHANGED: This method now instantiates the correct prefab ---
    public void ShowGameOver(int defeatedPlayerActorId)
    {
        bool localPlayerWon = (defeatedPlayerActorId != PhotonNetwork.LocalPlayer.ActorNumber);
        Debug.Log($"Game Over! Local Player Won: {localPlayerWon}");

        // Make sure a canvas is assigned to parent the new UI to.
        if (mainCanvas == null)
        {
            Debug.LogError("Main Canvas has not been assigned in GameStateUI!");
            return;
        }

        if (localPlayerWon)
        {
            if (winPanelPrefab != null)
            {
                Debug.Log("Instantiating Win Panel Prefab.");
                Instantiate(winPanelPrefab);
            }
        }
        else
        {
            if (lossPanelPrefab != null)
            {
                Debug.Log("Instantiating Loss Panel Prefab.");
                Instantiate(lossPanelPrefab);
            }
        }
    }

    [PunRPC]
    public void RPC_AnnouncePhase(int phaseValue, int currentTurnActorId)
    {
        if (phaseAnimator == null) return;
        GamePhase phase = (GamePhase)phaseValue;

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
    #endregion
}
