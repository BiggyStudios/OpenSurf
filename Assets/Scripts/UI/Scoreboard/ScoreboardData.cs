using UnityEngine;

[System.Serializable]
public class ScoreData
{
    public string PlayerID { get; set; }
    public string Username { get; set; }
    public float BestTime { get; set; }
    public float CurrentTime { get; set; }

    public ScoreData(string playerID, string username)
    {
        PlayerID = playerID;
        Username = username;
        BestTime = float.MaxValue;
        CurrentTime = 0f;
    }
}
