using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitInstance : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    [SerializeField] private Image cardImage;      // UI Image for artwork (on world-space canvas)
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI attackText;

    private int cardId;
    private int currentHealth;
    private int currentAttack;

    // Called automatically when Photon instantiates this prefab
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
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

    // Use this to initialize or refresh visuals locally and set current stats
    private void InitializeVisuals()
    {
        CardData cardData = CardDatabase.Instance.GetCardById(cardId);
        if (cardData == null)
        {
            Debug.LogError($"Unit3D: CardData not found for cardId {cardId}");
            return;
        }

        currentHealth = cardData.health;
        currentAttack = cardData.attack;

        if (cardImage != null && cardData.cardImage != null)
            cardImage.sprite = cardData.cardImage;

        UpdateHealth(currentHealth);
        UpdateAttack(currentAttack);
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            GameManager.Instance.UnitDied(photonView.ViewID);
        }
        UpdateHealth(currentHealth);
    }

    public int GetHealth() => currentHealth;
    public int GetAttack() => currentAttack;

    [PunRPC]
    public void RPC_SetHealth(int newHealth)
    {
        currentHealth = newHealth;
        UpdateHealth(newHealth);
    }

    public void UpdateHealth(int newHealth)
    {
        if (healthText != null)
            healthText.text = newHealth.ToString();
    }

    public void UpdateAttack(int newAttack)
    {
        if (attackText != null)
            attackText.text = newAttack.ToString();
    }
}
