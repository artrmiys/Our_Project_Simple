using UnityEngine;

public class FireflySpawner : MonoBehaviour
{
    public GameObject fireflyPrefab;
    public int fireflyCount = 1000;
    public float spawnRadius = 50f;     // радиус вокруг игрока
    public float minHeight = 5f;      // нижняя граница по Y
    public float maxHeight = 100f;        // верхняя граница по Y
    public Transform player;            // ссылка на игрока

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

        // случайная точка в сфере вокруг игрока
        Vector3 randomPos = player.position + Random.insideUnitSphere * spawnRadius;

        // ограничиваем высоту (чтобы не улетали в потолок)
        randomPos.y = Random.Range(player.position.y + minHeight, player.position.y + maxHeight);

        Instantiate(fireflyPrefab, randomPos, Quaternion.identity, transform);
    }
}
