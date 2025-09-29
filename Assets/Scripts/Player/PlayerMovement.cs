
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
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
    public Animator anim;         // Animator
    public Transform modelRoot;   // object for rotation
    public float turnSpeed = 10f; // turn speed

    
    Vector3 velocity;
    bool turning;
    Quaternion targetTurnRot;

    void Update()
    {
        // floor ground
        bool grounded = controller.isGrounded;
        if (grounded && velocity.y < 0f) velocity.y = -2f;

        // animation stop when stubbing
        bool isStabbing = anim && anim.GetCurrentAnimatorStateInfo(0).IsName("Stabbing");

        // attack animation
        if (Input.GetMouseButtonDown(0))
        {
            if (anim) anim.SetTrigger("Stab");
            // stop turn
            turning = false;
        }

        // animation stop when stubbing
        float x = isStabbing ? 0f : Input.GetAxisRaw("Horizontal");
        float z = isStabbing ? 0f : Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(x, 0f, z);
        float move01 = Mathf.Clamp01(input.magnitude);


        bool isWalking = !isStabbing && Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = isWalking ? walkSpeed : runSpeed;


        // horizontal move
        Vector3 moveWorld = transform.TransformDirection(input.normalized) * (move01 > 0f ? targetSpeed : 0f);

        // if stay stop move animation
        if (move01 > 0f) turning = false;

        // don't jump when attack
        if (!isStabbing && Input.GetButtonDown("Jump") && grounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (anim) anim.SetTrigger("Jump");
        }

        // gravity
        velocity.y += gravity * Time.deltaTime;

        // final move
        Vector3 motion = new Vector3(moveWorld.x, velocity.y, moveWorld.z) * Time.deltaTime;
        controller.Move(motion);

        // turn to move
        if (!isStabbing && modelRoot && move01 > 0f)
        {
            Vector3 flat = new Vector3(moveWorld.x, 0f, moveWorld.z);
            if (flat.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(flat, Vector3.up);
                modelRoot.rotation = Quaternion.Slerp(modelRoot.rotation, look, Time.deltaTime * turnSpeed);
            }
        }

        // a/d iddle
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

        // turn slow
        if (!isStabbing && turning && modelRoot)
        {
            modelRoot.rotation = Quaternion.Slerp(modelRoot.rotation, targetTurnRot, Time.deltaTime * turnSpeed);
            if (Quaternion.Angle(modelRoot.rotation, targetTurnRot) < 1f) turning = false;
        }

        // animation parameters
        if (anim)
        {
            anim.SetFloat("Speed", move01);        // 0..1 — Idle/Walk/Run
            anim.SetBool("IsRunning", !isWalking); // run default
        }


    }
}
