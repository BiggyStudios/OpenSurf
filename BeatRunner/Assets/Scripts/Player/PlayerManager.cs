using System;
using System.Collections;
using FishNet.Object;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [Header("Referances")]
    [SerializeField] private Vector3 _spawnPosition;

    [Header("Values")] 
    [SerializeField] private float _restartLerpSpeed;
    [SerializeField] private AnimationCurve _respawnAnimationCurve;

    private bool _playerRespawning;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        
        if (!base.IsOwner)
            Destroy(this);
    }

    private void Update()
    {
        if (transform.position.y < -50)
        {
            if (_restartPlayerCoroutine != null)
            {
                StopCoroutine(_restartPlayerCoroutine);
                _restartPlayerCoroutine = StartCoroutine(RestartPlayerRoutine());
            }

            _restartPlayerCoroutine = StartCoroutine(RestartPlayerRoutine());
        }
    }

    private Coroutine _restartPlayerCoroutine;
    private IEnumerator RestartPlayerRoutine()
    {
        Vector3 startPos = transform.position;

        _playerRespawning = true;
        float lerpPos = 0f;

        while (lerpPos < 1f)
        {
            lerpPos = Mathf.Clamp01(Time.deltaTime / 1.5f + lerpPos);
            float t = _respawnAnimationCurve.Evaluate(lerpPos);

            transform.position = Vector3.Lerp(startPos, _spawnPosition, t);

            yield return null;
        }

        _playerRespawning = false;
    }
}
