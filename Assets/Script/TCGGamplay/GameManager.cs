using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    private bool phaseEnded = false;

    [SerializeField] private Button endPhaseButton;
    [SerializeField] private TMP_Text phaseText;
    [SerializeField] private AudioSource songAudio;

    private bool gameStarted = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (endPhaseButton != null)
        {
            Debug.Log("[GameManager] Adding listener to endPhaseButton");
            endPhaseButton.onClick.AddListener(OnEndPhaseButtonPressed);
            endPhaseButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[GameManager] endPhaseButton is NOT assigned!");
        }
    }


    private void Start()
    {
        StartCoroutine(WaitForRoomAndPlayers());
    }

    IEnumerator WaitForRoomAndPlayers()
    {
        // Wait until room UI is closed (like your menu UI)
        GameObject roomUI = GameObject.FindWithTag("RoomUi");
        while (roomUI != null && roomUI.activeInHierarchy)
            yield return null;

        // Wait until room exists and 2 players are joined
        while (PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.PlayerCount < 2)
            yield return null;

        if (PhotonNetwork.IsMasterClient)
        {
            // Start the game with fixed player IDs 1 and 2 (Photon actor numbers)
            photonView.RPC("StartGameRPC", RpcTarget.All);
        }
    }

    [PunRPC]
    private void StartGameRPC()
    {
        Debug.Log("[GameManager] Game started");
        songAudio?.Play();
        gameStarted = true;

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(GameLoop());
        }
    }

    IEnumerator GameLoop()
    {
        int round = 1;
        while (true)
        {
            yield return StartCoroutine(DrawPhase());

            yield return StartCoroutine(PlacementPhase("Player 1", 1));
            yield return StartCoroutine(PlacementPhase("Player 2", 2));

            yield return StartCoroutine(CombatPhase());

            round++;
        }
    }

    IEnumerator DrawPhase()
    {
        photonView.RPC("SetPhaseTextRPC", RpcTarget.All, "Draw Phase");
        photonView.RPC("DrawCardForAllPlayers", RpcTarget.All);
        yield return new WaitForSeconds(2f);
    }

    [PunRPC]
    private void DrawCardForAllPlayers()
    {
        var handSpawner = FindObjectOfType<HandSpawner>();
        handSpawner?.DrawCard();
    }

    IEnumerator PlacementPhase(string label, int actorId)
    {
        Debug.Log($"[GameManager] {label} Placement Phase (ActorID {actorId})");

        photonView.RPC("SetPhaseTextRPC", RpcTarget.All, $"{label} Placement");

        phaseEnded = false;

        // Tell all clients which player has the turn (so only that player gets the button)
        photonView.RPC("SetEndPhaseButtonActive", RpcTarget.All, actorId);

        while (!phaseEnded)
            yield return null;

        // Disable button at phase end for all clients
        photonView.RPC("SetEndPhaseButtonActive", RpcTarget.All, 0);
    }

    IEnumerator CombatPhase()
    {
        photonView.RPC("SetPhaseTextRPC", RpcTarget.All, "Combat Phase");
        Debug.Log("[GameManager] Combat Phase");
        yield return new WaitForSeconds(5f); // Replace with real combat logic
    }

    public void OnEndPhaseButtonPressed()
    {
        Debug.Log("[GameManager] OnEndPhaseButtonPressed called");

        // Only send RPC if button active
        if (!endPhaseButton.gameObject.activeSelf) return;

        // Call RPC to MasterClient
        photonView.RPC("SetPhaseEndedRPC", RpcTarget.MasterClient);
    }

    [PunRPC]
    void SetPhaseEndedRPC()
    {
        Debug.Log("[GameManager] SetPhaseEndedRPC called on MasterClient");
        phaseEnded = true;
        photonView.RPC("HideEndPhaseButtonRPC", RpcTarget.All);
    }

    [PunRPC]
    void HideEndPhaseButtonRPC()
    {
        if (endPhaseButton != null)
            endPhaseButton.gameObject.SetActive(false);
    }




    [PunRPC]
    void SetPhaseTextRPC(string text)
    {
        if (phaseText != null)
        {
            Debug.Log($"[GameManager] Phase text set to: {text}");
            phaseText.text = text;
        }
    }

    // Enable button only if local player actorId matches given actorId
    [PunRPC]
    void SetEndPhaseButtonActive(int actorId)
    {
        bool isMyTurn = PhotonNetwork.LocalPlayer.ActorNumber == actorId;
        if (endPhaseButton != null)
            endPhaseButton.gameObject.SetActive(isMyTurn);
    }
}
