using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text manaText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image artImage;

    private HandSpawner handSpawner;  // Reference to HandSpawner to handle the card selection
    private CardData cardData;        // The card data that this UI represents

    void Start()
    {
        // Find the HandSpawner in the scene to notify when this card is clicked
        handSpawner = FindObjectOfType<HandSpawner>();

        // Add click event listener to the button on the card
        GetComponent<Button>().onClick.AddListener(OnCardClicked);
    }

    public void Setup(CardData data)
    {
        if (nameText != null) nameText.text = data.cardName;
        if (manaText != null) manaText.text = data.manaCost.ToString();
        if (attackText != null) attackText.text = data.type == CardType.Spell ? "" : data.attack.ToString();
        if (healthText != null) healthText.text = data.type == CardType.Spell ? "" : data.health.ToString();
        if (descriptionText != null) descriptionText.text = data.description;
        if (artImage != null) artImage.sprite = data.art;
    }

    private void OnCardClicked()
    {
        Debug.Log("Card selected");
        handSpawner.SelectCardToPlace(cardData);  // Notify HandSpawner to select this card
    }
}
