using UnityEngine;
using System.IO;

public class ModLoader : MonoBehaviour
{
    private void Start()
    {   
        string modFolderPath = Path.Combine(Application.dataPath, "../Mods/");
        string modPath = Path.Combine(Application.dataPath, "../Mods/mod.brmf");

        if (!Directory.Exists(modFolderPath))
            Directory.CreateDirectory(modFolderPath);
        else
        {
            if (File.Exists(modPath))
            {
                AssetBundle mod = AssetBundle.LoadFromFile(modPath);

                if (mod != null)
                {
                    Debug.Log("Mod bundle loaded successfully");

                    GameObject prefab = mod.LoadAsset<GameObject>("TestPrefab");
                    if (prefab != null)
                    {
                        var obj = Instantiate(prefab);
                        obj.transform.position = new Vector3(0f, 100f, 0f);
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
}
