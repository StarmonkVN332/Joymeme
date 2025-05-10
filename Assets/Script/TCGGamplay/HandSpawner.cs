using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class HandSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform handPanel;  // The panel in the 2D UI where the cards will appear
    [SerializeField] private GameObject cardUIPrefab;  // The card prefab to spawn in the hand
    [SerializeField] private List<CardData> cardsToSpawn;  // List of cards to be spawned in the hand

    private CardData selectedCard;  // The currently selected card to place on the lane

    void Start()
    {
        if (photonView.IsMine)
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

    public void SelectCardToPlace(CardData card)
    {
        selectedCard = card;  // Store the selected card for placement
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse click registered");
        }
        if (Input.GetMouseButtonDown(0))
        {
            Camera cam = Camera.main;
            Debug.Log("Camera.main: " + cam);

            if (cam != null)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);
                Debug.Log("Ray Origin: " + ray.origin + ", Direction: " + ray.direction);

                if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                {
                    Debug.Log("Raycast hit: " + hit.collider.name);
                }
                else
                {
                    Debug.Log("Raycast missed");
                }
            }
            else
            {
                Debug.LogWarning("Camera.main is null! Raycast cannot work.");
            }
        }

        // Check if the player has selected a card and clicks on the 3D world
        if (selectedCard != null && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.Log("Ray created at mouse position: " + Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f)) // no layer mask
            {
                Debug.Log("Raycast hit: " + hit.collider.name);
            }
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 1f);

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Raycast hit: " + hit.collider.name);

                if (hit.collider.CompareTag("Lane"))
                {
                    Debug.Log("Lane clicked: " + hit.collider.name);
                    Transform lane = hit.collider.transform;
                    PlaceCardInLane(lane, selectedCard);
                    selectedCard = null;
                }
            }
            else
            {
                Debug.Log("Raycast did not hit anything.");
            }
        }
    }

    void PlaceCardInLane(Transform lane, CardData card)
    {
        // Instantiate the 3D card prefab at the lane's position
        GameObject cardObject = Instantiate(cardUIPrefab, lane.position, Quaternion.identity);
        cardObject.GetComponent<CardUI>().Setup(card);

        // You can add additional game logic here, such as checking mana costs or triggering effects
    }
}
