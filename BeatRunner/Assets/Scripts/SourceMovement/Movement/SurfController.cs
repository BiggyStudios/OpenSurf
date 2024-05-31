using Fragsurf.Movement;
using Fragsurf.TraceUtil;
using UnityEngine;

namespace SourceMovement.Movement
{
    public class SurfController {

        ///// Fields /////

        [HideInInspector] public Transform PlayerTransform;
        private ISurfControllable _surfer;
        private MovementConfig _config;
        private float _deltaTime;

        public bool Jumping;
        public bool Crouching;
        public float Speed;
        
        public Transform Camera;
        public float CameraYPos = 0f;

        private float _slideSpeedCurrent;
        private Vector3 _slideDirection = Vector3.forward;

        private bool _sliding;
        private bool _wasSliding;
        private float _slideDelay;
        
        private bool _uncrouchDown;
        private float _crouchLerp;

        private float _frictionMult = 1f;

        ///// Methods /////

        Vector3 _groundNormal = Vector3.up;

        /// <summary>
        /// 
        /// </summary>
        public void ProcessMovement (ISurfControllable surfer, MovementConfig config, float deltaTime) {
            
            // cache instead of passing around parameters
            _surfer = surfer;
            _config = config;
            _deltaTime = deltaTime;
            
            if (_surfer.moveData.laddersEnabled && !_surfer.moveData.climbingLadder) {

                // Look for ladders
                LadderCheck (new Vector3(1f, 0.95f, 1f), _surfer.moveData.velocity * Mathf.Clamp (Time.deltaTime * 2f, 0.025f, 0.25f));

            }
            
            if (_surfer.moveData.laddersEnabled && _surfer.moveData.climbingLadder) {
                
                LadderPhysics ();
                
            } else if (!_surfer.moveData.underwater) {

                if (_surfer.moveData.velocity.y <= 0f)
                    Jumping = false;

                // apply gravity
                if (_surfer.groundObject == null) {

                    _surfer.moveData.velocity.y -= (_surfer.moveData.gravityFactor * _config.gravity * _deltaTime);
                    _surfer.moveData.velocity.y += _surfer.baseVelocity.y * _deltaTime;

                }
                
                // input velocity, check for ground
                CheckGrounded ();
                CalculateMovementVelocity ();
                
            } else {

                // Do underwater logic
                UnderwaterPhysics ();

            }

            float yVel = _surfer.moveData.velocity.y;
            _surfer.moveData.velocity.y = 0f;
            _surfer.moveData.velocity = Vector3.ClampMagnitude (_surfer.moveData.velocity, _config.maxVelocity);
            Speed =  _surfer.moveData.velocity.magnitude;
            _surfer.moveData.velocity.y = yVel;
            
            if (_surfer.moveData.velocity.sqrMagnitude == 0f) {

                // Do collisions while standing still
                SurfPhysics.ResolveCollisions (_surfer.collider, ref _surfer.moveData.origin, ref _surfer.moveData.velocity, _surfer.moveData.rigidbodyPushForce, 1f, _surfer.moveData.stepOffset, _surfer);

            } else {

                float maxDistPerFrame = 0.2f;
                Vector3 velocityThisFrame = _surfer.moveData.velocity * _deltaTime;
                float velocityDistLeft = velocityThisFrame.magnitude;
                float initialVel = velocityDistLeft;
                while (velocityDistLeft > 0f) {

                    float amountThisLoop = Mathf.Min (maxDistPerFrame, velocityDistLeft);
                    velocityDistLeft -= amountThisLoop;

                    // increment origin
                    Vector3 velThisLoop = velocityThisFrame * (amountThisLoop / initialVel);
                    _surfer.moveData.origin += velThisLoop;

                    // don't penetrate walls
                    SurfPhysics.ResolveCollisions (_surfer.collider, ref _surfer.moveData.origin, ref _surfer.moveData.velocity, _surfer.moveData.rigidbodyPushForce, amountThisLoop / initialVel, _surfer.moveData.stepOffset, _surfer);

                }

            }

            _surfer.moveData.groundedTemp = _surfer.moveData.grounded;

            _surfer = null;
            
        }

