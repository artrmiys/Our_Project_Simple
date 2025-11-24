using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlyCamera : MonoBehaviour
{
    [Header("Timing")]
    public float flyTime = 1.5f;        // fly duration
    public float waitAfter = 1.0f;      // stay at end
    public float waitBefore = 1.0f;     // wait before fly
    public float switchDelay = 1.0f;    // delay before sequence

    [Header("Cameras / UI")]
    public Canvas flyCanvas;            // optional UI
    public Camera mainCamera;           // main camera
    public float activeDepth = 10f;     // depth when active

    [Header("Player freeze (optional)")]
    public Transform player;                    // player root
    public string playerTag = "Player";
    public CharacterController playerController; // optional CC
    public Rigidbody playerRigidbody;           // optional RB

    [Tooltip("Specific movement/look scripts to disable")]
    public MonoBehaviour[] movementToDisable;   // drag your movement/look scripts

    [Tooltip("Disable ALL scripts on player + children")]
    public bool disableAllPlayerScripts = true; // hard freeze

    [Tooltip("Scripts that must stay active even on hard freeze")]
    public MonoBehaviour[] excludeFromAutoDisable; // e.g. Rope emission, ropes, VFX

    private Camera thisCam;
    private Transform fromPoint;
    private bool isPlaying = false;
    private float originalFlyDepth;
    private float originalMainDepth;

    // cached states for unfreeze
    private bool hadController, controllerPrevEnabled;
    private bool hadRigidbody;
    private bool rbPrevKinematic;
    private RigidbodyConstraints rbPrevConstraints;
    private Vector3 rbPrevVelocity, rbPrevAngularVelocity;

    // rotation lock
    private bool lockPlayerRotation = false;
    private Quaternion frozenPlayerRotation;

    // auto disabled scripts cache
    private List<MonoBehaviour> autoDisabledScripts = new List<MonoBehaviour>();

    private void Awake()
    {
        thisCam = GetComponent<Camera>();

        originalFlyDepth = thisCam.depth;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
            originalMainDepth = mainCamera.depth;

        if (flyCanvas != null)
            flyCanvas.gameObject.SetActive(false);

        // auto-find player & components if not assigned
        if (!player)
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go) player = go.transform;
        }
        if (!playerController && player) playerController = player.GetComponent<CharacterController>();
        if (!playerRigidbody && player) playerRigidbody = player.GetComponent<Rigidbody>();
    }

    public void PlayFly(Transform startPoint, Transform targetPoint, Transform lookAt, Camera mainCamFromTrigger = null)
    {
        if (isPlaying) return;

        if (mainCamFromTrigger != null)
            mainCamera = mainCamFromTrigger;

        // recheck player refs
        if (!player)
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go) player = go.transform;
        }
        if (!playerController && player) playerController = player.GetComponent<CharacterController>();
        if (!playerRigidbody && player) playerRigidbody = player.GetComponent<Rigidbody>();

        fromPoint = startPoint;
        StartCoroutine(StartFlyWithDelay(targetPoint, lookAt));
    }

    private IEnumerator StartFlyWithDelay(Transform targetPoint, Transform lookAt)
    {
        isPlaying = true;

        // wait before starting sequence
        yield return new WaitForSeconds(switchDelay);

        // run actual fly
        yield return StartCoroutine(FlyRoutine(targetPoint, lookAt));

        isPlaying = false;
    }

    private IEnumerator FlyRoutine(Transform targetPoint, Transform lookAt)
    {
        // set start pose
        transform.position = fromPoint.position;
        transform.rotation = fromPoint.rotation;

        // wait before movement (main camera still on top)
        yield return new WaitForSeconds(waitBefore);

        // freeze player
        FreezePlayer();

        // bring this camera to front
        thisCam.depth = activeDepth;

        // keep main camera under
        if (mainCamera != null)
            mainCamera.depth = originalMainDepth;

        // show UI
        if (flyCanvas != null)
            flyCanvas.gameObject.SetActive(true);

        // movement lerp
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / flyTime;
            t = Mathf.Clamp01(t);

            transform.position = Vector3.Lerp(fromPoint.position, targetPoint.position, t);

            if (lookAt != null)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookAt.position - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            }

            yield return null;
        }

        // hold at final point
        yield return new WaitForSeconds(waitAfter);

        // restore depth
        thisCam.depth = originalFlyDepth;

        // hide UI
        if (flyCanvas != null)
            flyCanvas.gameObject.SetActive(false);

        // unfreeze player
        UnfreezePlayer();
    }

    private void FreezePlayer()
    {
        if (!player) return;

        // lock current rotation
        frozenPlayerRotation = player.rotation;
        lockPlayerRotation = true;

        // disable specific movement scripts
        if (movementToDisable != null)
        {
            foreach (var mb in movementToDisable)
            {
                if (mb != null && mb.enabled)
                    mb.enabled = false;
            }
        }

        // disable ALL scripts on player hierarchy if enabled
        autoDisabledScripts.Clear();
        if (disableAllPlayerScripts)
        {
            var all = player.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var mb in all)
            {
                if (mb == null) continue;
                if (!mb.enabled) continue;

                // do not disable this FlyCamera
                if (mb == this) continue;

                // skip excluded scripts
                if (IsInExcludeList(mb)) continue;

                mb.enabled = false;
                autoDisabledScripts.Add(mb);
            }
        }

        // CharacterController
        hadController = playerController != null;
        if (hadController)
        {
            controllerPrevEnabled = playerController.enabled;
            playerController.enabled = false;
        }

        // Rigidbody
        hadRigidbody = playerRigidbody != null;
        if (hadRigidbody)
        {
            rbPrevVelocity = playerRigidbody.velocity;
            rbPrevAngularVelocity = playerRigidbody.angularVelocity;
            rbPrevKinematic = playerRigidbody.isKinematic;
            rbPrevConstraints = playerRigidbody.constraints;

            // stop motion
            playerRigidbody.velocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;

            // freeze physics
            playerRigidbody.constraints = RigidbodyConstraints.FreezeAll;
            playerRigidbody.isKinematic = true;
        }
    }

    private void UnfreezePlayer()
    {
        if (!player) return;

        // restore CharacterController
        if (hadController && playerController)
            playerController.enabled = controllerPrevEnabled;

        // restore Rigidbody
        if (hadRigidbody && playerRigidbody)
        {
            playerRigidbody.isKinematic = rbPrevKinematic;
            playerRigidbody.constraints = rbPrevConstraints;
            // keep velocity zero
        }

        // re-enable auto disabled scripts
        for (int i = 0; i < autoDisabledScripts.Count; i++)
        {
            if (autoDisabledScripts[i] != null)
                autoDisabledScripts[i].enabled = true;
        }
        autoDisabledScripts.Clear();

        // re-enable specific movement scripts
        if (movementToDisable != null)
        {
            foreach (var mb in movementToDisable)
            {
                if (mb != null)
                    mb.enabled = true;
            }
        }

        // unlock rotation
        lockPlayerRotation = false;
    }

    private void LateUpdate()
    {
        // hard lock rotation
        if (lockPlayerRotation && player)
        {
            player.rotation = frozenPlayerRotation;
        }
    }

    // helper: check exclude list
    private bool IsInExcludeList(MonoBehaviour mb)
    {
        if (excludeFromAutoDisable == null) return false;
        for (int i = 0; i < excludeFromAutoDisable.Length; i++)
        {
            if (excludeFromAutoDisable[i] == mb)
                return true;
        }
        return false;
    }
}
