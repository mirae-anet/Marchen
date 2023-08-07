using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;

/// @brief 플레이어 스폰 관련된 클래스
public class Spawner : MonoBehaviour, INetworkRunnerCallbacks
{
    /// @brief 플레이어 아바타 프리팹
    [SerializeField]
    NetworkPlayer playerPrefab;
    
    /// @brief connection token과 NetworkPlayer를 짝지어서 저장하는 맵
    /// @details Mapping between Token ID and Re-created Players. 재접속 시 자신의 아바타에 대한 권한을 재획득하기 위해서 사용.
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

    /// @brief Connection Token 가져오기.
    /// @details 자신의 접속이면 자신의 GameManager에 저장된 connection token을, remote player의 접속이면 NetworkRunner에 저장된 connection token을 반환.
    /// @return 비정상적인 상황에서는 0을 반환
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

    /// @brief Host migration 때 처음에 맵을 생성하기 위해서 필요함
    /// @see NetworkRunnerHandler.HostMigrationResume()
    public void SetConnectionTokenMapping(int token, NetworkPlayer networkPlayer)
    {
        mapTokenIDWithNetworkPlayer.Add(token, networkPlayer);
    }

    /// @brief 새로운 플레이어가 접속했을때 실행.
    /// @details Connection token을 바탕으로 재접속인지 확인. 재접속이면 기존의 아바타에 연결. 최초의 접속이면 아바타를 스폰.
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

    /// @brief 플레이어 퇴장 시
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("OnPlayerLeft");
    }

    /// @brief input 발생 시 CharacterInputHandler.GetNetworkInput()을 실행하도록 함.
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

    /// @brief 호스트마이그레이션 발생 시 최초로 실행되는 작업.
    /// @details NetworkRunner를 shutdown하고 NetworkRunnerHandler.StartHostMigration()을 실행.
    /// @see NetworkRunnerHandler.StartHostMigration()
    public async void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        Debug.Log("OnHostMigration");

        //shut down the current runner
        await runner.Shutdown(shutdownReason: ShutdownReason.HostMigration);

        //find the network runner handler and start the host migration
        FindObjectOfType<NetworkRunnerHandler>().StartHostMigration(hostMigrationToken);

    }


    /// @brief 호스트마이그레이션 단계에서 재연결되지 않은 플레이어 아바타 및 connectionToken 삭제.
    /// @see NetworkRunnerHandler.CleanUpHostMigrationCO
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

    /// @brief 접속할 수 있는 방(세션)이 업데이트 될때 호출되는 함수.
    /// @details SessionList의 정보를 갱신한다.
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
