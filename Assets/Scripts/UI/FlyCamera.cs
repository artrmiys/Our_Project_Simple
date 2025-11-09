using UnityEngine;
using System.Collections;

public class FlyCamera : MonoBehaviour
{
    public float flyTime = 1.5f;        // how long the fly movement takes
    public float waitAfter = 1.0f;      // how long to stay at the end
    public float waitBefore = 1.0f;     // how long to wait before starting the fly movement
    public float switchDelay = 1.0f;    // how long to wait before the whole sequence
    public Canvas flyCanvas;            // UI for this camera (optional)
    public Camera mainCamera;           // main camera reference (can be auto-filled)
    public float activeDepth = 10f;     // depth to use while fly camera is active

    private Camera thisCam;
    private Transform fromPoint;
    private bool isPlaying = false;
    private float originalFlyDepth;
    private float originalMainDepth;

    private void Awake()
    {
        thisCam = GetComponent<Camera>();

        // remember starting depths
        originalFlyDepth = thisCam.depth;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
            originalMainDepth = mainCamera.depth;

        if (flyCanvas != null)
            flyCanvas.gameObject.SetActive(false);
    }

    public void PlayFly(Transform startPoint, Transform targetPoint, Transform lookAt, Camera mainCamFromTrigger = null)
    {
        if (isPlaying) return;

        if (mainCamFromTrigger != null)
            mainCamera = mainCamFromTrigger;

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
    }
}
