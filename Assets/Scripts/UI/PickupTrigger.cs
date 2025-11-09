using UnityEngine;

public class PickupTrigger : MonoBehaviour
{
    public FlyCamera flyCam;
    public Transform flyTarget;
    public Transform lookAt;
    public Transform startPoint;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        // just start fly, no camera toggling here
        Camera mainCam = Camera.main;
        flyCam.PlayFly(startPoint, flyTarget, lookAt, mainCam);

        // optionally:
        // Destroy(gameObject);
    }
}
