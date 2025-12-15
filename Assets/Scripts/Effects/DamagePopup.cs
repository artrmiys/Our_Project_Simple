using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public TextMeshPro text;
    public float riseSpeed = 1.5f;
    public float lifeTime = 1f;
    public Vector3 randomOffset = new Vector3(0.5f, 0.5f, 0f);

    [Header("Glow (optional)")]
    public Renderer glowRenderer;        // сюда ДОЛЖЕН быть перетащен объект с Mesh/SpriteRenderer
    public float emissionIntensity = 3f; // во сколько умножать цвет

    float timer;
    Transform cam;

    void Awake()
    {
        cam = Camera.main ? Camera.main.transform : null;
    }

    public void Setup(float damage, Color color)
    {
        // рандомный сдвиг, как у тебя
        transform.position += new Vector3(
            Random.Range(-randomOffset.x, randomOffset.x),
            Random.Range(0f, randomOffset.y),
            0f
        );

        if (text)
        {
            text.text = "-" + damage.ToString("0");
            text.color = color;

            float scale = Mathf.Clamp(1f + (damage / 20f), 1f, 3f);
            transform.localScale = Vector3.one * scale;
        }
        else
        {
            Debug.LogWarning("[DamagePopup] TextMeshPro не назначен");
        }

        // ===== ЭМИССИЯ =====
        if (glowRenderer != null)
        {
            // создаём ИНСТАНС материала, иначе все попапы будут менять один и тот же
            Material m = glowRenderer.material;

            if (!m.IsKeywordEnabled("_EMISSION"))
                m.EnableKeyword("_EMISSION");

            Color emissive = color * emissionIntensity;
            m.SetColor("_EmissionColor", emissive);

            //Debug.Log($"[DamagePopup] emission set on {glowRenderer.name} to {emissive}");
        }
        else
        {
            //Debug.Log("[DamagePopup] glowRenderer НЕ назначен в инспекторе, светиться нечему");
        }
    }

    void Update()
    {
        if (cam)
            transform.LookAt(transform.position + cam.forward);

        transform.position += Vector3.up * riseSpeed * Time.deltaTime;

        timer += Time.deltaTime;
        if (text)
        {
            Color c = text.color;
            c.a = Mathf.Lerp(1f, 0f, timer / lifeTime);
            text.color = c;
        }

        if (timer >= lifeTime)
            Destroy(gameObject);
    }
}
