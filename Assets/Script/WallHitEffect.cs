using Photon.Pun;
using UnityEngine;

public class WallHitEffect : MonoBehaviour
{
    [Tooltip("Which player owns this wall and will receive damage?")]
    public int ownerActorId; // Set this in the inspector

    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float cooldownTime = 2f;

    private float lastSpawnTime = -Mathf.Infinity;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Va chạm với: " + other.name); // Kiểm tra trigger có gọi không

        if (Time.time - lastSpawnTime < cooldownTime) return;
        if (hitEffectPrefab == null) return;

        Vector3 contactPoint = other.ClosestPoint(transform.position);
        Vector3 direction = (transform.position - other.transform.position).normalized;

        Instantiate(hitEffectPrefab, contactPoint, Quaternion.LookRotation(direction));
        lastSpawnTime = Time.time;


        UnitInstance unit = other.GetComponent<UnitInstance>();
        if (unit == null || unit.HasDealtDamage) return;

        int damage = unit.GetAttack();
        if (damage <= 0) return;

        unit.HasDealtDamage = true;

        // Play attack animation
        //unit.photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "AttackPlayer");

        // Call damage RPC on GameManager
        GameManager.Instance.photonView.RPC("RPC_TakePlayerDamage", RpcTarget.All, ownerActorId, damage);
    }
}
