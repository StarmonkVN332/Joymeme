using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

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

    [SerializeField] private Button endPhaseButton;
    [SerializeField] private TMP_Text phaseText;
    [SerializeField] private AudioSource songAudio;

    private bool gameStarted = false;

    private int firstPlayerActorId;
    private int secondPlayerActorId;

    private const int StartingHealth = 30;

    private Dictionary<int, int> playerHealths = new Dictionary<int, int>();
    private Dictionary<int, Lane3D[]> playerLanes = new Dictionary<int, Lane3D[]>();

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
        GameObject roomUI = GameObject.FindWithTag("RoomUi");
        while (roomUI != null && roomUI.activeInHierarchy)
            yield return null;

        while (PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.PlayerCount < 2)
            yield return null;

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("StartGameRPC", RpcTarget.All);
        }
    }

    [PunRPC]
    private void StartGameRPC()
    {
        Debug.Log("[GameManager] Game started");
        songAudio?.Play();
        gameStarted = true;

        var players = PhotonNetwork.PlayerList;
        if (players.Length < 2) return;

        InitializePlayers();

        if (PhotonNetwork.IsMasterClient)
        {
            int randomIndex = Random.Range(0, 2);
            int first = players[randomIndex].ActorNumber;
            int second = players[1 - randomIndex].ActorNumber;

            photonView.RPC("AssignPlayerOrderRPC", RpcTarget.AllBuffered, first, second);
        }
    }

    private void InitializePlayers()
    {
        var players = PhotonNetwork.PlayerList;
        playerHealths.Clear();
        playerLanes.Clear();

        foreach (var player in players)
        {
            playerHealths[player.ActorNumber] = StartingHealth;

            var lanes = FindObjectsOfType<Lane3D>()
                .Where(l => l.PlayerOwnerId == player.ActorNumber)
                .OrderBy(l => l.BoardLaneIndex)
                .ToArray();

            playerLanes[player.ActorNumber] = lanes;
        }

        // Update UI health for all players
        foreach (var player in players)
        {
            GameStateUI.Instance?.UpdatePlayerHealthUI(player.ActorNumber, StartingHealth);
        }
    }

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
            StartCoroutine(GameLoop());
        }
    }

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

    IEnumerator CombatPhase()
    {
        photonView.RPC("RPC_SetPhase", RpcTarget.All, (int)GamePhase.Combat);
        photonView.RPC("SetPhaseTextRPC", RpcTarget.All, "Combat Phase");
        Debug.Log("[GameManager] Combat Phase");

        var player1Lanes = GetLanesForPlayer(firstPlayerActorId);
        var player2Lanes = GetLanesForPlayer(secondPlayerActorId);
        int lanesCount = Mathf.Min(player1Lanes.Length, player2Lanes.Length);


        for (int i = 0; i < lanesCount; i++)
        {
            Debug.Log($"[Combat] Resolving Lane {i}");

            var lane1 = player1Lanes[i];
            var lane2 = player2Lanes[i];

            var unit1 = lane1.CurrentUnit();
            var unit2 = lane2.CurrentUnit();

            Debug.Log($"[CombatPhase] Lane {i}: {unit1?.name} vs {unit2?.name}");

            if (unit1 != null && unit2 != null)
            {
                Debug.Log($"[Combat] {unit1.name} vs {unit2.name}");

                ResolveUnitCombat(unit1, unit2);
            }
            else if (unit1 != null)
            {
                Debug.Log($"[Combat] {unit1.name} attacks player {secondPlayerActorId}");

                DealDamageToPlayer(secondPlayerActorId, unit1.GetComponent<UnitInstance>().GetAttack());
            }
            else if (unit2 != null)
            {
                Debug.Log($"[Combat] {unit2.name} attacks player {firstPlayerActorId}");

                DealDamageToPlayer(firstPlayerActorId, unit2.GetComponent<UnitInstance>().GetAttack());
            }

            // Let animations/effects play out before continuing
            yield return new WaitForSeconds(1f);

            CleanupDeadUnitInLane(lane1);
            CleanupDeadUnitInLane(lane2);
        }
    }


    private void ResolveUnitCombat(GameObject unit1, GameObject unit2)
    {
        var u1 = unit1.GetComponent<UnitInstance>();
        var u2 = unit2.GetComponent<UnitInstance>();

        if (u1 == null || u2 == null) return;

        int u1Attack = u1.GetAttack();
        int u2Attack = u2.GetAttack();

        int u1HealthAfter = Mathf.Max(0, u1.GetHealth() - u2Attack);
        int u2HealthAfter = Mathf.Max(0, u2.GetHealth() - u1Attack);

        u1.photonView.RPC("RPC_SetHealth", RpcTarget.All, u1HealthAfter);
        u2.photonView.RPC("RPC_SetHealth", RpcTarget.All, u2HealthAfter);
    }



    private void CleanupDeadUnitInLane(Lane3D lane)
    {
        var unit = lane.CurrentUnit();
        if (unit == null) return;

        var unitInstance = unit.GetComponent<UnitInstance>();
        if (unitInstance == null) return;

        if (unitInstance.GetHealth() <= 0)
        {
            PhotonView pv = unit.GetComponent<PhotonView>();

            if (pv.IsMine || PhotonNetwork.IsMasterClient)
            {
                Debug.Log($"[Cleanup] Destroying unit {unit.name} in lane {lane.BoardLaneIndex}");
                PhotonNetwork.Destroy(unit);
            }
            else
            {
                Debug.LogWarning($"[Cleanup] Cannot destroy {unit.name} in lane {lane.BoardLaneIndex} - not owner or MasterClient");
            }

            lane.RemoveUnit(unit);
        }

    }
    public Lane3D FindLane(int actorId, int boardLaneIndex)
    {
        if (playerLanes.TryGetValue(actorId, out var lanes))
        {
            if (boardLaneIndex >= 0 && boardLaneIndex < lanes.Length)
            {
                return lanes[boardLaneIndex];
            }
            else
            {
                Debug.LogWarning($"[GameManager] Invalid boardLaneIndex {boardLaneIndex} for player {actorId}");
            }
        }
        else
        {
            Debug.LogWarning($"[GameManager] No lanes found for player {actorId}");
        }

        return null;
    }


    public int GetPlayerHealth(int actorId)
    {
        return playerHealths.TryGetValue(actorId, out int hp) ? hp : 0;
    }

    public void DealDamageToPlayer(int actorId, int damage)
    {
        if (!playerHealths.ContainsKey(actorId)) return;

        playerHealths[actorId] -= damage;
        if (playerHealths[actorId] < 0) playerHealths[actorId] = 0;

        photonView.RPC("UpdatePlayerHealthRPC", RpcTarget.All, actorId, playerHealths[actorId]);

        if (playerHealths[actorId] == 0)
        {
            photonView.RPC("OnPlayerDefeatedRPC", RpcTarget.All, actorId);
        }
    }
    [PunRPC]
    public void RPC_RequestUnitSpawn(string prefabName, int boardLaneIndex, int requesterActorId, string cardId)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        Lane3D lane = FindLane(requesterActorId, boardLaneIndex);
        if (lane == null || !lane.CanAddUnit())
        {
            Debug.LogWarning($"[GameManager] MasterClient: Invalid lane or lane full for request by actor {requesterActorId}");
            return;
        }

        Vector3 spawnPos = lane.GetUnitSpawnPosition();
        Quaternion spawnRot = requesterActorId == 1 ? Quaternion.identity : Quaternion.Euler(0, 180, 0);

        GameObject unit = PhotonNetwork.Instantiate(prefabName, spawnPos, spawnRot, 0, new object[] { cardId });

        bool isOwnerView = requesterActorId == lane.PlayerOwnerId;
        lane.photonView.RPC("RPC_AddUnit", RpcTarget.AllBuffered, unit.GetComponent<PhotonView>().ViewID, isOwnerView);
    }

    [PunRPC]
    private void UpdatePlayerHealthRPC(int actorId, int newHealth)
    {
        playerHealths[actorId] = newHealth;
        Debug.Log($"[GameManager] Player {actorId} health updated: {newHealth}");

        GameStateUI.Instance?.UpdatePlayerHealthUI(actorId, newHealth);
    }

    [PunRPC]
    private void OnPlayerDefeatedRPC(int actorId)
    {
        Debug.Log($"[GameManager] Player {actorId} defeated!");
        GameStateUI.Instance?.ShowGameOver(actorId);
        StopAllCoroutines();
    }

    public Lane3D[] GetLanesForPlayer(int actorId)
    {
        return playerLanes.TryGetValue(actorId, out var lanes) ? lanes : new Lane3D[0];
    }

    public void OnEndPhaseButtonPressed()
    {
        Debug.Log("[GameManager] OnEndPhaseButtonPressed called");

        if (!endPhaseButton.gameObject.activeSelf) return;

        photonView.RPC("SetPhaseEndedRPC", RpcTarget.MasterClient);
    }
    public void UnitDied(int viewID)
    {
        if (!PhotonNetwork.IsMasterClient) return; // Only MasterClient destroys objects

        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView != null)
        {
            PhotonNetwork.Destroy(targetView.gameObject);
        }
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
        if (endPhaseButton == null) return;

        if (PhotonNetwork.LocalPlayer.ActorNumber == actorId)
        {
            endPhaseButton.gameObject.SetActive(true);
        }
        else
        {
            endPhaseButton.gameObject.SetActive(false);
        }
    }
}
