using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using System.Collections;

public class Lane3D : MonoBehaviourPun
{
    [SerializeField] public int playerOwnerId;
    [SerializeField] public int boardLaneIndex; // New: Global lane index on board
    [SerializeField] private int sideId; // 0 = bottom, 1 = top — FIXED, the side id is used for animation

    [SerializeField] private GameObject currentUnit;
    [SerializeField] private AudioSource addCard;
    public int SideId => sideId; // Optional getter if needed

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
    {
        currentUnit = null;
        BoardManager.Instance?.ClearUnitAt(boardLaneIndex);
    }
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

        // Assign side ID to unit
        UnitInstance unitInstance = unit.GetComponent<UnitInstance>();
        if (unitInstance != null)
        {
            unitInstance.SetSide(sideId);
            unitInstance.SetPlayerOwnerId(playerOwnerId);
            unitInstance.SetLaneIndex(boardLaneIndex);
            // Ensure playerOwnerId is set correctly
        }
        else
        {
            Debug.LogWarning($"[Lane3D] Unit {unit.name} missing UnitInstance script!");
        }

        currentUnit = unit;
        unit.transform.SetParent(transform, false);
        unit.transform.localPosition = Vector3.zero;
        unit.transform.localScale = isOwnerView ? Vector3.one * 1.2f : Vector3.one;

        // Play entry animation
        unitInstance?.photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "Entry");

        if (addCard != null)
            addCard.Play();
        unitInstance?.photonView.RPC("Initialize", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber);
        // 🔥 Register with BoardManager
        BoardManager.Instance?.SetUnitAt(boardLaneIndex, unitInstance);

        GameManager.Instance.NotifyUnitSpawnFinished();
        Debug.Log($"[Late Join Safe] Lane3D: Unit '{unit.name}' added to lane {boardLaneIndex} (OwnerView={isOwnerView})");
    }


}
