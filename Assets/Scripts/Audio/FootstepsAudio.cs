using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource source;
    public AudioClip[] theatreClips;
    public AudioClip[] outsideClips;
    [Range(0f, 1f)] public float volume = 0.8f;
    public Vector2 pitchJitter = new Vector2(0.95f, 1.05f);

    [Header("Jump Sound")]
    public AudioClip jumpClip;                     // звук прыжка (вздох)
    [Range(0f, 1f)] public float jumpVolume = 0.9f;

    [Header("Step timing (Timer mode)")]
    public bool useAnimationEvents = false;
    public float baseStepInterval = 0.5f;
    public float speedToIntervalFactor = 0.25f;

    [Header("Surface detection")]
    public Vector3 raycastOffset = new Vector3(0, 0.4f, 0);
    public float raycastDistance = 1.2f;
    public LayerMask groundMask = ~0;
    public bool debugDrawRay = true;
    [Range(0.5f, 1.0f)] public float capsuleRadiusScale = 0.85f;

    [Header("Debug")]
    [SerializeField] bool printDebug = false;
    [SerializeField] bool verboseDiagnostics = false;

    [Header("Refs")]
    public CharacterController controller;
    public Animator animator;

    private float stepTimer;
    private string currentSurface = "Surface_Theatre";
    private bool wasGrounded = true; // для прыжка

    void Awake() { Debug.Log("[Footstep] Awake on " + name); }
    void OnEnable() { Debug.Log("[Footstep] OnEnable on " + name); }

    void Reset()
    {
        source = GetComponent<AudioSource>();
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        UpdateSurfaceByCast();

        if (debugDrawRay)
        {
            Vector3 a = transform.position + raycastOffset;
            Vector3 b = a + Vector3.down * raycastDistance;
            Debug.DrawLine(a, b, Color.yellow, 0f, false);
        }

        // --- прыжок (вздох) ---
        bool grounded = controller ? controller.isGrounded : true;
        if (wasGrounded && !grounded && jumpClip != null && source != null)
        {
            source.pitch = Random.Range(pitchJitter.x, pitchJitter.y);
            source.PlayOneShot(jumpClip, jumpVolume);
        }
        wasGrounded = grounded;

        if (useAnimationEvents) return;

        Vector3 vel = controller ? controller.velocity : Vector3.zero;
        float planarSpeed = new Vector2(vel.x, vel.z).magnitude;

        bool isMoving = planarSpeed > 0.1f;
        bool isGrounded = grounded;

        if (!isGrounded || !isMoving)
        {
            stepTimer = 0f;
            return;
        }

        float interval = Mathf.Max(0.1f, baseStepInterval - planarSpeed * speedToIntervalFactor);
        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            PlayFootstep();
            stepTimer = interval;
        }
    }

    void LateUpdate()
    {
        Vector3 o = transform.position + raycastOffset;
        if (Physics.Raycast(o, Vector3.down, out var h, raycastDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log($"[Footstep][Raycast] Hit: {h.collider.name}, tag={h.collider.tag}, layer={LayerMask.LayerToName(h.collider.gameObject.layer)}");
        }
        else
        {
            Debug.Log("[Footstep][Raycast] No hit");
        }
    }

    void UpdateSurfaceByCast()
    {
        string foundTag = null;

        if (controller)
        {
            Vector3 worldCenter = transform.TransformPoint(controller.center);
            float half = controller.height * 0.5f - controller.radius;
            Vector3 top = worldCenter + Vector3.up * half;
            Vector3 bottom = worldCenter - Vector3.up * half;
            float radius = Mathf.Max(0.05f, controller.radius * capsuleRadiusScale);

            var hits = Physics.CapsuleCastAll(top, bottom, radius, Vector3.down, raycastDistance, groundMask, QueryTriggerInteraction.Ignore);
            if (hits.Length > 0)
            {
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                foreach (var h in hits)
                {
                    var col = h.collider;
                    if (col.CompareTag("Surface_Outside")) { foundTag = "Surface_Outside"; break; }
                    if (col.CompareTag("Surface_Theatre")) { foundTag = "Surface_Theatre"; break; }
                    if (printDebug) Debug.Log($"[Footstep] Skip {col.name} tag={col.tag} layer={LayerMask.LayerToName(col.gameObject.layer)}");
                }
            }
        }

        if (foundTag == null)
        {
            Vector3 origin = transform.position + raycastOffset;
            var hits = Physics.RaycastAll(origin, Vector3.down, raycastDistance, groundMask, QueryTriggerInteraction.Ignore);
            if (hits.Length == 0)
            {
                if (printDebug) Debug.Log("[Footstep] No ground hit");
            }
            else
            {
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                foreach (var h in hits)
                {
                    var col = h.collider;
                    if (col.CompareTag("Surface_Outside")) { foundTag = "Surface_Outside"; break; }
                    if (col.CompareTag("Surface_Theatre")) { foundTag = "Surface_Theatre"; break; }
                    if (printDebug) Debug.Log($"[Footstep] Skip {col.name} tag={col.tag} layer={LayerMask.LayerToName(col.gameObject.layer)}");
                }
            }
        }

        if (foundTag != null && foundTag != currentSurface)
        {
            Debug.Log($"[Footstep] Surface: {currentSurface} -> {foundTag}");
            currentSurface = foundTag;
        }
        else if (foundTag == null)
        {
            Debug.Log("[Footstep] No valid surface tag under character");
        }
    }

    void PlayFootstep()
    {
        AudioClip clip = null;

        if (currentSurface == "Surface_Outside" && outsideClips != null && outsideClips.Length > 0)
            clip = outsideClips[Random.Range(0, outsideClips.Length)];
        else if (theatreClips != null && theatreClips.Length > 0)
            clip = theatreClips[Random.Range(0, theatreClips.Length)];

        if (clip == null || source == null) return;

        source.pitch = Random.Range(pitchJitter.x, pitchJitter.y);
        source.PlayOneShot(clip, volume);
    }

    public void AnimEvent_Footstep() => PlayFootstep();

    void OnDrawGizmos()
    {
        if (!debugDrawRay) return;

        Gizmos.color = Color.yellow;
        Vector3 origin = transform.position + raycastOffset;
        Gizmos.DrawLine(origin, origin + Vector3.down * raycastDistance);

        if (controller)
        {
            Vector3 worldCenter = transform.TransformPoint(controller.center);
            float half = controller.height * 0.5f - controller.radius;
            Vector3 top = worldCenter + Vector3.up * half;
            Vector3 bottom = worldCenter - Vector3.up * half;
            float radius = Mathf.Max(0.05f, controller.radius * capsuleRadiusScale);

            Gizmos.DrawWireSphere(top, radius);
            Gizmos.DrawWireSphere(bottom, radius);
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 400, 22), $"Surface = {currentSurface}");
    }
}
