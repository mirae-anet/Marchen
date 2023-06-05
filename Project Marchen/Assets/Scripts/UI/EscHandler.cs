using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EscHandler : NetworkBehaviour
{
    public GameObject escPanel;
    LocalCameraHandler localCameraHandler;
    CharacterInputHandler inputHandler;
    private ReadyUIHandler readyUIHandler;

    private void Awake()
    {
        localCameraHandler = GetComponentInParent<LocalCameraHandler>();
        inputHandler = GetComponentInParent<CharacterInputHandler>();
    }

    private void Update()
    {
        EscMenu();
    }
    public void ExitRoom()
    {
        Runner.Shutdown();
        SceneManager.LoadScene("Lobby");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void EscMenu()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //카메라
            readyUIHandler = FindObjectOfType<ReadyUIHandler>();
            if (escPanel.activeSelf)
            {

                if (readyUIHandler != null) // 다른 UI가 켜져있으면
                {
                    escPanel.SetActive(false);
                    localCameraHandler.EnableRotationEsc(true);
                    return;
                }
                else
                {
                    escPanel.SetActive(false);
                    localCameraHandler.EnableRotationEsc(true);
                    inputHandler.EnableinPut(true);
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
            else
            {
                if (readyUIHandler != null)
                {
                    escPanel.SetActive(true);
                    localCameraHandler.EnableRotationEsc(false);
                    return;
                }
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                escPanel.SetActive(true);
                localCameraHandler.EnableRotationEsc(false);
                inputHandler.EnableinPut(false);
            }
        }
    }
    
    public bool ActiveEsc()
    {
        return escPanel.activeSelf;
    }

}
