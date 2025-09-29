using UnityEngine;

public class FloatRotate : MonoBehaviour
{
    [Header("rotate")]
    public float rotSpeed = 30f;   // deg per sec

    [Header("float")]
    public float floatAmp = 0.2f;  // height
    public float floatSpeed = 1f;  // cycle speed

    Vector3 startPos;

    void Start()
    {
        // save start pos
        startPos = transform.localPosition;
    }

    void Update()
    {
        // rotate y
        transform.Rotate(Vector3.up * rotSpeed * Time.deltaTime, Space.World);

        // float y
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmp;
        Vector3 pos = transform.localPosition;
        pos.y = newY;
        transform.localPosition = pos;
    }
}
