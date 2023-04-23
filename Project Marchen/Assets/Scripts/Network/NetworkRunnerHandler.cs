using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using UnityEngine;

public class NetworkRunnerHandler : MonoBehaviour
{
    public NetworkRunner networkRunnerPrefab;

    NetworkRunner networkRunner;
    void Start()
    {
        networkRunner = Instantiate(networkRunnerPrefab);
        networkRunner.name = "Network runner";

        // 자동으로 방 입장. 호스트, 클라이언트 자동 설정
        var clientTask = InitializeNetworkRunner(networkRunner, GameMode.AutoHostOrClient, GameManager.instance.GetConnectionToken(), NetAddress.Any(), SceneManager.GetActiveScene().buildIndex, null);

        Debug.Log($"Server NetworkRunner started.");
    }

    public void StartHostMigration(HostMigrationToken hostMigrationToken)
    {
        networkRunner = Instantiate(networkRunnerPrefab);
        networkRunner.name = "Network runner - Migrated";

        var clientTask = InitializeNetworkRunnerHostMigration(networkRunner, hostMigrationToken);

        Debug.Log($"Host migration started.");
    }

    INetworkSceneManager GetSceneManager(NetworkRunner runner)
    {
        //check if there are any unity objs that we need to consider. 
        var sceneManager = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();
        if(sceneManager == null)
        {
            //Handel networked objects that already exits in the scene
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        return sceneManager;
    }

    protected virtual Task InitializeNetworkRunner(NetworkRunner runner, GameMode gameMode, byte[] connectionToken, NetAddress address, SceneRef scene, Action<NetworkRunner> initialized)
    {
        var sceneManager = GetSceneManager(runner);

        runner.ProvideInput = true;
        
        return runner.StartGame(new StartGameArgs{
            GameMode = gameMode,
            Address = address,
            Scene = scene,
            SessionName = "TestRoom",
            Initialized = initialized,
            SceneManager = sceneManager,
            ConnectionToken = connectionToken,
        });
    }

    protected virtual Task InitializeNetworkRunnerHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        var sceneManager = GetSceneManager(runner);

        runner.ProvideInput = true;
        
        return runner.StartGame(new StartGameArgs{
            // GameMode = gameMode,
            // Address = address,
            // Scene = scene,
            // SessionName = "TestRoom",
            // Initialized = initialized,
            SceneManager = sceneManager,
            HostMigrationToken = hostMigrationToken, //contain all necessary info to restart the runner.
            HostMigrationResume = HostMigrationResume, //this will be invoked to resume the simulation
            ConnectionToken = GameManager.instance.GetConnectionToken()
        });
    }

    //resume simulation
    void HostMigrationResume(NetworkRunner runner)
    {
        Debug.Log($"HostMigrationResum started");

        //Get a reference for for each Network object from the old host
        foreach(var resumeNetworkObject in runner.GetResumeSnapshotNetworkObjects())
        {
            //Grab all the player objects, they have a NetworkCharacterControllerPrototypeCustom
            if(resumeNetworkObject.TryGetBehaviour<NetworkCharacterControllerPrototypeCustom>(out var characterController))
            {
                runner.Spawn(resumeNetworkObject, position: characterController.ReadPosition(), characterController.ReadRotation(), onBeforeSpawned: (runner, newNetworkObject) =>
                {
                    newNetworkObject.CopyStateFrom(resumeNetworkObject);

                    //Map the connection token with the new Network player
                    if(resumeNetworkObject.TryGetBehaviour<NetworkPlayer>(out var oldNetworkPlayer))
                    {
                        //Store Player token for reconnection. Host migration 재접속에 사용할 Dictionary을 작성.
                        FindObjectOfType<Spawner>().SetConnectionTokenMapping(oldNetworkPlayer.token, newNetworkObject.GetComponent<NetworkPlayer>());
                    }
                });
            }
        }

        Debug.Log($"HostMigrationResum completed");

    }
}
