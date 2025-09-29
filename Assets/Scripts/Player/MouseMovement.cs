using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerRoot;

    float xRotation = 0f;

    // fly-in
    Vector3 startPos;
    Vector3 targetPos;
    float flyTime = 1f;   // fly duration
    float flyTimer = 0f;
    bool controlEnabled = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // final position = inspector setup
        targetPos = transform.localPosition;

        // start pos = higher and back
        startPos = targetPos + new Vector3(0f, 3f, -6f);

        // place camera at start
        transform.localPosition = startPos;
    }

    void LateUpdate()
    {
        // fly-in
        if (flyTimer < flyTime)
        {
            flyTimer += Time.deltaTime;
            float t = flyTimer / flyTime;

            // smooth step
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            // arc lerp
            Vector3 midPos = (startPos + targetPos) / 2f + Vector3.up * 2f; // arc peak
            Vector3 p1 = Vector3.Lerp(startPos, midPos, smoothT);
            Vector3 p2 = Vector3.Lerp(midPos, targetPos, smoothT);
            transform.localPosition = Vector3.Lerp(p1, p2, smoothT);

            // when done, sync rotation
            if (flyTimer >= flyTime)
            {
                float currentX = transform.localEulerAngles.x;
                if (currentX > 180f) currentX -= 360f;
                xRotation = currentX;
                controlEnabled = true;
            }
            return;
        }

        if (!controlEnabled) return;

        // mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation = Mathf.Clamp(xRotation - mouseY, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (playerRoot)
            playerRoot.Rotate(Vector3.up * mouseX);
    }
}
