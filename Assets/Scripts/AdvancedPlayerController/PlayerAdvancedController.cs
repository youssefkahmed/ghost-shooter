using System;
using Ghost.StateMachine;
using Ghost.Utils;
using ImprovedTimers;
using UnityEngine;
using UnityUtils;

namespace Ghost.AdvancedPlayerController
{
    public class PlayerAdvancedController : MonoBehaviour
    {
        [SerializeField] private InputReader inputReader;
        [SerializeField] private Transform cameraTransform;
        
        public event Action<Vector3> OnJump = delegate { };
        public event Action<Vector3> OnLand = delegate { };
        
        public float movementSpeed = 7f;
        public float airControlRate = 2f;
        public float jumpSpeed = 10f;
        public float jumpDuration = 0.2f;
        public float airFriction = 0.5f;
        public float groundFriction = 100f;
        public float gravity = 30f;
        public float slideGravity = 5f;
        public float slopeLimit = 30f;
        public bool useLocalMomentum;
        
        private Transform _transform;
        private PlayerMover _mover;
        private CeilingDetector _ceilingDetector;

        private StateMachine.StateMachine _stateMachine;
        private CountdownTimer _jumpTimer;
        
        private Vector3 _momentum;
        private Vector3 _savedVelocity;
        private Vector3 _savedMovementVelocity;
        
        private bool _jumpKeyIsPressed;    // Tracks whether the jump key is currently being held down by the player
        private bool _jumpKeyWasPressed;   // Indicates if the jump key was pressed since the last reset, used to detect jump initiation
        private bool _jumpKeyWasLetGo;     // Indicates if the jump key was released since it was last pressed, used to detect when to stop jumping
        private bool _jumpInputIsLocked;   // Prevents jump initiation when true, used to ensure only one jump action per press

