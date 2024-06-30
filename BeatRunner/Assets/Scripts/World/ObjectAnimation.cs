using System;
using System.Collections;
using System.Collections.Generic;
using BananaUtils.OnScreenDebugger.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObjectAnimation : MonoBehaviour
{
    [SerializeField] private AnimationCurve _animationCurve;
    
    [SerializeField] private float _loadDistance;
    [SerializeField] private float _positionOffset;
    [SerializeField] private float _rotationOffset;
    [SerializeField] private float _lerpSpeed;

    private bool _animated;
    
    private Vector3 _startPosition;
    private Vector3 _startScale;
    private Quaternion _startRotation;
    private Vector3 _randomPos;
    private Quaternion _randomRot;

    private void Start()
    {
        _startPosition = transform.position;
        _startScale = transform.localScale;
        _startRotation = transform.rotation;

        _randomPos = _startPosition + new Vector3(
            Random.Range(-_positionOffset, _positionOffset),
            Random.Range(-_positionOffset, _positionOffset),
            Random.Range(-_positionOffset, _positionOffset)
            );
        
        Vector3 randomRot = new Vector3(
            Random.Range(-_rotationOffset, _rotationOffset),
            Random.Range(-_rotationOffset, _rotationOffset),
            Random.Range(-_rotationOffset, _rotationOffset)
            );

        _randomRot = Quaternion.Euler(randomRot);

        transform.position = _randomPos;
        transform.localScale = Vector3.zero;
        transform.rotation = _randomRot;
    }

    private void Update()
    {
        if (PlayerManager.Instance == null)
            return;
        
        float distance = Vector3.Distance(PlayerManager.Instance.PlayerTransform.position, _startPosition);

        if (distance <= _loadDistance && !_animated)
        {
            AnimateObject(true);
            _animated = true;
        }

        else if (distance > _loadDistance && _animated)
        {
            AnimateObject(false);
            _animated = false;
        }
    }

    private void AnimateObject(bool animate)
    {
        if (_animateCoroutine != null)
        {
            StopCoroutine(_animateCoroutine);
            _animateCoroutine = StartCoroutine(AnimateRoutine(animate));
        }

        _animateCoroutine = StartCoroutine(AnimateRoutine(animate));
    }
    

    private Coroutine _animateCoroutine;
    private IEnumerator AnimateRoutine(bool animate)
    {
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        Quaternion startRot = transform.rotation;
        float lerpPos = 0f;

        while (lerpPos < 1f)
        {
            lerpPos = Mathf.Clamp01(Time.deltaTime / _lerpSpeed + lerpPos);
            float t = _animationCurve.Evaluate(lerpPos);

            transform.position =
                animate ? Vector3.Lerp(startPos, _startPosition, t) : Vector3.Lerp(startPos, _randomPos, t);
            
            transform.localScale = 
                animate ? Vector3.Lerp(startScale, _startScale, t) : Vector3.Lerp(startScale, Vector3.zero, t);

            transform.rotation =
                animate ? Quaternion.Lerp(startRot, _startRotation, t) : Quaternion.Lerp(startRot, _randomRot, t);

            yield return null;
        }

        transform.position = animate ? _startPosition : _randomPos;
        transform.localScale = animate ? _startScale : Vector3.zero;
        transform.rotation = animate ? _startRotation : _randomRot;
    }
}
