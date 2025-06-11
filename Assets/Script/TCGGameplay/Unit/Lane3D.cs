using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using System.Collections;

public class Lane3D : MonoBehaviourPun
{
    [SerializeField] public int playerOwnerId;
    [SerializeField] public int boardLaneIndex; // New: Global lane index on board
    [SerializeField] private GameObject currentUnit;
    [SerializeField] private AudioSource addCard;

    public bool CanAddUnit()
    {
        Debug.Log($"[Lane3D] CanAddUnit? {currentUnit == null} on lane {boardLaneIndex} (Player {playerOwnerId})");
        return currentUnit == null;
    }   
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
        StartCoroutine(DelayedAddUnit(unitViewID, isOwnerView));
    }

    private IEnumerator DelayedAddUnit(int unitViewID, bool isOwnerView)
    {
        // Wait until this lane is ready
        while (!gameObject.activeInHierarchy || !this.enabled)
            yield return null;

        PhotonView unitPV = null;
        float timeout = 3f;
        float timer = 0f;

        while (unitPV == null && timer < timeout)
        {
            unitPV = PhotonView.Find(unitViewID);
            timer += Time.deltaTime;
            yield return null;
        }

        if (unitPV == null)
        {
            Debug.LogError($"Lane3D: Could not find PhotonView with ID {unitViewID} after waiting.");
            yield break;
        }

        GameObject unit = unitPV.gameObject;

        if (currentUnit != null)
        {
            Debug.LogWarning($"Lane {boardLaneIndex} for Player {playerOwnerId} already has a unit!");
            yield break;
        }

        currentUnit = unit;
        unit.transform.SetParent(transform, false);
        unit.transform.localPosition = Vector3.zero;
        unit.transform.localScale = isOwnerView ? Vector3.one * 1.2f : Vector3.one;

        if (addCard != null)
            addCard.Play();

        Debug.Log($"[Late Join Safe] Lane3D: Unit '{unit.name}' added to lane {boardLaneIndex} (OwnerView={isOwnerView})");
    }

}