        /// <summary>
        /// 
        /// </summary>
        private void CalculateMovementVelocity () {
            switch (_surfer.moveType) {

                case MoveType.Walk:

                if (_surfer.groundObject == null) {

                    /*
                    // AIR MOVEMENT
                    */

                    _wasSliding = false;

                    // apply movement from input
                    _surfer.moveData.velocity += AirInputMovement ();

                    // let the magic happen
                    SurfPhysics.Reflect (ref _surfer.moveData.velocity, _surfer.collider, _surfer.moveData.origin, _deltaTime);

                } else {

                    /*
                    //  GROUND MOVEMENT
                    */

                    // Sliding
                    if (!_wasSliding) {

                        _slideDirection = new Vector3 (_surfer.moveData.velocity.x, 0f, _surfer.moveData.velocity.z).normalized;
                        _slideSpeedCurrent = Mathf.Max (_config.maximumSlideSpeed, new Vector3 (_surfer.moveData.velocity.x, 0f, _surfer.moveData.velocity.z).magnitude);

                    }

                    _sliding = false;
                    if (_surfer.moveData.velocity.magnitude > _config.minimumSlideSpeed && _surfer.moveData.slidingEnabled && _surfer.moveData.crouching && _slideDelay <= 0f) {

                        if (!_wasSliding)
                            _slideSpeedCurrent = Mathf.Clamp (_slideSpeedCurrent * _config.slideSpeedMultiplier, _config.minimumSlideSpeed, _config.maximumSlideSpeed);

                        _sliding = true;
                        _wasSliding = true;
                        SlideMovement ();
                        return;

                    } else {

                        if (_slideDelay > 0f)
                            _slideDelay -= _deltaTime;

                        if (_wasSliding)
                            _slideDelay = _config.slideDelay;

                        _wasSliding = false;

                    }
                    
                    float fric = Crouching ? _config.crouchFriction : _config.friction;
                    float accel = Crouching ? _config.crouchAcceleration : _config.acceleration;
                    float decel = Crouching ? _config.crouchDeceleration : _config.deceleration;
                    
                    // Get movement directions
                    Vector3 forward = Vector3.Cross (_groundNormal, -PlayerTransform.right);
                    Vector3 right = Vector3.Cross (_groundNormal, forward);
                    
                    float speed = _surfer.moveData.sprinting ? _config.walkSpeed : _config.sprintSpeed;
                    if (Crouching)
                        speed = _config.crouchSpeed;

                    Vector3 wishDir;

                    // Jump and friction
                    if (_surfer.moveData.wishJump) {

                        ApplyFriction (0.0f, true, true);
                        Jump ();
                        return;

                    } else {

                        ApplyFriction (1.0f * _frictionMult, true, true);

                    }

                    float forwardMove = _surfer.moveData.verticalAxis;
                    float rightMove = _surfer.moveData.horizontalAxis;

                    wishDir = forwardMove * forward + rightMove * right;
                    wishDir.Normalize ();
                    Vector3 moveDirNorm = wishDir;

                    Vector3 forwardVelocity = Vector3.Cross (_groundNormal, Quaternion.AngleAxis (-90, Vector3.up) * new Vector3 (_surfer.moveData.velocity.x, 0f, _surfer.moveData.velocity.z));

                    // Set the target speed of the player
                    float wishSpeed = wishDir.magnitude;
                    wishSpeed *= speed;

                    // Accelerate
                    float yVel = _surfer.moveData.velocity.y;
                    Accelerate (wishDir, wishSpeed, accel * Mathf.Min (_frictionMult, 1f), false);

                    float maxVelocityMagnitude = _config.maxVelocity;
                    _surfer.moveData.velocity = Vector3.ClampMagnitude (new Vector3 (_surfer.moveData.velocity.x, 0f, _surfer.moveData.velocity.z), maxVelocityMagnitude);
                    _surfer.moveData.velocity.y = yVel;

                    // Calculate how much slopes should affect movement
                    float yVelocityNew = forwardVelocity.normalized.y * new Vector3 (_surfer.moveData.velocity.x, 0f, _surfer.moveData.velocity.z).magnitude;

                    // Apply the Y-movement from slopes
                    _surfer.moveData.velocity.y = yVelocityNew * (wishDir.y < 0f ? 1.2f : 1.0f);
                    float removableYVelocity = _surfer.moveData.velocity.y - yVelocityNew;

                }

                break;

            } // END OF SWITCH STATEMENT
        }

