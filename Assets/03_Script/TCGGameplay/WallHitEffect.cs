using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WallHitEffect : MonoBehaviour
{
    [Tooltip("Which player owns this wall and will receive damage?")]
    public int ownerActorId; // Set this in the inspector

    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float cooldownTime = 2f;

    [Header("Transparency Fade Settings")]
    [SerializeField] private Renderer wallRenderer;
    [SerializeField] private float fadeAlpha = 0.3f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float restoreDuration = 1.0f;

    [Header("Shatter Settings")]
    [SerializeField] private GameObject shatteredWallPrefab;
    [SerializeField] private Transform shatteredSpawnPoint;
    [SerializeField] private float explosionForce = 500f;
    [SerializeField] private float explosionRadius = 3f;

    private float lastSpawnTime = -Mathf.Infinity;
    private Coroutine fadeCoroutine;
    private Color originalColor;
    private bool initialized = false;
    public PhotonView photonView { get; set; }
    private HashSet<int> recentAttackers = new HashSet<int>();
    private float attackerCooldown = 0.2f;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - lastSpawnTime < cooldownTime) return;
        if (hitEffectPrefab == null) return;

        Vector3 contactPoint = other.ClosestPoint(transform.position);
        Vector3 direction = (transform.position - other.transform.position).normalized;
        Instantiate(hitEffectPrefab, contactPoint, Quaternion.LookRotation(direction));
        lastSpawnTime = Time.time;

        UnitInstance unit = other.GetComponent<UnitInstance>();
        if (unit == null || unit.HasDealtDamage) return;

        // Only send logic trigger to MasterClient
        photonView.RPC("RPC_NotifyWallHit", RpcTarget.MasterClient, unit.photonView.ViewID, contactPoint);
        unit.HasDealtDamage = true;
    }

    [PunRPC]
    public void RPC_TriggerWallFade()
    {
        if (wallRenderer == null) return;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeTransparency());
    }
    [PunRPC]
    public void RPC_NotifyWallHit(int attackerViewID, Vector3 contactPoint)
    {
        PhotonView attackerView = PhotonView.Find(attackerViewID);
        if (attackerView == null) return;

        UnitInstance unit = attackerView.GetComponent<UnitInstance>();
        if (unit == null || unit.HasDealtDamage) return;

        int damage = unit.GetAttack();
        if (damage <= 0) return;

        GameManager.Instance.photonView.RPC("RPC_TakePlayerDamage", RpcTarget.All, ownerActorId, damage, contactPoint);
        unit.HasDealtDamage = true;
        int currentHealth = GameManager.Instance.GetPlayerHealth(ownerActorId);
        if (currentHealth == 0)
        {
            photonView.RPC("RPC_ShatterWall", RpcTarget.All, contactPoint);
        }

        photonView.RPC("RPC_TriggerWallFade", RpcTarget.All);

        
    }

    private IEnumerator FadeTransparency()
    {
        Material mat = wallRenderer.material;

        // Only cache the original color once
        if (!initialized)
        {
            originalColor = mat.color;
            initialized = true;
        }

        // Step 1: Fade out
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, fadeAlpha, t / fadeDuration);
            mat.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // Step 2: Restore
        t = 0;
        while (t < restoreDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(fadeAlpha, originalColor.a, t / restoreDuration);
            mat.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // Final reset
        mat.color = originalColor;
    }
    [PunRPC]
    public void RPC_ShatterWall(Vector3 hitPosition)
    {
        if (wallRenderer != null)
            wallRenderer.enabled = false;

        if (shatteredWallPrefab != null)
        {
            GameObject shattered = Instantiate(shatteredWallPrefab, transform.position, transform.rotation);

            foreach (var rb in shattered.GetComponentsInChildren<Rigidbody>())
            {
                Vector3 forceDir = (rb.transform.position - hitPosition).normalized;
                rb.AddForce(forceDir * Random.Range(100, 300)); // adjust force as needed
            }

            Destroy(gameObject); // Optional: remove this wall entirely
        }
    }


}
