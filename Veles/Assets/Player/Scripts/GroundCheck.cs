using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    //Unused script
    
    [SerializeField] private PlayerMovement _playerMovement;
    private bool isGrounded = false;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            isGrounded = true;
            if (!_playerMovement.CanJump)
            {
                // _playerMovement.OnGroundTouch();
            }
        }
        
        
    }

    // private void OnTriggerStay2D(Collider2D other)
    // {
    //     if (other.CompareTag("Ground"))
    //     {
    //         isGrounded = true;
    //         if (!_playerMovement.CanJump)
    //         {
    //             _playerMovement.onGroundTouch();
    //         }
    //     }
    //     
    //     
    // }

    private void OnTriggerExit2D(Collider2D other)
    {
        
        if (other.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
