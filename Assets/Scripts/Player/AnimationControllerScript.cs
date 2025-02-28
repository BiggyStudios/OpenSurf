using P90brush;
using UnityEngine;

public class AnimationControllerScript : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float _animationSmoothTime;
    [SerializeField] private PlayerLogic _playerMovementScript;

    [SerializeField] private Transform _neckBone;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private float _maxLookUpAngle;
    [SerializeField] private float _maxLookDownAngle;
    [SerializeField] private float _lookSmoothTime;

    private float _currentXRotation = 0f;
    private float _xRotationVelocity = 0f;
    private Quaternion _initialRotation;

    private float _currentSpeed;
    private float _speedVelocity;
    private bool _isGrounded;
    private bool _wasInAir = false;

    private void Start()
    {
        _initialRotation = _neckBone.localRotation;
    }

    private void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        float inputMagnitude = new Vector2(horizontalInput, verticalInput).magnitude;
        inputMagnitude = Mathf.Clamp01(inputMagnitude);

        _currentSpeed = Mathf.SmoothDamp(_currentSpeed, inputMagnitude, ref _speedVelocity, _animationSmoothTime);
        _animator.SetFloat("Velocity", _currentSpeed);

        if (Input.GetKeyDown(KeyCode.Space) && _playerMovementScript.PlayerData.IsGrounded())
        {
            _animator.SetBool("IsJumping", true);
            _wasInAir = true;
        }

        if (_playerMovementScript.PlayerData.IsGrounded() && _wasInAir)
        {
            _animator.SetBool("IsJumping", false);
            _wasInAir = false;
        }
    }

    private void LateUpdate()
    {
        float cameraPitch = _cameraTransform.eulerAngles.x;

        if (cameraPitch > 180f)
            cameraPitch -= 360f;

        float targetXRotation = Mathf.Clamp(cameraPitch, -_maxLookDownAngle, _maxLookUpAngle);

        _currentXRotation = Mathf.SmoothDamp(_currentXRotation, targetXRotation, ref _xRotationVelocity, _lookSmoothTime);

        _neckBone.localRotation = _initialRotation * Quaternion.Euler(_currentXRotation, 0, 0);
    }
}
