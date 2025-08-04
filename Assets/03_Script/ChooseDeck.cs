using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro; // Added for optional text feedback

public class ChooseDeck : MonoBehaviourPunCallbacks
{
    [Header("Deck Assets")]
    [SerializeField] private DeckData memeDeck;
    [SerializeField] private DeckData minecraftDeck;

    [Header("UI References")]
    [SerializeField] private Button memeDeckBtn;
    [SerializeField] private Button minecraftDeckBtn;
    [SerializeField] private TextMeshProUGUI feedbackText; // Optional: Text to show selection

    public const string DECK_CHOICE_PROPERTY = "deckName";

    private void Start()
    {
        // Make buttons interactable at the start
        SetButtonsInteractable(true);
        if (feedbackText != null) feedbackText.text = "Choose your deck";

        memeDeckBtn.onClick.AddListener(() => SelectDeck(memeDeck));
        minecraftDeckBtn.onClick.AddListener(() => SelectDeck(minecraftDeck));
    }

    public void SelectDeck(DeckData selectedDeck)
    {
        if (selectedDeck == null)
        {
            Debug.LogError("Selected DeckData is null!");
            return;
        }

        // Create a Hashtable to store the custom property.
        // This is Photon's way of saving extra data to a player.
        Hashtable playerProps = new Hashtable();
        playerProps[DECK_CHOICE_PROPERTY] = selectedDeck.name; // Use the asset's file name

        // Set the custom property for the local player. This syncs across the network.
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        Debug.Log($"Deck selected: {selectedDeck.name}. Property sent to Photon.");

        // Provide UI feedback
        SetButtonsInteractable(false);
        if (feedbackText != null) feedbackText.text = $"Deck Selected:\n{selectedDeck.name}";
    }

    // A helper function to enable/disable buttons
    private void SetButtonsInteractable(bool isInteractable)
    {
        memeDeckBtn.interactable = isInteractable;
        minecraftDeckBtn.interactable = isInteractable;
    }

    // Optional: If a player leaves and rejoins, re-enable the buttons
    public override void OnLeftRoom()
    {
        SetButtonsInteractable(true);
        if (feedbackText != null) feedbackText.text = "Choose your deck";
    }
}
