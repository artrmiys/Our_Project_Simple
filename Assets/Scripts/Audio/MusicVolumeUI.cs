using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MusicVolumeUI : MonoBehaviour
{
    public AudioMixer mixer;     
    public Slider volumeSlider;  

    void Start()
    {
        float v = PlayerPrefs.GetFloat("music_volume", 0.8f);
        volumeSlider.value = v;
        Apply(v);
        volumeSlider.onValueChanged.AddListener(Apply);
    }

    void Apply(float value)
    {
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        mixer.SetFloat("MusicVol", dB);         
        PlayerPrefs.SetFloat("music_volume", value);
    }
}