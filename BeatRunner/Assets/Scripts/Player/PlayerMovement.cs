using System;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private Rigidbody _rb;
    
    [SerializeField] private Camera _cam;

    [SerializeField] private GameObject _playerMesh;

    public bool WallRunningEnabled;
    public bool WallRunCameraEffects;

    public Transform playerCam;
    public Transform orientation;
    public float WallRunSpeed;
    public bool WallRunning;

    public float CameraTilt;

    public float moveSpeed = 4500;
    public float maxSpeed = 20;
    public bool grounded;
    public LayerMask whatIsGround;

    public float counterMovement = 0.175f;
    public float maxSlopeAngle = 35f;
    
    public float slideForce = 400;
    public float slideCounterMovement = 0.2f;
    
    public float jumpForce = 550f;
    
    private bool _readyToJump = true;
    
    private bool _playerRespawnining;
    private float _threshold = 0.01f;
    private Vector3 _crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 _playerScale;
    
    private float _jumpCooldown = 0.25f;
    
    private float x, y;
    private bool jumping, sprinting, crouching;

    private float _xRotation;
    private float _sensitivity = 50f;
    private float _sensMultiplier = 1f;
    
    private Vector3 _normalVector = Vector3.up;
    private Vector3 _wallNormalVector;

    private float StartFov;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner)
        {
            Destroy(_rb);
        }
    }

    void Start()
    {
        StartFov = _cam.fieldOfView;

        _playerScale = _playerMesh.transform.localScale;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void FixedUpdate()
    {
        if (!base.IsOwner)
            return;
        
        Movement();
    }

    private void Update()
    {
        if (!base.IsOwner)
            return;
        
        MyInput();
        Look();
    }

    private void MyInput()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        jumping = Input.GetButton("Jump");
        crouching = Input.GetKey(KeyCode.LeftControl);

        if (Input.GetKeyDown(KeyCode.LeftControl))
            StartCrouch();

        if (Input.GetKeyUp(KeyCode.LeftControl))
            StopCrouch();

        if (Input.GetKeyDown(KeyCode.O) && !WallRunCameraEffects)
        {
            WallRunCameraEffects = true;
        }

        else if (Input.GetKeyDown(KeyCode.O) && WallRunCameraEffects)
        {
            WallRunCameraEffects = false;
        }

    }

    private void StartCrouch()
    {
        _playerMesh.transform.localScale = _crouchScale;
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        if (_rb.velocity.magnitude > 0.5f)
        {
            if (grounded)
            {
                _rb.AddForce(orientation.transform.forward * slideForce);
            }
        }

        _playerMesh.GetComponent<MeshRenderer>().enabled = false;
    }

    private void StopCrouch()
    {
        _playerMesh.transform.localScale = _playerScale;
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        _playerMesh.GetComponent<MeshRenderer>().enabled = true;
    }

    private void Movement()
    {
        _rb.AddForce(Vector3.down * Time.deltaTime * 10);

        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        CounterMovement(x, y, mag);

        if (_readyToJump && jumping) Jump();

        float maxSpeed = this.maxSpeed;

        if (crouching && grounded && _readyToJump)
        {
            _rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }

        if (x > 0 && xMag > maxSpeed) x = 0;
        if (x < 0 && xMag < -maxSpeed) x = 0;
        if (y > 0 && yMag > maxSpeed) y = 0;
        if (y < 0 && yMag < -maxSpeed) y = 0;

        float multiplier = 1f, multiplierV = 1f;

        if (!grounded)
        {
            multiplier = 0.8f;
            multiplierV = 0.8f;
        }

        
        if (grounded && crouching) multiplierV = 0f;
        
        _rb.AddForce(orientation.transform.forward * y * moveSpeed * Time.deltaTime * multiplier * multiplierV);
        _rb.AddForce(orientation.transform.right * x * moveSpeed * Time.deltaTime * multiplier);

        if (WallRunning && WallRunningEnabled)
        {
            moveSpeed = WallRunSpeed;
        }

        else
        {
            moveSpeed = 4500;
        }
    }

    private void Jump()
    {
        if (grounded && _readyToJump)
        {
            _readyToJump = false;

            _rb.AddForce(Vector2.up * jumpForce * 1.5f);
            _rb.AddForce(_normalVector * jumpForce * 0.5f);

            Vector3 vel = _rb.velocity;
            if (_rb.velocity.y < 0.5f)
                _rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (_rb.velocity.y > 0)
                _rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

            Invoke(nameof(ResetJump), _jumpCooldown);
        }
    }

    public void ResetJump()
    {
        _readyToJump = true;
    }

    private float desiredX;
    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * _sensitivity * Time.fixedDeltaTime * _sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * _sensitivity * Time.fixedDeltaTime * _sensMultiplier;

        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        playerCam.transform.localRotation = Quaternion.Euler(_xRotation, desiredX, CameraTilt);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded || jumping) return;

        if (crouching)
        {
            _rb.AddForce(moveSpeed * Time.deltaTime * -_rb.velocity.normalized * slideCounterMovement);
            return;
        }

        if (Math.Abs(mag.x) > _threshold && Math.Abs(x) < 0.05f || (mag.x < -_threshold && x > 0) || (mag.x > _threshold && x < 0))
        {
            _rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > _threshold && Math.Abs(y) < 0.05f || (mag.y < -_threshold && y > 0) || (mag.y > _threshold && y < 0))
        {
            _rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }

        if (Mathf.Sqrt((Mathf.Pow(_rb.velocity.x, 2) + Mathf.Pow(_rb.velocity.z, 2))) > maxSpeed)
        {
            float fallspeed = _rb.velocity.y;
            Vector3 n = _rb.velocity.normalized * maxSpeed;
            _rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }
    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(_rb.velocity.x, _rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = _rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;
    
    private void OnCollisionStay(Collision other)
    {
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            if (IsFloor(normal))
            {
                grounded = true;
                cancellingGrounded = false;
                _normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded()
    {
        grounded = false;
    }

    private void DynamicFov()
    {
        if (_rb.velocity.x > 4 || _rb.velocity.x < -4)
        {
            _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, StartFov + 8, 2f * Time.deltaTime);
        }

        else if (_rb.velocity.x > 8 || _rb.velocity.x < -8)
        {
            _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, StartFov + 16, 2f * Time.deltaTime);
        }

        else if (_rb.velocity.x > 12 || _rb.velocity.x < -12)
        {
            _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, StartFov + 24, 2f * Time.deltaTime);
        }

        else if (_rb.velocity.x > 20 || _rb.velocity.x < -20)
        {
            _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, StartFov + 16, 2f * Time.deltaTime);
        }

        else if (_rb.velocity.x < 4 || _rb.velocity.x > -4)
        {
            _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, StartFov, 2f * Time.deltaTime);
        }
    }
}

