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

    [Header("Audio")]
    public FootstepAudio footstepAudio;   // <-- Added

    Vector3 velocity;
    bool turning;
    Quaternion targetTurnRot;

    void Start()
    {
        // Auto-grab FootstepAudio if missing
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

        // Rotate model
        if (!isStabbing && modelRoot && move01 > 0f)
        {
            Vector3 flat = new Vector3(moveWorld.x, 0f, moveWorld.z);
            if (flat.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(flat, Vector3.up);
                modelRoot.rotation = Quaternion.Slerp(modelRoot.rotation, look, Time.deltaTime * turnSpeed);
            }
        }

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

        // Input.GetKeyDown

        // Create a ray from the camera’s position forward
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        // Ignore the Player layer so the ray doesn't hit the character model
        int mask = ~LayerMask.GetMask("Player");

        // Perform the raycast
        if (Physics.Raycast(ray, out RaycastHit hit, 4f, mask))
        {
            //Debug.Log("Raycast hit: " + hit.collider.name);

            // Try to get a highlightable component on the hit object or its parent
            var highlight = hit.collider.GetComponentInParent<IHighlightable>();

            // If we found a new highlightable object (different from the previous one)
            if (highlight != null && highlight != currentHighlighted)
            {
                // Remove highlight from the previously highlighted object
                if (currentHighlighted != null)
                    currentHighlighted.Unhighlight();

                // Apply highlight to the new object
                highlight.Highlight();
                currentHighlighted = highlight;
            }

            // If the object under the ray is NOT highlightable, remove highlight
            if (highlight == null && currentHighlighted != null)
            {
                currentHighlighted.Unhighlight();
                currentHighlighted = null;
            }

            // Interact if object is interactable and the player presses E
            var interactable = hit.collider.GetComponentInParent<Interactable>();
            if (interactable != null && Input.GetKeyDown(KeyCode.E))
            {
                interactable.Interact();
            }
        }
        else
        {
            // If raycast did not hit anything, remove highlight if needed
            if (currentHighlighted != null)
            {
                currentHighlighted.Unhighlight();
                currentHighlighted = null;
            }
        }

    }
}