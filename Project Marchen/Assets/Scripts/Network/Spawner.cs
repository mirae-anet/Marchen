using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;

public class Spawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField]
    NetworkPlayer playerPrefab;
    
    //Mapping between Token ID and Re-created Players
    public Dictionary<int, NetworkPlayer> mapTokenIDWithNetworkPlayer;

    //other components
    CharacterInputHandler characterInputHandler;
    //추가
    SessionListUIHandler sessionListUIHandler;

    private void Awake()
    {
        mapTokenIDWithNetworkPlayer = new Dictionary<int, NetworkPlayer>();

        sessionListUIHandler = FindObjectOfType<SessionListUIHandler>(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    int GetPlayerToken(NetworkRunner runner, PlayerRef player) 
    {
        if(runner.LocalPlayer == player) // 접속한 플레이어가 본인인 경우 
        {
            //Just use the local Player Connection Token
            return ConnectionTokenUtils.HashToken(GameManager.instance.GetConnectionToken());
        }    
        else // remote player
        {
            // Get the Connection Token stored when the Client connects to this Host
            var token = runner.GetPlayerConnectionToken(player); // remote player의 Connection token을 받아온다
            if(token!=null)
                return ConnectionTokenUtils.HashToken(token);
            Debug.LogError($"GetPlayerTokens returned invalid token");
            return 0;
        }
    }

    //Host migration 때 처음에 맵을 생성하기 위해서 필요함
    public void SetConnectionTokenMapping(int token, NetworkPlayer networkPlayer)
    {
        mapTokenIDWithNetworkPlayer.Add(token, networkPlayer);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if(runner.IsServer)
        {
            //Get the token for the player
            int playerToken = GetPlayerToken(runner, player);
            Debug.Log($"OnPlayerJoined we are server. Connection token {playerToken}");

            // Check if the token is already recorded by the server. 재접속
            if(mapTokenIDWithNetworkPlayer.TryGetValue(playerToken, out NetworkPlayer networkPlayer))
            {
                Debug.Log($"Found old connection token for token {playerToken}. Assigning controlls to that player");
                networkPlayer.GetComponent<NetworkObject>().AssignInputAuthority(player);
                networkPlayer.Spawned();
            }
            else 
            {
                Debug.Log($"Spawning new player for connection token {playerToken}");
                NetworkPlayer spawnedNetworkPlayer = runner.Spawn(playerPrefab, Utils.GetRandomSpawnPoint(), Quaternion.identity, player);

                //Store the mapping between playerToken and the spawned network player
                mapTokenIDWithNetworkPlayer[playerToken] = spawnedNetworkPlayer;

                //Store the token for the player
                spawnedNetworkPlayer.token = playerToken;
            }
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

    public async void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        Debug.Log("OnHostMigration");

        //shut down the current runner
        await runner.Shutdown(shutdownReason: ShutdownReason.HostMigration);

        //find the network runner handler and start the host migration
        FindObjectOfType<NetworkRunnerHandler>().StartHostMigration(hostMigrationToken);

    }

    public void OnHostMigrationCleanUp()
    {
        Debug.Log("Spawner OnHostMigrationCleanUp started");
        LinkedList<int> removeTokens = new LinkedList<int>(); 
        foreach(KeyValuePair<int, NetworkPlayer> entry in mapTokenIDWithNetworkPlayer)
        {
            NetworkObject networkObjectDictionary = entry.Value.GetComponent<NetworkObject>();
            if(networkObjectDictionary.InputAuthority.IsNone)
            {
                Debug.Log($"{Time.time} Found player that has not reconnected. Despawning {entry.Value.nickName}");
                networkObjectDictionary.Runner.Despawn(networkObjectDictionary);
                removeTokens.AddLast(entry.Key);

            }
        }
        for(int i=0; i < removeTokens.Count; i++)
        {
            mapTokenIDWithNetworkPlayer.Remove(removeTokens.Last.Value);
            removeTokens.RemoveLast();
        }


        Debug.Log("Spawner OnHostMigrationCleanUp completed");

    }

    //not use
    public void OnConnectedToServer(NetworkRunner runner){Debug.Log("OnConnectServer");}
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason){Debug.Log("OnConnectFailed");}
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token){Debug.Log("OnConnectRequest");}
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data){Debug.Log("OnCustomAuthenticationResponse");}
    public void OnDisconnectedFromServer(NetworkRunner runner){Debug.Log("OnDisconnectedFromServer");}
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        // Debug.Log("OnInputMissing");
    }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data){Debug.Log("OnReliableDataReceived");}
    public void OnSceneLoadDone(NetworkRunner runner){Debug.Log("OnSceneLoadDone");}
    public void OnSceneLoadStart(NetworkRunner runner){Debug.Log("OnSceneLoadStart");}
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        if (sessionListUIHandler == null)
            return;

        if(sessionList.Count==0)
        {
            sessionListUIHandler.OnNoSessionsFound();
        }else
        {
            sessionListUIHandler.ClearList();
            foreach(SessionInfo sessionInfo in sessionList)
            {
                sessionListUIHandler.AddToList(sessionInfo);
                Debug.Log($"Found session {sessionInfo.Name} playerCount {sessionInfo.PlayerCount}");
            }
        }
        
        
        Debug.Log("OnSessionListUpdated");
    }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason){Debug.Log("OnShutdown");}
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message){Debug.Log("OnUserSimulationMessage");}
}
