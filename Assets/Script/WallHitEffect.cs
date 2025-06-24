using UnityEngine;

public class WallHitEffect : MonoBehaviour
{
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
    }
}
