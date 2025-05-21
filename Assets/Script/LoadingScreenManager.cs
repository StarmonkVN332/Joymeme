using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LoadingScreenManager : MonoBehaviourPunCallbacks
{
    public static LoadingScreenManager Instance;

    [SerializeField] private GameObject loadingScreenPanel;

    private int playersReady = 0;
    private int totalPlayers = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        totalPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
        loadingScreenPanel.SetActive(true);

        // The local player is ready after spawning, so immediately tell the master
        photonView.RPC("PlayerIsReadyRPC", RpcTarget.MasterClient);
    }

    [PunRPC]
    public void PlayerIsReadyRPC()
    {
        playersReady++;
        Debug.Log($"Players ready: {playersReady}/{totalPlayers}");

        if (playersReady >= totalPlayers)
        {
            Debug.Log("All players ready! Telling everyone to start game.");
            photonView.RPC("AllPlayersReadyStartGameRPC", RpcTarget.All);
        }
    }

    [PunRPC]
    void AllPlayersReadyStartGameRPC()
    {
        loadingScreenPanel.SetActive(false);

        // You can also call a GameManager function here if needed
        GameManager.Instance?.StartGameAfterLoading();
    }
}
