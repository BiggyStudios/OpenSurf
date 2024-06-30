using System;
using System.Collections;
using FishNet.Object;
using P90brush;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; }
    [HideInInspector] public Transform PlayerTransform;
    
    [Header("Values")] [SerializeField] private float _restartLerpSpeed;
    [SerializeField] private AnimationCurve _respawnAnimationCurve;

    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private Slider _fovSlider;
    
    private CapsuleCollider _capsuleCollider;
    private Vector3 _spawnPosition;
    private bool _pauseMenuOpen;
    private Camera _playerCam;

    private PlayerLogic _playerLogic;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.IsOwner)
        {
            Instance = this;
            PlayerTransform = transform;
        }

        if (!base.IsOwner)
            Destroy(this);
    }

    private void Start()
    {
        _playerLogic = GetComponent<PlayerLogic>();
        _spawnPosition = transform.position;
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _playerCam = GetComponentInChildren<Camera>();

        _fovSlider.value = _playerCam.fieldOfView;
    }

    private void Update()
    {
        if (!base.IsOwner)
            return;

        if (transform.position.y < -300)
        {
            Restart();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && !_pauseMenuOpen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            _pauseMenuOpen = true;
            _pauseMenu.SetActive(true);
        }

         else if (Input.GetKeyDown(KeyCode.Escape) && _pauseMenuOpen)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            _pauseMenuOpen = false;
            _pauseMenu.SetActive(false);
        }


        if (_pauseMenuOpen)
            _playerCam.fieldOfView = _fovSlider.value;
    }


    public void Restart()
    {
        if (_restartPlayerCoroutine != null)
        {
            StopCoroutine(_restartPlayerCoroutine);
            _restartPlayerCoroutine = StartCoroutine(RestartPlayerRoutine());
        }

        _restartPlayerCoroutine = StartCoroutine(RestartPlayerRoutine());
    }

    private Coroutine _restartPlayerCoroutine;

    private IEnumerator RestartPlayerRoutine()
    {

        Vector3 startPos = transform.position;
        float lerpPos = 0f;
        _capsuleCollider.enabled = false;

        while (lerpPos < 1f)
        {
            lerpPos = Mathf.Clamp01(Time.deltaTime / _restartLerpSpeed + lerpPos);
            float t = _respawnAnimationCurve.Evaluate(lerpPos);

            _playerLogic.PlayerData.Origin = Vector3.Lerp(startPos, _spawnPosition, t);

            yield return null;
        }

        _playerLogic.PlayerData.Origin = _spawnPosition;
        _playerLogic.PlayerData.Velocity = Vector3.zero;
        _capsuleCollider.enabled = true;
    }
}
