using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TemperarySceneTransition : MonoBehaviour
{
    [SerializeField] private InputAction menu;
    [SerializeField] private InputAction camp;
    [SerializeField] private InputAction gobi;
    [SerializeField] private InputAction indoor;
    
    private void Update()
    {
        if (menu.ReadValue<float>() > 0)
        {
            SceneManager.LoadScene(0);
        } else if (camp.ReadValue<float>() > 0)
        {
            SceneManager.LoadScene(1);
        }
        else if (gobi.ReadValue<float>() > 0)
        {
            SceneManager.LoadScene(2);
        } else if (indoor.ReadValue<float>() > 0)
        {
            SceneManager.LoadScene(3);
        }
    }

    private void OnEnable()
    {
        menu.Enable();
        camp.Enable();
        gobi.Enable();
        indoor.Enable();
    }

    private void OnDisable()
    {
        menu.Disable();
        camp.Disable();
        gobi.Disable();
        indoor.Disable();
    }
}
