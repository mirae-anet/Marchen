using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Fusion;

/// @brief 채팅 및 게임메시지 기능
public class NetworkInGameMessages : NetworkBehaviour
{
    /// @brief 키보드의 return키 즉 엔터를 눌렀는지 여부
    private bool returnInput = false;
    /// @brief 채팅을 입력중인지 여부
    public bool isTyping = false;

    private InGameMessagesUIHandler inGameMessagesUIHandler;
    public TMP_InputField inputField;

    void Awake()
    {
    }

    void Start()
    {
        if(inputField != null)
            inputField.onEndEdit.AddListener(SendMyMessage);
    }
    private void Update() 
    {
        if(Object.HasInputAuthority)
        {
            if(Input.GetKeyDown(KeyCode.Return))
            {
                returnInput = !returnInput;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if(Object.HasInputAuthority)
            Chat();
    }

    /// @brief 채팅 입력
    public void Chat()
    {
        if(returnInput && !isTyping)
        {
            RPC_SetIsTyping(true);
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
            inputField.interactable = true;
            inputField.ActivateInputField();
            return;
        }
    }

    /// @breif 채팅 입력이 마무리되면 리스너에서 호출하는 콜백함수.
    /// @details 문자열를 전달하고 채팅창을 초기화 및 비활성화
    private void SendMyMessage(string newMessage)
    {
        if(!Object.HasInputAuthority)
            return;
        
        string inputText = inputField.text;
                
        if (string.IsNullOrEmpty(inputText))
        {
            inputField.DeactivateInputField(true);
            inputField.interactable = false;
            EventSystem.current.SetSelectedGameObject(null);
            RPC_SetIsTyping(false);
            return;
        }

        SendMessage(GameManager.instance.playerNickName, inputText);
        // 비우기
        inputField.text = string.Empty;
        // 비활성화
        inputField.DeactivateInputField(true);
        inputField.interactable = false;
        EventSystem.current.SetSelectedGameObject(null);
        RPC_SetIsTyping(false);
    }

    public void SendMessage(string userNickName, string message) 
    {
        RPC_InGameMessage($"<b>{userNickName }: </b> {message}");
    }

    public void SendInGameRPCMessage(string NickName, string  message)
    {
        RPC_InGameMessage($"<b>{NickName}</b> {message}");
    }

    /// @brief 메시지를 전송
    /// @see InGaeMessagesUIHandler.OnGameMessageReceived()
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_InGameMessage(string message, RpcInfo info = default)
    {
        Debug.Log($"[RPC] InGameMessage {message}");
        if (inGameMessagesUIHandler == null)
            inGameMessagesUIHandler = NetworkPlayer.Local.localCameraHandler.GetComponentInChildren<InGameMessagesUIHandler>();
        
        if (inGameMessagesUIHandler != null)
            inGameMessagesUIHandler.OnGameMessageReceived(message);
    }

    /// @brief 타이핑 중에는 아바타가 움직이지 않도록 다른 컴퓨터에 알리기 위해서 필요함.
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_SetIsTyping(bool isEnabled)
    {
        isTyping = isEnabled;
    }
}
