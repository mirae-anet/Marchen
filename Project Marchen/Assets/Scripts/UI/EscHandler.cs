using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EscHandler : MonoBehaviour
{


    public void OutGame()
    {
        MainMenuUIHandler mainMenuUIHandler = FindObjectOfType<MainMenuUIHandler>();
        NetworkRunner networkRunner = FindObjectOfType<NetworkRunner>();
        networkRunner.Shutdown();
        SceneManager.LoadScene("Lobby");
    }


}
