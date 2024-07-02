using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using BNNUtils;
using TMPro;
using UnityEngine;

public class Scoreboard : NetworkBehaviour
{
    public static Scoreboard Instance;

    [SerializeField] private ScoreboardItemScript _scorePrefab;
    [SerializeField] private Transform _scoreboardPanelTransform;

    [HideInInspector] public List<string> ExistingUsers;
    private Dictionary<string, ScoreboardItemScript> _playerScores = new Dictionary<string, ScoreboardItemScript>();
    
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

    public static void OnPlayerJoined(string playerID)
    {
        ScoreboardItemScript _playerScore = Instantiate(Instance._scorePrefab, Instance._scoreboardPanelTransform);
        Instance._playerScores.Add(playerID, _playerScore);
        _playerScore.Init(playerID);
        
        Instance.ExistingUsers.Add(playerID);
    }

    public static void OnPlayerLeft(string playerID)
    {
        if (Instance._playerScores.TryGetValue(playerID, out ScoreboardItemScript playerScore))
        {
            Destroy(playerScore.gameObject);
            Instance._playerScores.Remove(playerID);
        }
    }

    public static void SetTime(string playerID, float time)
    {
        Instance.SetTimeServerRpc(playerID, time);
    }

    [ServerRpc (RequireOwnership = false)]
    private void SetTimeServerRpc(string playerID, float time)
    { 
        SetTimeObservers(playerID, time);
    }

    [ObserversRpc]
    private void SetTimeObservers(string playerID, float time)
    {
        Instance._playerScores[playerID].SetTime(time);
    }

    public static void SetUsername(string playerID, string username)
    {
        Instance.SetUsernameServerRpc(playerID, username);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetUsernameServerRpc(string playerID, string username)
    {
        Instance.SetUsernameObservers(playerID, username);
    }

    [ObserversRpc]
    private void SetUsernameObservers(string playerID, string username)
    {
        Instance._playerScores[playerID].SetUsername(username);
    }

}
