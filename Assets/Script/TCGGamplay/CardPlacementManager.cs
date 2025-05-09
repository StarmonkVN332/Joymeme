using UnityEngine;
using Photon.Pun;

public class CardPlacementManager : MonoBehaviourPun
{
    public static CardPlacementManager Instance;

    [HideInInspector] public GameObject selectedCard;

    private void Awake()
    {
        Instance = this;
    }

    public void SelectCard(GameObject card)
    {
        if (!card.GetComponent<PhotonView>().IsMine)
            return;

        selectedCard = card;
    }

    public void TryPlaceSelectedCard(Transform laneTransform)
    {
        if (selectedCard == null) return;

        // Place card at the lane position
        selectedCard.transform.position = laneTransform.position;
        selectedCard = null;
    }
}
