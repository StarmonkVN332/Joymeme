using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

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
            card?.ResetToHand();
            return;
        }

        // ✅ Prevent re-drop
        if (card.HasBeenDropped)
        {
            Debug.Log("DropZone: Card already dropped, ignoring.");
            StartCoroutine(card.FadeOutAndDestroy(card));
            return;
        }

        // ✅ Early out if card is grayed out/unplayable
        if (card.IsGrayedOut()) // <- Add this helper
        {
            Debug.Log("[DropZone] Card is grayed out/unplayable — resetting to hand.");
            //card.ResetToHand();
            return;
        }

        if (!CanPlayCard(card.cardData))
        {
            Debug.Log($"DropZone: Cannot play card {card.cardData.cardName} on UI lane {localLaneIndex}");
            card.ResetToHand();
            return;
        }

        if (!ManaManager.Instance.HasEnoughMana(PhotonNetwork.LocalPlayer.ActorNumber, card.cardData.manaCost))
        {
            Debug.Log("[DropZone] Not enough mana to play this card.");
            card.ResetToHand();
            return;
        }

        // 🔒 Now mark it as dropped
        card.MarkAsDropped();

        if (card.cardData.cardType == CardType.Unit)
        {
            PhotonView gameManagerView = PhotonView.Get(GameManager.Instance);
            gameManagerView.RPC("RPC_RequestPlayUnit", RpcTarget.MasterClient,
                card.cardData.cardId, correspondingLane.BoardLaneIndex, PhotonNetwork.LocalPlayer.ActorNumber);
        }
        else if (card.cardData.cardType == CardType.Spell)
        {
            Debug.Log($"DropZone: Spell '{card.cardData.cardName}' played on UI lane {localLaneIndex}");

            UnitInstance targetUnit = correspondingLane.GetCurrentUnit(); // 🔍 Fetch the unit from the lane
            var effect = card.cardData.spellEffect;
            string resourcePath = "SpellEffects/" + effect.name; // adjust path to match Resources folder
            object[] effectData = effect.GetEffectData();

            PhotonView gameManagerView = PhotonView.Get(GameManager.Instance);
            gameManagerView.RPC(
            "RPC_ApplySpellEffect",
            RpcTarget.All,
            targetUnit.photonView.ViewID,
            resourcePath,
            effectData,
            targetUnit.OwnerActorId // 🔥 Pass it explicitly
        );
            Debug.Log($"[DropZone] Spell effect applied to target unit {targetUnit.name} (Owner: {targetUnit.OwnerActorId})");
    }
        


        HandLayout handLayout = card.originalParent.GetComponentInParent<HandLayout>();
        handLayout?.RepositionCards();
    }


    public void PlayCard(CardData data, UnitInstance target)
    {
        if (data.cardType == CardType.Spell && data.spellEffect != null)
        {
            data.spellEffect.ApplyEffect(target);
        }
    }



}
