using UnityEngine;

public class TarakanSpawner : MonoBehaviour
{
    public GameObject tarakanPrefab;
    public int groupSize = 5;          // ������� ��������� �� ���
    public float spawnInterval = 3f;   // ������� ���������
    public float spawnRadius = 1f;     // ������ "������"

    private float timer;

    void Start()
    {
        timer = spawnInterval;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnGroup();
            timer = spawnInterval;
        }
    }

    void SpawnGroup()
    {
        for (int i = 0; i < groupSize; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0.2f,
                Random.Range(-spawnRadius, spawnRadius)
            );

            Instantiate(tarakanPrefab, spawnPos, Quaternion.identity);
        }
    }
}
