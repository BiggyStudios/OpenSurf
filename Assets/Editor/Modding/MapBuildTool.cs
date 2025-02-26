using System.IO;
using UnityEditor;
using UnityEngine;

public class MapBuildTool : EditorWindow
{
    [MenuItem("Mapping/Build Map")]
    public static void ShowWindow()
    {
        GetWindow<MapBuildTool>("Map Builder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Map Builder", EditorStyles.boldLabel);

        if (GUILayout.Button("Build Map"))
        {
            var config = Selection.activeObject as MapConfig;
            if (config != null)
            {
                BuildMap(config);
            }

            else
            {
                EditorUtility.DisplayDialog("Error", "Please select a MapConfig asset", "OK");
            }
        }
    }

    private void BuildMap(MapConfig config)
    {
        string baseOutputPath = EditorUtility.SaveFolderPanel("Select output folder", "", "");

        if (string.IsNullOrEmpty(baseOutputPath))
        {
            Debug.Log("Build cancelled - no output path selected.");
            return;
        }

        BuildMapForPlatform(config, baseOutputPath, BuildTarget.StandaloneWindows64);
        BuildMapForPlatform(config, baseOutputPath, BuildTarget.StandaloneLinux64);
    }
    
    private void BuildMapForPlatform(MapConfig config, string baseOutputPath, BuildTarget buildTarget)
    {
        string platformName = buildTarget.ToString();
        string outputPathFolder = Path.Combine(baseOutputPath, platformName);
        Directory.CreateDirectory(outputPathFolder);
        string outputPath = Path.Combine(outputPathFolder, config.MapID.ToLower() + ".brmf");

        string tempFolder = Path.Combine(Path.GetTempPath(), "MapBuild_" + System.Guid.NewGuid().ToString() + "_" + platformName);
        Directory.CreateDirectory(tempFolder);

        try
        {
            AssetBundleBuild[] buildMap = new AssetBundleBuild[]
            {
                new AssetBundleBuild
                {
                    assetBundleName = config.MapID.ToLower(),
                    assetNames = new string[] { AssetDatabase.GetAssetPath(config) },
                    assetBundleVariant = "",
                    addressableNames = new string[] { "MapConfig" }
                }
            };

            BuildPipeline.BuildAssetBundles(tempFolder, buildMap, BuildAssetBundleOptions.None, buildTarget);

            string builtBundle = Path.Combine(tempFolder, config.MapID.ToLower());
            if (File.Exists(builtBundle))
            {
                File.Copy(builtBundle, outputPath, true);
                Debug.Log($"Map built successfully");
            }
        }

        finally
        {
            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }
        }
    }
}
