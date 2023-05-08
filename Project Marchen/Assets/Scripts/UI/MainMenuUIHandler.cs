using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuUIHandler : MonoBehaviour
{

    [Header("Panels")]
    public GameObject playerDetailsPanel;
    public GameObject sessionBrowserPanel;
    public GameObject createSessionPanel;
    public GameObject statusPanel;

    [Header("Player settings")]
    public TMP_InputField playerNameInputField;

    [Header("New game session")]
    public TMP_InputField sessionNameInputField;

    void Start()
    {
        if(PlayerPrefs.HasKey("PlayerNickname"))
            playerNameInputField.text = PlayerPrefs.GetString("PlayerNickname");
    }
    

    void HideAllPanels()
    {
        playerDetailsPanel.SetActive(false);
        sessionBrowserPanel.SetActive(false);
        createSessionPanel.SetActive(false);
        statusPanel.SetActive(false);
}
    public void OnFindGameClicked()
    {
        //PlayerPrefs는 게임 설정이나 데이터를 저장하고 로드하는 데 사용되는 기본적인 저장소입니다. 
        //이 클래스를 사용하면 게임이 종료되거나 기기가 종료된 후에도 데이터를 유지할 수 있습니다.
        PlayerPrefs.SetString("PlayerNickname", playerNameInputField.text);
        PlayerPrefs.Save();

        GameManager.instance.playerNickName = playerNameInputField.text;

        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();

        networkRunnerHandler.OnJoinLobby();

        HideAllPanels(); 

        sessionBrowserPanel.gameObject.SetActive(true);

        FindObjectOfType<SessionListUIHandler>(true).OnLookingForGameSessions();
    }

    public void OnCreateNewGameClicked()
    {
        HideAllPanels();

        createSessionPanel.SetActive(true);
    }

    public void OnStartNewSessionClicked()
    {
        NetworkRunnerHandler networkRunnerHandler = FindAnyObjectByType<NetworkRunnerHandler>();

        networkRunnerHandler.CreateGame(sessionNameInputField.text, "TestScene(network)");

        HideAllPanels();

        statusPanel.gameObject.SetActive(true);
    }

    public void OnjoiningServer()
    {
        HideAllPanels();

        statusPanel.gameObject.SetActive(true);
    }

    public void GoBackSession()
    {
        HideAllPanels();

        sessionBrowserPanel.gameObject.SetActive(true);
    }
}
