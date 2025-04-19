using UnityEngine;

public class WeaponRecoilScript : MonoBehaviour
{
    public WeaponScriptObj WeaponScriptObj;
    public Transform WeaponMesh;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    private Vector3 _currentRecoilPosition;
    private Quaternion _currentRecoilRotation;

    private Vector3 _positionVelocity;

    private void Start()
    {
        _originalPosition = WeaponMesh.transform.localPosition;
        _originalRotation = WeaponMesh.transform.localRotation;

        _currentRecoilPosition = Vector3.zero;
        _currentRecoilRotation = Quaternion.identity;
    }

    private void Update()
    {
        HandleRecoil();
    }

    private void HandleRecoil()
    {
        _currentRecoilPosition = Vector3.SmoothDamp(_currentRecoilPosition, Vector3.zero, ref _positionVelocity, 1f / WeaponScriptObj.PositionReturnSpeed);
        _currentRecoilRotation = Quaternion.Slerp(_currentRecoilRotation, Quaternion.identity, Time.deltaTime * WeaponScriptObj.RotationReturnSpeed);

        WeaponMesh.transform.localPosition = _originalPosition + _currentRecoilPosition;
        WeaponMesh.transform.localRotation = _originalRotation * _currentRecoilRotation;
    }
    public void TriggerRecoil()
    {
        _currentRecoilPosition += WeaponScriptObj.PositionRecoilAmount;
        _currentRecoilRotation *= Quaternion.Euler(WeaponScriptObj.RotationRecoilAmount);
    }
}
