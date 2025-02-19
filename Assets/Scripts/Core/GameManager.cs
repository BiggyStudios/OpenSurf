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
    
    private MapLoader _mapLoader;
    public static MapLoader MapLoader => Instance._mapLoader;

    private void Awake()
    {
        if (_instance == null) Instance = this;

        _menuManager = GetComponent<MenuManager>();
        _mapLoader = GetComponent<MapLoader>();
    }
}
