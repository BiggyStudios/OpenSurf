using TMPro;
using UnityEngine;

public class MapEntryScript : MonoBehaviour
{
    private TMP_Text _text;
    private MapModConfig _mapConfig;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    public void SetValues(string text, MapModConfig mapMod)
    {
        _text.text = text;
        _mapConfig = mapMod;
    }
}
