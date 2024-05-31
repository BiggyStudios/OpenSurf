using System.Collections.Generic;
using FishNet.Object;
using Fragsurf.Movement;
using UnityEngine;

namespace SourceMovement.Movement {

    /// <summary>
    /// Easily add a surfable character to the scene
    /// </summary>
    [AddComponentMenu ("Fragsurf/Surf Character")]
    public class SurfCharacter : NetworkBehaviour, ISurfControllable {

        public enum ColliderType {
            Capsule,
            Box
        }

        ///// Fields /////
        
        public GameObject Cam;
        public PlayerAiming CamScript;
        
        [Header("Physics Settings")]
        public Vector3 ColliderSize = new(1f, 2f, 1f);

        public ColliderType CollisionType => ColliderType.Box;
        public float Weight = 75f;
        public float RigidbodyPushForce = 2f;
        public bool SolidCollider;

        [Header("View Settings")]
        public Transform ViewTransform;
        public Transform PlayerRotationTransform;

        [Header ("Crouching setup")]
        public float CrouchingHeightMultiplier = 0.5f;
        public float CrouchingSpeed = 10f;
        float _defaultHeight;
        bool _allowCrouch = true; // This is separate because you shouldn't be able to toggle crouching on and off during gameplay for various reasons

        [Header ("Features")]
        public bool CrouchingEnabled = true;
        public bool SlidingEnabled = false;
        public bool LaddersEnabled = true;
        public bool SupportAngledLadders = true;

        [Header ("Step offset (can be buggy, enable at your own risk)")]
        public bool UseStepOffset = false;
        public float StepOffset = 0.35f;

        [Header ("Movement Config")]
        [SerializeField]
        public MovementConfig MovementConfig;
        
        private GameObject _groundObject;
        private Vector3 _baseVelocity;
        private Collider _collider;
        private Vector3 _angles;
        private Vector3 _startPosition;
        private GameObject _colliderObject;
        private GameObject _cameraWaterCheckObject;
        private CameraWaterCheck _cameraWaterCheck;

        private MoveData _moveData = new MoveData ();
        private SurfController _controller = new SurfController ();

        private Rigidbody _rb;

        private List<Collider> _triggers = new List<Collider> ();
        private int _numberOfTriggers = 0;

        private bool _underwater = false;

        ///// Properties /////

        public MoveType moveType { get { return MoveType.Walk; } }
        public MovementConfig MoveConfig { get { return MovementConfig; } }
        public MoveData moveData { get { return _moveData; } }
        public new Collider collider { get { return _collider; } }

        public GameObject groundObject {

            get { return _groundObject; }
            set { _groundObject = value; }

        }

        public Vector3 baseVelocity { get { return _baseVelocity; } }

        public Vector3 forward { get { return ViewTransform.forward; } }
        public Vector3 right { get { return ViewTransform.right; } }
        public Vector3 up { get { return ViewTransform.up; } }

        Vector3 _prevPosition;

        ///// Methods /////
        
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube( transform.position, ColliderSize );
		}

        public override void OnStartClient()
        {
            base.OnStartClient();
            
            if (!IsOwner)
            {
                Cam.SetActive(false);
            }
        }

        private void Awake () {
            
            _controller.PlayerTransform = PlayerRotationTransform;
            
            if (ViewTransform != null) {

                _controller.Camera = ViewTransform;
                _controller.CameraYPos = ViewTransform.localPosition.y;

            }

        }

