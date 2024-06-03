using FishNet.Object;
using UnityEngine;

public class WallRunning : NetworkBehaviour
{
    [SerializeField] private GameObject PlayerCamObject;
    [SerializeField] private Camera PlayerCam;
    [SerializeField] private LayerMask WhatIsWall;
    [SerializeField] private LayerMask WhatIsGround;
    [SerializeField] private float WallRunForce;
    [SerializeField] private float WallJumpUpForce;
    [SerializeField] private float WallJumpSideForce;
    [SerializeField] private float MaxWallRunTime;
    [SerializeField] private float WallClimbSpeed;
    private float WallRunTimer;

    [SerializeField] private KeyCode WallJumpKey = KeyCode.Space;

    private float HorizontalInput;
    private float VerticalInput;

    [SerializeField] private float WallCheckDistance;
    [SerializeField] private float MinJumpHeight;
    private RaycastHit LeftWallHit;
    private RaycastHit RightWallHit;
    private bool LeftWall;
    private bool RightWall;

    [SerializeField] private float ExitWallTime;
    private float ExitWallTimer;
    private bool ExitingWall;

    public bool UseGravity;
    [SerializeField] float GravityCounterForce;

    [SerializeField] private Transform Orientation;
    private PlayerMovement PlayerMovementScript;
    private Rigidbody Rb;

    void Start()
    {
        Rb = GetComponent<Rigidbody>();
        PlayerMovementScript = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (!base.IsOwner)
            return;
        
        if (PlayerMovementScript.WallRunningEnabled)
        {
            CheckForWall();
            StateManager();

            if (PlayerMovementScript.WallRunCameraEffects)
            {
                if (RightWall)
                {
                    TiltCamera(25f);
                    FovChange(80f);
                }

                if (LeftWall)
                {
                    TiltCamera(-25f);
                    FovChange(80f);
                }

                if (!RightWall)
                {
                    TiltCamera(0f);
                    FovChange(60f);
                }

                if (!LeftWall)
                {
                    TiltCamera(0f);
                    FovChange(60f);
                }

            }
        }
    }

    private void FixedUpdate()
    {
        if (PlayerMovementScript.WallRunningEnabled)
        {
            if (PlayerMovementScript.WallRunning)
                WallRunMovement();
        }
    }

    private void CheckForWall()
    {
        RightWall = Physics.Raycast(transform.position, Orientation.right, out RightWallHit, WallCheckDistance, WhatIsWall);
        LeftWall = Physics.Raycast(transform.position, -Orientation.right, out LeftWallHit, WallCheckDistance, WhatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, MinJumpHeight, WhatIsGround);
    }

    private void StateManager()
    {
        HorizontalInput = Input.GetAxisRaw("Horizontal");
        VerticalInput = Input.GetAxisRaw("Vertical");

        if ((LeftWall || RightWall) && VerticalInput > 0 && AboveGround() && !ExitingWall)
        {
            if (!PlayerMovementScript.WallRunning)
                StartWallRun();

            if (WallRunTimer > 0)
                WallRunTimer -= Time.deltaTime;

            if (WallRunTimer <= 0 && PlayerMovementScript.WallRunning)
            {
                ExitingWall = true;
                ExitWallTimer = ExitWallTime;
            }

            if (Input.GetKeyDown(WallJumpKey))
                WallJump();
        }

        else if (ExitingWall)
        {
            if (PlayerMovementScript.WallRunning)
                StopWallRun();

            if (ExitWallTimer > 0)
                ExitWallTimer -= Time.deltaTime;

            if (ExitWallTimer <= 0)
                ExitingWall = false;
        }

        else
        {
            if (PlayerMovementScript.WallRunning)
                StopWallRun();
        }
    }

    private void StartWallRun()
    {
        PlayerMovementScript.WallRunning = true;

        WallRunTimer = MaxWallRunTime;

        Rb.velocity = new Vector3(Rb.velocity.x, 0f, Rb.velocity.z);
    }

    private void WallRunMovement()
    {
        Rb.useGravity = UseGravity;

        Vector3 WallNormal = RightWall ? RightWallHit.normal : LeftWallHit.normal;

        Vector3 WallForward = Vector3.Cross(WallNormal, transform.up);

        if ((Orientation.forward - WallForward).magnitude > (Orientation.forward - -WallForward).magnitude)
            WallForward = -WallForward;

        Rb.AddForce(WallForward * WallRunForce, ForceMode.Force);

        if (!(LeftWall && HorizontalInput > 0) && !(RightWall && HorizontalInput > 0))
            Rb.AddForce(-WallNormal * 100, ForceMode.Force);

        if (UseGravity)
            Rb.AddForce(transform.up * GravityCounterForce, ForceMode.Force);
    }

    private void StopWallRun()
    {
        PlayerMovementScript.WallRunning = false;
        Rb.useGravity = true;
    }

    private void WallJump()
    {
        ExitingWall = true;
        ExitWallTimer = ExitWallTime;

        Vector3 WallNomral = RightWall ? RightWallHit.normal : LeftWallHit.normal;

        Vector3 ForceToAdd = transform.up * WallJumpUpForce + WallNomral * WallJumpSideForce;

        Rb.velocity = new Vector3(Rb.velocity.x, 0f, Rb.velocity.z);
        Rb.AddForce(ForceToAdd, ForceMode.Impulse);
    }

    private void TiltCamera(float TiltAmmount)
    {
        PlayerMovementScript.CameraTilt = Mathf.Lerp(PlayerMovementScript.CameraTilt, PlayerMovementScript.CameraTilt = TiltAmmount, Time.deltaTime * 2.5f);
    }

    private void FovChange(float NewFov)
    {
        PlayerCam.fieldOfView = Mathf.Lerp(PlayerCam.fieldOfView, PlayerCam.fieldOfView = NewFov, Time.deltaTime * 1f);
    }
}
