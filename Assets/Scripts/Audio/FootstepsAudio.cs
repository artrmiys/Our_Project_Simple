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
    public AudioClip jumpClip;
    [Range(0f, 1f)] public float jumpVolume = 0.9f;

    [Header("Step timing (Timer mode)")]
    public bool useAnimationEvents = false;
    public float baseStepInterval = 0.5f;
    public float speedToIntervalFactor = 0.25f;

    [Header("Surface detection")]
    public Vector3 raycastOffset = new Vector3(0, 0.4f, 0);
    public float raycastDistance = 1.2f;
    public LayerMask groundMask = ~0;
    [Range(0.5f, 1.0f)] public float capsuleRadiusScale = 0.85f;

    [Header("Refs")]
    public CharacterController controller;
    public Animator animator;

    private float stepTimer;
    private string currentSurface = "Surface_Theatre";

    void Reset()
    {
        source = GetComponent<AudioSource>();
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        UpdateSurfaceByCast();

        if (useAnimationEvents)
            return;

        Vector3 vel = controller ? controller.velocity : Vector3.zero;
        float planarSpeed = new Vector2(vel.x, vel.z).magnitude;

        bool grounded = controller ? controller.isGrounded : true;

        bool isMoving = planarSpeed > 0.1f;

        // No steps if in air OR not moving
        if (!grounded || !isMoving)
        {
            stepTimer = 0f;
            return;
        }

        // Step timing
        float interval = Mathf.Max(0.1f, baseStepInterval - planarSpeed * speedToIntervalFactor);
        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            PlayFootstep();
            stepTimer = interval;
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
                }
            }
        }

        if (foundTag == null)
        {
            Vector3 origin = transform.position + raycastOffset;
            var hits = Physics.RaycastAll(origin, Vector3.down, raycastDistance, groundMask, QueryTriggerInteraction.Ignore);
            if (hits.Length > 0)
            {
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                foreach (var h in hits)
                {
                    var col = h.collider;
                    if (col.CompareTag("Surface_Outside")) { foundTag = "Surface_Outside"; break; }
                    if (col.CompareTag("Surface_Theatre")) { foundTag = "Surface_Theatre"; break; }
                }
            }
        }

        if (foundTag != null)
            currentSurface = foundTag;
    }

    void PlayFootstep()
    {
        if (!source || !source.enabled) return;

        AudioClip clip = null;

        if (currentSurface == "Surface_Outside" && outsideClips.Length > 0)
            clip = outsideClips[Random.Range(0, outsideClips.Length)];
        else if (theatreClips.Length > 0)
            clip = theatreClips[Random.Range(0, theatreClips.Length)];

        if (!clip) return;

        source.pitch = Random.Range(pitchJitter.x, pitchJitter.y);
        source.PlayOneShot(clip, volume);
    }

    // Jump sound called manually from PlayerMovement
    public void PlayJumpSound()
    {
        if (!jumpClip || !source || !source.enabled)
            return;

        source.pitch = Random.Range(pitchJitter.x, pitchJitter.y);
        source.PlayOneShot(jumpClip, jumpVolume);
    }

    public void AnimEvent_Footstep() => PlayFootstep();
}
