using P90brush;
using UnityEngine;

[CreateAssetMenu(fileName = "ModConfig", menuName = "Modding/New Map Config")]
public class MapModConfig : ModConfig
{
    public GameObject MapPrefab;
    public MovementConfig MapMovementConfig;

    private void OnEnable()
    {
        ModType = ModTypes.Map;
    }
}
