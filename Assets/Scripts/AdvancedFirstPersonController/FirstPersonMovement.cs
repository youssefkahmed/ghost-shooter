using System.Collections;
using UnityEngine;

namespace Ghost.AdvancedFirstPersonController
{
    public class PlayerMovementAdvanced : MonoBehaviour
    {
        [SerializeField] private Transform orientation;

        [Header("Movement")] [SerializeField] private float walkSpeed;
        [SerializeField] private float sprintSpeed;
        [SerializeField] private float groundDrag;

        [Header("Jumping")] [SerializeField] private float jumpForce;
        [SerializeField] private float jumpCooldown;
        [SerializeField] private float airMultiplier;

        [Header("Crouching")] [SerializeField] private float crouchSpeed;
        [SerializeField] private float crouchYScale;

        [Header("Key Binds")] [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;

        [Header("Ground Check")] [SerializeField]
        private float playerHeight;

        [SerializeField] private LayerMask groundLayerMask;

        [Header("Slope Handling")] [SerializeField]
        private float maxSlopeAngle;

        private Rigidbody _rb;

        private float _moveSpeed;
        private Vector3 _moveDirection;
        private float _horizontalInput;
        private float _verticalInput;

        private bool _readyToJump;
        private bool _isGrounded;
        private float _startYScale;

        private RaycastHit _slopeHit;
        private bool _exitingSlope;

        private MovementState _state;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.freezeRotation = true;

            _readyToJump = true;
            _startYScale = transform.localScale.y;
        }

        private void Update()
        {
            // Ground check
            _isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayerMask);

            HandleInput();
            SpeedControl();
            StateHandler();
            HandleDrag();
        }

        private void FixedUpdate()
        {
            MovePlayer();
        }

        private void HandleInput()
        {
            _horizontalInput = Input.GetAxisRaw("Horizontal");
            _verticalInput = Input.GetAxisRaw("Vertical");

            // Jumping
            if (Input.GetKey(jumpKey) && _readyToJump && _isGrounded)
            {
                _readyToJump = false;

                StartCoroutine(Jump());
            }

            // Crouching
            if (Input.GetKeyDown(crouchKey))
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                _rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            }

            // Stopping crouch
            if (Input.GetKeyUp(crouchKey))
            {
                transform.localScale = new Vector3(transform.localScale.x, _startYScale, transform.localScale.z);
            }
        }

        private void StateHandler()
        {
            // Mode - Crouching
            if (Input.GetKey(crouchKey))
            {
                _state = MovementState.Crouching;
                _moveSpeed = crouchSpeed;
            }

            // Mode - Sprinting
            else if (_isGrounded && Input.GetKey(sprintKey))
            {
                _state = MovementState.Sprinting;
                _moveSpeed = sprintSpeed;
            }

            // Mode - Walking
            else if (_isGrounded)
            {
                _state = MovementState.Walking;
                _moveSpeed = walkSpeed;
            }

            // Mode - Airborne
            else
            {
                _state = MovementState.Airborne;
            }
        }

        private void MovePlayer()
        {
            // Calculate movement direction
            _moveDirection = orientation.forward * _verticalInput + orientation.right * _horizontalInput;

            // On slope
            if (OnSlope() && !_exitingSlope)
            {
                _rb.AddForce(_moveSpeed * 20f * GetSlopeMoveDirection(), ForceMode.Force);
                if (_rb.linearVelocity.y > 0)
                {
                    _rb.AddForce(Vector3.down * 80f, ForceMode.Force);
                }
            }
            // On ground
            else if (_isGrounded)
            {
                _rb.AddForce(_moveSpeed * 10f * _moveDirection.normalized, ForceMode.Force);
            } 
            // In air
            else if (!_isGrounded)
            {
                _rb.AddForce(_moveSpeed * 10f * airMultiplier * _moveDirection.normalized, ForceMode.Force);
            }

            // Turn gravity off while on slope
            _rb.useGravity = !OnSlope();
        }

        private void SpeedControl()
        {
            // Limiting speed on slope
            if (OnSlope() && !_exitingSlope)
            {
                if (_rb.linearVelocity.magnitude > _moveSpeed)
                {
                    _rb.linearVelocity = _rb.linearVelocity.normalized * _moveSpeed;
                }
            } 
            // Limiting speed on ground or in air
            else
            {
                var flatVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
                if (flatVelocity.magnitude > _moveSpeed)
                {
                    Vector3 limitedVelocity = flatVelocity.normalized * _moveSpeed;
                    _rb.linearVelocity = new Vector3(limitedVelocity.x, _rb.linearVelocity.y, limitedVelocity.z);
                }
            }
        }

        private void HandleDrag()
        {
            if (_isGrounded)
            {
                _rb.linearDamping = groundDrag;
            }
            else
            {
                _rb.linearDamping = 0;
            }
        }
        
        private IEnumerator Jump()
        {
            _exitingSlope = true;

            // Reset y velocity
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            _rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            yield return new WaitForSeconds(jumpCooldown);
            
            ResetJump();
        }

        private void ResetJump()
        {
            _readyToJump = true;

            _exitingSlope = false;
        }

        private bool OnSlope()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit, playerHeight * 0.5f + 0.3f))
            {
                float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
                return angle < maxSlopeAngle && angle != 0;
            }

            return false;
        }

        private Vector3 GetSlopeMoveDirection()
        {
            return Vector3.ProjectOnPlane(_moveDirection, _slopeHit.normal).normalized;
        }
    }
    
    public enum MovementState
    {
        Walking,
        Sprinting,
        Crouching,
        Airborne
    }
}