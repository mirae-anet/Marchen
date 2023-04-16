using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;

public class Spawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField]
    NetworkPlayer playerPrefab;
    CharacterInputHandler characterInputHandler;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if(runner.IsServer)
        {
            Debug.Log("OnPlayerJoined we are server. Spawning player");
            runner.Spawn(playerPrefab, Utils.GetRandomSpawnPoint(), Quaternion.identity, player);
        }
        else Debug.Log("OnPlayerJoined");
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("OnPlayerLeft");
    }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if(characterInputHandler == null && NetworkPlayer.Local != null)
        {
            characterInputHandler = NetworkPlayer.Local.GetComponent<CharacterInputHandler>();
        }
        if(characterInputHandler != null)
        {
            input.Set(characterInputHandler.GetNetworkInput());
        }
    }

    //not use
    public void OnConnectedToServer(NetworkRunner runner){Debug.Log("OnConnectServer");}
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason){Debug.Log("OnConnectFailed");}
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token){Debug.Log("OnConnectRequest");}
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data){Debug.Log("OnCustomAuthenticationResponse");}
    public void OnDisconnectedFromServer(NetworkRunner runner){Debug.Log("OnDisconnectedFromServer");}
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken){Debug.Log("OnHostMigration");}
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input){Debug.Log("OnInputMissing");}
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data){Debug.Log("OnReliableDataReceived");}
    public void OnSceneLoadDone(NetworkRunner runner){Debug.Log("OnSceneLoadDone");}
    public void OnSceneLoadStart(NetworkRunner runner){Debug.Log("OnSceneLoadStart");}
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList){Debug.Log("OnSessionListUpdated");}
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason){Debug.Log("OnShutdown");}
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message){Debug.Log("OnUserSimulationMessage");}
}
