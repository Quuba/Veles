using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool isMoving;
    // [SerializeField] private bool isfalling;

    [SerializeField] private Vector2 movingDir;

    //TODO: change set trigger methods to index based search
    private void OnEnable()
    {
        PlayerEventManager.onJump += Jump;
        PlayerEventManager.onTouchGround += GroundTouch;
        PlayerEventManager.onStartFalling += StartFalling;
        PlayerEventManager.onWallStuck += WallStuck;
    }

    private void OnDisable()
    {
        PlayerEventManager.onJump -= Jump;
        PlayerEventManager.onTouchGround -= GroundTouch;
        PlayerEventManager.onStartFalling -= StartFalling;
        PlayerEventManager.onWallStuck -= WallStuck;
    }

    private void Jump(object sender, EventArgs e)
    {
        Debug.Log("Jump");
        SetIsGrounded(false);
        animator.SetTrigger("JumpTrigger");
    }

    private void GroundTouch(object sender, EventArgs e)
    {
        SetIsGrounded(true);
    }

    private void StartFalling(object sender, EventArgs e)
    {
        SetIsGrounded(false);
        animator.SetTrigger("FallTrigger");
    }

    private void WallStuck(object sender, EventArgs e)
    {
        animator.SetTrigger("WallStuckTrigger");
    }

    private void SetIsGrounded(bool state)
    {
        animator.SetBool("IsGrounded", state);
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerMovement.CanReceiveInput) return; // Todo: can use event here
        
        var movingDir = playerMovement.LastDirection;

        
        if (movingDir.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true;
        }

    }
}