using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandSpawner : MonoBehaviourPunCallbacks
{
    [Header("Data")]
    [SerializeField] private DeckDatabase deckDatabase;

    [SerializeField] private Transform handPanel;
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private DeckData startingDeck;
    [SerializeField] private DropZone[] dropZones;
    [SerializeField] private HandManager handManager;

    [SerializeField] private AudioSource songGame;

    private List<CardData> remainingDeck;

    private void Awake()
    {
        Debug.Log("awake da chay dell dau");
        // Chỉ thực thi logic cho người chơi sở hữu đối tượng này
        if (!photonView.IsMine) return;

        var owner = photonView.Owner;
        // Thử đọc lựa chọn deck từ Custom Properties
        if (owner.CustomProperties.TryGetValue(ChooseDeck.DECK_CHOICE_PROPERTY, out object deckNameObject))
        {
            string deckName = deckNameObject as string;
            startingDeck = deckDatabase.GetDeckByName(deckName);
            Debug.Log($"[HandSpawner] Người chơi {owner.NickName} sử dụng bộ bài: {deckName}");
        }

        // Kiểm tra nếu không tìm thấy deck (quan trọng)
        if (startingDeck == null)
        {
            Debug.LogError($"[HandSpawner] Không tìm thấy deck cho người chơi {owner.NickName}! Hãy chắc chắn deck đã được chọn và có trong DeckDatabase.");
            return;
        }
        Debug.Log("check thanh cong deck");
        // Khởi tạo và xào bài
        remainingDeck = new List<CardData>(startingDeck.cards);
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
            //for (int i = 0; i < 4; i++)
            //    DrawCard(false); // No layout yet

            StartCoroutine(DelayedLayout());
            LinkDropZonesToLanes();
        }
        songGame.Play();
    }

    private IEnumerator DelayedLayout()
    {
        yield return null; // wait one frame
        GetComponent<HandLayout>()?.RepositionCards();
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
        Debug.Log("Game Stared!");
        Debug.Log("Song playing!!");
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
                    Debug.Log($"[HandSpawner] Linked DropZone (UI index {dz.localLaneIndex}) to Lane3D (Board index {lane.BoardLaneIndex}) for Player {myId}");
                    break;
                }
            }
        }
    }


}
