using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class AnimationController : MonoBehaviourPun
{
    public CharacterController controller;
    public PlayerInput input;
    public Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();

        if (!photonView.IsMine) return;

        controller = GetComponentInParent<CharacterController>();
        input = GetComponentInParent<PlayerInput>();
    }

    bool lastJump;

    void Update()
    {
        if (!photonView.IsMine) return;
        Vector2 move = input.move;
        bool jumpPressed = input.jumpPressed;
        bool isGrounded = controller.isGrounded;

        animator.SetBool("isMoving", move.sqrMagnitude > 0.01f);
        animator.SetBool("isGrounded", isGrounded);

        bool justPressed = jumpPressed && !lastJump;
        lastJump = jumpPressed;

        if (justPressed && isGrounded)
        {
            animator.ResetTrigger("Jump");
            animator.SetTrigger("Jump");
        }

        move.Normalize();
        animator.SetFloat("MoveX", move.x);
        animator.SetFloat("MoveY", move.y);
    }

    public void SetWeaponType(WeaponType type)
    {
        animator.SetInteger("WeaponType", (int)type);
    }

    public void SetWeaponState(int state)
    {
        animator.SetInteger("WeaponState", state);
    }
}
