using UnityEngine;
using UnityEngine.UI;

public class ProximityAura : MonoBehaviour
{
    [Header("References")]
    public Transform player;          // игрок
    public Image auraImage;           // UI Image (сама аура)
    public Transform target;          // объект, возле которого аура

    [Header("Settings")]
    public float triggerDistance = 5f;   // дистанция активации
    public float hideDistance = 1f;      // при какой дистанции исчезает
    public float fadeSpeed = 3f;         // скорость плавности
    public float scaleMultiplier = 1.4f; // увеличение при приближении
    public float basePulseSpeed = 2f;    // базовая скорость "дыхания"
    public float maxPulseSpeed = 8f;     // максимальная скорость при приближении
    public float pulseStrength = 0.05f;  // амплитуда пульсации

    private Color baseColor;
    private Vector3 baseScale;
    private float currentAlpha;
    private bool isHiding = false;
    private bool hasFlashed = false;

    void Start()
    {
        if (!player)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p) player = p.transform;
        }

        if (!target) target = transform;

        if (auraImage)
        {
            baseColor = auraImage.color;
            baseScale = auraImage.rectTransform.localScale;
        }
    }

    void Update()
    {
        if (!auraImage || !player || !target) return;

        float dist = Vector3.Distance(player.position, target.position);

        // если слишком близко — начать исчезновение
        if (dist < hideDistance)
        {
            isHiding = true;
        }

        if (isHiding)
        {
            FadeAndFlash();
            return;
        }

        // интенсивность по расстоянию
        float t = Mathf.Clamp01(1f - dist / triggerDistance);

        // прозрачность
        float targetAlpha = t;
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

        // применяем альфу, но сохраняем цвет из инспектора
        Color c = baseColor;
        c.a = currentAlpha;
        auraImage.color = c;

        // адаптивная частота пульса
        float pulseSpeed = Mathf.Lerp(basePulseSpeed, maxPulseSpeed, t);

        // пульсация и масштаб
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseStrength;
        float scaleFactor = Mathf.Lerp(1f, scaleMultiplier, currentAlpha) + pulse * currentAlpha;
        auraImage.rectTransform.localScale = baseScale * scaleFactor;
    }

    void FadeAndFlash()
    {
        // однократная вспышка
        if (!hasFlashed)
        {
            hasFlashed = true;
            auraImage.rectTransform.localScale = baseScale * (scaleMultiplier * 1.8f);
        }

        // плавное исчезновение
        currentAlpha = Mathf.Lerp(currentAlpha, 0f, Time.deltaTime * fadeSpeed * 3f);

        Color c = baseColor;
        c.a = currentAlpha;
        auraImage.color = c;

        if (currentAlpha < 0.02f)
            Destroy(gameObject);
    }
}
