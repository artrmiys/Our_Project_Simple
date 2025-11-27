using UnityEditor;
using UnityEngine;

public class DownscaleTextures
{
    [MenuItem("Tools/Textures/Downscale All To 1024")]
    static void DownscaleAll()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            importer.maxTextureSize = 1024; // можно 2048
            importer.textureCompression = TextureImporterCompression.Compressed;

            importer.SaveAndReimport();
        }

        Debug.Log("Done: all textures set to 1024 & Compressed");
    }
}

