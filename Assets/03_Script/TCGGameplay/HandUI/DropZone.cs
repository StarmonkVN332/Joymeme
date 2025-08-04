using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    public int localLaneIndex;
    [SerializeField] private Lane3D correspondingLane;

    // --- NEW: Direct reference to the unit in this lane ---
    private UnitInstance currentUnitInLane;

    // --- NEW: Methods for the Lane3D to call ---
    public void RegisterUnit(UnitInstance unit)
    {
        currentUnitInLane = unit;
        Debug.Log($"DropZone {localLaneIndex} registered unit: {unit.name}");
    }

    public void ClearUnit()
    {
        currentUnitInLane = null;
        Debug.Log($"DropZone {localLaneIndex} cleared its unit reference.");
    }

    public void LinkToLane(Lane3D lane)
    {
        correspondingLane = lane;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        // We only care about right-clicks for inspection.
        if (eventData.button != PointerEventData.InputButton.Right)
        {
            return;
        }

        // Check if there's a unit to inspect and that we aren't currently dragging another card.
        if (currentUnitInLane != null && eventData.pointerDrag == null)
        {
            // Find the unit's original CardData to show its base stats and artwork.
            // This requires a CardDatabase singleton and a public CardId on UnitInstance.
            CardData unitCardData = CardDatabase.Instance.GetCardById(currentUnitInLane.cardId);

            if (unitCardData != null)
            {
                // Tell the inspector to show the details for this unit.
                UnitInspector.Instance?.ShowUnitDetails(unitCardData, currentUnitInLane);
            }
        }
    }
    public bool CanPlayCard(CardData cardData)
    {
        if (GameManager.Instance.CurrentTurnActorId != PhotonNetwork.LocalPlayer.ActorNumber)
            return false;

        if (GameManager.Instance.CurrentPhase != GamePhase.Placement)
            return false;

        if (cardData == null) return false;

        // Use the new direct reference for checks
        if (cardData.cardType == CardType.Unit && currentUnitInLane != null)
        {
            Debug.Log("DropZone: Lane is occupied!");
            return false;
        }
        if (cardData.cardType == CardType.Spell && currentUnitInLane == null)
        {
            Debug.Log("DropZone: Cannot play a targeted spell on an empty lane!");
            return false;
        }

        return true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableCard card = eventData.pointerDrag?.GetComponent<DraggableCard>();
        if (card == null || card.cardData == null || correspondingLane == null)
        {
            return;
        }

        if (card.HasBeenDropped)
        {
            return;
        }

        if (!CanPlayCard(card.cardData) || !ManaManager.Instance.HasEnoughMana(PhotonNetwork.LocalPlayer.ActorNumber, card.cardData.manaCost))
        {
            return;
        }

        card.MarkAsDropped();

        if (card.cardData.cardType == CardType.Unit)
        {
            PhotonView gameManagerView = PhotonView.Get(GameManager.Instance);
            gameManagerView.RPC("RPC_RequestPlayUnit", RpcTarget.MasterClient,
                card.cardData.cardId, correspondingLane.BoardLaneIndex, PhotonNetwork.LocalPlayer.ActorNumber);
        }
        else if (card.cardData.cardType == CardType.Spell)
        {
            // --- THIS IS THE KEY CHANGE ---
            // Use the direct reference instead of fetching from the lane.
            UnitInstance targetUnit = currentUnitInLane;

            var effect = card.cardData.spellEffect;
            string resourcePath = "SpellEffects/" + effect.name;
            object[] effectData = effect.GetEffectData();

            PhotonView gameManagerView = PhotonView.Get(GameManager.Instance);
            gameManagerView.RPC(
                "RPC_RequestPlaySpell",
                RpcTarget.MasterClient,
                card.cardData.cardId,
                targetUnit.photonView.ViewID,
                resourcePath,
                effectData,
                PhotonNetwork.LocalPlayer.ActorNumber
            );
            Debug.Log($"[DropZone] Sent request to play spell on target unit {targetUnit.name}");
        }
    }
}
