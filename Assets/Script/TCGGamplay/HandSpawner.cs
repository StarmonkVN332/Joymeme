using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform handPanel;
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private List<CardData> cardPool;
    [SerializeField] private DropZone[] dropZones;
    [SerializeField] private HandManager handManager;

    private List<CardData> remainingDeck;

    private void Awake()
    {
        remainingDeck = new List<CardData>(cardPool);
        ShuffleDeck(remainingDeck);
    }

    IEnumerator Start()
    {
        GameObject roomUI = GameObject.FindWithTag("RoomUi");
        while (roomUI != null && roomUI.activeInHierarchy)
        {
            yield return null;
        }

        if (photonView.IsMine)
        {
            for (int i = 0; i < 4; i++)
                DrawCard(false); // No layout yet

            GetComponent<HandLayout>()?.RepositionCards(); // Layout once
            LinkDropZonesToLanes();
        }
    }


    public void DrawCard(bool repositionAfter = true)
    {
        if (!photonView.IsMine || remainingDeck.Count == 0)
            return;

        CardData card = remainingDeck[0];
        remainingDeck.RemoveAt(0);

        GameObject cardGO = Instantiate(cardUIPrefab, handPanel);
        CardUI cardUI = cardGO.GetComponent<CardUI>();
        cardUI.Setup(card);

        DraggableCard draggableCard = cardGO.GetComponent<DraggableCard>();
        if (draggableCard != null)
            draggableCard.cardData = card;

        handManager?.AddCardToHand(cardGO);

        if (repositionAfter)
            GetComponent<HandLayout>()?.RepositionCards();

        Debug.Log($"[HandSpawner] Drew card: {card.cardName}");
    }

    void ShuffleDeck(List<CardData> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = Random.Range(i, deck.Count);
            (deck[i], deck[randomIndex]) = (deck[randomIndex], deck[i]);
        }
    }

    void LinkDropZonesToLanes()
    {
        Lane3D[] allLanes = FindObjectsOfType<Lane3D>();
        int myId = PhotonNetwork.LocalPlayer.ActorNumber;

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
}
