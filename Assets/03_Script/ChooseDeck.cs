using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class ChooseDeck : MonoBehaviourPunCallbacks
{
    [Header("Deck Assets")]
    [SerializeField] private DeckData memeDeck;
    [SerializeField] private DeckData minecraftDeck;

    [Header("UI References")]
    [SerializeField] private Button memeDeckBtn;
    [SerializeField] private Button minecraftDeckBtn;

    public const string DECK_CHOICE_PROPERTY = "deckName";

    private void Start()
    {
        // Gán sự kiện cho các nút chọn bài
        memeDeckBtn.onClick.AddListener(() => SelectDeck(memeDeck));
        minecraftDeckBtn.onClick.AddListener(() => SelectDeck(minecraftDeck));
    }

    public void SelectDeck(DeckData selectedDeck)
    {
        Hashtable playerProps = new Hashtable();
        playerProps[DECK_CHOICE_PROPERTY] = selectedDeck.name;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
        Debug.Log($"Bạn đã chọn: {selectedDeck.name}.");
    }

}