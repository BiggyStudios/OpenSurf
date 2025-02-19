using UnityEngine;
using FishNet.Managing;
using FishNet.Transporting;
using BananaUtils.OnScreenDebugger.Scripts;
using FishNet.Transporting.Tugboat;

public class MenuManager : MonoBehaviour
{
    [HideInInspector] public Mode GameMode;

    public enum Mode
    {
        Solo,
        Multi
    }

    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private GameObject _mainMenu;

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _menuCamera;

    public string TestValue = "CumBall";

    public void StartSolo()
    {
        var transport = _networkManager.GetComponent<Tugboat>();
        transport.SetServerBindAddress("127.0.0.1", IPAddressType.IPv4);
        transport.SetClientAddress("127.0.0.1");
        transport.StartConnection(true);
        transport.StartConnection(false);
        GameMode = Mode.Solo;
        InitializeSingleplayer();
        _menuCamera.SetActive(false);
        _mainMenu.SetActive(false);
    }

    public void StartMulti()
    {
        OSDebug.LogWarning("Work In Progress!!!");
    }

    private void InitializeSingleplayer()
    {
        GameObject player = Instantiate(_playerPrefab);
    }
}
