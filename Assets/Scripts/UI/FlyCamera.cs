using UnityEngine;
using System.Collections;

public class FlyCamera : MonoBehaviour
{
    [Header("Timing")]
    public float flyTime = 1.5f;        // how long the fly movement takes
    public float waitAfter = 1.0f;      // how long to stay at the end
    public float waitBefore = 1.0f;     // how long to wait before starting the fly movement
    public float switchDelay = 1.0f;    // how long to wait before the whole sequence

    [Header("Cameras / UI")]
    public Canvas flyCanvas;            // UI for this camera (optional)
    public Camera mainCamera;           // main camera reference (can be auto-filled)
    public float activeDepth = 10f;     // depth to use while fly camera is active

    [Header("Player freeze (optional)")]
    public Transform player;                            // player root transform (auto-find by tag if null)
    public string playerTag = "Player";
    public CharacterController playerController;        // optional
    public Rigidbody playerRigidbody;                   // optional
    public MonoBehaviour[] movementToDisable;           // drag&drop your movement scripts here

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

        // recheck player refs in case they appeared later
        if (!player && GameObject.FindGameObjectWithTag(playerTag))
            player = GameObject.FindGameObjectWithTag(playerTag).transform;
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

        // wait before movement (main camera is still on top)
        yield return new WaitForSeconds(waitBefore);

        // freeze player controls/physics
        FreezePlayer();

        // bring this camera to front
        thisCam.depth = activeDepth;

        // keep main camera underneath
        if (mainCamera != null)
            mainCamera.depth = originalMainDepth;

        // show UI
        if (flyCanvas != null)
            flyCanvas.gameObject.SetActive(true);

        // movement
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

        // disable arbitrary movement scripts
        if (movementToDisable != null)
        {
            foreach (var mb in movementToDisable)
            {
                if (mb != null) mb.enabled = false;
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

            // stop motion & freeze physics
            playerRigidbody.velocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.isKinematic = true; // or: playerRigidbody.constraints = RigidbodyConstraints.FreezeAll;
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
            // не возвращаем скорость, оставл€ем ноль Ч чтобы игрок не Ђдернулс€ї после кат-сцены
        }

        // re-enable movement scripts
        if (movementToDisable != null)
        {
            foreach (var mb in movementToDisable)
            {
                if (mb != null) mb.enabled = true;
            }
        }
    }
}
