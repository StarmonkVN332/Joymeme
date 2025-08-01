using Photon.Pun;
using UnityEngine;
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
    public PhotonView photonView { get; private set; }

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // --- THIS IS THE FIX ---
        // Only the Master Client will process any collision logic.
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        // The rest of the code now only runs on the Master Client.
        UnitInstance unit = other.GetComponent<UnitInstance>();

        // Authoritative check for cooldown and if the unit has already attacked.
        if (Time.time - lastSpawnTime < cooldownTime || unit == null || unit.HasDealtDamage)
        {
            return;
        }

        // Since this is the Master Client and the checks passed, we proceed.
        lastSpawnTime = Time.time;

        // --- AUTHORITATIVE GAME LOGIC ---

        // 1. Set the flag immediately to prevent any other triggers from this attack.
        unit.HasDealtDamage = true;

        // 2. Calculate damage.
        int damage = unit.GetAttack();
        if (damage <= 0) return;

        Vector3 contactPoint = other.ClosestPoint(transform.position);

        // 3. Tell the GameManager to deal damage (this is an RPC to all clients).
        GameManager.Instance.photonView.RPC("RPC_TakePlayerDamage", RpcTarget.All, ownerActorId, damage, contactPoint);

        // 4. Tell all clients to play the visual effects for this hit.
        photonView.RPC("RPC_PlayVisualEffects", RpcTarget.All, contactPoint);

        // 5. Check if the player is defeated and shatter the wall if so.
        int currentHealth = GameManager.Instance.GetPlayerHealth(ownerActorId);
        if (currentHealth <= 0)
        {
            photonView.RPC("RPC_ShatterWall", RpcTarget.All, contactPoint);
        }
    }

    // --- NEW RPC ---
    // This is called by the Master Client to tell all clients to play the VFX.
    [PunRPC]
    public void RPC_PlayVisualEffects(Vector3 contactPoint)
    {
        // Spawn the particle effect
        if (hitEffectPrefab != null)
        {
            Vector3 direction = (transform.position - contactPoint).normalized;
            Instantiate(hitEffectPrefab, contactPoint, Quaternion.LookRotation(direction));
        }

        // Start the wall fade animation
        if (wallRenderer != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeTransparency());
        }
    }

    // The old RPC_NotifyWallHit and RPC_TriggerWallFade are no longer needed
    // as their logic has been moved into OnTriggerEnter and the new RPC above.

    private IEnumerator FadeTransparency()
    {
        Material mat = wallRenderer.material;
        if (!initialized)
        {
            originalColor = mat.color;
            initialized = true;
        }
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, fadeAlpha, t / fadeDuration);
            mat.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        t = 0;
        while (t < restoreDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(fadeAlpha, originalColor.a, t / restoreDuration);
            mat.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
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
                rb.AddForce(forceDir * Random.Range(100, 300));
            }
            Destroy(gameObject);
        }
    }
}