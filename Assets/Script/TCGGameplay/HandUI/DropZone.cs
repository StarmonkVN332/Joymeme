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

    public void OnDrop(PointerEventData eventData) //Chỗ card sẽ spawn
    {
        DraggableCard card = eventData.pointerDrag?.GetComponent<DraggableCard>();
        if (card == null || card.cardData == null || correspondingLane == null)
        {
            Debug.LogWarning("DropZone: Invalid drop!");
            return;
        }
        else
        {
            Debug.Log("Valid Card");
        }

        if (!CanPlayCard(card.cardData))
        {
            Debug.Log($"DropZone: Cannot play card {card.cardData.cardName} on UI lane {localLaneIndex}");
            return;
        }

        GameManager gameManager = FindObjectOfType<GameManager>();

        if (gameManager != null)
        {
            gameManager.photonView.RPC(
                "RPC_RequestCardDeploy",
                RpcTarget.MasterClient,
                PhotonNetwork.LocalPlayer.ActorNumber,
                card.cardData.cardId,
                correspondingLane.PlayerOwnerId,
                localLaneIndex
            );
        }
        else
        {
            Debug.LogError("DropZone: Could not find GameManager to send RPC.");
        }

        Destroy(card.gameObject);
    }
}
