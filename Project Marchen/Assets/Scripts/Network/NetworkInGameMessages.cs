using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Fusion;

public class NetworkInGameMessages : NetworkBehaviour
{
    private bool returnInput = false;
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

    private void SendMyMessage(string newMessage)
    {
        if(!Object.HasInputAuthority)
            return;
        
        string inputText = inputField.text;
                
        if (string.IsNullOrEmpty(inputText))
        {
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

    //CHAT
    public void SendMessage(string userNickName, string message) 
    {
        RPC_InGameMessage($"<b>{userNickName }: </b> {message}");
    }

    public void SendInGameRPCMessage(string NickName, string  message)
    {
        RPC_InGameMessage($"<b>{NickName}</b> {message}");
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_InGameMessage(string message, RpcInfo info = default)
    {
        Debug.Log($"[RPC] InGameMessage {message}");
        if (inGameMessagesUIHandler == null)
            inGameMessagesUIHandler = NetworkPlayer.Local.localCameraHandler.GetComponentInChildren<InGameMessagesUIHandler>();
        
        if (inGameMessagesUIHandler != null)
            inGameMessagesUIHandler.OnGameMessageReceived(message);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_SetIsTyping(bool isEnabled)
    {
        isTyping = isEnabled;
    }
}
