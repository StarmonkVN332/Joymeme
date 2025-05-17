using UnityEngine;
using Photon.Pun;

public class Lane3D : MonoBehaviourPun
{
    [SerializeField] public int playerOwnerId;
    [SerializeField] public int laneIndex;
    [SerializeField] private GameObject currentUnit;
    [SerializeField] private AudioSource addCard;

    public bool CanAddUnit() => currentUnit == null;

    //public void AddUnit(GameObject unit, bool isOwnerView)
    //{
    //    if (currentUnit != null)
    //    {
    //        Debug.LogWarning($"Lane {laneIndex} for Player {playerOwnerId} already has a unit!");
    //        return;
    //    }

    //    if (unit == null)
    //    {
    //        Debug.LogError("Lane3D: Tried to AddUnit with a null GameObject!");
    //        return;
    //    }

    //    Debug.Log($"Lane3D: Adding unit '{unit.name}' to lane {laneIndex} for player {playerOwnerId}");

    //    currentUnit = unit;
    //    unit.transform.SetParent(transform, false);
    //    unit.transform.localPosition = Vector3.zero;

    //    Debug.Log($"Lane3D: Set unit '{unit.name}' parent to lane and localPosition to {unit.transform.localPosition}");

    //    unit.transform.localScale = isOwnerView ? Vector3.one * 1.2f : Vector3.one;

    //    Debug.Log($"Lane3D: Set unit '{unit.name}' scale to {unit.transform.localScale}");

    //    if (addCard != null)
    //        addCard.Play();

    //    Debug.Log("Lane3D: Card Added!!");
    //}


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
    public void RPC_AddUnit(int unitViewID, bool isOwnerView)
    {
        PhotonView unitPV = PhotonView.Find(unitViewID);
        if (unitPV == null)
        {
            Debug.LogError("RPC_AddUnit: Cannot find PhotonView with ID " + unitViewID);
            return;
        }

        GameObject unit = unitPV.gameObject;

        if (currentUnit != null)
        {
            Debug.LogWarning($"Lane {laneIndex} for Player {playerOwnerId} already has a unit!");
            return;
        }

        currentUnit = unit;

        unit.transform.SetParent(transform, false);
        unit.transform.localPosition = Vector3.zero;
        unit.transform.localScale = isOwnerView ? Vector3.one * 1.2f : Vector3.one;

        Debug.Log($"Lane3D: Unit '{unit.name}' added as child and positioned in lane {laneIndex} (OwnerView={isOwnerView})");

        if (addCard != null)
            addCard.Play();
    }

}
