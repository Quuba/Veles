using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    public static InputController Instance;
    public InputActions inputActions;
    public InputActions.PlayerActionMapActions playerActionMap;
    public InputActions.UIActions UIActionMap;

    private void Awake()
    {
        Instance = this;
        
        inputActions = new InputActions();
        playerActionMap = inputActions.PlayerActionMap;
        UIActionMap = inputActions.UI;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }
    
}
