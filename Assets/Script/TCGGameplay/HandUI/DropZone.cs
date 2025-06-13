using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;

public class DropZone : MonoBehaviour, IDropHandler
{
    public int localLaneIndex; // New: Index from player UI perspective (0–4)
    [SerializeField] private Lane3D correspondingLane;

    public void LinkToLane(Lane3D lane)
    {
        correspondingLane = lane;
    }

    public bool CanPlayCard(CardData cardData)
    {
        if (GameManager.Instance.CurrentTurnActorId != PhotonNetwork.LocalPlayer.ActorNumber)
            return false;

        if (GameManager.Instance.CurrentPhase == GamePhase.Combat ||
            GameManager.Instance.CurrentPhase == GamePhase.Draw)
            return false;

        if (cardData == null)
        {
            Debug.LogWarning("DropZone: CardData is null!");
            return false;
        }

        if (cardData.cardType == CardType.Unit && correspondingLane != null && !correspondingLane.CanAddUnit())
        {
            Debug.Log("DropZone: Lane is occupied!");
            return false;
        }

        return true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableCard card = eventData.pointerDrag?.GetComponent<DraggableCard>();
        if (card == null || card.cardData == null || correspondingLane == null)
        {
            Debug.LogWarning("DropZone: Invalid drop!");
            return;
        }

        if (!CanPlayCard(card.cardData))
        {
            Debug.Log($"DropZone: Cannot play card {card.cardData.cardName} on UI lane {localLaneIndex}");
            return;
        }

        if (card.cardData.cardType == CardType.Unit)
        {
            // Ask MasterClient to handle the unit spawn
            PhotonView gameManagerView = PhotonView.Get(GameManager.Instance);
            gameManagerView.RPC("RPC_RequestPlayUnit", RpcTarget.MasterClient,
                card.cardData.cardId, correspondingLane.BoardLaneIndex, PhotonNetwork.LocalPlayer.ActorNumber);
        }
        else if (card.cardData.cardType == CardType.Spell)
        {
            Debug.Log($"DropZone: Spell '{card.cardData.cardName}' played on UI lane {localLaneIndex}");
        }

        // Destroy card locally after drop — optional: delay if you want server confirmation
        Destroy(card.gameObject);
    }

}
