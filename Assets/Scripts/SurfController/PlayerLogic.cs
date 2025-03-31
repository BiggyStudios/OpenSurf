using FishNet.Object;
using TMPro;
using UnityEngine;

namespace P90brush
{
    public class PlayerLogic : NetworkBehaviour, ISurfControllable
    {
        public float _walkSpeed = 80f;
        public float _jumpForce = 40f;

        [Header("TEMP")]
        [SerializeField]
        private MeshRenderer _glassesMeshRenderer;
        [SerializeField]
        private GameObject _ui;
        [SerializeField]
        private TMP_Text _speed;

        [Header("Physics Settings")]
        public int _tickRate = 128;
        public Camera _fpsCamera;

        [Header("Movement Config")]
        [SerializeField]
        public MovementConfig moveConfig = new MovementConfig();

        [Header("Hookshot")]
        [SerializeField]
        public Hookshot _hookshot;

        #region == == == == == == == == == == == == == == == = Private Properties == == == == == == == == == == == == == == == = #

        private PlayerManager _playerManager;

        // Var set in Start() callback
        private Rigidbody _rb;
        private Collider _collider;
        // private Quaternion originalRotation;
        private Vector3 _startPosition;
        private Quaternion _originalRotation;

        // Surf Oject
        private SurfController _controller = new SurfController();

        #endregion == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == = #

        #region == == == == == == == == == == == == == == == = ISurfControllable Impl == == == == == == == == == == == == == == == = #

        public MovementConfig MoveConfig
        {
            get
            {
                return moveConfig;
            }
        }

        public PlayerData PlayerData { get; } = new PlayerData();

        public InputData InputData { get; } = new InputData();

        public Collider Collider
        {
            get
            {
                return _collider;
            }
        }

        public Vector3 BaseVelocity { get; }

        public Camera FpsCamera
        {
            get
            {
                return _fpsCamera;
            }
        }

        #endregion == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == == = #

        private void Awake()
        {
            // Setup TickRate
            Time.fixedDeltaTime = 1f / _tickRate;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!base.IsOwner)
            {
                _fpsCamera.enabled = false;
                _glassesMeshRenderer.enabled = true;
                Destroy(_rb);
                Destroy(_ui);
                this.enabled = false;
            }

            if (base.IsOwner)
            {
                _glassesMeshRenderer.enabled = false;
            }
        }

        void Start()
        {
            // Setup RB
            _playerManager = GetComponent<PlayerManager>();
            _rb = GetComponent<Rigidbody>();
            _rb.isKinematic = true;
            _rb.freezeRotation = true; // Encore utile ??

            // Setup Collider
            _collider = gameObject.GetComponent<Collider>();
            _collider.isTrigger = true;

            // Setup Spawn Point & Rotation
            PlayerData.Origin = transform.position;
            _startPosition = transform.position;
            _originalRotation = transform.localRotation;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            GameManager.MapLoader.OnMapChanged += UpdateMoveConfig;
        }

        void Update()
        {
            if (PauseMenu.GameIsPaused)
            {
                return;
            }

            // Updates
            InputData.Update(moveConfig);
            //UpdateHook();
            UpdateViewAngle();

            _speed.text = PlayerData.Velocity.magnitude.ToString();
        }

        void FixedUpdate()
        {
            if (InputData.ResetPressed)
            {
                _playerManager.Restart();
            }

            float fixedDeltaTime = Time.fixedDeltaTime;
            //_hookshot.CatchMovement(this, fixedDeltaTime); // Todo: Improve
            _controller.ProcessMovement(this, moveConfig, fixedDeltaTime);

            ApplyPlayerMovement();
        }

        private void LateUpdate()
        {
            ApplyMouseMovement();
        }

        private void UpdateHook()
        {
            if (!InputData.HookPressedLastUpdate && InputData.HookPressed) // Player has Started to Press to the Hook Button
                _hookshot.TriggerHook(this);
            _hookshot.CheckForRelease(this);
        }

        private void UpdateViewAngle()
        {
            var rot = PlayerData.ViewAngles + new Vector3(-InputData.MouseY, InputData.MouseX, 0f);
            rot.x = GameUtils.ClampAngle(rot.x, -85f, 85f);
            PlayerData.ViewAngles = rot;
        }

        private void ApplyPlayerMovement()
        {
            transform.position = PlayerData.Origin;
        }

        private void ApplyMouseMovement()
        {
            if (PlayerManager.Instance == null) return;
            if (PlayerManager.Instance.PauseMenuOpen) return;
            // Get the rotation you will be at next as a Quaternion
            Quaternion yQuaternion = Quaternion.AngleAxis(PlayerData.ViewAngles.x, Vector3.right);
            Quaternion xQuaternion = Quaternion.AngleAxis(PlayerData.ViewAngles.y, Vector3.up);

            // Rotate the rigidbody for horizontal move
            transform.localRotation = xQuaternion;

            // Rotate the attached camera for vertival move
            _fpsCamera.transform.localRotation = yQuaternion;
        }

        private void UpdateMoveConfig()
        {
            moveConfig = GameManager.MapLoader.MapMovementConfig;
        }

        private void OnDestroy()
        {
            GameManager.MapLoader.OnMapChanged -= UpdateMoveConfig;
        }
    }
}
