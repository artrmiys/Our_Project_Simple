using UnityEngine;

public class RopeSpawner : MonoBehaviour
{
    public Rope_Follow_WithCollision ropePrefab; // префаб с настроенными полями
    public Transform[] anchors;                  // верхние точки
    public Transform[] ends;                     // точки на персонажах/объектах

    public float ropeLength = 3f;
    public int segments = 16;
    public bool enableCollision = true;
    public float colliderRadius = 0.02f;

    void Start()
    {
        int n = Mathf.Min(anchors.Length, ends.Length);
        for (int i = 0; i < n; i++)
        {
            var rope = Instantiate(ropePrefab, anchors[i].position, Quaternion.identity, transform);
            rope.startPoint = anchors[i];
            rope.endPoint = ends[i];
            rope.ropeLength = ropeLength;
            rope.segments = segments;
            rope.enableCollision = enableCollision;
            rope.colliderRadius = colliderRadius;
            rope.Build();
        }
    }
}
