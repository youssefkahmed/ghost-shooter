using System.Linq;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Ghost.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private Transform headFollowTarget;
        [SerializeField] private AudioSource footstepsAudioSource;
        
        [Header("Values")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeedMultiplier = 2f;
        [SerializeField] private float sprintTransitionSpeed = 5f;
        
        [SerializeField] private float turnSensitivity;
        
        [SerializeField] private float gravity = 9.81f;
        [SerializeField] private float jumpHeight;

        [Header("Footsteps Settings")]
        [SerializeField] private LayerMask terrainLayerMask;
        [SerializeField] private float stepInterval = 1f;
        
        [Header("Camera Bob Settings")]
        [SerializeField] private float bobFrequency = 1f;
        [SerializeField] private float bobAmplitude = 1f;
        
        [Header("SFX")]
        [SerializeField] private AudioClip[] groundFootsteps;
        [SerializeField] private AudioClip[] grassFootsteps;
        [SerializeField] private AudioClip[] gravelFootsteps;
        
        private PlayerControls _playerControls;
        private CharacterController _characterController;
        
        private Vector3 _moveInput;
        private float _currentSpeed;
        private float _currentSpeedMultiplier;
        private float _verticalVelocity;
        
        private Vector2 _turnInput;
        private float _xRotation;

        private bool _performedJump;

        private float _nextStepTimer;

        private CinemachineBasicMultiChannelPerlin _cinemachineNoise;
        
        private void Awake()
        {
            _playerControls ??= new PlayerControls();
            _playerControls.Enable();

            _characterController = GetComponent<CharacterController>();
            Assert.IsNotNull(_characterController);

            _cinemachineNoise = cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
            
            _currentSpeedMultiplier = 1;
            _currentSpeed = walkSpeed;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnEnable()
        {
            InitializeInputListeners();
        }

        private void Update()
        {
            // Turning
            ApplyHeadTurning();

            // Moving
            ApplyMovement();

            PlayFootstepsSound();
        }

        private void LateUpdate()
        {
            ApplyCameraBob();
        }

        private void InitializeInputListeners()
        {
            _playerControls.Movement.Move.performed += ctx => SetMoveInput(ctx.ReadValue<Vector2>());
            _playerControls.Movement.Move.canceled += _ => SetMoveInput(Vector2.zero);
            
            _playerControls.Movement.Turn.performed += ctx => SetTurnInput(ctx.ReadValue<Vector2>());
            _playerControls.Movement.Turn.canceled += _ => SetTurnInput(Vector2.zero);
            
            _playerControls.Movement.Jump.performed += _ => Jump();
            
            _playerControls.Movement.Sprint.performed += _ => SetSprintMultiplier();
            _playerControls.Movement.Sprint.canceled += _ => SetWalkMultiplier();
        }

        #region Movement Functions

        private void ApplyMovement()
        {
            var camTransform = cinemachineCamera.transform;
            var move = _moveInput.x * camTransform.right + _moveInput.z * camTransform.forward;
            move.y = CalculateVerticalVelocity();
            _currentSpeed = Mathf.Lerp(_currentSpeed, walkSpeed * _currentSpeedMultiplier, sprintTransitionSpeed * Time.deltaTime);
            _characterController.Move(_currentSpeed * Time.deltaTime * move);
        }

        private float CalculateVerticalVelocity()
        {
            if (_characterController.isGrounded)
            {
                _verticalVelocity = -1f;
                if (!_performedJump) return _verticalVelocity;
                
                _verticalVelocity = Mathf.Sqrt(jumpHeight * gravity * 2);
                _performedJump = false;
            }
            else
            {
                _verticalVelocity -= gravity * Time.deltaTime;
            }

            return _verticalVelocity;
        }

        #endregion

        #region Camera & Rotaion Functions

        private void ApplyHeadTurning()
        {
            var lookX = turnSensitivity * Time.deltaTime * _turnInput.x;
            var lookY = turnSensitivity * Time.deltaTime * _turnInput.y;
            
            _xRotation = Mathf.Clamp(_xRotation - lookY, -90, 90);
            
            headFollowTarget.localRotation = Quaternion.Euler(_xRotation, 0, 0);
            transform.Rotate(Vector3.up * lookX);
        }
        
        private void ApplyCameraBob()
        {
            if (_characterController.isGrounded && _characterController.velocity.magnitude > 0.1f)
            {
                _cinemachineNoise.FrequencyGain = bobFrequency * _currentSpeedMultiplier;
                _cinemachineNoise.AmplitudeGain = bobAmplitude * _currentSpeedMultiplier;
            }
            else
            {
                _cinemachineNoise.FrequencyGain = 0;
                _cinemachineNoise.AmplitudeGain = 0;
            }
        }

        #endregion

        #region SFX Functions

        private void PlayFootstepsSound()
        {
            if (!_characterController.isGrounded || _characterController.velocity.magnitude <= 0.1f) return;

            if (Time.time < _nextStepTimer) return;
            
            var clips = GetFootstepsClips();
            if (!clips.Any()) return;

            var clip = clips[Random.Range(0, clips.Length)];
            footstepsAudioSource.PlayOneShot(clip);

            _nextStepTimer = Time.time + stepInterval / _currentSpeedMultiplier;
        }

        private AudioClip[] GetFootstepsClips()
        {
            if (!Physics.Raycast(transform.position, -Vector3.up, out var hit, 1.5f, terrainLayerMask))
            {
                return groundFootsteps;
            }

            return hit.collider.tag switch
            {
                "Grass" => grassFootsteps,
                "Gravel" => gravelFootsteps,
                _ => groundFootsteps
            };
        }

        #endregion
        
        #region Input Actions

        private void SetMoveInput(Vector2 moveVector)
        {
            _moveInput = new Vector3(moveVector.x, 0, moveVector.y);
        }
        
        private void SetTurnInput(Vector2 turnVector)
        {
            _turnInput = turnVector;
        }
        
        private void Jump()
        {
            _performedJump = true;
        }
        
        private void SetSprintMultiplier()
        {
            _currentSpeedMultiplier = sprintSpeedMultiplier;
        }
        
        private void SetWalkMultiplier()
        {
            _currentSpeedMultiplier = 1;
        }
        
        #endregion
    }
}
