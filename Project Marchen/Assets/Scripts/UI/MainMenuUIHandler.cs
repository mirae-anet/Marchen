using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
/*
@brief 게임 선택창에 관련된 클래스 
 */
public class MainMenuUIHandler : MonoBehaviour
{
    // @brief 닉네임 입력/세션/방생성/상태창을 나타내는 변수
    [Header("Panels")]
    public GameObject playerDetailsPanel;
    public GameObject sessionBrowserPanel;
    public GameObject createSessionPanel;
    public GameObject statusPanel;

    // @brief 닉네임 입력창
    [Header("Player settings")]
    public TMP_InputField playerNameInputField;

    //@brief 세션명 입력창
    [Header("New game session")]
    public TMP_InputField sessionNameInputField;

    // @brief 플레이어가 예전에 입력한 닉네임으로 초기화
    void Start()
    {
        if(PlayerPrefs.HasKey("PlayerNickname"))
            playerNameInputField.text = PlayerPrefs.GetString("PlayerNickname");
    }

    // @brief 전체 패널 비활성화
    void HideAllPanels()
    {
        playerDetailsPanel.SetActive(false);
        sessionBrowserPanel.SetActive(false);
        createSessionPanel.SetActive(false);
        statusPanel.SetActive(false);
    }

    // @brief 세션 리스트(로비) 접근 및 표시
    public void OnFindGameClicked()
    {
        //PlayerPrefs는 게임 설정이나 데이터를 저장하고 로드하는 데 사용되는 기본적인 저장소입니다. 
        //이 클래스를 사용하면 게임이 종료되거나 기기가 종료된 후에도 데이터를 유지할 수 있습니다.
        PlayerPrefs.SetString("PlayerNickname", playerNameInputField.text);
        PlayerPrefs.Save();

        GameManager.instance.playerNickName = playerNameInputField.text;

        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        //로비 접속
        networkRunnerHandler.OnJoinLobby();

        HideAllPanels(); 

        sessionBrowserPanel.gameObject.SetActive(true);

        FindObjectOfType<SessionListUIHandler>(true).OnLookingForGameSessions();
    }

    // @brief 세션 생성창 표시 
    public void OnCreateNewGameClicked()
    {
        HideAllPanels();

        createSessionPanel.SetActive(true);
    }

    //@brief 새로운 세션 생성
    public void OnStartNewSessionClicked()
    {
        //네트워크 핸들러를 찾아 게임 시작
        NetworkRunnerHandler networkRunnerHandler = FindAnyObjectByType<NetworkRunnerHandler>();
        networkRunnerHandler.CreateGame(sessionNameInputField.text,"1_Final/Scene_2");
        
        HideAllPanels();

        statusPanel.gameObject.SetActive(true);
    }

    //@brief 게임 접속중 표시
    public void OnjoiningServer()
    {
        HideAllPanels();

        statusPanel.gameObject.SetActive(true);
    }

    // @brief 세션 생성창에서 뒤로가기
    public void GoBackSession()
    {
        HideAllPanels();

        sessionBrowserPanel.gameObject.SetActive(true);
    }
}
