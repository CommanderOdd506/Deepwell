using Photon.Pun;
using UnityEngine;

public class MouseLook : MonoBehaviourPun
{
    public Transform playerBody;
    public Transform head;

    public float sensitivity = 120f;
    public float pitchMin = -80f;
    public float pitchMax = 80f;

    private float _pitch;
    private Camera cam;

    public bool canMove = true;

    [Header("Recoil")]
    [SerializeField] private PlayerRecoil playerRecoil;

    void Start()
    {
        cam = GetComponentInChildren<Camera>();

        if (cam)
        {
            cam.enabled = photonView.IsMine;

            var listener = cam.GetComponent<AudioListener>();
            if (listener)
                listener.enabled = photonView.IsMine;
        }

        if (playerBody == null) playerBody = transform;
        if (head == null && cam) head = cam.transform;
        if (!photonView.IsMine)
        {
            Destroy(GetComponentInChildren<AudioListener>());
        }
        if (photonView.IsMine)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (playerRecoil == null)
            playerRecoil = GetComponentInChildren<PlayerRecoil>();
    }

    public void ToggleMovement(bool value)
    {
        canMove = value;

        if (!canMove)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        if (!canMove) return;

        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity;

        // Get recoil offset
        Vector2 recoilOffset = playerRecoil != null ? playerRecoil.RecoilRotation : Vector2.zero;

        // Horizontal rotation (body + horizontal recoil)
        playerBody.Rotate(Vector3.up * (mouseX + recoilOffset.y));

        // CHANGED: Apply recoil directly to pitch (snappy!)
        _pitch -= mouseY;
        _pitch += recoilOffset.x;  // Add recoil BEFORE clamping
        _pitch = Mathf.Clamp(_pitch, pitchMin, pitchMax);
        head.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }
}
