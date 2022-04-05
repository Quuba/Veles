using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public bool isPaused = false;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        InputController.Instance.UIActionMap.TogglePause.performed += TogglePause;
    }

    private void OnDisable()
    {
        InputController.Instance.UIActionMap.TogglePause.performed -= TogglePause;
    }
    
    private void TogglePause(InputAction.CallbackContext obj)
    {
        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }


    public void Pause()
    {
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        isPaused = false;
    }
}
