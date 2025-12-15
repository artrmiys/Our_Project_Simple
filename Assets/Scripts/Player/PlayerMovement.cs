using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private IHighlightable currentHighlighted;
    public CharacterController controller;

    [Header("Speeds")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 7f;

    [Header("Jump/Gravity")]
    public float gravity = -9.81f * 2f;
    public float jumpHeight = 3f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Animation / Model")]
    public Animator anim;
    public Transform modelRoot;
    public float turnSpeed = 10f;

    [Header("Attack Facing")]
    public bool faceAimWhileAttacking = true;     // rotate model during attack
    public bool lockAimOnAttackStart = true;      // lock direction on click
    Vector3 lockedAimDir;
    bool hasLockedAim;

    [Header("Audio")]
    public FootstepAudio footstepAudio;

    Vector3 velocity;
    bool turning;
    Quaternion targetTurnRot;

    void Start()
    {
        if (!footstepAudio)
            footstepAudio = GetComponent<FootstepAudio>();
    }

    void Update()
    {
        // Ground reset
        bool grounded = controller.isGrounded;
        if (grounded && velocity.y < 0f) velocity.y = -2f;

        // Check if stabbing animation locks input
        bool isStabbing = anim && anim.GetCurrentAnimatorStateInfo(0).IsName("Stabbing");

        // Attack input
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            PlayerAttack pa = GetComponent<PlayerAttack>();
            if (pa != null && pa.hasWhip)
            {
                // lock aim dir at attack start (optional)
                if (faceAimWhileAttacking && lockAimOnAttackStart)
                {
                    lockedAimDir = GetAimDirXZ();
                    hasLockedAim = true;
                }

                if (anim) anim.SetTrigger("Stab");
                turning = false;
            }
        }

        // Movement input
        float x = isStabbing ? 0f : Input.GetAxisRaw("Horizontal");
        float z = isStabbing ? 0f : Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(x, 0f, z);
        float move01 = Mathf.Clamp01(input.magnitude);

        bool isWalking = !isStabbing && Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = isWalking ? walkSpeed : runSpeed;

        // Move direction
        Vector3 moveWorld = transform.TransformDirection(input.normalized) * (move01 > 0f ? targetSpeed : 0f);

        if (move01 > 0f) turning = false;

        // JUMP
        if (!isStabbing && Input.GetButtonDown("Jump") && grounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (anim) anim.SetTrigger("Jump");

            if (footstepAudio)
                footstepAudio.PlayJumpSound();
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;

        // Final move
        Vector3 motion = new Vector3(moveWorld.x, velocity.y, moveWorld.z) * Time.deltaTime;
        controller.Move(motion);

        // Rotate model (movement-based)
        if (!isStabbing && modelRoot && move01 > 0f)
        {
            Vector3 flat = new Vector3(moveWorld.x, 0f, moveWorld.z);
            if (flat.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(flat, Vector3.up);
                modelRoot.rotation = Quaternion.Slerp(modelRoot.rotation, look, Time.deltaTime * turnSpeed);
            }
        }

        // Rotate model while attacking (aim/whip direction)
        if (isStabbing && faceAimWhileAttacking && modelRoot)
        {
            Vector3 dir = (lockAimOnAttackStart && hasLockedAim) ? lockedAimDir : GetAimDirXZ();
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
                modelRoot.rotation = Quaternion.Slerp(modelRoot.rotation, look, Time.deltaTime * turnSpeed);
            }

            turning = false; // prevent conflict with idle turn system
        }

        // reset lock when not attacking
        if (!isStabbing) hasLockedAim = false;

        // Idle Turns (A/D)
        if (!isStabbing && modelRoot && move01 <= 0.001f)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                if (anim) anim.SetTrigger("TurnLeft");
                targetTurnRot = Quaternion.Euler(0f, modelRoot.eulerAngles.y - 90f, 0f);
                turning = true;
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                if (anim) anim.SetTrigger("TurnRight");
                targetTurnRot = Quaternion.Euler(0f, modelRoot.eulerAngles.y + 90f, 0f);
                turning = true;
            }
        }

        // Smooth turn
        if (!isStabbing && turning && modelRoot)
        {
            modelRoot.rotation = Quaternion.Slerp(modelRoot.rotation, targetTurnRot, Time.deltaTime * turnSpeed);
            if (Quaternion.Angle(modelRoot.rotation, targetTurnRot) < 1f) turning = false;
        }

        // Animator parameters
        if (anim)
        {
            anim.SetFloat("Speed", move01);
            anim.SetBool("IsRunning", !isWalking);
        }

        // Ray from camera forward (interaction)
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        int mask = ~LayerMask.GetMask("Player");

        if (Physics.Raycast(ray, out RaycastHit hit, 4f, mask))
        {
            var highlight = hit.collider.GetComponentInParent<IHighlightable>();

            if (highlight != null && highlight != currentHighlighted)
            {
                if (currentHighlighted != null)
                    currentHighlighted.Unhighlight();

                highlight.Highlight();
                currentHighlighted = highlight;
            }

            if (highlight == null && currentHighlighted != null)
            {
                currentHighlighted.Unhighlight();
                currentHighlighted = null;
            }

            var interactable = hit.collider.GetComponentInParent<Interactable>();
            if (interactable != null && Input.GetKeyDown(KeyCode.E))
            {
                interactable.Interact();
            }
        }
        else
        {
            if (currentHighlighted != null)
            {
                currentHighlighted.Unhighlight();
                currentHighlighted = null;
            }
        }
    }

    // Aim direction = center of screen (same as WhipSimple)
    Vector3 GetAimDirXZ()
    {
        Camera cam = Camera.main;
        if (!cam)
            return modelRoot ? modelRoot.forward : transform.forward;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 dir = ray.direction;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            dir = modelRoot ? modelRoot.forward : transform.forward;

        return dir.normalized;
    }
}
