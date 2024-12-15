using UnityEngine;

[CreateAssetMenu(fileName = "ModConfig", menuName = "Modding/New Mod Config")]
public class ModConfig : ScriptableObject
{
    public string ModID;
    public string ModName;
    public string Version;
    public ModTypes ModType;
}

public enum ModTypes
{
    Map,
}