        private void UnderwaterPhysics () {

            _surfer.moveData.velocity = Vector3.Lerp (_surfer.moveData.velocity, Vector3.zero, _config.underwaterVelocityDampening * _deltaTime);

            // Gravity
            if (!CheckGrounded ())
                _surfer.moveData.velocity.y -= _config.underwaterGravity * _deltaTime;

            // Swimming upwards
            if (Input.GetButton ("Jump"))
                _surfer.moveData.velocity.y += _config.swimUpSpeed * _deltaTime;

            float fric = _config.underwaterFriction;
            float accel = _config.underwaterAcceleration;
            float decel = _config.underwaterDeceleration;

            ApplyFriction (1f, true, false);

            // Get movement directions
            Vector3 forward = Vector3.Cross (_groundNormal, -PlayerTransform.right);
            Vector3 right = Vector3.Cross (_groundNormal, forward);

            float speed = _config.underwaterSwimSpeed;

            Vector3 wishDir;

            float forwardMove = _surfer.moveData.verticalAxis;
            float rightMove = _surfer.moveData.horizontalAxis;

            wishDir = forwardMove * forward + rightMove * right;
            wishDir.Normalize ();
            Vector3 moveDirNorm = wishDir;

            Vector3 forwardVelocity = Vector3.Cross (_groundNormal, Quaternion.AngleAxis (-90, Vector3.up) * new Vector3 (_surfer.moveData.velocity.x, 0f, _surfer.moveData.velocity.z));

            // Set the target speed of the player
            float wishSpeed = wishDir.magnitude;
            wishSpeed *= speed;

            // Accelerate
            float yVel = _surfer.moveData.velocity.y;
            Accelerate (wishDir, wishSpeed, accel, false);

            float maxVelocityMagnitude = _config.maxVelocity;
            _surfer.moveData.velocity = Vector3.ClampMagnitude (new Vector3 (_surfer.moveData.velocity.x, 0f, _surfer.moveData.velocity.z), maxVelocityMagnitude);
            _surfer.moveData.velocity.y = yVel;

            float yVelStored = _surfer.moveData.velocity.y;
            _surfer.moveData.velocity.y = 0f;

            // Calculate how much slopes should affect movement
            float yVelocityNew = forwardVelocity.normalized.y * new Vector3 (_surfer.moveData.velocity.x, 0f, _surfer.moveData.velocity.z).magnitude;

            // Apply the Y-movement from slopes
            _surfer.moveData.velocity.y = Mathf.Min (Mathf.Max (0f, yVelocityNew) + yVelStored, speed);

            // Jumping out of water
            bool movingForwards = PlayerTransform.InverseTransformVector (_surfer.moveData.velocity).z > 0f;
            Trace waterJumpTrace = TraceBounds (PlayerTransform.position, PlayerTransform.position + PlayerTransform.forward * 0.1f, SurfPhysics.groundLayerMask);
            if (waterJumpTrace.hitCollider != null && Vector3.Angle (Vector3.up, waterJumpTrace.planeNormal) >= _config.slopeLimit && Input.GetButton ("Jump") && !_surfer.moveData.cameraUnderwater && movingForwards)
                _surfer.moveData.velocity.y = Mathf.Max (_surfer.moveData.velocity.y, _config.jumpForce);

        }
        
