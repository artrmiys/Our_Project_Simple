using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-102)]
[ExecuteAlways]
[AddComponentMenu("Navigation/NavMeshSurface", 30)]
public class NavMeshSurface : MonoBehaviour
{
    public int agentTypeID = 0;
    public LayerMask layerMask = ~0;
    public NavMeshCollectGeometry useGeometry = NavMeshCollectGeometry.RenderMeshes;
    public int defaultArea = 0;

    private NavMeshData navMeshData;
    private NavMeshDataInstance navMeshDataInstance;

    public void BuildNavMesh()
    {
        // очищаем предыдущий
        if (navMeshDataInstance.valid)
            NavMesh.RemoveNavMeshData(navMeshDataInstance);

        // источники
        var sources = new List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(null, layerMask, useGeometry, defaultArea, new List<NavMeshBuildMarkup>(), sources);

        // границы
        var bounds = new Bounds(Vector3.zero, new Vector3(500, 500, 500));

        // создаём
        navMeshData = NavMeshBuilder.BuildNavMeshData(
            NavMesh.GetSettingsByID(agentTypeID),
            sources,
            bounds,
            Vector3.zero,
            Quaternion.identity
        );

        // добавляем в сцену
        navMeshDataInstance = NavMesh.AddNavMeshData(navMeshData);
    }
}
