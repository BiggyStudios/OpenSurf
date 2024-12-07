using TMPro;
using UnityEngine;

public class ScoreboardItemScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _usernameText;
    [SerializeField] private TextMeshProUGUI _currentTimeText;
    [SerializeField] private TextMeshProUGUI _bestTimeText;

    public void Init(string username)
    {
        _usernameText.text = username;
        _currentTimeText.text = "---";
        _bestTimeText.text = "---";
    }

    public void UpdateTimes(float currentTime, float bestTime)
    {
        _currentTimeText.text = FormatTime(currentTime);
        _bestTimeText.text = FormatTime(bestTime);
    }

    private string FormatTime(float time)
    {
        if (time == float.MaxValue)
            return "---";
        
        int mins = (int)(time / 60f);
        int secs = (int)(time % 60f);
        int milisecs = (int)((time * 100f) % 100f);

        return $"{mins:00}:{secs:00}.{milisecs:00}";
    }
}
