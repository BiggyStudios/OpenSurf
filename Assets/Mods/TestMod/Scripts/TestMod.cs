using UnityEngine;

public class TestMod : IMod
{
    public string ModID => "com.example.testmod";
    public string ModName => "Test Mod";
    public string Version => "1.0.0";

    public void Initialize()
    {
        Debug.Log("Mod Init");
    }

    public void OnEnabled()
    {
        Debug.Log("Mod Enabled");
    }

    public void OnDisabled()
    {
        Debug.Log("Mod Disabled");
    }
}
