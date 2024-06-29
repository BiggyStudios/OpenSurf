using System;
using System.Collections;
using FishNet.Object;
using P90brush;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [Header("Values")] [SerializeField] private float _restartLerpSpeed;
    [SerializeField] private AnimationCurve _respawnAnimationCurve;

    private CapsuleCollider _capsuleCollider;
    private Vector3 _spawnPosition;

    private PlayerLogic _playerLogic;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner)
            Destroy(this);
    }

    private void Start()
    {
        _playerLogic = GetComponent<PlayerLogic>();
        _spawnPosition = transform.position;
        _capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        if (!base.IsOwner)
            return;

        if (transform.position.y < -50)
        {
            Restart();
        }
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
