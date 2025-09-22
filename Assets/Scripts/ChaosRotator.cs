using UnityEngine;

public class ChaosRotator : MonoBehaviour
{
    [Header("�����")]
    public Transform player;     // ������ �� ������

    [Header("������� ��� ��������")]
    public Transform[] targets;  // 5 ��������

    [Header("��������� ��������")]
    public float moveRadius = 1f;      // ������ ���������� ��������
    public float moveSpeed = 1f;       // �������� �����������
    public float rotationSpeed = 20f;  // �������� ��������

    private Vector3[] baseOffsets;     // ������������� �������� ������ �����
    private Vector3[] randomOffsets;   // ��������� ��������� ��������

    void Start()
    {
        baseOffsets = new Vector3[targets.Length];
        randomOffsets = new Vector3[targets.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            // ��������� ��������� ��������� ��� �������� �� ������
            baseOffsets[i] = targets[i].position - player.position;

            // ��������� ��������� ��������� ��������
            randomOffsets[i] = Random.insideUnitSphere * moveRadius;
        }
    }

    void Update()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null) continue;

            // �������� �� �����
            targets[i].Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            targets[i].Rotate(Vector3.right, rotationSpeed * 0.5f * Time.deltaTime);

            // �������� ���� = ������� ������ + �������������� �������� + ��������� "���������"
            Vector3 targetPos = player.position + baseOffsets[i] + randomOffsets[i];

            // ������� �������� � ����
            targets[i].position = Vector3.MoveTowards(
                targets[i].position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            // ���� ����� � ��������� ����� ��������� ��������
            if (Vector3.Distance(targets[i].position, targetPos) < 0.1f)
            {
                randomOffsets[i] = Random.insideUnitSphere * moveRadius;
            }
        }
    }
}


