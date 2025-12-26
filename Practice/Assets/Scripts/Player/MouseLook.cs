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
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity;

        // Horizontal rotation
        playerBody.Rotate(Vector3.up * mouseX);

        // Vertical rotation
        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, pitchMin, pitchMax);
        head.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }
}
