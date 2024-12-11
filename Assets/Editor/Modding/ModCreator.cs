using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ModCreator : EditorWindow
{
        [MenuItem("Modding/Create/New Mod")]
        public static void CreateMod()
    {
        AssetDatabase.CreateFolder("Assets/Modding", "Mod");
        AssetDatabase.CreateFolder("Assets/Modding/Mod", "Assets");
        AssetDatabase.CreateFolder("Assets/Modding/Mod", "Prefabs");
        AssetDatabase.CreateFolder("Assets/Modding/Mod", "Scenes");
        AssetDatabase.CreateFolder("Assets/Modding/Mod", "Scripts");
    }

    [MenuItem("Modding/Package Mod")]
    public static void PackageMod()
    {
        string modFolder = "Assets/Modding/Mod";
        string outputFolder = "Assets/Modding/PackagedMod";

        if (!Directory.Exists(outputFolder))
            Directory.CreateDirectory(outputFolder);

        string bundleName = "mod.brmf";

        AssignBundleNames(modFolder, bundleName);

        BuildPipeline.BuildAssetBundles(outputFolder, BuildAssetBundleOptions.None, BuildTarget.StandaloneLinux64);

        Debug.Log("Successfully packaged mod");
    }

    private static void AssignBundleNames(string folderPath, string bundleName)
    {
        string[] assetPaths = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);

        foreach (string assetPath in assetPaths)
        {
            if (assetPath.EndsWith(".meta")) continue;
            string relativePath = assetPath.Replace(Application.dataPath, "Assets");

            AssetImporter importer = AssetImporter.GetAtPath(relativePath);
            if (importer != null)
            {
                importer.assetBundleName = bundleName;
            }
        }
    }
}
