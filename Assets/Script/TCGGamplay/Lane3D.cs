using UnityEngine;
using Photon.Pun;

public class Lane3D : MonoBehaviourPun
{
    public int playerOwnerId; // Set by owning Player (1 or 2)
    public int laneIndex;     // 0–4 for 5 lanes
    [SerializeField] private GameObject currentUnit; // Tracks the unit in this lane

    public bool CanAddUnit()
    {
        return currentUnit == null; // Lane is empty if no unit exists
    }

    public void AddUnit(GameObject unit)
    {
        if (currentUnit != null)
        {
            Debug.LogWarning($"Lane3D: Lane {laneIndex} for Player {playerOwnerId} already has a unit!");
            return;
        }
        currentUnit = unit;
        // Parent unit to lane for organization and to maintain position
        unit.transform.SetParent(transform, false);
        // Position unit at lane's origin (relative to lane)
        unit.transform.localPosition = Vector3.zero;
    }

    public void RemoveUnit(GameObject unit)
    {
        if (currentUnit == unit)
        {
            currentUnit = null;
        }
    }

    public Vector3 GetUnitSpawnPosition()
    {
        // Return lane's world position
        return transform.position;
    }

    // Public getters for Inspector-set fields
    public int PlayerOwnerId => playerOwnerId;
    public int LaneIndex => laneIndex;
}