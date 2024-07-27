using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    private PlayerControls _controls;
    private CharacterController _characterController;
    private Animator _animator;
    
    [Header("Movement info")]
    [SerializeField] private float walkSpeed;

    [SerializeField] private float runSpeed;
    private float _speed;
    private Vector3 _movementDirection;
    private float _verticalVelocity; // so that character falls if in the air
    private const float Gravity = 9.81f;

    [Header("Aim info")] 
    [SerializeField] private Transform aim;
    [SerializeField] private LayerMask aimLayerMask;
    private Vector3 _lookingDirection;
    private bool _isRunning;
    
    private Vector2 _moveInput;
    private Vector2 _aimInput;

    private void Awake()
    {
        // assign control to PlayerControl script when the game is run
        AssignInputEvents();
    }

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();

        _speed = walkSpeed;
    }

    private void Update()
    {
        ApplyMovement();
        AimTowardsMouse();
        AnimatorControllers();
    }

    private void Shoot()
    {
        _animator.SetTrigger("Fire");
    }

    private void AnimatorControllers()
    {
        bool playRunAnimation = _isRunning && _movementDirection.magnitude > 0;
        // delay in transitions time. Value > 1.0 character looks like its walking in tar
        float dampTime = 0.1f; 
        // Vector3.Dot tells us how close two vectors from each other and its value ranges from -1 to 1 
        float xVelocity = Vector3.Dot(_movementDirection.normalized, transform.right); // how much is character moving right/left
        float zVelocity = Vector3.Dot(_movementDirection.normalized, transform.forward); // how much is character moving forward/backward
        
        //"xVelocity" "yVelocity" are Parameters in Animator -> Parameters for the animation blend tree
        _animator.SetFloat("xVelocity", xVelocity, dampTime, Time.deltaTime);
        _animator.SetFloat("zVelocity", zVelocity, dampTime, Time.deltaTime);
        _animator.SetBool("isRunning", playRunAnimation);
    }

    private void AimTowardsMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(_aimInput);

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, aimLayerMask))
        {
            _lookingDirection = hitInfo.point - transform.position;
            _lookingDirection.y = 0f;
            _lookingDirection.Normalize();
            transform.forward = _lookingDirection;

            aim.position = new Vector3(hitInfo.point.x, transform.position.y, hitInfo.point.z);
        }
    }

    private void ApplyMovement()
    {
        _movementDirection = new Vector3(_moveInput.x, 0, _moveInput.y);
        ApplyGravity();

        if (_movementDirection.magnitude > 0)
        {
            _characterController.Move(_movementDirection * (Time.deltaTime * _speed));
        }
    }

    private void ApplyGravity()
    {
        if (!_characterController.isGrounded)
        {
            _verticalVelocity -= Gravity * Time.deltaTime;
            _movementDirection.y = _verticalVelocity;
        }
        else
        {
            _verticalVelocity -= .5f;
        }
    }

    #region New Input System
    private void AssignInputEvents()
    {
        _controls = new PlayerControls();

        _controls.Character.Fire.performed += context => Shoot();
        
        _controls.Character.Movement.performed += context => _moveInput = context.ReadValue<Vector2>();
        _controls.Character.Movement.canceled += context => _moveInput = Vector2.zero;

        _controls.Character.Aim.performed += context => _aimInput = context.ReadValue<Vector2>();
        _controls.Character.Aim.canceled += context => _aimInput = Vector2.zero;

        _controls.Character.Run.performed += context =>
        {
                _speed = runSpeed;
                _isRunning = true;
        };
        
        _controls.Character.Run.canceled += context =>
        {
            _speed = walkSpeed;
            _isRunning = false;
        };
    }
    
    // enable and disable controls
    private void OnEnable()
    {
        _controls.Enable();
    }

    private void OnDisable()
    {
        _controls.Disable();
    }
    #endregion
}
