using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI loreText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI atkText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image cardSprite;
    public void Setup(CardData data, UnitInstance liveUnit)
    {
        if (data == null) return;

        // Populate with base data from the CardData asset
        nameText.text = data.cardName;
        descriptionText.text = data.abilityDescription;
        loreText.text = data.description;
        costText.text = data.manaCost.ToString();
        cardSprite.sprite = data.cardImage;

        // If a live unit is provided, display its current stats.
        // Otherwise, display the base stats from the CardData.
        if (liveUnit != null)
        {
            atkText.text = liveUnit.currentAttack.ToString();
            healthText.text = liveUnit.currentHealth.ToString();
        }
        else
        {
            if (data.cardType == CardType.Unit)
            {
                atkText.text = data.attack.ToString();
                healthText.text = data.health.ToString();
            }
            else // For spell cards
            {
                atkText.text = "-";
                healthText.text = "-";
            }
        }
    }
}
