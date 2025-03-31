using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField] private List<GameObject> _levels;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        else
        {
            Destroy(gameObject);
        }
    }

    public void SwitchLevel(int index)
    {
        //SwitchLevelServerRpc(index);
    }
    
    /*
    [ServerRpc]
    private void SwitchLevelServerRpc(int index)
    {
        SwitchLevelObserver(index);
    }

    [ObserversRpc]
    private void SwitchLevelObserver(int index)
    {
        foreach (var level in _levels)
        {
            level.SetActive(false);
        }

        _levels[index].SetActive(true);
    }
    */
}
