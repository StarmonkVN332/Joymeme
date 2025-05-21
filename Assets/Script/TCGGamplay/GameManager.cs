using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

public enum GamePhase
{
    None,
    Draw,
    Placement,
    Combat
}

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    public int CurrentTurnActorId { get; private set; }
    public GamePhase CurrentPhase { get; private set; }

    private bool phaseEnded = false;

    [Header("UI Elements")]
    [SerializeField] private Button endPhaseButton;
    [SerializeField] private TMP_Text phaseText;
    [SerializeField] private GameObject loadingScreenPanel;  // The loading screen UI panel

    [Header("Audio")]
    [SerializeField] private AudioSource songAudio;

    private bool gameStarted = false;

    private int firstPlayerActorId;
    private int secondPlayerActorId;

    // Cached PlayerHealth references
    private PlayerHealth player1Health;
    private PlayerHealth player2Health;

    // Tracking player readiness for loading screen
    private int playersReady = 0;
    private int totalPlayers = 0;

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
        // Show loading screen UI on start
        if (loadingScreenPanel != null)
            loadingScreenPanel.SetActive(true);

        totalPlayers = PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.PlayerCount : 0;
        playersReady = 0;

        // Notify MasterClient that this player is ready
        photonView.RPC("PlayerIsReadyRPC", RpcTarget.MasterClient);
    }
    public void StartGameAfterLoading()
    {
        photonView.RPC("StartGameRPC", RpcTarget.All);
    }
    // RPC called on MasterClient when a player signals ready
    [PunRPC]
    public void PlayerIsReadyRPC()
    {
        playersReady++;
        Debug.Log($"[GameManager] Players ready: {playersReady}/{totalPlayers}");

        if (playersReady >= totalPlayers)
        {
            Debug.Log("[GameManager] All players ready! Starting game...");
            photonView.RPC("StartGameRPC", RpcTarget.All);
        }
    }

    // Called on all clients to start the game
    [PunRPC]
    private void StartGameRPC()
    {
        if (loadingScreenPanel != null)
            loadingScreenPanel.SetActive(false); // hide loading screen

        Debug.Log("[GameManager] Game started");
        songAudio?.Play();
        gameStarted = true;

        // Cache player healths after players are spawned
        StartCoroutine(WaitAndCachePlayerHealths());

        var players = PhotonNetwork.PlayerList;
        if (players.Length < 2) return;

        if (PhotonNetwork.IsMasterClient)
        {
            int randomIndex = Random.Range(0, 2);
            int first = players[randomIndex].ActorNumber;
            int second = players[1 - randomIndex].ActorNumber;

            photonView.RPC("AssignPlayerOrderRPC", RpcTarget.AllBuffered, first, second);
        }
    }

    // Cache PlayerHealth references by playerId after they are spawned
    private IEnumerator WaitAndCachePlayerHealths()
    {
        PlayerHealth[] healths = null;

        // Wait until both PlayerHealths are in the scene
        while (healths == null || healths.Length < 2)
        {
            healths = FindObjectsOfType<PlayerHealth>();
            yield return null;
        }

        foreach (var health in healths)
        {
            if (health.playerId == 1) player1Health = health;
            else if (health.playerId == 2) player2Health = health;
        }

        if (player1Health == null || player2Health == null)
            Debug.LogError("[GameManager] Failed to cache both PlayerHealths!");
        else
            Debug.Log("[GameManager] Successfully cached PlayerHealth references.");
    }

    // Assign turn order to players
    [PunRPC]
    private void AssignPlayerOrderRPC(int player1Id, int player2Id)
    {
        this.firstPlayerActorId = player1Id;
        this.secondPlayerActorId = player2Id;

        Debug.Log($"[GameManager] First player: {player1Id}, Second player: {player2Id}");

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_SetPhase", RpcTarget.All, (int)GamePhase.Draw);
            photonView.RPC("RPC_SetTurn", RpcTarget.AllBuffered, firstPlayerActorId);
            StartCoroutine(GameLoop()); // Start the main game loop coroutine
        }
    }

    // Main game loop
    IEnumerator GameLoop()
    {
        Debug.Log($"[GameManager] Game loop started with first: {firstPlayerActorId}, second: {secondPlayerActorId}");
        int round = 1;
        while (true)
        {
            yield return StartCoroutine(DrawPhase());

            yield return StartCoroutine(PlacementPhase("Player 1", firstPlayerActorId));
            yield return StartCoroutine(PlacementPhase("Player 2", secondPlayerActorId));

            yield return StartCoroutine(CombatPhase());

            round++;
        }
    }

    // Draw phase logic
    IEnumerator DrawPhase()
    {
        photonView.RPC("RPC_SetPhase", RpcTarget.All, (int)GamePhase.Draw);
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

    // Placement phase logic for each player turn
    IEnumerator PlacementPhase(string label, int actorId)
    {
        Debug.Log($"[GameManager] {label} Placement Phase (ActorID {actorId})");
        photonView.RPC("RPC_SetPhase", RpcTarget.All, (int)GamePhase.Placement);
        photonView.RPC("RPC_SetTurn", RpcTarget.All, actorId);
        photonView.RPC("SetPhaseTextRPC", RpcTarget.All, $"{label} Placement");

        phaseEnded = false;
        photonView.RPC("SetEndPhaseButtonActive", RpcTarget.All, actorId);

        while (!phaseEnded)
            yield return null;

        photonView.RPC("SetEndPhaseButtonActive", RpcTarget.All, 0);
    }

    // Combat phase logic (simplified)
    IEnumerator CombatPhase()
    {
        photonView.RPC("RPC_SetPhase", RpcTarget.All, (int)GamePhase.Combat);
        photonView.RPC("SetPhaseTextRPC", RpcTarget.All, "Combat Phase");

        yield return new WaitForSeconds(1f);

        Lane3D[] allLanes = FindObjectsOfType<Lane3D>();

        foreach (int laneIndex in Enumerable.Range(0, 5))
        {
            Lane3D p1Lane = allLanes.FirstOrDefault(l => l.PlayerOwnerId == 1 && l.LaneIndex == laneIndex);
            Lane3D p2Lane = allLanes.FirstOrDefault(l => l.PlayerOwnerId == 2 && l.LaneIndex == laneIndex);

            GameObject p1Unit = p1Lane?.GetCurrentUnit();
            GameObject p2Unit = p2Lane?.GetCurrentUnit();

            if (p1Unit != null && p2Unit != null)
            {
                UnitStats p1Stats = p1Unit.GetComponent<UnitStats>();
                UnitStats p2Stats = p2Unit.GetComponent<UnitStats>();

                // Simultaneous combat
                p1Stats.TakeDamage(p2Stats.attack);
                p2Stats.TakeDamage(p1Stats.attack);

                yield return new WaitForSeconds(0.2f);

                if (p1Stats.IsDead) Destroy(p1Unit);
                if (p2Stats.IsDead) Destroy(p2Unit);
            }
            else if (p1Unit != null)
            {
                // Direct damage to player 2
                player2Health?.TakeDamage(p1Unit.GetComponent<UnitStats>().attack);
            }
            else if (p2Unit != null)
            {
                // Direct damage to player 1
                player1Health?.TakeDamage(p2Unit.GetComponent<UnitStats>().attack);
            }
        }

        yield return new WaitForSeconds(2f);
    }

    public void OnEndPhaseButtonPressed()
    {
        Debug.Log("[GameManager] OnEndPhaseButtonPressed called");

        if (!endPhaseButton.gameObject.activeSelf) return;

        photonView.RPC("SetPhaseEndedRPC", RpcTarget.MasterClient);
    }

    private void UpdateCardGrayStates()
    {
        bool isMyTurn = PhotonNetwork.LocalPlayer.ActorNumber == CurrentTurnActorId;
        bool isDrawPhase = CurrentPhase == GamePhase.Draw;
        bool isPlacementPhase = CurrentPhase == GamePhase.Placement;

        int currentMana = 99; // TEMP: Assume infinite for now

        foreach (var card in FindObjectsOfType<DraggableCard>())
        {
            if (card.cardData != null)
            {
                bool hasEnoughMana = card.cardData.manaCost <= currentMana;
                bool isPlayableNow = (isMyTurn && isPlacementPhase && hasEnoughMana) || isDrawPhase;
                bool shouldBeGray = !isPlayableNow;

                card.SetGrayOut(shouldBeGray);
                Debug.Log($"[GameManager] Card {card.name} grayOut: {shouldBeGray} | Phase: {CurrentPhase} | MyTurn: {isMyTurn}");
            }
        }
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
    public void RPC_SetTurn(int actorId)
    {
        CurrentTurnActorId = actorId;
        Debug.Log($"[GameManager] CurrentTurnActorId set to {actorId} (Local Actor: {PhotonNetwork.LocalPlayer.ActorNumber})");

        UpdateCardGrayStates();
    }

    [PunRPC]
    public void RPC_SetPhase(int phaseValue)
    {
        CurrentPhase = (GamePhase)phaseValue;
        Debug.Log($"[GameManager] Phase set to: {CurrentPhase}");

        UpdateCardGrayStates();
    }

    [PunRPC]
    public void RPC_GameOver(int loserPlayerId)
    {
        int winner = (loserPlayerId == 1) ? 2 : 1;
        Debug.Log($"Game Over! Player {winner} wins!");
        phaseText.text = $"Player {winner} Wins!";
        Time.timeScale = 0f;
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

    [PunRPC]
    void SetEndPhaseButtonActive(int actorId)
    {
        bool isMyTurn = PhotonNetwork.LocalPlayer.ActorNumber == actorId;
        if (endPhaseButton != null)
            endPhaseButton.gameObject.SetActive(isMyTurn);
    }
}
