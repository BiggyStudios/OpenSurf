using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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

            string[] scriptPaths = Directory.GetFiles("Assets/Mods", "*.cs", SearchOption.AllDirectories);
            if (scriptPaths.Length > 0)
            {
                string dllPath = Path.Combine(tempDir, "mod.dll");
                BuildModDLL(scriptPaths, dllPath);
            }

            if (File.Exists(outputPath))
                File.Delete(outputPath);

            ZipFile.CreateFromDirectory(tempDir, outputPath);

            UnityEngine.Debug.Log($"Mod packaged successfully to: {outputPath}");
        }

        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    private void BuildModDLL(string[] scriptPaths, string outputPath)
    {
        string compilerPath = GetCompilerPath();

        if (string.IsNullOrEmpty(compilerPath))
        {
            UnityEngine.Debug.LogError("Could not find C# compiler (csc.exe)");
            return;
        }

        StringBuilder args = new StringBuilder();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            args.Append("/target:library ");
            args.Append($"/out:\"{outputPath}\" ");
            args.Append($"/reference:\"{Path.Combine(EditorApplication.applicationContentsPath, "Managed/UnityEngine.dll")}\" ");
            args.Append($"/reference:\"{Path.Combine(EditorApplication.applicationContentsPath, "Managed/UnityEngine.CoreModule.dll")}\" ");
        }

        else
        {
            args.Append("-target:library ");
            args.Append($"-out:\"{outputPath}\" ");
            args.Append($"-reference:\"{Path.Combine(EditorApplication.applicationContentsPath, "Managed/UnityEngine.dll")}\" ");
            args.Append($"-reference:\"{Path.Combine(EditorApplication.applicationContentsPath, "Managed/UnityEngine.CoreModule.dll")}\" ");
        }

        foreach (string scriptPath in scriptPaths)
        {
            args.Append($"\"{scriptPaths}\" ");
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = compilerPath,
            Arguments = args.ToString(),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(startInfo))
        {
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                UnityEngine.Debug.LogError($"Compilation failed:\n{error}");
                throw new System.Exception("Failed to build DLL");
            }

            else if (!string.IsNullOrEmpty(output))
            {
                UnityEngine.Debug.Log($"Compiler output:\n{output}");
            }
        }
    }

    private string GetCompilerPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "csc.exe");
        }

        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string[] possiblePaths =
            {
                "/usr/bin/mcs",
                "/usr/bin/csc",
                "/usr/local/bin/mcs",
                "/usr/local/bin/csc"
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                    return path;
            }
        }
        
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string[] possiblePaths =
            {
                "/usr/local/bin/mcs",
                "/usr/local/bin/csc",
                "/usr/bin/mcs",
                "/usr/bin/csc"
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                    return path;
            }
        }

        return null;
    }
}
