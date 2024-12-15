using NUnit.Framework;

using P90brush;

using UnityEngine;

[CreateAssetMenu(fileName = "ModConfig", menuName = "Modding/New Map Config", order = -500)]
public class MapModConfig : ModConfig
{
    public GameObject MapPrefab;
    public MovementConfig MapMovementConfig;

    private void OnEnable()
    {
        ModType = ModTypes.Map;
    }
}
