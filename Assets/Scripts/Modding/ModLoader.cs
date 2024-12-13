using System.Collections.Generic;
using System.IO;
using P90brush;
using UnityEngine;

public class ModLoader : MonoBehaviour
{
    public static ModLoader Instance { get; private set; }
    public static MovementConfig MapMovementConfig;
    [SerializeField] private Transform _mapTransform;
    private Dictionary<string, AssetBundle> _loadedMods = new Dictionary<string, AssetBundle>();

    private void Awake()
    {
        Instance = this;
        LoadMods();
    }

    private void LoadMods()
    {
        string modPath = Path.Combine(Application.dataPath, "../Mods");
        Directory.CreateDirectory(modPath);

        foreach (string bundlePath in Directory.GetFiles(modPath, "*.brmf"))
        {
            try
            {
                LoadMod(bundlePath);
            }

            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load map {bundlePath}: {e.Message}");
            }
        }
    }

    private void LoadMod(string bundlePath)
    {
        Debug.Log($"Loading bundle: {bundlePath}");
        var bundle = AssetBundle.LoadFromFile(bundlePath);

        if (bundle == null)
        {
            Debug.LogError($"No ModConfig found in {bundlePath}");
            return;
        }

        var config = bundle.LoadAsset<ModConfig>("ModConfig");
        if (config == null)
        {
            Debug.LogError($"No ModConfig found in {bundlePath}");
            return;
        }

        _loadedMods.Add(config.ModID, bundle);

        switch (config.ModType)
        {
            case ModTypes.Map:
                LoadMapMod(bundle);
            break;

            default:
                Debug.LogWarning($"Unknown mod type: {config.ModType}");
            break;
        }

        Debug.Log($"Loaded {config.ModType} mod: {config.ModName} v{config.Version}");
    }

    private void LoadMapMod(AssetBundle bundle)
    {
        var mapConfig = bundle.LoadAsset<MapModConfig>("ModConfig");
        MapMovementConfig = mapConfig.MapMovementConfig;
        if (mapConfig != null && mapConfig.MapPrefab != null)
        {
            _mapTransform.GetComponentInChildren<MapScript>().DestoryMap();
            var map = Instantiate(mapConfig.MapPrefab, Vector3.zero, Quaternion.identity);
            map.transform.SetParent(_mapTransform);
            Debug.Log($"Loaded map: {mapConfig.ModName}");
        }

        else
        {
            Debug.LogError("Failed to load map config or prefab");
        }
    }

    private void OnDestroy()
    {
        foreach (var bundle in _loadedMods.Values)
        {
            bundle.Unload(true);
        }
    }
}
