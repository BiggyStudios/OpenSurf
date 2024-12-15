using System;

using UnityEngine;
using UnityEngine.Serialization;

public class DynamicCamera : MonoBehaviour
{
    [SerializeField] private AnimationCurve _movementCurve;
    [SerializeField] private float _curveTime;
    [SerializeField] private float _speed;

    [HideInInspector] public Transform Target;

    private void Update() => MoveCamera();

    private void MoveCamera()
    {
        float curvePos = (Time.time % _curveTime) / _curveTime;
        float movementOffset = _movementCurve.Evaluate(curvePos);

        Vector3 desiredPos = Target.position + new Vector3(0, movementOffset, 0);
        Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, _speed * Time.deltaTime);

        transform.position = smoothedPos;
    }
}
