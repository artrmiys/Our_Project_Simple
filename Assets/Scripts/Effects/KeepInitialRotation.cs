using UnityEngine;

public class KeepInitialRotation : MonoBehaviour
{
    private Quaternion initialRotation;

    void Awake()
    {
        // ���������� ��� ������ ����� � ���������
        initialRotation = transform.localRotation;
    }

    void Start()
    {
        // ���������� � ���������� ����
        transform.localRotation = initialRotation;
    }
}
