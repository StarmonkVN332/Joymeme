using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    public Image cardArt;
    public TMP_Text nameText;
    public TMP_Text descText;
    public TMP_Text manaText;
    public TMP_Text attackText;
    public TMP_Text healthText;

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
