using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapEntryScript : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Button _button;

    private MapConfig _mapConfig;

    private void Awake()
    {
        _button.onClick.AddListener(PlayMap);
    }

    public void SetValues(string text, MapConfig mapMod)
    {
        _text.text = text;
        _mapConfig = mapMod;
    }

    public void PlayMap()
    {
        GameManager.MapLoader.LoadMapFromConfig(_mapConfig);
    }
}
