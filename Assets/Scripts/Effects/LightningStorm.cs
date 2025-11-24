using UnityEngine;

public class LightningStorm : MonoBehaviour
{
    public ParticleSystem lightningFX;
    public ParticleSystem smokeFX;
    public float stormDuration = 3f;

    public void PlayStorm()
    {
        if (lightningFX) lightningFX.Play();
        if (smokeFX) smokeFX.Play();

        // удалить объект эффекта после шторма
        Destroy(gameObject, stormDuration + 1f);
    }
}
