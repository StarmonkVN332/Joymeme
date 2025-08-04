using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandSpawner : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] private Transform handPanel;
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private DropZone[] dropZones;
    [SerializeField] private HandManager handManager;
    [SerializeField] private AudioSource songGame;

    private DeckData startingDeck;
    private List<CardData> remainingDeck;

    // Awake is now empty, as the logic needs to be delayed.
    private void Awake()
    {
    }

    // --- THIS IS THE NEW METHOD ---
    // The GameManager will call this method at the correct time.
    public void InitializeDeck()
    {
        if (!photonView.IsMine) return;

        var owner = photonView.Owner;

        if (owner.CustomProperties.TryGetValue(ChooseDeck.DECK_CHOICE_PROPERTY, out object deckNameObject))
        {
            string deckName = deckNameObject as string;
            string resourcePath = "Decks/" + deckName;
            startingDeck = Resources.Load<DeckData>(resourcePath);

            Debug.Log($"[HandSpawner] Player {owner.NickName} chose deck '{deckName}'. Attempting to load from '{resourcePath}'.");
        }

        // Add a fallback to a default deck if none was chosen
        if (startingDeck == null)
        {
            Debug.LogWarning($"[HandSpawner] No deck choice found for player {owner.NickName}. Loading a default deck.");
            startingDeck = Resources.Load<DeckData>("Decks/memeDeck"); // Make sure you have a default deck with this name
        }

        if (startingDeck == null)
        {
            Debug.LogError($"[HandSpawner] FAILED to load any deck for player {owner.NickName}!");
            return;
        }

        Debug.Log($"[HandSpawner] Successfully loaded deck: {startingDeck.name}");

        remainingDeck = new List<CardData>(startingDeck.cards);
        ShuffleDeck(remainingDeck);
    }


    // --- The rest of your HandSpawner script remains the same ---
    #region Existing Methods
    IEnumerator Start()
    {
        GameObject roomUI = GameObject.FindWithTag("RoomUi");
        while (roomUI != null && roomUI.activeInHierarchy)
        {
            yield return null;
        }

        if (photonView.IsMine)
        {
            StartCoroutine(DelayedLayout());
            LinkDropZonesToLanes();
        }
        songGame.Play();
    }

    private IEnumerator DelayedLayout()
    {
        yield return null;
        GetComponent<HandLayout>()?.RepositionCards();
    }
    public void DrawCard(bool repositionAfter = true)
    {
        if (!photonView.IsMine || remainingDeck == null || remainingDeck.Count == 0)
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
        bool isSecondPlayer = myId != 1;

        foreach (DropZone dz in dropZones)
        {
            int boardIndex = isSecondPlayer ? 4 - dz.localLaneIndex : dz.localLaneIndex;

            foreach (Lane3D lane in allLanes)
            {
                if (lane.PlayerOwnerId == myId && lane.BoardLaneIndex == boardIndex)
                {
                    dz.LinkToLane(lane);
                    break;
                }
            }
        }
    }
    #endregion
}
