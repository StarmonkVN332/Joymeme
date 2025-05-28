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
        currentHealth = Mathf.Max(currentHealth, 0); // Clamp to zero

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            GameManager.Instance.UnitDied(photonView.ViewID);
        }
        UpdateHealth(currentHealth);
    }
    [PunRPC]
    public void RequestUnitDestroyRPC(int viewID)
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv != null)
        {
            if (!pv.IsMine)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    pv.TransferOwnership(PhotonNetwork.LocalPlayer);
                }
                else
                {
                    Debug.LogWarning("Cannot destroy object: not owner and not MasterClient.");
                    return;
                }
            }

            PhotonNetwork.Destroy(pv.gameObject);
        }
    }

    public int GetHealth() => currentHealth;
    public int GetAttack() => currentAttack;
    [PunRPC]
    public void RPC_SetHealth(int newHealth)
    {
        Debug.Log($"[UnitInstance] RPC_SetHealth called. New health: {newHealth}");
        currentHealth = Mathf.Max(newHealth, 0); // Clamp to zero
        UpdateHealth(newHealth);
        
        //if (currentHealth <= 0)
        //{
        //    if (PhotonNetwork.IsMasterClient)
        //    {
        //        GameManager.Instance.UnitDied(photonView.ViewID);
        //    }
        //}
    }


    public void UpdateHealth(int newHealth)
    {
        if (healthText != null)
            healthText.text = Mathf.Max(newHealth, 0).ToString(); // Visual clamping
    }

    public void UpdateAttack(int newAttack)
    {
        if (attackText != null)
            attackText.text = newAttack.ToString();
    }
}
