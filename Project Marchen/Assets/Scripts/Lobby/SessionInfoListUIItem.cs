using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System;

//@brief 세션의 정보에 관련된 클래스

public class SessionInfoListUIItem : MonoBehaviour
{
    //@brief 세션의 이름, 세션에 접속한 유저 수, 입장버튼, 진행 표시
    public TextMeshProUGUI sessionNameText;
    public TextMeshProUGUI playerCountText;
    public Button joinButton;
    public TextMeshProUGUI isPlayMark;

    SessionInfo sessionInfo;
    public event Action<SessionInfo> onJoinSession;

    //@brief 세션의 정보 설정
    public void SetInformation(SessionInfo sessionInfo)
    {
        this.sessionInfo = sessionInfo;
        sessionNameText.text = sessionInfo.Name;

        // 입장가능 유저 수
        int MaxPlayer = 4;
        playerCountText.text = $"{sessionInfo.PlayerCount.ToString()}/{(MaxPlayer).ToString()}";

        //입장 버튼 활성화
        bool isJoinButtonActive = true;

        //입장 가능 수 조절/입장 닫기
        if (sessionInfo.PlayerCount >= MaxPlayer)
            isJoinButtonActive = false;

        if (sessionInfo.IsOpen == false)
        {
            isJoinButtonActive = false;
            isPlayMark.gameObject.SetActive(true);
        }

        joinButton.gameObject.SetActive(isJoinButtonActive);
    }

    //@brief 입장 버튼 클릭시 세션 호출
    public void OnClick()
    {
        onJoinSession?.Invoke(sessionInfo); // null이 아닌경우에 invoke호출
    }
}
