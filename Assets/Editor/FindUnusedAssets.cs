using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FindUnusedAssets
{
    [MenuItem("Tools/Cleanup/Find Unused Assets")]
    static void Find()
    {
        // 1. Собираем все ассеты, которые реально используются
        var usedAssets = new HashSet<string>();

        // Берём все сцены из Build Settings
        var scenes = EditorBuildSettings.scenes;
        foreach (var scene in scenes)
        {
            if (!scene.enabled) continue;
            string[] deps = AssetDatabase.GetDependencies(scene.path, true);
            foreach (var d in deps)
                usedAssets.Add(d);
        }

        // Можно добавить сюда префабы, если нужно:
        // string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

        // 2. Ищем все ассеты в проекте
        string[] guids = AssetDatabase.FindAssets(""); // всё подряд
        var unused = new List<string>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // пропускаем папки и сцены (сцены уже учли)
            if (AssetDatabase.IsValidFolder(path)) continue;
            if (path.EndsWith(".unity")) continue;

            // если ассет не в списке зависимостей — кандидат на мусор
            if (!usedAssets.Contains(path))
                unused.Add(path);
        }

        Debug.Log($"Найдены неиспользуемые ассеты: {unused.Count}");
        foreach (var a in unused)
            Debug.Log(a);
    }
}
