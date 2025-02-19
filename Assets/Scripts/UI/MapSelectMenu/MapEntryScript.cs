using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapEntryScript : MonoBehaviour
{
    private TMP_Text _text;
    private Button _button;
    private MapConfig _mapConfig;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
        _button = GetComponent<Button>();

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
