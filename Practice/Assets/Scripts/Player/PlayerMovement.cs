using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviourPun
{
    public CharacterController controller;
    public Transform head;

    [Header("Speed")]
    public float walkSpeed = 4.5f;
    public float sprintSpeed = 6.5f;

    [Header("Jump / Gravity")]
    public float groundStickForce = -5f;
    public float gravity = -30f;
    public float jumpHeight = 1.1f;
    public float jumpBuffer = 0.12f;
    public float coyoteTime = 0.12f;

    [Header("Crouch")]
    public float standingHeight = 1.8f;
    public float crouchHeight = 1.2f;
    public float crouchTransitionSpeed = 12f;
    public float crouchEyeDrop = 0.35f;
    public LayerMask standUpObstructionMask = ~0;
    float _standHeight;
    Vector3 _standCenter;
    static readonly Collider[] _overlapHits = new Collider[16];
    int _playerRootInstanceId;

    public bool isGrounded;
    public bool isSprinting;
    public bool canMove = true;

    //runtime variables

    private PlayerInput input;
    private Vector3 _velocity;
    private float _currentSpeed;
    private float _timeSinceLeftGround;
    private float _timeSinceJumpPressed;

    void Awake()
    {
        if (!controller) controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        input = GetComponent<PlayerInput>();
    }

    public void ToggleMovement(bool value)
    {
        canMove = value;
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        
            Vector3 forward = head.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = head.right;
        right.y = 0f;
        right.Normalize();
        isGrounded = controller.isGrounded;
        Vector2 move = input.move;
        bool sprintHeld = input.sprintHeld;
        bool jumpPressed = input.jumpPressed;

        if (!canMove)
        {
            if (isGrounded && _velocity.y < 0f) _velocity.y = groundStickForce;
            else _velocity.y += gravity * Time.deltaTime;
            Vector3 pausedMotion = new Vector3(0f, _velocity.y, 0f);
            controller.Move(pausedMotion * Time.deltaTime);
            return;
        }

        if (jumpPressed) _timeSinceJumpPressed = 0f; else _timeSinceJumpPressed += Time.deltaTime;

        if (isGrounded) _timeSinceLeftGround = 0f; else _timeSinceLeftGround += Time.deltaTime;

        isSprinting = sprintHeld;


        float targetSpeed = isSprinting ? sprintSpeed : walkSpeed;
        targetSpeed *= Mathf.Clamp01(move.magnitude);

        if (targetSpeed > _currentSpeed)
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, 10 * Time.deltaTime);
        else
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, 10 * Time.deltaTime);

        Vector3 wishDir = (forward * move.y + right * move.x);
        if (wishDir.sqrMagnitude > 1f) wishDir.Normalize();
        Vector3 horizontal = wishDir * _currentSpeed;

        bool canCoyoteJump = _timeSinceLeftGround <= coyoteTime;
        bool bufferedJump = _timeSinceJumpPressed <= jumpBuffer;
        if (bufferedJump && (isGrounded || canCoyoteJump))
        {
            _timeSinceJumpPressed = jumpBuffer + 1f;
            _velocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight);
        }

        if (isGrounded && _velocity.y < 0f) _velocity.y = groundStickForce;
        else _velocity.y += gravity * Time.deltaTime;

        Vector3 motion = horizontal + new Vector3(0f, _velocity.y, 0f);
        controller.Move(motion * Time.deltaTime);
    }
}