using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;

public class SessionListUIHandler : MonoBehaviour
{
    public TextMeshProUGUI statusText;
    public GameObject sessionItemListPrefab;
    public VerticalLayoutGroup verticalLayoutGroup;


    private void Awake()
    {
        ClearList();
    }
    public void ClearList()
    {
        //모든 레이아웃 자식 삭제
        foreach (Transform child in verticalLayoutGroup.transform)
        {
            Destroy(child.gameObject);
        }
        //메시지 상태 숨키기
        statusText.gameObject.SetActive(false);
    }

    public void AddToList(SessionInfo sessionInfo)
    {
        //리스트에 더하기
        SessionInfoListUIItem addedSessionInfoListUIItem = Instantiate(sessionItemListPrefab, verticalLayoutGroup.transform).GetComponent<SessionInfoListUIItem>();
        addedSessionInfoListUIItem.SetInformation(sessionInfo);

        //연결
        addedSessionInfoListUIItem.onJoinSession += AddedSessionInfoListUIItem_OnJoinSession;

    }

    private void AddedSessionInfoListUIItem_OnJoinSession(SessionInfo sessionInfo)
    {
        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();

        networkRunnerHandler.JoinGame(sessionInfo);

        MainMenuUIHandler mainMenuUIHandler = FindObjectOfType<MainMenuUIHandler>();
        mainMenuUIHandler.OnjoiningServer();
    }

    public void OnNoSessionsFound()
    {
        ClearList();
        statusText.text = "No game session found";
        statusText.gameObject.SetActive(true);
    }
    public void OnLookingForGameSessions()
    {
        ClearList();
        statusText.text = "Looking for game sessions";
        statusText.gameObject.SetActive(true);
    }
}
