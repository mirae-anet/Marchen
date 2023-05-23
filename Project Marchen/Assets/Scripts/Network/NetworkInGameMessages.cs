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
    public void SendInGameRPCMessage(string NickName, string  message)
    {
        RPC_InGameMessage($"<b>{NickName}</b> {message}");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_InGameMessage(string message, RpcInfo info = default)
    {
        Debug.Log($"[RPC] InGameMessage {message}");

        if(inGameMessagesUIHandler == null)
            inGameMessagesUIHandler = NetworkPlayer.Local.localCameraHandler.GetComponentInChildren<InGameMessagesUIHandler>();

        if(inGameMessagesUIHandler != null)
            inGameMessagesUIHandler.OnGameMessageReceived(message);
    }
}
