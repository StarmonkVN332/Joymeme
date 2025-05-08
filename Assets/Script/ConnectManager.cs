using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;

public class ConnectManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI textState;
    [SerializeField] private TMP_InputField ipRoomName;
    public Button buttonCreateRoom;
    public Button buttonJoinRoom;

    public GameObject createRoomPanel;



    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();

        buttonCreateRoom.onClick.AddListener(CreateRoom);
        buttonJoinRoom.onClick.AddListener(JoinRoom);
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
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Debug.Log("Create room successful");
        if (createRoomPanel != null)
        {
            createRoomPanel.SetActive(false);
        }
        //SpawnPlayer();
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
            PhotonNetwork.Instantiate("Player", new Vector3(0, 1, 0), Quaternion.identity);
        }
    }
}
