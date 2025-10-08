using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource), typeof(Collider))]
public class MarionPortalVoice : MonoBehaviour
{
    [Header("Voice Line")]
    public AudioClip firstPortalClip;
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Music Fade")]
    public AudioSource musicSource;      
    [Range(0f, 1f)] public float fadeTo = 0.2f;
    public float fadeSpeed = 2f;

    [Header("Settings")]
    public bool playOnlyOnce = true;
    public bool triggerOnPlayerOnly = true;

    private bool hasPlayed = false;
    private AudioSource voiceSource;

    void Awake()
    {
        voiceSource = GetComponent<AudioSource>();
        voiceSource.playOnAwake = false;
        voiceSource.spatialBlend = 1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playOnlyOnce && hasPlayed) return;
        if (triggerOnPlayerOnly && !other.CompareTag("Player")) return;

        StartCoroutine(PlayVoiceWithFade());
    }

    private IEnumerator PlayVoiceWithFade()
    {
        hasPlayed = true;

        if (musicSource != null)
        {
            float start = musicSource.volume;
            for (float t = 0; t < 1; t += Time.deltaTime * fadeSpeed)
            {
                musicSource.volume = Mathf.Lerp(start, fadeTo, t);
                yield return null;
            }
        }

        voiceSource.PlayOneShot(firstPortalClip, volume);
        yield return new WaitForSeconds(firstPortalClip.length);

        if (musicSource != null)
        {
            float start = musicSource.volume;
            for (float t = 0; t < 1; t += Time.deltaTime * fadeSpeed)
            {
                musicSource.volume = Mathf.Lerp(start, 1f, t);
                yield return null;
            }
        }
    }
}