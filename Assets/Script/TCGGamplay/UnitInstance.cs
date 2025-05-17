using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class UnitInstance : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    [SerializeField] private Image cardImage;      // UI Image for artwork (on world-space canvas)
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI attackText;

    private int cardId;

    // Called automatically when Photon instantiates this prefab
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // The instantiation data from Photon includes the cardId (passed from DropZone)
        object[] instantiationData = photonView.InstantiationData;
        if (instantiationData != null && instantiationData.Length > 0)
        {
            cardId = (int)instantiationData[0];
            InitializeVisuals();
        }
        else
        {
            Debug.LogError("Unit3D: No instantiation data found for cardId!");
        }
    }

    // Use this to initialize or refresh visuals locally
    private void InitializeVisuals()
    {
        CardData cardData = CardDatabase.Instance.GetCardById(cardId);
        if (cardData == null)
        {
            Debug.LogError($"Unit3D: CardData not found for cardId {cardId}");
            return;
        }

        if (cardImage != null && cardData.cardImage != null)
        {
            cardImage.sprite = cardData.cardImage;
        }

        if (healthText != null)
        {
            healthText.text = cardData.health.ToString();
        }

        if (attackText != null)
        {
            attackText.text = cardData.attack.ToString();
        }
    }

    // Example: method to update health visually if it changes later
    public void UpdateHealth(int newHealth)
    {
        if (healthText != null)
        {
            healthText.text = newHealth.ToString();
        }
    }

    // Example: method to update attack visually if it changes later
    public void UpdateAttack(int newAttack)
    {
        if (attackText != null)
        {
            attackText.text = newAttack.ToString();
        }
    }
}
