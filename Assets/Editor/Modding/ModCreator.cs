using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ModCreator : EditorWindow
{
    private ModManifest _manifest = new ModManifest();

    [MenuItem("Modding/Mod Creator")]
    public static void ShowWindow()
    {
        GetWindow<ModCreator>("Mod Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Mod Settings", EditorStyles.boldLabel);

        _manifest.ModID = EditorGUILayout.TextField("Mod ID", _manifest.ModID);
        _manifest.ModName = EditorGUILayout.TextField("Mod Name", _manifest.ModName);
        _manifest.Version = EditorGUILayout.TextField("Version", _manifest.Version);
        _manifest.EntryPoint = EditorGUILayout.TextField("Entry Point", _manifest.EntryPoint);
        _manifest.MinGameVersion = EditorGUILayout.TextField("Min Game Version", _manifest.MinGameVersion);

        if (GUILayout.Button("Package Mod"))
        {
            PackageMod();
        }
    }

    private void PackageMod()
    {
        string outputPath = EditorUtility.SaveFilePanel("Save Mod Package", "", $"{_manifest.ModID}.brmf", "brmf");

        if (string.IsNullOrEmpty(outputPath))
            return;
        
        string tempDir = Path.Combine(Path.GetTempPath(), "ModPackaging");
        if (Directory.Exists(tempDir))
            Directory.Delete(tempDir, true);
        Directory.CreateDirectory(tempDir);

        try
        {
            File.WriteAllText(Path.Combine(tempDir, "manifest.json"), JsonConvert.SerializeObject(_manifest, Formatting.Indented));

            BuildPipeline.BuildAssetBundles(tempDir, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

            if (File.Exists(outputPath))
                File.Delete(outputPath);

            ZipFile.CreateFromDirectory(tempDir, outputPath);

            Debug.Log($"Mod packaged successfully to: {outputPath}");
        }

        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }
}
