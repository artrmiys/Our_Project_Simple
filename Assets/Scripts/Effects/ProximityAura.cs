using UnityEngine;
using UnityEngine.UI;

public class ProximityAura : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Image auraImage;
    public Transform target;

    [Header("Unlock (optional)")]
    [Tooltip("If assigned, aura will start hiding only after this controller is unlocked")]
    public EnableScriptsOnKey unlockController;
    public bool useUnlock = true;   // if false, behaves like original

    [Header("Settings")]
    public float triggerDistance = 5f;
    public float hideDistance = 1f;
    public float fadeSpeed = 3f;
    public float scaleMultiplier = 1.4f;
    public float basePulseSpeed = 2f;
    public float maxPulseSpeed = 8f;
    public float pulseStrength = 0.05f;

    Color baseColor;
    Vector3 baseScale;
    float currentAlpha;
    bool isHiding;
    bool hasFlashed;

    void Start()
    {
        if (!player)
        {
            var p = GameObject.FindWithTag("Player");
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
        if (!auraImage || !player) return;

        // 🔒 можно ли сейчас прятать ауру?
        bool canHideNow = true;
        if (useUnlock && unlockController != null && !unlockController.IsUnlocked)
        {
            // до нажатия E: запрещаем переход в режим скрытия
            canHideNow = false;
        }

        // target was destroyed OR just disabled -> hide
        if (canHideNow && (target == null || !target.gameObject.activeInHierarchy))
        {
            isHiding = true;
        }

        if (isHiding)
        {
            FadeAndFlash();
            return;
        }

        float dist = Vector3.Distance(player.position, target.position);

        // прячемся при касании / близости — ТОЛЬКО если уже можно
        if (canHideNow && dist < hideDistance)
        {
            isHiding = true;
            return;
        }

        float t = Mathf.Clamp01(1f - dist / triggerDistance);

        float targetAlpha = t;
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

        Color c = baseColor;
        c.a = currentAlpha;
        auraImage.color = c;

        float pulseSpeed = Mathf.Lerp(basePulseSpeed, maxPulseSpeed, t);
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseStrength;
        float scaleFactor = Mathf.Lerp(1f, scaleMultiplier, currentAlpha) + pulse * currentAlpha;
        auraImage.rectTransform.localScale = baseScale * scaleFactor;
    }

    void FadeAndFlash()
    {
        if (!hasFlashed)
        {
            hasFlashed = true;
            auraImage.rectTransform.localScale = baseScale * (scaleMultiplier * 1.8f);
        }

        currentAlpha = Mathf.Lerp(currentAlpha, 0f, Time.deltaTime * fadeSpeed * 3f);

        Color c = baseColor;
        c.a = currentAlpha;
        auraImage.color = c;

        if (currentAlpha < 0.02f)
            Destroy(gameObject);
    }
}
