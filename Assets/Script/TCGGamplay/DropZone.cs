using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using System.Collections;

public class DropZone : MonoBehaviour, IDropHandler
{
    public int laneIndex; // 0–4, matches Lane3D.laneIndex
    [SerializeField] private Lane3D correspondingLane; // Linked lane

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
            return;
        }

        if (card.cardData.cardType == CardType.Unit)
        {
            if (card.cardData.unitPrefab != null)
            {
                Vector3 spawnPos = correspondingLane.GetUnitSpawnPosition();
                Quaternion spawnRot = PhotonNetwork.LocalPlayer.ActorNumber == 1 ? Quaternion.identity : Quaternion.Euler(0, 180, 0);
                GameObject unit = PhotonNetwork.Instantiate(card.cardData.unitPrefab.name, spawnPos, spawnRot);
                correspondingLane.AddUnit(unit);
            }
            else
            {
                Debug.LogWarning($"DropZone: Unit prefab missing for card {card.cardData.cardName}!");
            }
        }
        else if (card.cardData.cardType == CardType.Spell)
        {
            Debug.Log($"DropZone: Spell {card.cardData.cardName} played on lane {laneIndex}");
        }

        StartCoroutine(PlayCardAndCleanUp(card.gameObject));
    }

    private IEnumerator PlayCardAndCleanUp(GameObject card)
    {
        Transform handPanel = card.transform.parent;
        HandLayout handLayout = handPanel.GetComponent<HandLayout>();
        float waitTime = 0.2f;
        if (handLayout != null)
        {
            waitTime = handLayout.AnimationDuration * 1.1f; // Add 10% buffer
            handLayout.RepositionCards();
            yield return new WaitForSeconds(waitTime);
        }
        if (card != null)
        {
            Destroy(card);
        }
    }
}