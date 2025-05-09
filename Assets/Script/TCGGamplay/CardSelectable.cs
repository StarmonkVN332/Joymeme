using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;

public class CardSelectable : MonoBehaviour, IPointerClickHandler
{
    private PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!photonView.IsMine) return;

        CardPlacementManager.Instance.SelectCard(gameObject);
        Debug.Log("Card Selected: " + gameObject.name);
    }
}
