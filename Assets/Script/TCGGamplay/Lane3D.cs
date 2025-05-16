using UnityEngine;
using Photon.Pun;

public class Lane3D : MonoBehaviourPun
{
    [SerializeField] public int playerOwnerId;
    [SerializeField] public int laneIndex;
    [SerializeField] private GameObject currentUnit;
    [SerializeField] private AudioSource addCard;

    public bool CanAddUnit() => currentUnit == null;

    public void AddUnit(GameObject unit, bool isOwnerView)
    {
        if (currentUnit != null)
        {
            Debug.LogWarning($"Lane {laneIndex} for Player {playerOwnerId} already has a unit!");
            return;
        }

        if (unit == null)
        {
            Debug.LogError("Lane3D: Tried to AddUnit with a null GameObject!");
            return;
        }

        currentUnit = unit;
        unit.transform.SetParent(transform, false);
        unit.transform.localPosition = Vector3.zero;

        // Adjust scale so that owner sees it bigger (optional UX)
        unit.transform.localScale = isOwnerView ? Vector3.one * 1.2f : Vector3.one;
        addCard.Play();
        Debug.Log("Card Added!!");
    }

    public void RemoveUnit(GameObject unit)
    {
        if (currentUnit == unit)
            currentUnit = null;
    }

    public Vector3 GetUnitSpawnPosition() => transform.position;

    public int PlayerOwnerId => playerOwnerId;
    public int LaneIndex => laneIndex;

    // Photon RPC to add unit on all clients
    [PunRPC]
    public void RPC_AddUnit(int viewID, bool isOwnerView)
    {
        PhotonView unitPV = PhotonView.Find(viewID);
        if (unitPV == null)
        {
            Debug.LogError("RPC_AddUnit: Cannot find PhotonView with ID " + viewID);
            return;
        }

        AddUnit(unitPV.gameObject, isOwnerView);
    }
}
