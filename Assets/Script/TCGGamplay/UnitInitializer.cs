using UnityEngine;
using Photon.Pun;

public class UnitInitializer : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    public UnitStats unitStats;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (photonView.InstantiationData != null && photonView.InstantiationData.Length > 0)
        {
            int cardId = (int)photonView.InstantiationData[0];
            CardData card = CardDatabase.Instance.GetCardById(cardId);

            if (card != null)
            {
                unitStats.attack = card.attack;
                unitStats.health = card.health;
                Debug.Log($"[UnitInitializer] Initialized '{card.cardName}' with {card.attack}/{card.health}");
            }
            else
            {
                Debug.LogError($"[UnitInitializer] Card ID '{cardId}' not found in database!");
            }
        }
    }
}
