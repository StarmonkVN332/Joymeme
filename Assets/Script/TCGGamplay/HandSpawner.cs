using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;

public class HandSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform handPanel;
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private List<CardData> cardsToSpawn;
    [SerializeField] private DropZone[] dropZones;
    IEnumerator Start()
    {
        GameObject roomUI = GameObject.FindWithTag("RoomUi");

        // Nếu roomUI tồn tại, chờ cho tới khi nó bị tắt
        while (roomUI != null && roomUI.activeInHierarchy)
        {
            yield return null; // đợi 1 frame
        }

        if (photonView.IsMine)
        {
            SpawnInitialHand();
            LinkDropZonesToLanes(); 
        }
    }


    void LinkDropZonesToLanes()
    {
        Lane3D[] allLanes = FindObjectsOfType<Lane3D>();
        int myId = PhotonNetwork.LocalPlayer.ActorNumber;
        Debug.Log("myId");
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
            // Instantiate the card UI prefab
            GameObject cardGO = Instantiate(cardUIPrefab, handPanel);

            // Setup the card UI
            CardUI cardUI = cardGO.GetComponent<CardUI>();
            cardUI.Setup(card);

            // Find and set the DraggableCard component
            DraggableCard draggableCard = cardGO.GetComponent<DraggableCard>();
            if (draggableCard != null)
            {
                draggableCard.cardData = card;  // Assign the card data to the draggable card
            }

            Debug.Log("Card spawned with data: " + card.cardName);
        }

        // Optional: realign after spawn
        GetComponent<HandLayout>()?.RepositionCards();
    }

}
