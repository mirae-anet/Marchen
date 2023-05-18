using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkInGameMessages : NetworkBehaviour
{
    private InGameMessagesUIHandler inGameMessagesUIHandler;

    // Start is called before the first frame update
    void Start()
    {
        
    }



    //CHAT
    public void RPC_SendMessage(string userNickName, string message) 
    {

        RPC_InGameMessage($"<b>{userNickName }: </b> {message}");


    }


    // IN OUT
    public void SendInGameRPCMessage(string userNickName, string  message)
    {
        RPC_InGameMessage($"<b>{userNickName}</b> {message}");
    }


    //원래
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_InGameMessage(string message, RpcInfo info = default)
    {
        Debug.Log($"[RPC] InGameMessage {message}");
        if (inGameMessagesUIHandler == null)
            inGameMessagesUIHandler = NetworkPlayer.Local.localCameraHandler.GetComponentInChildren<InGameMessagesUIHandler>();
        
        if (inGameMessagesUIHandler != null)
            inGameMessagesUIHandler.OnGameMessageReceived(message);
    }
}
