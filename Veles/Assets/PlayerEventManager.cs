using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEventManager : MonoBehaviour
{
    public static PlayerEventManager Instance;

    [SerializeField]
    private PlayerMovement playerMovement;

    private void OnEnable()
    {
        if (playerMovement != null)
        {

            playerMovement.groundTouch += GroundTouchEvent;
            playerMovement.jumpEvent += JumpEvent;
            playerMovement.fallEvent += FallEvent;
            playerMovement.wallStuckEvent += WallStuckEvent;
        }
        else
        {
            throw new Exception("PlayerMovement not accessible");
        }
    }

    private void Start()
    {
        Instance = this;

    }


        private void OnDisable()
    {
        if (playerMovement != null)
        {
            playerMovement.groundTouch -= GroundTouchEvent;
            playerMovement.jumpEvent -= JumpEvent;
            playerMovement.wallStuckEvent -= FallEvent;
            playerMovement.wallStuckEvent -= WallStuckEvent;
        }
    }
    // public delegate void CustomEvent();

    public static event EventHandler onJump;

    public static event EventHandler onTouchGround;
    public static event EventHandler onStartFalling;
    public static event EventHandler onWallStuck;


    private void JumpEvent(object sender, EventArgs e)
    {
        onJump?.Invoke(this, EventArgs.Empty);
    }

    private void GroundTouchEvent(object sender, EventArgs e)
    {
        onTouchGround?.Invoke(this, EventArgs.Empty);
    }

    private void FallEvent(object sender, EventArgs e)
    {

        onStartFalling?.Invoke(this, EventArgs.Empty);
    }

    private void WallStuckEvent(object sender, EventArgs e)
    {
        onWallStuck?.Invoke(this, EventArgs.Empty);
    }
}
