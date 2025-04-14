using UnityEngine;
using Mirror;

public class MenuManager : MonoBehaviour
{
    [HideInInspector] public Mode GameMode;
    [SerializeField] private GameObject _mapSelectScreen;

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

    }

    public void StartMulti()
    {
        var transport = _networkManager.GetComponent<TelepathyTransport>();

        //transport.StartConnection(true);
        //transport.StartConnection(false);
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
