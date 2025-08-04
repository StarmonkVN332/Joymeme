using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;
using Photon.Pun.Demo.PunBasics;

public class ConnectManager : MonoBehaviourPunCallbacks
{
    // --- NEW: Globally accessible and synchronized game state ---
    public static bool GameStarted { get; private set; }

    [Header("Connect Setting")]
    [SerializeField] private TextMeshProUGUI textState;
    [SerializeField] private TMP_InputField ipRoomName;
    [SerializeField] private Button buttonCreateRoom;
    [SerializeField] private Button buttonJoinRoom;

    [Header("Create Room")]
    [SerializeField] private GameObject createRoomPanel;
    [SerializeField] private GameObject panelRoom;
    [SerializeField] private TextMeshProUGUI nameRoom;
    [SerializeField] private TextMeshProUGUI joinFailed;

    [Header("Room")]
    [SerializeField] private GameObject panelRoomUI;
    [SerializeField] private Button quitRoom;
    [SerializeField] private Button startGame;
    [SerializeField] private GameObject menuUi;

    [Header("Spawn Point Player")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Player Name Input")]
    [SerializeField] private TMP_InputField playerNameInput;  // InputField cho tên người chơi
    [SerializeField] private TextMeshProUGUI playerNameText1;
    [SerializeField] private TextMeshProUGUI playerNameText2;
    private string playerName;

    void Start()
    {
        // Initialize the game state
        GameStarted = false;

        // Load nhân vật từ file JSON
        PlayerData data = SaveManagerCharacter.Load();

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["Character"] = data.selectedCharacter;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        Debug.Log("Đã load nhân vật: " + data.selectedCharacter);

        PhotonNetwork.ConnectUsingSettings();

        buttonCreateRoom.onClick.AddListener(CreateRoom);
        buttonJoinRoom.onClick.AddListener(JoinRoom);
        quitRoom.onClick.AddListener(LeaveRoom);
        startGame.onClick.AddListener(OnClickStartGame);
    }
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby();
        textState.text = "Loading...";
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        textState.text = "Connected";
    }

    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;

        PhotonNetwork.NickName = playerNameInput.text;

        PhotonNetwork.CreateRoom(ipRoomName.text, roomOptions);
        nameRoom.text = "Code: " + ipRoomName.text;
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Debug.Log("Create room successful");
        if (createRoomPanel != null)
        {
            createRoomPanel.SetActive(false);
            panelRoom.SetActive(true);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log("Create room failed: " + message);
    }

    public void JoinRoom()
    {
        PhotonNetwork.NickName = playerNameInput.text;
        PhotonNetwork.JoinRoom(ipRoomName.text);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Joined room seccessful");

        playerName = playerNameInput.text;
        nameRoom.text = "Code: " + PhotonNetwork.CurrentRoom.Name;

        Player[] players = PhotonNetwork.PlayerList;
        if (players.Length >= 1)
            playerNameText1.text = "Player 1: " + players[0].NickName;
        if (players.Length >= 2)
            playerNameText2.text = "Player 2: " + players[1].NickName;

        if (createRoomPanel != null)
        {
            createRoomPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("createRoomPanel is not assigned!");
        }

        if (panelRoomUI != null)
        {
            panelRoomUI.SetActive(true);
        }
        else
        {
            Debug.LogError("panelRoomUI is not assigned!");
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            if (startGame != null)
            {
                startGame.gameObject.SetActive(false);
            }
        }
        UpdatePlayerListUI();
        CheckStartGameCondition();
        SpawnPlayer();
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        joinFailed.text = "Room does not exist";
    }

    public void SpawnPlayer()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            int index = PhotonNetwork.LocalPlayer.ActorNumber == 1 ? 0 : 1; // Host là 1
            Transform spawnPoint = spawnPoints[index];

            GameObject player = PhotonNetwork.Instantiate("Player", spawnPoint.position, spawnPoint.rotation);

            if (!PhotonNetwork.IsMasterClient)
            {
                player.transform.Rotate(0f, 180f, 0f);
            }
        }
    }

    public void OnClickStartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("StartGame", RpcTarget.All);
        }
    }

    [PunRPC]
    public void StartGame()
    {
        // --- This is the key change ---
        // Set the global state to true for all players
        GameStarted = true;
        Debug.Log("Game has started! ConnectManager.GameStarted is now true.");

        panelRoomUI.SetActive(false);
        menuUi.SetActive(false);
    }

    public void LeaveRoom()
    {
        // --- Good practice: Reset the state when leaving a room ---
        GameStarted = false;

        PhotonNetwork.LeaveRoom();
        panelRoomUI.SetActive(false);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        UpdatePlayerListUI();
        Debug.Log("Đã rời phòng.");
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player joined: " + newPlayer.NickName);
        UpdatePlayerListUI();
        CheckStartGameCondition();
    }
    private void CheckStartGameCondition()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startGame.interactable = PhotonNetwork.CurrentRoom.PlayerCount == 2;
        }
    }
    private void UpdatePlayerListUI()
    {
        playerNameText1.text = "";
        playerNameText2.text = "";

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber == 1)
                playerNameText1.text = "Player: " + player.NickName;
            else if (player.ActorNumber == 2)
                playerNameText2.text = "Player: " + player.NickName;
        }
    }
}