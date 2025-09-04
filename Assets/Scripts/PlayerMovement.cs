
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
    public Animator anim;         // Animator на модели
    public Transform modelRoot;   // Объект модели (его крутим поворотами)
    public float turnSpeed = 10f; // Плавность поворота модели

    // внутренние
    Vector3 velocity;
    bool turning;
    Quaternion targetTurnRot;

    void Update()
    {
        // --- Ground
        bool grounded = controller.isGrounded;
        if (grounded && velocity.y < 0f) velocity.y = -2f;

        // --- Узнаём, идёт ли сейчас анимация удара (Stabbing)
        // В Animator стейт должен называться ровно "Stabbing" (или переименуй здесь под свой)
        bool isStabbing = anim && anim.GetCurrentAnimatorStateInfo(0).IsName("Stabbing");

        // --- Атака по ЛКМ (разрешаем триггерить всегда; движение заблокируем ниже)
        if (Input.GetMouseButtonDown(0))
        {
            if (anim) anim.SetTrigger("Stab");
            // сразу отменим плавный поворот, чтобы удар не «боролся» с ним
            turning = false;
        }

        // --- Input движения (если Stabbing — блокируем)
        float x = isStabbing ? 0f : Input.GetAxisRaw("Horizontal");
        float z = isStabbing ? 0f : Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(x, 0f, z);
        float move01 = Mathf.Clamp01(input.magnitude);

        bool isRunning = !isStabbing && Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = isRunning ? runSpeed : walkSpeed;

        // --- Горизонтальное движение (локальные оси игрока)
        Vector3 moveWorld = transform.TransformDirection(input.normalized) * (move01 > 0f ? targetSpeed : 0f);

        // Если начали двигаться — отменяем возможный «поворот на месте»
        if (move01 > 0f) turning = false;

        // --- Прыжок (запрещён во время Stabbing)
        if (!isStabbing && Input.GetButtonDown("Jump") && grounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (anim) anim.SetTrigger("Jump");
        }

        // --- Гравитация
        velocity.y += gravity * Time.deltaTime;

        // --- Финальное перемещение
        Vector3 motion = new Vector3(moveWorld.x, velocity.y, moveWorld.z) * Time.deltaTime;
        controller.Move(motion);

        // --- Поворот модели к направлению движения (когда идём/бежим и не Stabbing)
        if (!isStabbing && modelRoot && move01 > 0f)
        {
            Vector3 flat = new Vector3(moveWorld.x, 0f, moveWorld.z);
            if (flat.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(flat, Vector3.up);
                modelRoot.rotation = Quaternion.Slerp(modelRoot.rotation, look, Time.deltaTime * turnSpeed);
            }
        }

        // --- Повороты A/D ТОЛЬКО на месте (Idle) и не во время Stabbing
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

        // --- Плавное доворачивание модели под «turn»
        if (!isStabbing && turning && modelRoot)
        {
            modelRoot.rotation = Quaternion.Slerp(modelRoot.rotation, targetTurnRot, Time.deltaTime * turnSpeed);
            if (Quaternion.Angle(modelRoot.rotation, targetTurnRot) < 1f) turning = false;
        }

        // --- Параметры анимации (локомоция)
        if (anim)
        {
            anim.SetFloat("Speed", move01);       // 0..1 — Idle/Walk
            anim.SetBool("IsRunning", isRunning); // Shift — Run
        }
    }
}
