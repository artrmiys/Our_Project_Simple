using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public TextMeshPro text;
    public float riseSpeed = 1.5f;     // move up speed
    public float lifeTime = 1f;        // lifetime
    public Vector3 randomOffset = new Vector3(0.5f, 0.5f, 0f);

    private float timer;
    private Transform cam;

    void Awake()
    {
        cam = Camera.main.transform;
    }

    public void Setup(float damage, Color color)
    {
        transform.position += new Vector3(
            Random.Range(-randomOffset.x, randomOffset.x),
            Random.Range(0f, randomOffset.y),
            0f
        );

        if (text)
        {
            // text + color
            text.text = "-" + damage.ToString("0");
            text.color = color;

            // scale by damage (min 1x, max 3x)
            float scale = Mathf.Clamp(1f + (damage / 20f), 1f, 3f);
            transform.localScale = Vector3.one * scale;
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
