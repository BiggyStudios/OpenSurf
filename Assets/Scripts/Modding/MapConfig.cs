using P90brush;

using UnityEngine;

[CreateAssetMenu(fileName = "MapConfig", menuName = "Mapping/New Map Config")]
public class MapConfig : ScriptableObject
{
    public string MapID;
    public string MapName;
    public string Version;

    public GameObject MapPrefab;
    public MovementConfig MapMovementConfig;
}
