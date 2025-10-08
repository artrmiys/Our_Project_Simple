using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LevelMusicFader : MonoBehaviour
{
    public float fadeDuration = 1.5f; // секунды
    AudioSource levelAS;

    void Awake()
    {
        levelAS = GetComponent<AudioSource>();
        levelAS.playOnAwake = false;
        levelAS.volume = 0f;
    }

    void Start()
    {
        var menuGO = GameObject.Find("MenuMusic");
        var menuAS = menuGO ? menuGO.GetComponent<AudioSource>() : null;
        StartCoroutine(Crossfade(menuAS, levelAS, fadeDuration, menuGO));
    }

    IEnumerator Crossfade(AudioSource from, AudioSource to, float d, GameObject destroyAfter)
    {
        if (to) to.Play();
        float t = 0f, fromStart = from ? from.volume : 0f, toStart = to ? to.volume : 0f;

        while (t < d)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / d);
            if (from) from.volume = Mathf.Lerp(fromStart, 0f, k);
            if (to) to.volume = Mathf.Lerp(toStart, 1f, k);
            yield return null;
        }

        if (from) { from.Stop(); from.volume = fromStart; }
        if (to) to.volume = 1f;
        if (destroyAfter) Destroy(destroyAfter);
    }
}
