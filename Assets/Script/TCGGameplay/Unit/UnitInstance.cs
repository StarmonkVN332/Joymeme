using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitInstance : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    [SerializeField] private Image cardImage;      // UI Image for artwork (on world-space canvas)
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI manaText;

    private Animator animator;

    private int sideId;
    private int cardId;
    private int currentHealth;
    private int currentAttack;
    private int currentMana;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    // Called automatically when Photon instantiates this prefab
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = photonView.InstantiationData;
        if (instantiationData != null && instantiationData.Length >= 2)
        {
            cardId = (int)instantiationData[0];
            sideId = (int)instantiationData[1]; // ✅ assign immediately
            InitializeVisuals();
        }
        else
        {
            Debug.LogError("UnitInstance: Instantiation data missing sideId!");
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
        currentMana = cardData.manaCost;

        if (cardImage != null && cardData.cardImage != null)
            cardImage.sprite = cardData.cardImage;

        UpdateHealth(currentHealth);
        UpdateAttack(currentAttack);
        UpdateMana(currentMana);

    }
    public void SetSide(int id) => sideId = id;
    public int GetSide() => sideId;
    [PunRPC]
    public void RPC_PlayAnimation(string type)
    {
        string trigger = "";

        switch (type)
        {
            case "Attack":
                trigger = sideId == 0 ? "Attack1" : "Attack2";
                break;
            case "AttackPlayer":
                trigger = sideId == 0 ? "P1UnitAtk" : "P2UnitAtk";
                break;
            case "Damaged":
                trigger = "Damaged";
                break;
            case "Die":
                trigger = "Die";
                break;
            case "Entry":
                trigger = sideId == 0 ? "Entry1" : "Entry2";
                break;
        }

        Debug.Log($"[UnitInstance] Playing trigger '{trigger}' on side {sideId}");
        animator?.SetTrigger(trigger);
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
            attackText.text = Mathf.Max(newAttack, 0).ToString(); // Clamp for UI
    }

    public void UpdateMana(int newMana)
    {
        if (manaText != null)
            manaText.text = Mathf.Max(newMana, 0).ToString(); // Clamp for UI
    }
}
