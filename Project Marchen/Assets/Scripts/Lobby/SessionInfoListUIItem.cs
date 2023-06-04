using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System;

public class SessionInfoListUIItem : MonoBehaviour
{

    public TextMeshProUGUI sessionNameText;
    public TextMeshProUGUI playerCountText;
    public Button joinButton;

    SessionInfo sessionInfo;

    public event Action<SessionInfo> onJoinSession;

    public void SetInformation(SessionInfo sessionInfo)
    {
        this.sessionInfo = sessionInfo;
        sessionNameText.text = sessionInfo.Name;
        int MaxPlayer = 4;
        playerCountText.text = $"{sessionInfo.PlayerCount.ToString()}/{(MaxPlayer).ToString()}";

        bool isJoinButtonActive = true;

        if (sessionInfo.PlayerCount >= MaxPlayer)
            isJoinButtonActive = false;

        if(sessionInfo.IsOpen==false)
            isJoinButtonActive = false;

        joinButton.gameObject.SetActive(isJoinButtonActive);
    }

    public void OnClick()
    {
        onJoinSession?.Invoke(sessionInfo); // null이 아닌경우에 invoke호출
    }
}
