using UnityEngine;

public class MapSelectMenuScript : MonoBehaviour
{
    [SerializeField] private MapEntryScript _mapEntry;
    [SerializeField] private Transform _spawnTransform;

    private void Start()
    {        
        foreach (MapConfig mapConfig in GameManager.MapLoader.MapConfigs)
        {
            var mapEntry = Instantiate(_mapEntry, _spawnTransform);
            mapEntry.SetValues(mapConfig.MapName, mapConfig);
        }
    }
}