        private void Start () {
            
            _colliderObject = new GameObject ("PlayerCollider");
            _colliderObject.layer = gameObject.layer;
            _colliderObject.transform.SetParent (transform);
            _colliderObject.transform.rotation = Quaternion.identity;
            _colliderObject.transform.localPosition = Vector3.zero;
            _colliderObject.transform.SetSiblingIndex (0);

            // Water check
            _cameraWaterCheckObject = new GameObject ("Camera water check");
            _cameraWaterCheckObject.layer = gameObject.layer;
            _cameraWaterCheckObject.transform.position = ViewTransform.position;

            SphereCollider cameraWaterCheckSphere = _cameraWaterCheckObject.AddComponent<SphereCollider> ();
            cameraWaterCheckSphere.radius = 0.1f;
            cameraWaterCheckSphere.isTrigger = true;

            Rigidbody cameraWaterCheckRb = _cameraWaterCheckObject.AddComponent<Rigidbody> ();
            cameraWaterCheckRb.useGravity = false;
            cameraWaterCheckRb.isKinematic = true;

            _cameraWaterCheck = _cameraWaterCheckObject.AddComponent<CameraWaterCheck> ();

            _prevPosition = transform.position;

            if (ViewTransform == null)
                ViewTransform = Camera.main.transform;

            if (PlayerRotationTransform == null && transform.childCount > 0)
                PlayerRotationTransform = transform.GetChild (0);

            _collider = gameObject.GetComponent<Collider> ();

            if (_collider != null)
                Destroy (_collider);

            //rigidbody is required to collide with triggers
            _rb = gameObject.GetComponent<Rigidbody> ();
            if (_rb == null)
                _rb = gameObject.AddComponent<Rigidbody> ();

            _allowCrouch = CrouchingEnabled;

            _rb.isKinematic = true;
            _rb.useGravity = false;
            _rb.angularDrag = 0f;
            _rb.drag = 0f;
            _rb.mass = Weight;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;


            switch (CollisionType) {

                // Box collider
                case ColliderType.Box:

                _collider = _colliderObject.AddComponent<BoxCollider> ();

                var boxc = (BoxCollider)_collider;
                boxc.size = ColliderSize;

                _defaultHeight = boxc.size.y;

                break;

                // Capsule collider
                case ColliderType.Capsule:

                _collider = _colliderObject.AddComponent<CapsuleCollider> ();

                var capc = (CapsuleCollider)_collider;
                capc.height = ColliderSize.y;
                capc.radius = ColliderSize.x / 2f;

                _defaultHeight = capc.height;

                break;

            }

            _moveData.slopeLimit = MovementConfig.slopeLimit;

            _moveData.rigidbodyPushForce = RigidbodyPushForce;

            _moveData.slidingEnabled = SlidingEnabled;
            _moveData.laddersEnabled = LaddersEnabled;
            _moveData.angledLaddersEnabled = SupportAngledLadders;

            _moveData.playerTransform = transform;
            _moveData.viewTransform = ViewTransform;
            _moveData.viewTransformDefaultLocalPos = ViewTransform.localPosition;

            _moveData.defaultHeight = _defaultHeight;
            _moveData.crouchingHeight = CrouchingHeightMultiplier;
            _moveData.crouchingSpeed = CrouchingSpeed;
            
            _collider.isTrigger = !SolidCollider;
            _moveData.origin = transform.position;
            _startPosition = transform.position;

            _moveData.useStepOffset = UseStepOffset;
            _moveData.stepOffset = StepOffset;

        }

        private void Update()
        {
            if (!IsOwner)
                return;
            
            _colliderObject.transform.rotation = Quaternion.identity;


            UpdateMoveData();
            //UpdateTestBinds ();
            
            // Previous movement code
            Vector3 positionalMovement = transform.position - _prevPosition;
            transform.position = _prevPosition;
            moveData.origin += positionalMovement;

            // Triggers
            if (_numberOfTriggers != _triggers.Count) {
                _numberOfTriggers = _triggers.Count;

                _underwater = false;
                _triggers.RemoveAll (item => item == null);
                foreach (Collider trigger in _triggers) {

                    if (trigger == null)
                        continue;

                    if (trigger.GetComponentInParent<Water> ())
                        _underwater = true;

                }

            }

            _moveData.cameraUnderwater = _cameraWaterCheck.IsUnderwater ();
            _cameraWaterCheckObject.transform.position = ViewTransform.position;
            moveData.underwater = _underwater;
            
            if (_allowCrouch)
                _controller.Crouch (this, MovementConfig, Time.deltaTime);

            _controller.ProcessMovement (this, MovementConfig, Time.deltaTime);

            transform.position = moveData.origin;
            _prevPosition = transform.position;

            _colliderObject.transform.rotation = Quaternion.identity;
        }
        