        private void Awake()
        {
            _transform = transform;
            _mover = GetComponent<PlayerMover>();
            _ceilingDetector = GetComponent<CeilingDetector>();
            
            _jumpTimer = new CountdownTimer(jumpDuration);
            SetupStateMachine();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Start()
        {
            inputReader.EnablePlayerActions();
            inputReader.Jump += HandleJumpKeyInput;
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        private void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
            _mover.CheckForGround();
            HandleMomentum();
            
            Vector3 velocity = _stateMachine.CurrentState is GroundedState ? CalculateMovementVelocity() : Vector3.zero;
            velocity += useLocalMomentum ? _transform.localToWorldMatrix * _momentum : _momentum;
            
            _mover.SetExtendSensorRange(IsGrounded());
            _mover.SetVelocity(velocity);
            
            _savedVelocity = velocity;
            _savedMovementVelocity = CalculateMovementVelocity();
            
            ResetJumpKeys();

            if (_ceilingDetector)
            {
                _ceilingDetector.Reset();
            }
        }

        #region Public Methods

        public Vector3 GetVelocity()
        {
            return _savedVelocity;
        }
        
        public Vector3 GetMomentum()
        {
            return useLocalMomentum ? _transform.localToWorldMatrix * _momentum : _momentum;
        }
        
        public Vector3 GetMovementVelocity()
        {
            return _savedMovementVelocity;
        }

        public void OnJumpStart()
        {
            if (useLocalMomentum)
            {
                _momentum = _transform.localToWorldMatrix * _momentum;
            }
            
            _momentum += _transform.up * jumpSpeed;
            _jumpTimer.Start();
            _jumpInputIsLocked = true;
            OnJump?.Invoke(_momentum);
            
            if (useLocalMomentum)
            {
                _momentum = _transform.worldToLocalMatrix * _momentum;
            }
        }

        public void OnGroundContactLost()
        {
            if (useLocalMomentum)
            {
                _momentum = _transform.localToWorldMatrix * _momentum;
            }
            
            Vector3 velocity = GetMovementVelocity();
            if (velocity.sqrMagnitude >= 0f && _momentum.sqrMagnitude > 0f)
            {
                Vector3 projectedMomentum = Vector3.Project(_momentum, velocity.normalized);
                var dot = VectorMath.GetDotProduct(projectedMomentum.normalized, velocity.normalized);
                
                if (projectedMomentum.sqrMagnitude >= velocity.sqrMagnitude && dot > 0f)
                {
                    velocity = Vector3.zero;
                }
                else if (dot > 0f)
                {
                    velocity -= projectedMomentum;
                }
            }
            _momentum += velocity;
            
            if (useLocalMomentum)
            {
                _momentum = _transform.worldToLocalMatrix * _momentum;
            }
        }

        public void OnGroundContactRegained()
        {
            Vector3 collisionVelocity = GetMomentum();
            OnLand?.Invoke(collisionVelocity);
        }

        public void OnFallStart()
        {
            Vector3 currentUpwardMomentum = VectorMath.ExtractDotVector(_momentum, _transform.up);
            _momentum = VectorMath.RemoveDotVector(_momentum, _transform.up);
            _momentum -= _transform.up * currentUpwardMomentum.magnitude;
        }
        
        #endregion

        #region State Machine Methods

        private void SetupStateMachine()
        {
            _stateMachine = new StateMachine.StateMachine();
            
            var grounded = new GroundedState(this);
            var falling = new FallingState(this);
            var sliding = new SlidingState(this);
            var rising = new RisingState(this);
            var jumping = new JumpingState(this);
            
            At(grounded, rising, IsRising);
            At(grounded, sliding, () => _mover.IsGrounded() && IsGroundTooSteep());
            At(grounded, falling, () => !_mover.IsGrounded());
            At(grounded, jumping, () => (_jumpKeyIsPressed || _jumpKeyWasPressed) && !_jumpInputIsLocked);
            
            At(falling, rising, IsRising);
            At(falling, grounded, () => _mover.IsGrounded() && !IsGroundTooSteep());
            At(falling, sliding, IsGroundTooSteep);
            
            At(sliding, rising, IsRising);
            At(sliding, falling, () => !_mover.IsGrounded());
            At(sliding, grounded, () => _mover.IsGrounded() && !IsGroundTooSteep());
            
            At(rising, grounded, () => _mover.IsGrounded() && !IsGroundTooSteep());
            At(rising, sliding, () => _mover.IsGrounded() && IsGroundTooSteep());
            At(rising, falling, IsFalling);
            At(rising, falling, () => _ceilingDetector != null && _ceilingDetector.HitCeiling());
            
            At(jumping, rising, () => _jumpTimer.IsFinished || _jumpKeyWasLetGo);
            At(jumping, falling, () => _ceilingDetector != null && _ceilingDetector.HitCeiling());
            
            _stateMachine.SetState(falling);
        }

        private void At(IState from, IState to, Func<bool> condition)
        {
            _stateMachine.AddTransition(from, to, condition);
        }

        private void Any<T>(IState to, Func<bool> condition)
        {
            _stateMachine.AddAnyTransition(to, condition);
        }
        
        private bool IsRising()
        {
            return VectorMath.GetDotProduct(GetMomentum(), _transform.up) > 0f;
        }

        private bool IsFalling()
        { 
            return VectorMath.GetDotProduct(GetMomentum(), _transform.up) < 0f;
        }

        private bool IsGroundTooSteep()
        {
            return !_mover.IsGrounded() || Vector3.Angle(_mover.GetGroundNormal(), _transform.up) > slopeLimit;
        }
        
        #endregion
        
        private bool IsGrounded()
        {
            return _stateMachine.CurrentState is GroundedState or SlidingState;
        }

        private Vector3 CalculateMovementVelocity()
        {
            return CalculateMovementDirection() * movementSpeed;
        }

        private Vector3 CalculateMovementDirection()
        {
            Vector3 direction = !cameraTransform 
                ? _transform.right * inputReader.Direction.x + _transform.forward * inputReader.Direction.y 
                : Vector3.ProjectOnPlane(cameraTransform.right, _transform.up).normalized * inputReader.Direction.x + 
                  Vector3.ProjectOnPlane(cameraTransform.forward, _transform.up).normalized * inputReader.Direction.y;
            
            return direction.magnitude > 1f ? direction.normalized : direction;
        }
        
        private void HandleMomentum()
        {
            if (useLocalMomentum)
            {
                _momentum = _transform.localToWorldMatrix * _momentum;
            }
            
            Vector3 verticalMomentum = VectorMath.ExtractDotVector(_momentum, _transform.up);
            Vector3 horizontalMomentum = _momentum - verticalMomentum;
            
            verticalMomentum -= _transform.up * (gravity * Time.deltaTime);
            if (_stateMachine.CurrentState is GroundedState && VectorMath.GetDotProduct(verticalMomentum, _transform.up) < 0f)
            {
                verticalMomentum = Vector3.zero;
            }

            if (!IsGrounded())
            {
                AdjustHorizontalMomentum(ref horizontalMomentum, CalculateMovementVelocity());
            }

            if (_stateMachine.CurrentState is SlidingState)
            {
                HandleSliding(ref horizontalMomentum);
            }
            
            var friction = _stateMachine.CurrentState is GroundedState ? groundFriction : airFriction;
            horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, Vector3.zero, friction * Time.deltaTime);
            
            _momentum = horizontalMomentum + verticalMomentum;

            if (_stateMachine.CurrentState is JumpingState)
            {
                HandleJumping();
            }
            
            if (_stateMachine.CurrentState is SlidingState)
            {
                _momentum = Vector3.ProjectOnPlane(_momentum, _mover.GetGroundNormal());
                if (VectorMath.GetDotProduct(_momentum, _transform.up) > 0f)
                {
                    _momentum = VectorMath.RemoveDotVector(_momentum, _transform.up);
                }
            
                Vector3 slideDirection = Vector3.ProjectOnPlane(-_transform.up, _mover.GetGroundNormal()).normalized;
                _momentum += slideDirection * (slideGravity * Time.deltaTime);
            }
            
            if (useLocalMomentum)
            {
                _momentum = _transform.worldToLocalMatrix * _momentum;
            }
        }

