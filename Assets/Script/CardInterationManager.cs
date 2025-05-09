using UnityEngine;

public class CardInteractionManager : MonoBehaviour
{
    public static CardInteractionManager Instance;

    private CardDisplay selectedCard;

    private void Awake()
    {
        Instance = this;
    }

    public bool CanSelect(CardDisplay card)
    {
        return card.transform.parent.name == "HandZone";
    }

    public void Select(CardDisplay card)
    {
        selectedCard = card;
    }

    public void TryPlaceCard(Transform laneTransform)
    {
        if (selectedCard == null) return;

        selectedCard.transform.SetParent(laneTransform);
        selectedCard.transform.localPosition = Vector3.zero;
        selectedCard = null;
    }
}
