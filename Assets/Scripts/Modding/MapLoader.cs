using System;
using System.Collections.Generic;
using System.IO;

using P90brush;

using UnityEngine;

public class MapLoader : MonoBehaviour
{  
    public MovementConfig MapMovementConfig;
    public event Action OnMapChanged;
    [SerializeField] private Transform _mapTransform;
    private Dictionary<string, AssetBundle> _loadedMaps = new Dictionary<string, AssetBundle>();
    [HideInInspector] public List<MapConfig> MapConfigs = new();

    private void Awake()
    {
        LoadMapFiles();
    }

    private void LoadMapFiles()
    {
        string mapPath = Path.Combine(Application.dataPath, "../Maps");
        Directory.CreateDirectory(mapPath);

        foreach (string bundlePath in Directory.GetFiles(mapPath, "*.brmf"))
        {
            try
            {
                LoadMapBundle(bundlePath);
            }

            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load map {bundlePath}: {e.Message}");
            }
        }
    }

    private void LoadMapBundle(string bundlePath)
    {
        Debug.Log($"Loading map: {bundlePath}");
        var bundle = AssetBundle.LoadFromFile(bundlePath);

        if (bundle == null)
        {
            Debug.LogError($"No MapConfig found in {bundlePath}");
            return;
        }

        var config = bundle.LoadAsset<MapConfig>("MapConfig");
        if (config == null)
        {
            Debug.LogError($"No MapConfig found in {bundlePath}");
            return;
        }

        _loadedMaps.Add(config.MapID, bundle);
        MapConfigs.Add(config);

        Debug.Log($"Loaded Map: {config.MapName} v{config.Version}");
    }

    public void LoadMapFromConfig(MapConfig mapConfig)
    {
        MapMovementConfig = mapConfig.MapMovementConfig;
        if (mapConfig != null && mapConfig.MapPrefab != null)
        {
            _mapTransform.GetComponentInChildren<MapScript>().DestoryMap();
            var map = Instantiate(mapConfig.MapPrefab, Vector3.zero, Quaternion.identity);
            map.transform.SetParent(_mapTransform);
            Debug.Log($"Loaded map: {mapConfig.MapName}");
            OnMapChanged?.Invoke();
        }

        else
        {
            Debug.LogError("Failed to load map config or prefab");
        }
    }

    private void OnDestroy()
    {
        foreach (var bundle in _loadedMaps.Values)
        {
            bundle.Unload(true);
        }
    }
}
