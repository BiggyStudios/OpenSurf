using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class MainMenuScript : MonoBehaviour
{
    [System.Serializable]
    public struct MenuScreenItem
    {
        public GameObject Screen;
        public MenuScreen ScreenType;
    }

    public enum MenuScreen
    {
        InitialScreen,
        ModeSelect
    }

    [SerializeField]
    private float _transitionTime;
    [SerializeField]
    private List<MenuScreenItem> _menuScreenItems;

    [SerializeField]
    private GameObject _capsule;
    [SerializeField]
    private GameObject _capsule2;
    [SerializeField]
    private GameObject _capsuleText;
    [SerializeField]
    private Vector3 _modeSelectPos;
    [SerializeField]
    private Quaternion _modeSelectRot;

    private MenuScreen _menuScreen;

    private Vector3 _targetRot;
    private Vector3 _startPos;

    private void Start()
    {
        _menuScreen = MenuScreen.InitialScreen;

        _startPos = _capsule.transform.localPosition;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            SetScreen(MenuScreen.InitialScreen);

        if (_menuScreen != MenuScreen.InitialScreen)
            return;

        RotateCapsule();
    }

    private void RotateCapsule()
    {
        _targetRot.y += Time.deltaTime * 80f;
        _targetRot.x += Time.deltaTime * 120f;

        _capsule.transform.localRotation = Quaternion.Euler(_targetRot);
    }

    private void SetScreen(MenuScreen newScreen)
    {
        switch (newScreen)
        {
            case MenuScreen.InitialScreen:
                if (InitialScreenCoroutine != null)
                {
                    StopCoroutine(InitialScreenCoroutine);
                    InitialScreenCoroutine = null;
                    InitialScreenCoroutine = StartCoroutine(InitialScreenRoutine());
                }

                else
                {
                    InitialScreenCoroutine = StartCoroutine(InitialScreenRoutine());
                }
                break;

            case MenuScreen.ModeSelect:
                if (InitialScreenCoroutine != null)
                {
                    StopCoroutine(ModeSelectCoroutine);
                    ModeSelectCoroutine = null;
                    ModeSelectCoroutine = StartCoroutine(ModeSelectScreenRoutine());
                }

                else
                {
                    ModeSelectCoroutine = StartCoroutine(ModeSelectScreenRoutine());
                }
                break;
        }

        foreach (var screen in _menuScreenItems)
        {
            if (screen.ScreenType != newScreen)
                screen.Screen.SetActive(false);
            else
                screen.Screen.SetActive(true);
        }
    }

    public void StartButton()
    {
        SetScreen(MenuScreen.ModeSelect);
    }

    public void Solo()
    {
        Debug.Log("Solo");
    }

    public void Multi()
    {
        Debug.Log("Multi");
    }

    private Coroutine InitialScreenCoroutine;
    private IEnumerator InitialScreenRoutine()
    {
        float lerpPos = 0f;

        Vector3 startPosition = _capsule.transform.localPosition;
        Vector3 startScale = _capsule2.transform.localScale;
        Vector3 startScaleText = _capsuleText.transform.localScale;
        _targetRot = _modeSelectRot.eulerAngles;

        while (lerpPos < 1)
        {
            lerpPos += Time.deltaTime / _transitionTime;
            lerpPos = Mathf.Clamp01(lerpPos);

            _capsule.transform.localPosition = Vector3.Lerp(startPosition, _startPos, lerpPos);
            _capsule2.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, lerpPos);
            _capsuleText.transform.localScale = Vector3.Lerp(startScaleText, Vector3.zero, lerpPos);

            yield return null;
        }

        _capsule.transform.localPosition = _startPos;
        _capsule2.transform.localScale = Vector3.zero;
        _capsuleText.transform.localScale = Vector3.zero;
        _menuScreen = MenuScreen.InitialScreen;
    }

    private Coroutine ModeSelectCoroutine;
    private IEnumerator ModeSelectScreenRoutine()
    {
        float lerpPos = 0f;

        Vector3 startPosition = _capsule.transform.localPosition;
        Quaternion startRotation = _capsule.transform.localRotation;
        Vector3 startScale = _capsule2.transform.localScale;
        Vector3 startScaleText = _capsuleText.transform.localScale;

        while (lerpPos < 1)
        {
            lerpPos += Time.deltaTime / _transitionTime;
            lerpPos = Mathf.Clamp01(lerpPos);

            _capsule.transform.localPosition = Vector3.Lerp(startPosition, _modeSelectPos, lerpPos);
            _capsule.transform.localRotation = Quaternion.Lerp(startRotation, _modeSelectRot, lerpPos);
            _capsule2.transform.localScale = Vector3.Lerp(startScale, new Vector3(1f, 1.5f, 1f), lerpPos);
            _capsuleText.transform.localScale = Vector3.Lerp(startScaleText, new Vector3(0.5f, 0.5f, 0.5f), lerpPos);

            yield return null;
        }

        _capsule.transform.localPosition = _modeSelectPos;
        _capsule.transform.localRotation = _modeSelectRot;
        _capsule2.transform.localScale = new Vector3(1f, 1.5f, 1f);
        _capsuleText.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        _menuScreen = MenuScreen.ModeSelect;
    }
}
