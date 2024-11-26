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
    private float _elapsedTime;

    private Vector3 _startLerpPos;
    private Vector3 _startLerpScale;
    private Quaternion _startLerpRot;

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

        if (distance <= _loadDistance)
        {
            if (!_animated)
            {
                _elapsedTime = 0f;
                
                _startLerpPos = transform.position;
                _startLerpScale = transform.localScale;
                _startLerpRot = transform.rotation;
                
                _animated = true;
            }
            
            _elapsedTime += Time.deltaTime;
            float lerpPos = _elapsedTime / _lerpSpeed;
            float t = _animationCurve.Evaluate(lerpPos);

            transform.position = Vector3.Lerp(_startLerpPos, _startPosition, t);

            transform.localScale = Vector3.Lerp(_startLerpScale, _startScale, t);

            transform.rotation = Quaternion.Lerp(_startLerpRot, _startRotation, t);
        }

        else if (distance > _loadDistance)
        {
            if (_animated)
            {
                _elapsedTime = 0f;
                _startLerpPos = transform.position;
                _startLerpScale = transform.localScale;
                _startLerpRot = transform.rotation;
                
                _animated = false;
            }
            
            _elapsedTime += Time.deltaTime;
            float lerpPos = _elapsedTime / _lerpSpeed;
            float t = _animationCurve.Evaluate(lerpPos);
            
            transform.position = Vector3.Lerp(_startLerpPos, _randomPos, t);
            
            transform.localScale = Vector3.Lerp(_startLerpScale, Vector3.zero, t);

            transform.rotation = Quaternion.Lerp(_startLerpRot, _randomRot, t);
        }
    }
}
