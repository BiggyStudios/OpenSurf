using System;
using System.Collections;
using System.Collections.Generic;
using BananaUtils.OnScreenDebugger.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObjectAnimation : MonoBehaviour
{
    [SerializeField] private float _loadDistance;
    [SerializeField] private float _positionOfffset;
    [SerializeField] private float _lerpSpeed;
    
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
        float distance = Vector3.Distance(PlayerManager.Instance.PlayerTransform.position, _startPosition);

        if (distance <= _loadDistance)
        {
            transform.position = Vector3.Lerp(transform.position, _startPosition, Time.deltaTime * _lerpSpeed);
            transform.localScale = Vector3.Lerp(transform.localScale, _startScale, Time.deltaTime * _lerpSpeed);
        }
        
        else
        {
            transform.position = Vector3.Lerp(transform.position, _randomPos, Time.deltaTime * _lerpSpeed);
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * _lerpSpeed);
        }
    }
}
