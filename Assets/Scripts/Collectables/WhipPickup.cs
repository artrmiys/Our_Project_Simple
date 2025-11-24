using UnityEngine;

public class WhipPickup : MonoBehaviour
{
    public enum WhipKind
    {
        Basic,
        Wide
    }

    [Header("Type")]
    public WhipKind kind = WhipKind.Basic;   // dropdown in Inspector

    [Header("sound")]
    public AudioSource audioSource;
    public AudioClip pickupSound;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerAttack pa = other.GetComponent<PlayerAttack>();
        if (pa != null)
        {
            if (kind == WhipKind.Basic)
                pa.CollectWhipBasic();  // LMB
            else
                pa.CollectWhipWide();   // RMB
        }

        if (audioSource && pickupSound)
            audioSource.PlayOneShot(pickupSound);

        gameObject.SetActive(false);
    }
}
