using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;

/*
 
 @brief 세션목록 관련 클래스 
 @see Spawner

 */
public class SessionListUIHandler : MonoBehaviour
{
    // @brief 세션 상태 표시, 세션과 방 레이아웃 변수
    public TextMeshProUGUI statusText;
    public GameObject sessionItemListPrefab;
    public VerticalLayoutGroup verticalLayoutGroup;

    // @brief 시작시 리스트 초기화
    private void Awake()
    {
        ClearList();
    }
    // @brief 리스트 초기화
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

    // @brief 리스트에 세션 추가
    public void AddToList(SessionInfo sessionInfo)
    {
        //리스트에 더하기
        SessionInfoListUIItem addedSessionInfoListUIItem = Instantiate(sessionItemListPrefab, verticalLayoutGroup.transform).GetComponent<SessionInfoListUIItem>();
        addedSessionInfoListUIItem.SetInformation(sessionInfo);

        // AddedSessionInfoListUIItem_OnJoinSession를 세션 호출 이벤트와 연결
        addedSessionInfoListUIItem.onJoinSession += AddedSessionInfoListUIItem_OnJoinSession;

    }

    // @brief 세션 호출
    private void AddedSessionInfoListUIItem_OnJoinSession(SessionInfo sessionInfo)
    {
        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();

        networkRunnerHandler.JoinGame(sessionInfo);

        MainMenuUIHandler mainMenuUIHandler = FindObjectOfType<MainMenuUIHandler>();
        mainMenuUIHandler.OnjoiningServer();
    }

    // @brief 세션이 없는 경우 초기화 후 상태 표기
    public void OnNoSessionsFound()
    {
        ClearList();
        statusText.text = "No game session found";
        statusText.gameObject.SetActive(true);
    }

    // @brief 세션을 찾는중이라는 상태 표시
    public void OnLookingForGameSessions()
    {
        ClearList();
        statusText.text = "Looking for game sessions";
        statusText.gameObject.SetActive(true);
    }
}