        private void LadderCheck (Vector3 colliderScale, Vector3 direction) {

            if (_surfer.moveData.velocity.sqrMagnitude <= 0f)
                return;
            
            bool foundLadder = false;

            RaycastHit [] hits = Physics.BoxCastAll (_surfer.moveData.origin, Vector3.Scale (_surfer.collider.bounds.size * 0.5f, colliderScale), Vector3.Scale (direction, new Vector3 (1f, 0f, 1f)), Quaternion.identity, direction.magnitude, SurfPhysics.groundLayerMask, QueryTriggerInteraction.Collide);
            foreach (RaycastHit hit in hits) {

                Ladder ladder = hit.transform.GetComponentInParent<Ladder> ();
                if (ladder != null) {

                    bool allowClimb = true;
                    float ladderAngle = Vector3.Angle (Vector3.up, hit.normal);
                    if (_surfer.moveData.angledLaddersEnabled) {

                        if (hit.normal.y < 0f)
                            allowClimb = false;
                        else {
                            
                            if (ladderAngle <= _surfer.moveData.slopeLimit)
                                allowClimb = false;

                        }

                    } else if (hit.normal.y != 0f)
                        allowClimb = false;

                    if (allowClimb) {
                        foundLadder = true;
                        if (_surfer.moveData.climbingLadder == false) {

                            _surfer.moveData.climbingLadder = true;
                            _surfer.moveData.ladderNormal = hit.normal;
                            _surfer.moveData.ladderDirection = -hit.normal * direction.magnitude * 2f;

                            if (_surfer.moveData.angledLaddersEnabled) {

                                Vector3 sideDir = hit.normal;
                                sideDir.y = 0f;
                                sideDir = Quaternion.AngleAxis (-90f, Vector3.up) * sideDir;

                                _surfer.moveData.ladderClimbDir = Quaternion.AngleAxis (90f, sideDir) * hit.normal;
                                _surfer.moveData.ladderClimbDir *= 1f/ _surfer.moveData.ladderClimbDir.y; // Make sure Y is always 1

                            } else
                                _surfer.moveData.ladderClimbDir = Vector3.up;
                            
                        }
                        
                    }

                }

            }

            if (!foundLadder) {
                
                _surfer.moveData.ladderNormal = Vector3.zero;
                _surfer.moveData.ladderVelocity = Vector3.zero;
                _surfer.moveData.climbingLadder = false;
                _surfer.moveData.ladderClimbDir = Vector3.up;

            }

        }

        private void LadderPhysics () {
            
            _surfer.moveData.ladderVelocity = _surfer.moveData.ladderClimbDir * _surfer.moveData.verticalAxis * 6f;

            _surfer.moveData.velocity = Vector3.Lerp (_surfer.moveData.velocity, _surfer.moveData.ladderVelocity, Time.deltaTime * 10f);

            LadderCheck (Vector3.one, _surfer.moveData.ladderDirection);
            
            Trace floorTrace = TraceToFloor ();
            if (_surfer.moveData.verticalAxis < 0f && floorTrace.hitCollider != null && Vector3.Angle (Vector3.up, floorTrace.planeNormal) <= _surfer.moveData.slopeLimit) {

                _surfer.moveData.velocity = _surfer.moveData.ladderNormal * 0.5f;
                _surfer.moveData.ladderVelocity = Vector3.zero;
                _surfer.moveData.climbingLadder = false;

            }

            if (_surfer.moveData.wishJump) {

                _surfer.moveData.velocity = _surfer.moveData.ladderNormal * 4f;
                _surfer.moveData.ladderVelocity = Vector3.zero;
                _surfer.moveData.climbingLadder = false;
                
            }
            
        }
        
        private void Accelerate (Vector3 wishDir, float wishSpeed, float acceleration, bool yMovement) {

            // Initialise variables
            float addSpeed;
            float accelerationSpeed;
            float currentSpeed;
            
            // again, no idea
            currentSpeed = Vector3.Dot (_surfer.moveData.velocity, wishDir);
            addSpeed = wishSpeed - currentSpeed;

            // If you're not actually increasing your speed, stop here.
            if (addSpeed <= 0)
                return;

            // won't bother trying to understand any of this, really
            accelerationSpeed = Mathf.Min (acceleration * _deltaTime * wishSpeed, addSpeed);

            // Add the velocity.
            _surfer.moveData.velocity.x += accelerationSpeed * wishDir.x;
            if (yMovement) { _surfer.moveData.velocity.y += accelerationSpeed * wishDir.y; }
            _surfer.moveData.velocity.z += accelerationSpeed * wishDir.z;

        }

