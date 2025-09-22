using UnityEngine;

public class KeepInitialRotation : MonoBehaviour
{
    private Quaternion initialRotation;

    void Awake()
    {
        // запоминаем как камера стоит в редакторе
        initialRotation = transform.localRotation;
    }

    void Start()
    {
        // возвращаем в сохранённый угол
        transform.localRotation = initialRotation;
    }
}
