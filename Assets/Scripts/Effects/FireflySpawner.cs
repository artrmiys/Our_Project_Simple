using UnityEngine;

public class FireflySpawner : MonoBehaviour
{
    public GameObject fireflyPrefab;
    public int fireflyCount = 1000;
    public float spawnRadius = 50f;     // ������ ������ ������
    public float minHeight = 5f;      // ������ ������� �� Y
    public float maxHeight = 100f;        // ������� ������� �� Y
    public Transform player;            // ������ �� ������

    void Start()
    {
        for (int i = 0; i < fireflyCount; i++)
        {
            SpawnFirefly();
        }
    }

    void SpawnFirefly()
    {
        if (player == null) return;

        // ��������� ����� � ����� ������ ������
        Vector3 randomPos = player.position + Random.insideUnitSphere * spawnRadius;

        // ������������ ������ (����� �� ������� � �������)
        randomPos.y = Random.Range(player.position.y + minHeight, player.position.y + maxHeight);

        Instantiate(fireflyPrefab, randomPos, Quaternion.identity, transform);
    }
}
