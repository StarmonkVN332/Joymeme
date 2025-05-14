using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class HandSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform handPanel;
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private List<CardData> cardsToSpawn;
    [SerializeField] private DropZone[] dropZones;
    void Start()
    {
        GameObject roomUI = GameObject.FindWithTag("RoomUi");
        if (photonView.IsMine && GameObject.FindWithTag("RoomUi") == null)
        {
            LinkDropZonesToLanes();
            SpawnInitialHand();
        }
    }

    void LinkDropZonesToLanes()
    {
        Lane3D[] allLanes = FindObjectsOfType<Lane3D>();
        int myId = PhotonNetwork.LocalPlayer.ActorNumber;
        Debug.Log(myId);
        foreach (DropZone dz in dropZones)
        {
            foreach (Lane3D lane in allLanes)
            {
                if (lane.playerOwnerId == photonView.OwnerActorNr && lane.laneIndex == dz.laneIndex)
                {
                    dz.LinkToLane(lane);
                    break;
                }
            }
        }
    }

    void SpawnInitialHand()
    {
        foreach (CardData card in cardsToSpawn)
        {
            GameObject cardGO = Instantiate(cardUIPrefab, handPanel);
            cardGO.GetComponent<CardUI>().Setup(card);
        }

        // Optional: realign after spawn
        GetComponent<HandLayout>()?.RepositionCards();
    }
}
