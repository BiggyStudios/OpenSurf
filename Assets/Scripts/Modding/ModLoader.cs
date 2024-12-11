using UnityEngine;
using System.IO;

public class ModLoader : MonoBehaviour
{
    private void Start()
    {
        string modPath = Path.Combine(Application.streamingAssetsPath, "Mods/mod.brmf");

        if (File.Exists(modPath))
        {
            AssetBundle mod = AssetBundle.LoadFromFile(modPath);

            if (mod != null)
            {
                Debug.Log("Mod bundle loaded successfully");

                GameObject prefab = mod.LoadAsset<GameObject>("TestPrefab");
                if (prefab != null)
                {
                    Instantiate(prefab);
                }

                else
                {
                    Debug.LogError("Prefab not found");
                }
            }

            else
            {
                Debug.LogError("Failed to load mod bundle");
            }
        }

        else
        {
            Debug.LogError("Mod bundle not found");
        }
    }
}
