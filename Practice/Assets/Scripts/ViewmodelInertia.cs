using UnityEngine;

/// <summary>
/// Jitter-free viewmodel inertia (put on ARMS PREFAB ROOT under the camera's ArmsMount).
/// - Rot/pos lag from look delta (uses Mouse X/Y for stability)
/// - Pos lag from player velocity (uses CharacterController if assigned)
/// - Executes late to avoid fighting camera scripts
/// </summary>
[DefaultExecutionOrder(10000)]
public class ViewmodelInertia : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Usually the ArmsMount under the camera. If null, uses parent.")]
    public Transform follow;
    [Tooltip("Optional: use CharacterController.velocity for clean movement velocity.")]
    public CharacterController characterController;

    [Header("Movement ? Inertia (position)")]
    public float forwardLag = 0.02f;   // z drag vs forward/back velocity
    public float strafeLag = 0.02f;   // x drag vs left/right velocity
    public float verticalLag = 0.008f; // y drag vs up/down velocity

    [Header("Look ? Inertia")]
    public float lookRotLag = 1.1f;    // deg per unit of mouse delta
    public float lookPosLag = 0.004f;  // meters per unit of mouse delta

    [Header("Smoothing & Limits")]
    public float posSmooth = 14f;      // higher = snappier
    public float rotSmooth = 14f;
    public Vector3 maxPosOffset = new Vector3(0.06f, 0.04f, 0.06f); // meters
    public Vector3 maxRotOffset = new Vector3(6f, 6f, 4f);          // degrees
    [Tooltip("Ignore micro mouse noise under this magnitude.")]
    public float mouseDeadzone = 0.001f;
    [Tooltip("Clamp how fast offsets can change per frame to kill spikes.")]
    public float maxOffsetDeltaPerFrame = 0.08f;

    [Header("Runtime (read-only)")]
    public Vector3 baseLocalPosition;
    public Quaternion baseLocalRotation;

    // internal state
    Vector3 _posOffset, _rotOffset;
    Vector3 _posVel, _rotVel; // for SmoothDamp-like behavior
    Vector3 _lastRootPos;

    void Awake()
    {
        if (!follow) follow = transform.parent;
        baseLocalPosition = transform.localPosition;
        baseLocalRotation = transform.localRotation;

        var root = follow ? follow.root : transform.root;
        _lastRootPos = root.position;

        // try find CC automatically if not assigned
        if (!characterController)
            characterController = root.GetComponentInChildren<CharacterController>();
    }

    /// <summary>Call after equipping/changing base pose.</summary>
    public void RebaseNow()
    {
        baseLocalPosition = transform.localPosition;
        baseLocalRotation = transform.localRotation;
        _posOffset = _rotOffset = Vector3.zero;
        _posVel = _rotVel = Vector3.zero;
        var root = follow ? follow.root : transform.root;
        _lastRootPos = root.position;
    }

    void LateUpdate()
    {
        if (!follow) return;
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // --- 1) LOOK DELTAS (stable: use input, not quaternion diff)
        float mx = Input.GetAxisRaw("Mouse X");
        float my = Input.GetAxisRaw("Mouse Y");
        Vector2 mouse = new Vector2(mx, my);
        if (mouse.sqrMagnitude < mouseDeadzone * mouseDeadzone) mouse = Vector2.zero;

        // target rotation & position offsets from look
        Vector3 targetRotFromLook = new Vector3(-my * lookRotLag, -mx * lookRotLag, 0f);
        Vector3 targetPosFromLook = new Vector3(-mx * lookPosLag, +my * lookPosLag, 0f);

        // --- 2) MOVEMENT VELOCITY (prefer CC.velocity; else root delta)
        Vector3 worldVel;
        if (characterController)
        {
            worldVel = characterController.velocity; // already world m/s
        }
        else
        {
            var root = follow.root;
            worldVel = (root.position - _lastRootPos) / Mathf.Max(0.0001f, dt);
            _lastRootPos = root.position;
        }
        // Convert to camera/follow local horizontal space (ignore roll/pitch effect)
        Vector3 camFwd = follow.forward; camFwd.y = 0f; camFwd.Normalize();
        Vector3 camRight = follow.right; camRight.y = 0f; camRight.Normalize();
        Vector3 camUp = Vector3.up;

        float vx = Vector3.Dot(worldVel, camRight);
        float vz = Vector3.Dot(worldVel, camFwd);
        float vy = Vector3.Dot(worldVel, camUp);

        Vector3 targetPosFromMove = new Vector3(
            -vx * strafeLag,
            -vy * verticalLag,
            -vz * forwardLag
        );

        // --- 3) Compose targets
        Vector3 targetPos = targetPosFromLook + targetPosFromMove;
        Vector3 targetRot = targetRotFromLook; // you can add a tiny roll from strafe if desired

        // --- 4) SmoothDamp toward targets (critically damped)
        _posOffset = SmoothDampVec3(_posOffset, targetPos, ref _posVel, 1f / Mathf.Max(0.001f, posSmooth), dt);
        _rotOffset = SmoothDampVec3(_rotOffset, targetRot, ref _rotVel, 1f / Mathf.Max(0.001f, rotSmooth), dt);

        // --- 5) Clamp & anti-spike
        _posOffset = ClampVec3(_posOffset, -maxPosOffset, maxPosOffset);
        _rotOffset = ClampVec3(_rotOffset, -maxRotOffset, maxRotOffset);

        float maxStep = maxOffsetDeltaPerFrame;
        _posOffset = Vector3.MoveTowards(transform.localPosition - baseLocalPosition, _posOffset, maxStep);
        _rotOffset = Vector3.MoveTowards(LocalEulerFrom(transform.localRotation, baseLocalRotation), _rotOffset, maxStep * 60f);

        // --- 6) Apply (purely additive to base pose)
        transform.localPosition = baseLocalPosition + _posOffset;
        transform.localRotation = baseLocalRotation * Quaternion.Euler(_rotOffset);
    }

    // Helpers
    static Vector3 SmoothDampVec3(Vector3 current, Vector3 target, ref Vector3 velocity, float smoothTime, float dt)
    {
        // classic SmoothDamp but dt-explicit (prevents framerate jitter)
        float omega = 2f / Mathf.Max(0.0001f, smoothTime);
        float x = omega * dt;
        float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
        Vector3 change = current - target;
        Vector3 temp = (velocity + omega * change) * dt;
        velocity = (velocity - omega * temp) * exp;
        return target + (change + temp) * exp;
    }

    static Vector3 ClampVec3(Vector3 v, Vector3 min, Vector3 max)
    {
        return new Vector3(
            Mathf.Clamp(v.x, min.x, max.x),
            Mathf.Clamp(v.y, min.y, max.y),
            Mathf.Clamp(v.z, min.z, max.z)
        );
    }

    static Vector3 LocalEulerFrom(Quaternion current, Quaternion basis)
    {
        Quaternion rel = Quaternion.Inverse(basis) * current;
        Vector3 e = rel.eulerAngles;
        if (e.x > 180f) e.x -= 360f;
        if (e.y > 180f) e.y -= 360f;
        if (e.z > 180f) e.z -= 360f;
        return e;
    }
}