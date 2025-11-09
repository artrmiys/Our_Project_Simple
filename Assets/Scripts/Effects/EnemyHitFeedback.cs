using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyHitFeedback : MonoBehaviour
{
    [Header("Renderer flash")]
    public Renderer targetRenderer;
    public Color hitColor = Color.red;
    public float flashTime = 0.1f;

    [Header("VFX")]
    public GameObject hitSmokePrefab;
    public GameObject hitSparksPrefab;
    public Transform vfxPoint;

    [Header("Animation (optional)")]
    public Animator animator;
    public string hitTriggerName = "Hit";

    private Health _health;
    private Material[] _mats;
    private Color[] _originalColors;
    private float _lastHP;

    void Awake()
    {
        _health = GetComponent<Health>();

        if (!targetRenderer)
            targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer)
        {
            _mats = targetRenderer.materials;
            _originalColors = new Color[_mats.Length];
            for (int i = 0; i < _mats.Length; i++)
                _originalColors[i] = _mats[i].color;
        }

        if (!vfxPoint)
            vfxPoint = transform;

        if (!animator)
            animator = GetComponentInChildren<Animator>();

        _health.onHealthChanged.AddListener(OnHealthChanged);

        _lastHP = _health.currentHP;
    }

    void OnHealthChanged(float current, float max)
    {
        if (current < _lastHP)
        {
            PlayHit();
        }

        _lastHP = current;
    }

    public void PlayHit()
    {
        if (_mats != null && _mats.Length > 0)
            StartCoroutine(FlashRoutine());

        if (hitSmokePrefab)
            Instantiate(hitSmokePrefab, vfxPoint.position, vfxPoint.rotation);

        if (hitSparksPrefab)
            Instantiate(hitSparksPrefab, vfxPoint.position, vfxPoint.rotation);

        if (animator && !string.IsNullOrEmpty(hitTriggerName))
            animator.SetTrigger(hitTriggerName);
    }

    IEnumerator FlashRoutine()
    {
        for (int i = 0; i < _mats.Length; i++)
            _mats[i].color = hitColor;

        yield return new WaitForSeconds(flashTime);

        for (int i = 0; i < _mats.Length; i++)
            _mats[i].color = _originalColors[i];
    }
}
