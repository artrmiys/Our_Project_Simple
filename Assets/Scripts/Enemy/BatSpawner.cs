using UnityEngine;

public class BatSpawner : MonoBehaviour
{
    public FlyingBat batPrefab;
    public Transform player;
    public float spawnInterval = 5f;
    public int maxAlive = 5;
    public float spawnRadius = 3f;

    [Header("Scale range")]
    public Vector2 scaleRange = new Vector2(0.8f, 1.3f);

    float _timer;
    int _alive;

    void Start()
    {
        if (!player)
        {
            var p = GameObject.FindWithTag("Player");
            if (p) player = p.transform;
        }
    }

    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= spawnInterval && _alive < maxAlive)
        {
            _timer = 0f;
            SpawnBat();
        }
    }

    void SpawnBat()
    {
        if (!batPrefab) return;

        Vector3 offset = Random.insideUnitSphere;
        offset.y = Mathf.Abs(offset.y);
        offset *= spawnRadius;

        Vector3 spawnPos = transform.position + offset;

        FlyingBat bat = Instantiate(batPrefab, spawnPos, Quaternion.identity);

        // random scale
        float s = Random.Range(scaleRange.x, scaleRange.y);
        bat.transform.localScale = Vector3.one * s;

        if (player)
            bat.player = player;

        // track life
        BatLifeTracker tracker = bat.gameObject.AddComponent<BatLifeTracker>();
        tracker.spawner = this;

        _alive++;
    }

    public void OnBatDied()
    {
        _alive = Mathf.Max(0, _alive - 1);
    }

    private class BatLifeTracker : MonoBehaviour
    {
        public BatSpawner spawner;

        private void OnDestroy()
        {
            if (spawner != null)
                spawner.OnBatDied();
        }
    }
}

