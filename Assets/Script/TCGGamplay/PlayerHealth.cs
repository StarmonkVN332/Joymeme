using UnityEngine;
using Photon.Pun;

public class PlayerHealth : MonoBehaviourPun
{
    public int playerId;
    public int maxHP = 20;
    public int currentHP;

    private void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        Debug.Log($"Player {playerId} took {amount} damage. HP: {currentHP}");

        if (currentHP <= 0)
        {
            GameManager.Instance.photonView.RPC("RPC_GameOver", RpcTarget.All, playerId);
        }
    }
}
