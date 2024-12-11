using UnityEngine;

[CreateAssetMenu(fileName = "ModConfig", menuName = "Modding/New Map Config")]
public class MapModConfig : ModConfig
{
    public GameObject MapPrefab;

    private void OnEnable()
    {
        ModType = ModTypes.Map;
    }
}
