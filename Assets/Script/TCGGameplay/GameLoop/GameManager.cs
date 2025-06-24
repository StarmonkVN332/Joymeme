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
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text turnIndicatorText;

    private int currentRound = 1;

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
        ManaManager.Instance.InitializePlayers();

        if (PhotonNetwork.IsMasterClient)
        {
            int randomIndex = Random.Range(0, 2);
            int first = players[randomIndex].ActorNumber;
            int second = players[1 - randomIndex].ActorNumber;

            photonView.RPC("AssignPlayerOrderRPC", RpcTarget.AllBuffered, first, second);
        }
        UpdateCardGrayStates();
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
        currentRound = 1;

        while (true)
        {
            photonView.RPC("RPC_SetRound", RpcTarget.All, currentRound);

            yield return StartCoroutine(DrawPhase());

            yield return StartCoroutine(PlacementPhase("Player 1", firstPlayerActorId));
            yield return StartCoroutine(PlacementPhase("Player 2", secondPlayerActorId));

            yield return StartCoroutine(CombatPhase());

            currentRound++;
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

    IEnumerator CombatPhase() //Chỗ này các card đấm nhau
    {
        photonView.RPC("RPC_SetPhase", RpcTarget.All, (int)GamePhase.Combat);
        photonView.RPC("SetPhaseTextRPC", RpcTarget.All, "Combat Phase");
        Debug.Log("[GameManager] Combat Phase");

        var player1Lanes = GetLanesForPlayer(firstPlayerActorId);
        var player2Lanes = GetLanesForPlayer(secondPlayerActorId);

        int lanesCount = Mathf.Min(player1Lanes.Length, player2Lanes.Length);

        for (int i = 0; i < lanesCount; i++)
        {
            var lane1 = player1Lanes[i];
            var lane2 = player2Lanes[i];

            var unit1 = lane1.CurrentUnit();
            var unit2 = lane2.CurrentUnit();

            if (unit1 != null && unit2 != null)
            {
                yield return StartCoroutine(ResolveUnitCombat(unit1, unit2));
            }
            else if (unit1 != null && unit2 == null)
            {
                DealDamageToPlayer(secondPlayerActorId, unit1.GetComponent<UnitInstance>().GetAttack());
                yield return new WaitForSeconds(1.0f); // delay before next lane
            }
            else if (unit2 != null && unit1 == null)
            {
                DealDamageToPlayer(firstPlayerActorId, unit2.GetComponent<UnitInstance>().GetAttack());
                yield return new WaitForSeconds(1.0f); // delay before next lane
            }
        }

        // Final cleanup pass
        //CleanupDeadUnits(player1Lanes);
        //CleanupDeadUnits(player2Lanes);

        yield return new WaitForSeconds(1f); // Final pause before next round
    }


    private IEnumerator ResolveUnitCombat(GameObject unit1, GameObject unit2) //Chỗ này card đấm nhau
    {
        var u1 = unit1?.GetComponent<UnitInstance>();
        var u2 = unit2?.GetComponent<UnitInstance>();

        if (u1 == null || u2 == null) yield break;
        
        int u1Attack = u1.GetAttack();
        int u2Attack = u2.GetAttack();

        int u1NewHealth = u1.GetHealth();
        int u2NewHealth = u2.GetHealth();

        if (u1Attack > 0)
        {
            u1.photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "Attack");
            u2.photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "Damaged");
            u2NewHealth -= u1Attack;
        }

        yield return new WaitForSeconds(0.5f);

        if (u2Attack > 0)
        {
            u2.photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "Attack");
            u1.photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "Damaged");
            u1NewHealth -= u2Attack;
        }


        int u1ViewId = u1.photonView.ViewID;
        int u2ViewId = u2.photonView.ViewID;

        u1.photonView.RPC("RPC_SetHealth", RpcTarget.All, u1NewHealth);
        u2.photonView.RPC("RPC_SetHealth", RpcTarget.All, u2NewHealth);

        yield return new WaitForSeconds(0.5f);

        // Safely destroy defeated units
        if (u1NewHealth <= 0)
        {
            PhotonView pv1 = PhotonView.Find(u1ViewId);
            if (pv1 != null)
            {
                GameManager.Instance.UnitDied(u1ViewId);
            }
        }

        if (u2NewHealth <= 0)
        {
            PhotonView pv2 = PhotonView.Find(u2ViewId);
            if (pv2 != null)
            {
                GameManager.Instance.UnitDied(u2ViewId);
            }
        }
        yield return new WaitForSeconds(1.0f);
    }
    private void CleanupDeadUnits(Lane3D[] lanes)
    {
        foreach (var lane in lanes)
        {
            var unit = lane.CurrentUnit();
            if (unit == null) continue;

            var unitInstance = unit.GetComponent<UnitInstance>();
            if (unitInstance == null) continue;

            if (unitInstance.GetHealth() <= 0)
            {
                PhotonNetwork.Destroy(unit);
                lane.RemoveUnit(unit);
            }
        }
    }
    public void UnitDied(int viewID)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView != null)
        {
            // Ask the owner to destroy the unit
            targetView.RPC("RequestUnitDestroyRPC", targetView.Owner, viewID);
        }
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

    [PunRPC]
    public void RPC_RequestPlayUnit(int cardId, int laneIndex, int requestingPlayerId)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        CardData cardData = CardDatabase.Instance.GetCardById(cardId);
        if (cardData == null || cardData.unitPrefab == null)
        {
            Debug.LogWarning($"[GameManager] Invalid card data for cardId {cardId}");
            return;
        }

        // MANA CHECK
        if (!ManaManager.Instance.HasEnoughMana(requestingPlayerId, cardData.manaCost))
        {
            Debug.LogWarning($"[GameManager] Player {requestingPlayerId} doesn't have enough mana for card {cardId}.");
            return;
        }

        // LANE VALIDATION
        Lane3D[] allLanes = FindObjectsOfType<Lane3D>();
        Lane3D lane = null;
        foreach (var l in allLanes)
        {
            if (l.BoardLaneIndex == laneIndex && l.PlayerOwnerId == requestingPlayerId)
            {
                lane = l;
                break;
            }
        }

        if (lane == null)
        {
            Debug.LogWarning($"[GameManager] Invalid lane for player {requestingPlayerId}.");
            return;
        }

        if (!lane.CanAddUnit())
        {
            Debug.LogWarning($"[GameManager] Lane {laneIndex} is already occupied.");
            return;
        }

        // SPEND MANA
        bool spent = ManaManager.Instance.TrySpendMana(requestingPlayerId, cardData.manaCost);
        if (!spent)
        {
            Debug.LogWarning($"[GameManager] Failed to spend mana for player {requestingPlayerId}");
            return;
        }
        photonView.RPC("RPC_ConfirmCardPlay", PhotonNetwork.CurrentRoom.GetPlayer(requestingPlayerId), cardId);

        // SPAWN UNIT
        Vector3 spawnPos = lane.GetUnitSpawnPosition();
        Quaternion rot = requestingPlayerId == 1 ? Quaternion.identity : Quaternion.Euler(0, 180, 0);

        GameObject unit = PhotonNetwork.Instantiate(cardData.unitPrefab.name, spawnPos, rot, 0, new object[] { cardId });

        bool isOwnerView = requestingPlayerId == lane.PlayerOwnerId;
        lane.photonView.RPC("RPC_AddUnit", RpcTarget.AllBuffered, unit.GetComponent<PhotonView>().ViewID, isOwnerView);

    }
    [PunRPC]
    public void RPC_ConfirmCardPlay(int cardId)
    {
        Debug.Log($"[GameManager] Confirmed play of card {cardId}");
        //HandLayout.Instance?.RepositionCards(); // optional
        GameManager.Instance?.UpdateCardGrayStates(); // 👈 Update after mana is spent
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

        int currentMana = ManaManager.Instance.GetCurrentMana(PhotonNetwork.LocalPlayer.ActorNumber);

        foreach (var card in FindObjectsOfType<DraggableCard>())
        {
            if (card.cardData != null)
            {
                bool hasEnoughMana = card.cardData.manaCost <= currentMana;
                bool isPlayableNow = (isMyTurn && isPlacementPhase && hasEnoughMana) || isDrawPhase;
                bool shouldBeGray = !isPlayableNow;

                card.SetGrayOut(shouldBeGray);
            }
        }
    }

    private void UpdateTurnIndicator()
    {
        if (turnIndicatorText == null) return;

        if (PhotonNetwork.LocalPlayer.ActorNumber == CurrentTurnActorId)
        {
            turnIndicatorText.text = "Your Turn";
        }
        else
        {
            turnIndicatorText.text = "Opponent's Turn";
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

        // Refill mana for the current turn player
        ManaManager.Instance.RefillMana(actorId, currentRound);
        UpdateCardGrayStates();
        UpdateTurnIndicator();
        
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
    [PunRPC]
    private void RPC_SetRound(int round)
    {
        currentRound = round;
        UpdateRoundText(round);
    }

    void UpdateRoundText(int round)
    {
        if (roundText != null)
        {
            roundText.text = $"Round {round}";
        }
        Debug.Log($"[GameManager] Round {round} started");
    }

}
