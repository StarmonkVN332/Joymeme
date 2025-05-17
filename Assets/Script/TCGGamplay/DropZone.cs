using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;

public class DropZone : MonoBehaviour, IDropHandler
{
    public int laneIndex; // 0–4
    [SerializeField] private Lane3D correspondingLane;

    public void LinkToLane(Lane3D lane)
    {
        correspondingLane = lane;
    }

    public bool CanPlayCard(CardData cardData)
    {
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
        var card = eventData.pointerDrag?.GetComponent<DraggableCard>();
        if (card == null || card.cardData == null || correspondingLane == null)
        {
            Debug.LogWarning("DropZone: Invalid drop (missing card, cardData, or lane)!");
            return;
        }

        if (!CanPlayCard(card.cardData))
        {
            Debug.Log($"DropZone: Cannot play card {card.cardData.cardName} on lane {laneIndex}");
            return;
        }

        if (card.cardData.cardType == CardType.Unit && card.cardData.unitPrefab != null)
        {
            Vector3 spawnPos = correspondingLane.GetUnitSpawnPosition();
            Quaternion spawnRot = PhotonNetwork.LocalPlayer.ActorNumber == 1 ? Quaternion.identity : Quaternion.Euler(0, 180, 0);

            Debug.Log($"DropZone: Instantiating unit '{card.cardData.unitPrefab.name}' at {spawnPos}");

            GameObject unit = PhotonNetwork.Instantiate(card.cardData.unitPrefab.name, spawnPos, spawnRot, 0, new object[] { card.cardData.cardId });

            // **Do NOT set parent here locally!**

            // Notify all clients to parent and register the unit on the lane
            bool isOwnerView = PhotonNetwork.LocalPlayer.ActorNumber == correspondingLane.PlayerOwnerId;
            correspondingLane.photonView.RPC("RPC_AddUnit", RpcTarget.AllBuffered, unit.GetComponent<PhotonView>().ViewID, isOwnerView);

            Debug.Log($"DropZone: Called RPC_AddUnit for unit with ViewID {unit.GetComponent<PhotonView>().ViewID}");
        }
        else if (card.cardData.cardType == CardType.Spell)
        {
            Debug.Log($"DropZone: Spell '{card.cardData.cardName}' played on lane {laneIndex}");
        }

        Destroy(card.gameObject);
    }
}
