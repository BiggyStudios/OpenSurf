using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

public class ModLoader : MonoBehaviour
{
    public static ModLoader Instance { get; private set; }

    private const string MOD_EXTENTION = ".brmf";
    private readonly Dictionary<string, IMod> _loadedMods = new Dictionary<string, IMod>();
    private readonly Dictionary<string, AssetBundle> _modAssets = new Dictionary<string, AssetBundle>();

    public string ModsDirectory => Path.Combine(Application.dataPath, "../Mods");

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        else
        {
            Destroy(gameObject);
            return;
        }

        Directory.CreateDirectory(ModsDirectory);
        LoadMods();
    }

    private void LoadMods()
    {
        string[] modFiles = Directory.GetFiles(ModsDirectory, $"*{MOD_EXTENTION}", SearchOption.TopDirectoryOnly);

        foreach (string modFile in modFiles)
        {
            try
            {
                LoadMod(modFile);
            }

            catch (Exception e)
            {
                Debug.LogError($"Failed to load mod {modFile}: {e.Message}");
            }
        }

        InitializeMods();
    }

    private void LoadMod(string modPath)
    {
        using (var archive = System.IO.Compression.ZipFile.OpenRead(modPath))
        {
            var manifestEntry = archive.GetEntry("manifest.json");
            if (manifestEntry == null)
                throw new Exception("Mod manifest not found");

            ModManifest manifest;
            using (var reader = new StreamReader(manifestEntry.Open()))
            {
                manifest = JsonConvert.DeserializeObject<ModManifest>(reader.ReadToEnd());
            }

            var assemblyEntry = archive.GetEntry("mod.dll");
            if (assemblyEntry != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var assemblyStream = assemblyEntry.Open())
                    {
                        assemblyStream.CopyTo(memoryStream);
                    }

                    Assembly assembly = Assembly.Load(memoryStream.ToArray());
                    LoadModAssembly(assembly, manifest);
                }
            }

            var assetsEntry = archive.GetEntry("assets");
            if (assemblyEntry != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var assetStream = assetsEntry.Open())
                    {
                        assetStream.CopyTo(memoryStream);
                    }

                    var assetBundle = AssetBundle.LoadFromMemory(memoryStream.ToArray());
                    _modAssets[manifest.ModID] = assetBundle;
                }
            }
        }
    }

    private void LoadModAssembly(Assembly assembly, ModManifest manifest)
    {
        Type modType = assembly.GetType(manifest.EntryPoint);
        if (modType == null || !typeof(IMod).IsAssignableFrom(modType))
            throw new Exception($"Invalid mod entry point: {manifest.EntryPoint}");
        
        IMod mod = (IMod)Activator.CreateInstance(modType);
        _loadedMods[manifest.ModID] = mod;
    }

    private void InitializeMods()
    {
        foreach (var mod in _loadedMods.Values)
        {
            try
            {
                mod.Initialize();
                mod.OnEnabled();
            }

            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize mod {mod.ModID}: {e.Message}");
            }
        }
    }

    public T GetModAsset<T>(string modID, string assetName) where T : UnityEngine.Object
    {
        if (_modAssets.TryGetValue(modID, out var bundle))
        {
            return bundle.LoadAsset<T>(assetName);
        }

        return null;
    }

    private void OnDestroy()
    {
        foreach (var mod in _loadedMods.Values)
        {
            try
            {
                mod.OnDisabled();
            }

            catch (Exception e)
            {
                Debug.LogError($"Error disabling mod {mod.ModID}: {e.Message}");
            }
        }

        foreach (var bundle in _modAssets.Values)
        {
            bundle.Unload(true);
        }
    }
}
