using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    [SerializeField] private Image cardArt;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text manaText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text healthText;

    public void Setup(CardData data)
    {
        cardArt.sprite = data.cardImage;
        nameText.text = data.cardName;
        descText.text = data.description;
        manaText.text = data.manaCost.ToString();

        if (data.cardType == CardType.Unit)
        {
            attackText.text = data.attack.ToString();
            healthText.text = data.health.ToString();
        }
        else
        {
            attackText.text = "";
            healthText.text = "";
        }
    }
}
