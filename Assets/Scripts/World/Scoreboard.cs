using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    public static Scoreboard Instance { get; private set ;}

    [SerializeField] private ScoreboardItemScript _scorePrefab;
    [SerializeField] private Transform _scoreboardContent;
    [SerializeField] private bool _sortByBestTime = true;

    private Dictionary<string, ScoreData> _playerScores = new Dictionary<string, ScoreData>();
    private Dictionary<string, ScoreboardItemScript> _scoreboardItems = new Dictionary<string, ScoreboardItemScript>();

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

    public void AddPlayer(string playerID, string username)
    {
        if (_playerScores.ContainsKey(playerID))
            return;

        var scoreData = new ScoreData(playerID, username);
        _playerScores.Add(playerID, scoreData);

        var scoreboardItem = Instantiate(_scorePrefab, _scoreboardContent);
        _scoreboardItems.Add(playerID, scoreboardItem);
        scoreboardItem.Init(username);
    }

    public void RemovePlayer(string playerID)
    {
        if (!_playerScores.ContainsKey(playerID))
            return;

        _playerScores.Remove(playerID);

        if (_scoreboardItems.TryGetValue(playerID, out var item))
        {
            Destroy(item.gameObject);
            _scoreboardItems.Remove(playerID);
        }
    }

    /*

    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerTime(string playerID, float time)
    {
        if (!_playerScores.ContainsKey(playerID))
            return;

        var scoreData = _playerScores[playerID];
        scoreData.CurrentTime = time;

        if (time < scoreData.BestTime)
            scoreData.BestTime = time;

        UpdateScoreboardClientRpc(playerID, scoreData.CurrentTime, scoreData.BestTime);
    }

    [ObserversRpc]
    private void UpdateScoreboardClientRpc(string playerID, float currentTime, float bestTime)
    {
        if (_scoreboardItems.TryGetValue(playerID, out var item))
        {
            item.UpdateTimes(currentTime, bestTime);
        }

        if (_sortByBestTime)
            SortScoreboard();
    }

    private void SortScoreboard()
    {
        var sortedPlayers = _playerScores.Values.OrderBy(x => x.BestTime).ToList();

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            if (_scoreboardItems.TryGetValue(sortedPlayers[i].PlayerID, out var item))
            {
                item.transform.SetSiblingIndex(i);
            }
        }
    }

    */
}
