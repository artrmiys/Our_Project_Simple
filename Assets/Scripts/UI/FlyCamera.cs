using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlyCamera : MonoBehaviour
{
    [Header("Timing")]
    public float flyTime = 1.5f;
    public float waitAfter = 1.0f;
    public float waitBefore = 1.0f;
    public float switchDelay = 1.0f;

    [Header("Cameras / UI")]
    public Canvas flyCanvas;
    public Camera mainCamera;
    public float activeDepth = 10f;

    [Header("Player freeze (optional)")]
    public Transform player;
    public string playerTag = "Player";
    public CharacterController playerController;
    public Rigidbody playerRigidbody;

    [Tooltip("Specific movement/look scripts to disable")]
    public MonoBehaviour[] movementToDisable;

    [Tooltip("Disable ALL scripts on player + children")]
    public bool disableAllPlayerScripts = true;

    [Tooltip("Scripts to exclude from auto disable")]
    public MonoBehaviour[] excludeFromAutoDisable;

    [Header("Cutscene Skip")]
    public KeyCode skipKey = KeyCode.Space; // key to skip cutscene
    private bool skipRequested = false;

    private Camera thisCam;
    private Transform fromPoint;
    private bool isPlaying = false;
    private float originalFlyDepth;
    private float originalMainDepth;

    // saved states for unfreeze
    private bool hadController, controllerPrevEnabled;
    private bool hadRigidbody;
    private bool rbPrevKinematic;
    private RigidbodyConstraints rbPrevConstraints;
    private Vector3 rbPrevVelocity, rbPrevAngularVelocity;

    // rotation lock
    private bool lockPlayerRotation = false;
    private Quaternion frozenPlayerRotation;

    // auto-disabled scripts cache
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

        // auto-find player
        if (!player)
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go) player = go.transform;
        }

        if (!playerController && player) playerController = player.GetComponent<CharacterController>();
        if (!playerRigidbody && player) playerRigidbody = player.GetComponent<Rigidbody>();
    }


    private void Update()
    {
        // detect skip input
        if (isPlaying && Input.GetKeyDown(skipKey))
        {
            skipRequested = true;
        }
    }


    public void PlayFly(Transform startPoint, Transform targetPoint, Transform lookAt, Camera mainCamFromTrigger = null)
    {
        if (isPlaying) return;

        if (mainCamFromTrigger != null)
            mainCamera = mainCamFromTrigger;

        // refresh player refs
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
        skipRequested = false;

        yield return new WaitForSeconds(switchDelay);

        yield return StartCoroutine(FlyRoutine(targetPoint, lookAt));

        isPlaying = false;
    }


    private IEnumerator FlyRoutine(Transform targetPoint, Transform lookAt)
    {
        // set start pose
        transform.position = fromPoint.position;
        transform.rotation = fromPoint.rotation;

        // WAIT BEFORE
        float beforeTimer = 0f;
        while (beforeTimer < waitBefore)
        {
            if (skipRequested) { EndCutsceneInstant(); yield break; }
            beforeTimer += Time.deltaTime;
            yield return null;
        }

        // freeze player
        FreezePlayer();

        // activate cinematic camera
        thisCam.depth = activeDepth;
        if (mainCamera != null)
            mainCamera.depth = originalMainDepth;

        if (flyCanvas != null)
            flyCanvas.gameObject.SetActive(true);

        // MOVEMENT LERP
        float t = 0f;
        while (t < 1f)
        {
            if (skipRequested) { EndCutsceneInstant(); yield break; }

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

        // WAIT AFTER
        float afterTimer = 0f;
        while (afterTimer < waitAfter)
        {
            if (skipRequested) { EndCutsceneInstant(); yield break; }
            afterTimer += Time.deltaTime;
            yield return null;
        }

        // normal end
        EndCutsceneInstant();
    }


    private void EndCutsceneInstant()
    {
        // restore depth
        thisCam.depth = originalFlyDepth;

        // hide UI
        if (flyCanvas != null)
            flyCanvas.gameObject.SetActive(false);

        // unfreeze player
        UnfreezePlayer();

        Debug.Log("Cutscene ended / skipped");
    }


    private void FreezePlayer()
    {
        if (!player) return;

        // lock rotation
        frozenPlayerRotation = player.rotation;
        lockPlayerRotation = true;

        // disable selected movement scripts
        if (movementToDisable != null)
        {
            foreach (var mb in movementToDisable)
                if (mb != null && mb.enabled)
                    mb.enabled = false;
        }

        // auto-disable all scripts
        autoDisabledScripts.Clear();
        if (disableAllPlayerScripts)
        {
            var all = player.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var mb in all)
            {
                if (mb == null) continue;
                if (!mb.enabled) continue;
                if (mb == this) continue;
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

            playerRigidbody.velocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;

            playerRigidbody.constraints = RigidbodyConstraints.FreezeAll;
            playerRigidbody.isKinematic = true;
        }
    }


    private void UnfreezePlayer()
    {
        if (!player) return;

        // restore CC
        if (hadController && playerController)
            playerController.enabled = controllerPrevEnabled;

        // restore RB
        if (hadRigidbody && playerRigidbody)
        {
            playerRigidbody.isKinematic = rbPrevKinematic;
            playerRigidbody.constraints = rbPrevConstraints;
        }

        // restore auto-disabled scripts
        foreach (var mb in autoDisabledScripts)
            if (mb != null)
                mb.enabled = true;

        autoDisabledScripts.Clear();

        // restore specifically disabled movement scripts
        if (movementToDisable != null)
        {
            foreach (var mb in movementToDisable)
                if (mb != null)
                    mb.enabled = true;
        }

        lockPlayerRotation = false;
    }


    private void LateUpdate()
    {
        // keep player rotation locked
        if (lockPlayerRotation && player)
            player.rotation = frozenPlayerRotation;
    }


    private bool IsInExcludeList(MonoBehaviour mb)
    {
        if (excludeFromAutoDisable == null) return false;

        for (int i = 0; i < excludeFromAutoDisable.Length; i++)
            if (excludeFromAutoDisable[i] == mb)
                return true;

        return false;
    }
}