using UnityEngine;
using Photon.Pun;

public class Lane3D : MonoBehaviourPun
{
    [Header("Lane Setup")]
    [SerializeField] private int playerOwnerId;        // 1 or 2
    [SerializeField] public int boardLaneIndex;       // 0–4 shared lane index across both sides
    [SerializeField] private GameObject currentUnit;

    [Header("Audio")]
    [SerializeField] private AudioSource addCard;

    // --- Properties ---
    public int PlayerOwnerId => playerOwnerId;
    public int LaneIndex => boardLaneIndex;
    public GameObject GetCurrentUnit() => currentUnit;
    public bool CanAddUnit() => currentUnit == null;
    public Vector3 GetUnitSpawnPosition() => transform.position;

    // --- Called when a card is placed ---
    [PunRPC]
    public void RPC_AddUnit(int unitViewID, bool isOwnerView)
    {
        PhotonView unitPV = PhotonView.Find(unitViewID);
        if (unitPV == null)
        {
            Debug.LogError("[Lane3D] Cannot find PhotonView with ID " + unitViewID);
            return;
        }

        GameObject unit = unitPV.gameObject;

        if (currentUnit != null)
        {
            Debug.LogWarning($"[Lane3D] Lane {boardLaneIndex} for Player {playerOwnerId} already has a unit!");
            return;
        }

        currentUnit = unit;
        unit.transform.SetParent(transform, false);
        unit.transform.localPosition = Vector3.zero;
        unit.transform.localScale = isOwnerView ? Vector3.one * 1.2f : Vector3.one;

        if (addCard != null) addCard.Play();

        Debug.Log($"[Lane3D] Unit '{unit.name}' added to lane {boardLaneIndex} (OwnerView={isOwnerView})");
    }

    public void RemoveUnit(GameObject unit)
    {
        if (currentUnit == unit)
        {
            currentUnit = null;
        }
    }
}
