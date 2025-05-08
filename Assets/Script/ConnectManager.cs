using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;

public class ConnectManager : MonoBehaviourPunCallbacks
{
    [Header("Connect Setting")]
    [SerializeField] private TextMeshProUGUI textState;
    [SerializeField] private TMP_InputField ipRoomName;
    [SerializeField] private Button buttonCreateRoom;
    [SerializeField] private Button buttonJoinRoom;

    [Header("Create Room")]
    [SerializeField] private GameObject createRoomPanel;
    [SerializeField] private GameObject panelRoom;
    [SerializeField] private TextMeshProUGUI nameRoom;

    [Header("Room")]
    [SerializeField] private GameObject panelRoomUI;
    [SerializeField] private Button quitRoom;
    [SerializeField] private Button startGame;
    [SerializeField] private GameObject menuUi;

    [Header("Spawn Point Player")]
    [SerializeField] private Transform[] spawnPoints;

    void Start()
    {
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

        PhotonNetwork.CreateRoom(ipRoomName.text, roomOptions);
        nameRoom.text = "Tên phòng: " + ipRoomName.text;
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
        PhotonNetwork.JoinRoom(ipRoomName.text);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Joined room seccessful");
        if (createRoomPanel != null)
        {
            createRoomPanel.SetActive(false);
        }
        panelRoomUI.SetActive(true);
        if (!PhotonNetwork.IsMasterClient)
        {
            startGame.gameObject.SetActive(false);
        }
        SpawnPlayer();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        Debug.Log("Join room failed: " + message);
    }

    public void SpawnPlayer()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            int index = PhotonNetwork.LocalPlayer.ActorNumber == 1 ? 0 : 1; // Host là 1
            Transform spawnPoint = spawnPoints[index];
            PhotonNetwork.Instantiate("Player", spawnPoint.position, spawnPoint.rotation);
        }
    }
    public void OnClickStartGame()
    {
        // Chỉ host mới được gọi RPC này
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("StartGame", RpcTarget.All);
        }
    }

    [PunRPC]
    public void StartGame()
    {
        panelRoomUI.SetActive(false);
        menuUi.SetActive(false);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        panelRoomUI.SetActive(false);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        Debug.Log("Đã rời phòng.");
    }
}
