using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [SerializeField] private TMP_Text phaseText;

    private int firstPlayerId;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(WaitForRoomUIAndPlayers());
    }

    IEnumerator WaitForRoomUIAndPlayers()
    {
        Debug.Log("[GameManager] Waiting for RoomUI to be disabled...");

        GameObject roomUI = GameObject.FindWithTag("RoomUi");
        while (roomUI != null && roomUI.activeInHierarchy)
        {
            yield return null;
        }

        Debug.Log("[GameManager] RoomUI disabled. Waiting for 2 players...");

        while (PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            yield return null;
        }

        Debug.Log("[GameManager] 2 players ready.");

        if (PhotonNetwork.IsMasterClient)
        {
            int startingPlayer = Random.Range(1, 3); // Randomly pick actor number 1 or 2
            photonView.RPC("StartGameWithFirstPlayer", RpcTarget.All, startingPlayer);
        }
    }


    IEnumerator WaitForPlayersAndStart()
    {
        Debug.Log("[GameManager] Waiting for 2 players to join...");

        while (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            yield return null;
        }

        Debug.Log("[GameManager] 2 players joined!");

        if (PhotonNetwork.IsMasterClient)
        {
            int startingPlayer = Random.Range(1, 3); // ActorNumbers are typically 1 and 2
            photonView.RPC("StartGameWithFirstPlayer", RpcTarget.All, startingPlayer);
        }
    }

    [PunRPC]
    private void StartGameWithFirstPlayer(int startingPlayer)
    {
        Debug.Log($"[GameManager] Starting game. First player: {startingPlayer}");
        firstPlayerId = startingPlayer;
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        int round = 1;

        while (true)
        {
            Debug.Log($"[GameManager] Round {round} start");

            yield return StartCoroutine(DrawPhase());

            if (PhotonNetwork.LocalPlayer.ActorNumber == firstPlayerId)
            {
                yield return StartCoroutine(Player1Phase());
                yield return StartCoroutine(Player2Phase());
            }
            else
            {
                yield return StartCoroutine(Player2Phase());
                yield return StartCoroutine(Player1Phase());
            }

            yield return StartCoroutine(CombatPhase());

            round++;
        }
    }

    IEnumerator DrawPhase()
    {
        Debug.Log("[GameManager] Draw Phase started");
        SetPhaseText("Draw Phase");

        photonView.RPC("DrawCardForPlayer", RpcTarget.All);

        yield return new WaitForSeconds(2f); // Wait for cards to be drawn
    }

    [PunRPC]
    private void DrawCardForPlayer()
    {
        Debug.Log($"[GameManager] {PhotonNetwork.NickName} is drawing a card");
        HandSpawner handSpawner = FindObjectOfType<HandSpawner>();
        handSpawner?.DrawCard();
    }

    IEnumerator Player1Phase()
    {
        SetPhaseText("Player 1 Placement");
        Debug.Log("[GameManager] Player 1 Phase");
        yield return new WaitForSeconds(5f); // Placeholder for player input
    }

    IEnumerator Player2Phase()
    {
        SetPhaseText("Player 2 Placement");
        Debug.Log("[GameManager] Player 2 Phase");
        yield return new WaitForSeconds(5f); // Placeholder for player input
    }

    IEnumerator CombatPhase()
    {
        SetPhaseText("Combat Phase");
        Debug.Log("[GameManager] Combat Phase");
        yield return new WaitForSeconds(5f); // Placeholder for combat logic
    }

    void SetPhaseText(string phase)
    {
        if (phaseText != null)
        {
            Debug.Log($"[GameManager] Phase text set to: {phase}");
            phaseText.text = phase;
        }
        else
        {
            Debug.LogWarning("[GameManager] phaseText is not assigned!");
        }
    }
}
