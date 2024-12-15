using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEditor.VersionControl;

using UnityEngine;

public class ModBuildTool : EditorWindow
{
    private BuildTarget _buildTarget;

    [MenuItem("Modding/Build Mod")]
    public static void ShowWindow()
    {
        GetWindow<ModBuildTool>("Mod Builder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Mod Builder", EditorStyles.boldLabel);

        _buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", _buildTarget);

        if (GUILayout.Button("Build Mod"))
        {
            var config = Selection.activeObject as ModConfig;
            if (config != null)
            {
                BuildMod(config);
            }

            else
            {
                EditorUtility.DisplayDialog("Error", "Please select a MapConfig asset", "OK");
            }
        }
    }

    private void BuildMod(ModConfig config)
    {
        string outputPath = EditorUtility.SaveFilePanel("Save Mod", "", config.ModID + ".brmf", "brmf");

        if (string.IsNullOrEmpty(outputPath))
        {
            Debug.Log("Build cancelled - no output path selected");
            return;
        }

        string tempFolder = Path.Combine(Path.GetTempPath(), "ModBuild_" + System.Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempFolder);

        try
        {
            AssetBundleBuild[] buildMod = new AssetBundleBuild[]
            {
                new AssetBundleBuild
                {
                    assetBundleName = config.ModID,
                    assetNames = new string[] { AssetDatabase.GetAssetPath(config) },
                    assetBundleVariant = "",
                    addressableNames = new string[] { "ModConfig" }
                }
            };

            BuildPipeline.BuildAssetBundles(tempFolder, buildMod, BuildAssetBundleOptions.None, _buildTarget);

            string builtBundle = Path.Combine(tempFolder, config.ModID);
            if (File.Exists(builtBundle))
            {
                File.Copy(builtBundle, outputPath, true);
                Debug.Log("Mod built successfully");
                EditorUtility.RevealInFinder(outputPath);
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
