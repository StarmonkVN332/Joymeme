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

        if (!CanPlayCard(card.cardData))
        {
            Debug.Log($"DropZone: Cannot play card {card.cardData.cardName} on UI lane {localLaneIndex}");
            return;
        }

        if (card.cardData.cardType == CardType.Unit && card.cardData.unitPrefab != null)
        {
            Vector3 spawnPos = correspondingLane.GetUnitSpawnPosition();
            Quaternion spawnRot = PhotonNetwork.LocalPlayer.ActorNumber == 1 ? Quaternion.identity : Quaternion.Euler(0, 180, 0);

            GameObject unit = PhotonNetwork.Instantiate(card.cardData.unitPrefab.name, spawnPos, spawnRot, 0, new object[] { card.cardData.cardId });

            bool isOwnerView = PhotonNetwork.LocalPlayer.ActorNumber == correspondingLane.PlayerOwnerId;
            correspondingLane.photonView.RPC("RPC_AddUnit", RpcTarget.AllBuffered, unit.GetComponent<PhotonView>().ViewID, isOwnerView);
        }
        else if (card.cardData.cardType == CardType.Spell)
        {
            Debug.Log($"DropZone: Spell '{card.cardData.cardName}' played on UI lane {localLaneIndex}");
        }

        Destroy(card.gameObject);
    }
}