        private void UpdateTestBinds () {

            if (Input.GetKeyDown (KeyCode.Backspace))
                ResetPosition ();

        }

        private void ResetPosition () {
            
            moveData.velocity = Vector3.zero;
            moveData.origin = _startPosition;

        }

        private void UpdateMoveData () {
            
            _moveData.verticalAxis = Input.GetAxisRaw ("Vertical");
            _moveData.horizontalAxis = Input.GetAxisRaw ("Horizontal");

            _moveData.sprinting = Input.GetKey(KeyCode.LeftShift);
            
            if (Input.GetKeyDown(KeyCode.LeftControl))
                _moveData.crouching = true;

            if (Input.GetKeyUp(KeyCode.LeftControl))
                _moveData.crouching = false;
            
            bool moveLeft = _moveData.horizontalAxis < 0f;
            bool moveRight = _moveData.horizontalAxis > 0f;
            bool moveFwd = _moveData.verticalAxis > 0f;
            bool moveBack = _moveData.verticalAxis < 0f;
            bool jump = Input.GetButton ("Jump");

            if (!moveLeft && !moveRight)
                _moveData.sideMove = 0f;
            else if (moveLeft)
                _moveData.sideMove = -MoveConfig.acceleration;
            else if (moveRight)
                _moveData.sideMove = MoveConfig.acceleration;

            if (!moveFwd && !moveBack)
                _moveData.forwardMove = 0f;
            else if (moveFwd)
                _moveData.forwardMove = MoveConfig.acceleration;
            else if (moveBack)
                _moveData.forwardMove = -MoveConfig.acceleration;
            
            if (Input.GetKeyDown(KeyCode.Space))
                _moveData.wishJump = true;

            if (Input.GetKeyUp(KeyCode.Space))
                _moveData.wishJump = false;
            
            _moveData.viewAngles = _angles;

        }

        private void DisableInput () {

            _moveData.verticalAxis = 0f;
            _moveData.horizontalAxis = 0f;
            _moveData.sideMove = 0f;
            _moveData.forwardMove = 0f;
            _moveData.wishJump = false;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static float ClampAngle (float angle, float from, float to) {

            if (angle < 0f)
                angle = 360 + angle;

            if (angle > 180f)
                return Mathf.Max (angle, 360 + from);

            return Mathf.Min (angle, to);

        }

        private void OnTriggerEnter (Collider other) {
            
            if (!_triggers.Contains (other))
                _triggers.Add (other);

        }

        private void OnTriggerExit (Collider other) {
            
            if (_triggers.Contains (other))
                _triggers.Remove (other);

        }

        private void OnCollisionStay (Collision collision) {

            if (collision.rigidbody == null)
                return;

            Vector3 relativeVelocity = collision.relativeVelocity * collision.rigidbody.mass / 50f;
            Vector3 impactVelocity = new Vector3 (relativeVelocity.x * 0.0025f, relativeVelocity.y * 0.00025f, relativeVelocity.z * 0.0025f);

            float maxYVel = Mathf.Max (moveData.velocity.y, 10f);
            Vector3 newVelocity = new Vector3 (moveData.velocity.x + impactVelocity.x, Mathf.Clamp (moveData.velocity.y + Mathf.Clamp (impactVelocity.y, -0.5f, 0.5f), -maxYVel, maxYVel), moveData.velocity.z + impactVelocity.z);

            newVelocity = Vector3.ClampMagnitude (newVelocity, Mathf.Max (moveData.velocity.magnitude, 30f));
            moveData.velocity = newVelocity;

        }

    }

}