        private void HandleJumping()
        {
            _momentum = VectorMath.RemoveDotVector(_momentum, _transform.up);
            _momentum += _transform.up * jumpSpeed;
        }

        private void HandleJumpKeyInput(bool isButtonPressed)
        {
            if (!_jumpKeyIsPressed && isButtonPressed)
            {
                _jumpKeyWasPressed = true;
            }

            if (_jumpKeyIsPressed && !isButtonPressed)
            {
                _jumpKeyWasLetGo = true;
                _jumpInputIsLocked = false;
            }
            
            _jumpKeyIsPressed = isButtonPressed;
        }
        
        private void ResetJumpKeys()
        {
            _jumpKeyWasLetGo = false;
            _jumpKeyWasPressed = false;
        }

        private void AdjustHorizontalMomentum(ref Vector3 horizontalMomentum, Vector3 movementVelocity)
        {
            if (horizontalMomentum.magnitude > movementSpeed)
            {
                if (VectorMath.GetDotProduct(movementVelocity, horizontalMomentum.normalized) > 0f)
                {
                    movementVelocity = VectorMath.RemoveDotVector(movementVelocity, horizontalMomentum.normalized);
                }
                horizontalMomentum += movementVelocity * (Time.deltaTime * airControlRate * 0.25f);
            }
            else
            {
                horizontalMomentum += movementVelocity * (Time.deltaTime * airControlRate);
                horizontalMomentum = Vector3.ClampMagnitude(horizontalMomentum, movementSpeed);
            }
        }

        private void HandleSliding(ref Vector3 horizontalMomentum)
        {
            Vector3 pointDownVector = Vector3.ProjectOnPlane(_mover.GetGroundNormal(), _transform.up).normalized;
            Vector3 movementVelocity = CalculateMovementVelocity();
            
            movementVelocity = VectorMath.RemoveDotVector(movementVelocity, pointDownVector);
            horizontalMomentum += movementVelocity * Time.fixedDeltaTime;
        }
    }
}
