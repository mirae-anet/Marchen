using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EscHandler : MonoBehaviour
{
    public GameObject escPanel;
    LocalCameraHandler localCameraHandler;
    CharacterInputHandler inputHandler;
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
        MainMenuUIHandler mainMenuUIHandler = FindObjectOfType<MainMenuUIHandler>();
        NetworkRunner networkRunner = FindObjectOfType<NetworkRunner>();

        networkRunner.Shutdown();
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

            if (escPanel.activeSelf)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                escPanel.SetActive(false);
                localCameraHandler.EnableCameraRotation(true);
                inputHandler.EnableinPut(true);

            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                escPanel.SetActive(true);
                localCameraHandler.EnableCameraRotation(false);
                inputHandler.EnableinPut(false);
            }
        }
    }

}
