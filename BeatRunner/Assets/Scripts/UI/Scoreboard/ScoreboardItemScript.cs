using TMPro;
using UnityEngine;

public class ScoreboardItemScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _timeText;

    public void Init(string name)
    {
        _nameText.text = name;
        _timeText.text = "0";
    }

    public void SetUsername(string name)
    {
        _nameText.text = name;
    }
    
    public void SetTime(float time)
    {
        _timeText.text = time.ToString();
    }
}
