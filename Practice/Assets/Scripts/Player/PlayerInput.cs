using UnityEngine;
using Photon.Pun;

public class PlayerInput : MonoBehaviourPun
{
    [Header("Movement")]
    public Vector2 move;
    public bool sprintHeld;

    public bool interactPressed;

    public bool jumpPressed;

    [Header("Mouse") ]
    public bool firePressed;
    public bool aimPressed;

    [Header("Combat")]
    public bool reloadPressed;


    void Awake()
    {
        if (!photonView.IsMine)
        {
            enabled = false;
            return;
        }
    }

    void Update()
    {
        move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (move.sqrMagnitude > 1f) move.Normalize();

        sprintHeld = Input.GetKey(KeyCode.LeftShift);
        interactPressed = Input.GetKeyDown(KeyCode.F);
        reloadPressed = Input.GetKeyDown(KeyCode.R);
        jumpPressed = Input.GetKeyDown(KeyCode.Space);
        firePressed = Input.GetMouseButton(0);
        aimPressed = Input.GetMouseButton(1);
    }
}
