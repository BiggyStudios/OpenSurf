using Unity.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get => _instance;

        private set
        {
            if (_instance != null && _instance != value)
            {
                Destroy(value.gameObject);
                return;
            }

            _instance = value;
        }
    }

    private MenuManager _menuManager;
    public static MenuManager MenuManager => Instance._menuManager;
    

    private ModLoader _modLoader;
    public static ModLoader ModLoader => Instance._modLoader;

    private void Awake()
    {
        _menuManager = GetComponent<MenuManager>();
        _modLoader = GetComponent<ModLoader>();
    }
}
