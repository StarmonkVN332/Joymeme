using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class HandSpawner : MonoBehaviourPunCallbacks
{
    public Transform handPanel; // assign via Inspector
    public GameObject cardUIPrefab;
    public List<CardData> cardsToSpawn;
    void Start()
    {
        GameObject roomUI = GameObject.FindWithTag("RoomUi");
        if (photonView.IsMine && GameObject.FindWithTag("RoomUi") == null)
        {
            SpawnInitialHand();
        }
    }

    void SpawnInitialHand()
    {
        foreach (CardData card in cardsToSpawn)
        {
            GameObject cardGO = Instantiate(cardUIPrefab, handPanel);
            cardGO.GetComponent<CardUI>().Setup(card);
        }
    }
}
