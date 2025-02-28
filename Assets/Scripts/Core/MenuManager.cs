using UnityEngine;
using FishNet.Managing;
using FishNet.Transporting;
using BananaUtils.OnScreenDebugger.Scripts;
using FishNet.Transporting.Tugboat;
using FishNet.Component.Spawning;

public class MenuManager : MonoBehaviour
{
    [HideInInspector] public Mode GameMode;
    [SerializeField] private GameObject _mapSelectScreen;
    private PlayerSpawner _playerSpawner;

    public enum Mode
    {
        Solo,
        Multi
    }

    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private GameObject _mainMenu;

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _menuCamera;

    public void StartSolo()
    {
        var transport = _networkManager.GetComponent<Tugboat>();
        transport.SetServerBindAddress("127.0.0.1", IPAddressType.IPv4);
        transport.SetClientAddress("127.0.0.1");
        transport.StartConnection(true);
        transport.StartConnection(false);
        GameMode = Mode.Solo;
        _menuCamera.SetActive(false);
        _mainMenu.SetActive(false);
    }

    public void StartMulti()
    {
        var transport = _networkManager.GetComponent<Tugboat>();

        transport.StartConnection(true);
        transport.StartConnection(false);
        GameMode = Mode.Multi;
        _menuCamera.SetActive(false);
        _mainMenu.SetActive(false);
    }

    public void InitializePlayer()
    {
        GameObject player = Instantiate(_playerPrefab);
    }

    public void SetMapSelect(bool state)
    {
        _mapSelectScreen.SetActive(state);
    }
}
