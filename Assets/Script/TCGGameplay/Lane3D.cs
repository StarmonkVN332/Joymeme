using UnityEngine;
using Photon.Pun;

public class Lane3D : MonoBehaviourPun
{
    [SerializeField] public int playerOwnerId;
    [SerializeField] public int boardLaneIndex; // New: Global lane index on board
    [SerializeField] private GameObject currentUnit;
    [SerializeField] private AudioSource addCard;

    public bool CanAddUnit() => currentUnit == null;
    public UnitInstance GetCurrentUnit()
    {
        if (currentUnit == null) return null;
        return currentUnit.GetComponent<UnitInstance>();
    }
    public GameObject CurrentUnit()
    {
        return currentUnit;
    }

    public void RemoveUnit(GameObject unit)
    {
        if (currentUnit == unit)
            currentUnit = null;
    }

    public Vector3 GetUnitSpawnPosition() => transform.position;

    public int PlayerOwnerId => playerOwnerId;
    public int BoardLaneIndex => boardLaneIndex;

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
            Debug.LogWarning($"Lane {boardLaneIndex} for Player {playerOwnerId} already has a unit!");
            return;
        }

        currentUnit = unit;
        unit.transform.SetParent(transform, false);
        unit.transform.localPosition = Vector3.zero;
        unit.transform.localScale = isOwnerView ? Vector3.one * 1.2f : Vector3.one;

        if (addCard != null)
            addCard.Play();

        Debug.Log($"Lane3D: Unit '{unit.name}' added to lane {boardLaneIndex} (OwnerView={isOwnerView})");
    }
}
