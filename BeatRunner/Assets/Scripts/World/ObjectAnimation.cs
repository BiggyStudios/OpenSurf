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
    [SerializeField] private float _positionOfffset;
    [SerializeField] private float _lerpSpeed;

    private bool _animated;
    
    private Vector3 _startPosition;
    private Vector3 _startScale;
    private Vector3 _randomPos;

    private void Start()
    {
        _startPosition = transform.position;
        _startScale = transform.localScale;

        _randomPos = _startPosition + new Vector3(
            Random.Range(-_positionOfffset, _positionOfffset),
            Random.Range(-_positionOfffset, _positionOfffset),
            Random.Range(-_positionOfffset, _positionOfffset)
            );

        transform.position = _randomPos;
        transform.localScale = Vector3.zero;
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
        float lerpPos = 0f;

        while (lerpPos < 1f)
        {
            lerpPos = Mathf.Clamp01(Time.deltaTime / _lerpSpeed + lerpPos);
            float t = _animationCurve.Evaluate(lerpPos);

            transform.position =
                animate ? Vector3.Lerp(startPos, _startPosition, t) : Vector3.Lerp(startPos, _randomPos, t);
            
            transform.localScale = 
                animate ? Vector3.Lerp(startScale, _startScale, t) : Vector3.Lerp(startScale, Vector3.zero, t);

            yield return null;
        }

        transform.position = animate ? _startPosition : _randomPos;
        transform.localScale = animate ? _startScale : Vector3.zero;
    }
}
