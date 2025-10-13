using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource), typeof(Collider))]
public class MarionPortalVoice : MonoBehaviour
{
    [Header("Voice Line")]
    public AudioClip firstPortalClip;
    [Range(0f, 1f)] public float voiceVolume = 1f;
    public AudioSource musicSource;

    [Header("Fade Settings")]
    [Range(0f, 1f)] public float fadeTo = 0.25f;
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
        voiceSource.loop = false;
        voiceSource.spatialBlend = 0f; // 2D звук
        voiceSource.volume = 1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playOnlyOnce && hasPlayed) return;
        if (triggerOnPlayerOnly && !other.CompareTag("Player")) return;

        StartCoroutine(PlayVoiceAndFadeMusic());
    }

    private IEnumerator PlayVoiceAndFadeMusic()
    {
        hasPlayed = true;

        // Плавно приглушаем музыку
        if (musicSource != null)
        {
            float start = musicSource.volume;
            for (float t = 0; t < 1; t += Time.deltaTime * fadeSpeed)
            {
                musicSource.volume = Mathf.Lerp(start, fadeTo, t);
                yield return null;
            }
        }

        // Проигрываем фразу
        if (firstPortalClip != null)
            voiceSource.PlayOneShot(firstPortalClip, voiceVolume);

        yield return new WaitForSeconds(firstPortalClip.length);

        // Возвращаем музыку обратно
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