        private void ApplyFriction (float t, bool yAffected, bool grounded) {

            // Initialise variables
            Vector3 vel = _surfer.moveData.velocity;
            float speed;
            float newSpeed;
            float control;
            float drop;

            // Set Y to 0, speed to the magnitude of movement and drop to 0. I think drop is the amount of speed that is lost, but I just stole this from the internet, idk.
            vel.y = 0.0f;
            speed = vel.magnitude;
            drop = 0.0f;

            float fric = Crouching ? _config.crouchFriction : _config.friction;
            float accel = Crouching ? _config.crouchAcceleration : _config.acceleration;
            float decel = Crouching ? _config.crouchDeceleration : _config.deceleration;

            // Only apply friction if the player is grounded
            if (grounded) {
                
                // i honestly have no idea what this does tbh
                vel.y = _surfer.moveData.velocity.y;
                control = speed < decel ? decel : speed;
                drop = control * fric * _deltaTime * t;

            }

            // again, no idea, but comments look cool
            newSpeed = Mathf.Max (speed - drop, 0f);
            if (speed > 0.0f)
                newSpeed /= speed;

            // Set the end-velocity
            _surfer.moveData.velocity.x *= newSpeed;
            if (yAffected == true) { _surfer.moveData.velocity.y *= newSpeed; }
            _surfer.moveData.velocity.z *= newSpeed;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Vector3 AirInputMovement () {

            Vector3 wishVel, wishDir;
            float wishSpeed;

            GetWishValues (out wishVel, out wishDir, out wishSpeed);

            if (_config.clampAirSpeed && (wishSpeed != 0f && (wishSpeed > _config.maxSpeed))) {

                wishVel = wishVel * (_config.maxSpeed / wishSpeed);
                wishSpeed = _config.maxSpeed;

            }

            return SurfPhysics.AirAccelerate (_surfer.moveData.velocity, wishDir, wishSpeed, _config.airAcceleration, _config.airCap, _deltaTime);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wishVel"></param>
        /// <param name="wishDir"></param>
        /// <param name="wishSpeed"></param>
        private void GetWishValues (out Vector3 wishVel, out Vector3 wishDir, out float wishSpeed) {

            wishVel = Vector3.zero;
            wishDir = Vector3.zero;
            wishSpeed = 0f;

            Vector3 forward = _surfer.forward,
                right = _surfer.right;

            forward [1] = 0;
            right [1] = 0;
            forward.Normalize ();
            right.Normalize ();

            for (int i = 0; i < 3; i++)
                wishVel [i] = forward [i] * _surfer.moveData.forwardMove + right [i] * _surfer.moveData.sideMove;
            wishVel [1] = 0;

            wishSpeed = wishVel.magnitude;
            wishDir = wishVel.normalized;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="jumpPower"></param>
        private void Jump () {
            
            if (!_config.autoBhop)
                _surfer.moveData.wishJump = false;
            
            _surfer.moveData.velocity.y += _config.jumpForce;
            Jumping = true;

        }

        /// <summary>
        /// 
        /// </summary>
        private bool CheckGrounded () {

            _surfer.moveData.surfaceFriction = 1f;
            var movingUp = _surfer.moveData.velocity.y > 0f;
            var trace = TraceToFloor ();

            float groundSteepness = Vector3.Angle (Vector3.up, trace.planeNormal);

            if (trace.hitCollider == null || groundSteepness > _config.slopeLimit || (Jumping && _surfer.moveData.velocity.y > 0f)) {

                SetGround (null);

                if (movingUp && _surfer.moveType != MoveType.Noclip)
                    _surfer.moveData.surfaceFriction = _config.airFriction;
                
                return false;

            } else {

                _groundNormal = trace.planeNormal;
                SetGround (trace.hitCollider.gameObject);
                return true;

            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void SetGround (GameObject obj) {

            if (obj != null) {

                _surfer.groundObject = obj;
                _surfer.moveData.velocity.y = 0;

            } else
                _surfer.groundObject = null;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="layerMask"></param>
        /// <returns></returns>
        private Trace TraceBounds (Vector3 start, Vector3 end, int layerMask) {

            return Tracer.TraceCollider (_surfer.collider, start, end, layerMask);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Trace TraceToFloor () {

            var down = _surfer.moveData.origin;
            down.y -= 0.15f;

            return Tracer.TraceCollider (_surfer.collider, _surfer.moveData.origin, down, SurfPhysics.groundLayerMask);

        }

        public void Crouch (ISurfControllable surfer, MovementConfig config, float deltaTime)
        {
            _surfer = surfer;
            _config = config;
            _deltaTime = deltaTime;

            if (_surfer == null) return;
            if (_surfer.collider == null) return;

            bool grounded = _surfer.groundObject != null;
            bool wantsToCrouch = _surfer.moveData.crouching;
            float crouchingHeight = Mathf.Clamp (_surfer.moveData.crouchingHeight, 0.01f, 1f);
            float heightDifference = _surfer.moveData.defaultHeight - _surfer.moveData.defaultHeight * crouchingHeight;
            BoxCollider boxCollider = (BoxCollider)_surfer.collider;
            
            if (grounded) _uncrouchDown = false;

            _crouchLerp = Mathf.Lerp (_crouchLerp, wantsToCrouch ? 1f : 0f, _deltaTime * _surfer.moveData.crouchingSpeed);
            var collSizeY = Mathf.Lerp(_surfer.moveData.defaultHeight, _surfer.moveData.defaultHeight * crouchingHeight, _crouchLerp);
            boxCollider.size = new Vector3 (boxCollider.size.x, collSizeY, boxCollider.size.z);
            
            foreach (Transform child in PlayerTransform)
            {
                if (child == _surfer.moveData.viewTransform) continue;
                var childPosY = Mathf.Lerp(child.localPosition.y / crouchingHeight, child.localPosition.y * crouchingHeight, _crouchLerp);
                child.localPosition = new Vector3 (child.localPosition.x, childPosY, child.localPosition.z);
            }
            
            if (_crouchLerp > 0.9f && !Crouching)
            {
                Crouching = true;
                _uncrouchDown = !grounded;
            }
            else if (Crouching)
            {
                // Check if the player can uncrouch
                bool canUncrouch = true;
                Vector3 halfExtents = boxCollider.size * 0.5f;
                Vector3 startPos = boxCollider.transform.position;
                Vector3 endPos = boxCollider.transform.position + (_uncrouchDown ? Vector3.down : Vector3.up) * heightDifference;
                Trace trace = Tracer.TraceBox (startPos, endPos, halfExtents, boxCollider.contactOffset, SurfPhysics.groundLayerMask);

                if (trace.hitCollider != null) canUncrouch = false;
                if (canUncrouch && _crouchLerp <= 0.9f) Crouching = false;
                if (!canUncrouch) _crouchLerp = 1f;
            }
        }

        void SlideMovement () {
            
            // Gradually change direction
            _slideDirection += new Vector3 (_groundNormal.x, 0f, _groundNormal.z) * (_slideSpeedCurrent * _deltaTime);
            _slideDirection = _slideDirection.normalized;

            // Set direction
            Vector3 slideForward = Vector3.Cross (_groundNormal, Quaternion.AngleAxis (-90, Vector3.up) * _slideDirection);
            
            // Set the velocity
            _slideSpeedCurrent -= _config.slideFriction * _deltaTime;
            _slideSpeedCurrent = Mathf.Clamp (_slideSpeedCurrent, 0f, _config.maximumSlideSpeed);
            _slideSpeedCurrent -= (slideForward * _slideSpeedCurrent).y * _deltaTime * _config.downhillSlideSpeedMultiplier; // Accelerate downhill (-y = downward, - * - = +)

            _surfer.moveData.velocity = slideForward * _slideSpeedCurrent;
            
            // Jump
            if (_surfer.moveData.wishJump && _slideSpeedCurrent < _config.minimumSlideSpeed * _config.slideSpeedMultiplier) {

                Jump ();
                return;

            }

        }

    }
}
