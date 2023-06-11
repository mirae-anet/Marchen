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

    /*추가*/
    private void Awake()
    {
        NetworkRunner networkRunnerInScene = FindObjectOfType<NetworkRunner>();

        if (networkRunnerInScene != null)
            networkRunner = networkRunnerInScene;

    }
    void Start()
    {
        if(networkRunner == null)
        {
            networkRunner = Instantiate(networkRunnerPrefab);
            networkRunner.name = "Network runner";

            // 자동으로 방 입장. 호스트, 클라이언트 자동 설정
            if(SceneManager.GetActiveScene().name != "Scene_1")//추가
            {
                var clientTask = InitializeNetworkRunner(networkRunner, GameMode.AutoHostOrClient,"TestSession" ,GameManager.instance.GetConnectionToken(), NetAddress.Any(), SceneManager.GetActiveScene().buildIndex, null);
            }

            Debug.Log($"Server NetworkRunner started.");

        }
    }

    public void StartHostMigration(HostMigrationToken hostMigrationToken)
    {
        networkRunner = Instantiate(networkRunnerPrefab);
        networkRunner.name = "Network runner - Migrated";

        var clientTask =InitializeNetworkRunnerHostMigration(networkRunner, hostMigrationToken);
        Debug.Log($"Host migration started.");
    }

    INetworkSceneManager GetSceneManager(NetworkRunner runner)
    {
        //check if there are any unity objs that we need to consider. 
        //Handel networked objects that already exits in the scene
        var sceneManager = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();
        if(sceneManager == null)
        {
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        return sceneManager;
   
    }

    protected virtual Task InitializeNetworkRunner(NetworkRunner runner, GameMode gameMode,String sessionName, byte[] connectionToken, NetAddress address, SceneRef scene, Action<NetworkRunner> initialized)
    {
        var sceneManager = GetSceneManager(runner);
        runner.ProvideInput = true;
        return runner.StartGame(new StartGameArgs{
            GameMode = gameMode,
            Address = address,
            Scene = scene,
            SessionName = sessionName,
            CustomLobbyName ="OurLobbyID",
            Initialized = initialized,
            SceneManager = sceneManager,
            ConnectionToken = connectionToken
            
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
            Debug.Log($"{resumeNetworkObject.name} in list");
            //Grab all the player objects, they have a NetworkRigidBody
            if(resumeNetworkObject.TryGetBehaviour<NetworkRigidbody>(out var oldRigidBody))
            {
                runner.Spawn(resumeNetworkObject, position: oldRigidBody.ReadPosition(), oldRigidBody.ReadRotation(), onBeforeSpawned: (runner, newNetworkObject) =>
                {
                    newNetworkObject.CopyStateFrom(resumeNetworkObject);

                    //Copy HP state
                    if(resumeNetworkObject.TryGetBehaviour<HPHandler>(out var oldHPHandler))
                    {
                        HPHandler newHPHandler = newNetworkObject.GetComponent<HPHandler>();
                        newHPHandler.CopyStateFrom(oldHPHandler);
                        newHPHandler.skipSettingStartValues = true;
                    }

                    if(resumeNetworkObject.TryGetBehaviour<CharacterRespawnHandler>(out var oldRespawnHandler))
                    {
                        CharacterRespawnHandler newRespawnHandler = newNetworkObject.GetComponent<CharacterRespawnHandler>();
                        newRespawnHandler.CopyStateFrom(oldRespawnHandler);
                        newRespawnHandler.skipSettingStartValues = true;
                    }

                    if(resumeNetworkObject.TryGetBehaviour<PlayerActionHandler>(out var oldActionHandler))
                    {
                        PlayerActionHandler newActionHandler = newNetworkObject.GetComponent<PlayerActionHandler>();
                        newActionHandler.CopyStateFrom(oldActionHandler);
                        newActionHandler.skipSettingStartValues = true;
                    }

                    //Map the connection token with the new Network player
                    if(resumeNetworkObject.TryGetBehaviour<NetworkPlayer>(out var oldNetworkPlayer))
                    {
                        //Store Player token for reconnection. Host migration 재접속에 사용할 Dictionary을 새로 작성.
                        FindObjectOfType<Spawner>().SetConnectionTokenMapping(oldNetworkPlayer.token, newNetworkObject.GetComponent<NetworkPlayer>());

                        
                    }

                });
            }
            //HeartQueen
            else if(resumeNetworkObject.TryGetBehaviour<HeartQueenHPHandler>(out var oldHeartQueen))
            {
                Transform oldTransform = oldHeartQueen.gameObject.transform;
                runner.Spawn(resumeNetworkObject, position: oldTransform.position, oldTransform.rotation, onBeforeSpawned: (runner, newNetworkObject) =>
                {
                    newNetworkObject.CopyStateFrom(resumeNetworkObject);
                    //Copy Enemy HP state
                    if(resumeNetworkObject.TryGetBehaviour<HeartQueenHPHandler>(out var oldHeartQueenHPHandler))
                    {
                        HeartQueenHPHandler newHPHandler = newNetworkObject.GetComponent<HeartQueenHPHandler>();
                        newHPHandler.CopyStateFrom(oldHeartQueenHPHandler);
                        newHPHandler.skipSettingStartValues = true;
                    }
                    if(resumeNetworkObject.TryGetBehaviour<TargetHandler>(out var oldTargetHandler))
                    {
                        TargetHandler newTargetHandler = newNetworkObject.GetComponent<TargetHandler>();
                        newTargetHandler.CopyStateFrom(oldTargetHandler);
                    }
                });
            }
            //enemy
            else if(resumeNetworkObject.TryGetBehaviour<NetworkEnemyController>(out var oldEnemy))
            {
                Transform oldTransform = oldEnemy.gameObject.transform;
                runner.Spawn(resumeNetworkObject, position: oldTransform.position, oldTransform.rotation, onBeforeSpawned: (runner, newNetworkObject) =>
                {
                    newNetworkObject.CopyStateFrom(resumeNetworkObject);
                    //Copy Enemy HP state
                    if(resumeNetworkObject.TryGetBehaviour<EnemyHPHandler>(out var oldHPHandler))
                    {
                        EnemyHPHandler newHPHandler = newNetworkObject.GetComponent<EnemyHPHandler>();
                        newHPHandler.CopyStateFrom(oldHPHandler);
                        newHPHandler.skipSettingStartValues = true;
                    }
                    if(resumeNetworkObject.TryGetBehaviour<TargetHandler>(out var oldTargetHandler))
                    {
                        TargetHandler newTargetHandler = newNetworkObject.GetComponent<TargetHandler>();
                        newTargetHandler.CopyStateFrom(oldTargetHandler);
                    }
                });
            }
            else if(resumeNetworkObject.TryGetBehaviour<CountSpawnHandler>(out var oldCountSpawner))
            {
                runner.Spawn(resumeNetworkObject, position: oldCountSpawner.transform.position, oldCountSpawner.transform.rotation, onBeforeSpawned: (runner, newNetworkObject) =>
                {
                    newNetworkObject.CopyStateFrom(resumeNetworkObject);
                    //Copy state
                    CountSpawnHandler newCountSpawner = newNetworkObject.GetComponent<CountSpawnHandler>();
                    newCountSpawner.CopyStateFrom(oldCountSpawner);
                    newCountSpawner.skipSettingStartValues = true;
                });
            }
            else if(resumeNetworkObject.TryGetBehaviour<SpawnHandler>(out var oldSpawner))
            {
                runner.Spawn(resumeNetworkObject, position: oldSpawner.transform.position, oldSpawner.transform.rotation, onBeforeSpawned: (runner, newNetworkObject) =>
                {
                    newNetworkObject.CopyStateFrom(resumeNetworkObject);
                    //Copy state
                    SpawnHandler newSpawner = newNetworkObject.GetComponent<SpawnHandler>();
                    newSpawner.CopyStateFrom(oldSpawner);
                    newSpawner.skipSettingStartValues = true;
                });
            }
            else if(resumeNetworkObject.TryGetBehaviour<BulletHandler>(out var oldBullet))
            {
                Transform oldBull = oldBullet.gameObject.transform;
                runner.Spawn(resumeNetworkObject, position: oldBull.position, oldBull.rotation, onBeforeSpawned: (runner, newNetworkObject) =>
                {
                    newNetworkObject.CopyStateFrom(resumeNetworkObject);
                    //Copy state
                    BulletHandler newBullet = newNetworkObject.GetComponent<BulletHandler>();
                    newBullet.CopyStateFrom(oldBullet);
                });
            }
            else if(resumeNetworkObject.TryGetBehaviour<GrenadeHandler>(out var oldGrenade))
            {
                Transform oldGren = oldGrenade.gameObject.transform;
                runner.Spawn(resumeNetworkObject, position: oldGren.position, oldGren.rotation, onBeforeSpawned: (runner, newNetworkObject) =>
                {
                    newNetworkObject.CopyStateFrom(resumeNetworkObject);
                    //Copy state
                    GrenadeHandler newGrenade = newNetworkObject.GetComponent<GrenadeHandler>();
                    newGrenade.CopyStateFrom(oldGrenade);
                });
            }
            else if(resumeNetworkObject.TryGetBehaviour<RocketHandler>(out var oldRocket))
            {
                Transform oldRoc = oldRocket.gameObject.transform;
                runner.Spawn(resumeNetworkObject, position: oldRoc.position, oldRoc.rotation, onBeforeSpawned: (runner, newNetworkObject) =>
                {
                    newNetworkObject.CopyStateFrom(resumeNetworkObject);
                    //Copy state
                    RocketHandler newRocket = newNetworkObject.GetComponent<RocketHandler>();
                    newRocket.CopyStateFrom(oldRocket);
                });
            }
            else if(resumeNetworkObject.TryGetBehaviour<DoorActionHandler>(out var oldDoor))
            {
                Transform oldDoorTrans = oldDoor.gameObject.transform;
                runner.Spawn(resumeNetworkObject, position: oldDoorTrans.position, oldDoorTrans.rotation, onBeforeSpawned: (runner, newNetworkObject) =>
                {
                    newNetworkObject.CopyStateFrom(resumeNetworkObject);
                    DoorActionHandler newDoor = newNetworkObject.GetComponent<DoorActionHandler>();
                    newDoor.CopyStateFrom(oldDoor);
                    newDoor.skipSettingStartValues = true;
                });
            }
            else if(resumeNetworkObject.TryGetBehaviour<ClockActionHandler>(out var oldClock))
            {
                Transform oldClockTrans = oldClock.gameObject.transform;
                runner.Spawn(resumeNetworkObject, position: oldClockTrans.position, oldClockTrans.rotation, onBeforeSpawned: (runner, newNetworkObject) =>
                {
                    newNetworkObject.CopyStateFrom(resumeNetworkObject);
                    ClockActionHandler newClock = newNetworkObject.GetComponent<ClockActionHandler>();
                    newClock.CopyStateFrom(oldClock);
                    newClock.skipSettingStartValues = true;
                });
            }
            else if(resumeNetworkObject.TryGetBehaviour<ShelfActionHandler>(out var oldShelf))
            {
                Transform oldShelfTrans = oldShelf.gameObject.transform;
                runner.Spawn(resumeNetworkObject, position: oldShelfTrans.position, oldShelfTrans.rotation, onBeforeSpawned: (runner, newNetworkObject) =>
                {
                    newNetworkObject.CopyStateFrom(resumeNetworkObject);
                    //Copy state
                    ShelfActionHandler newShelf = newNetworkObject.GetComponent<ShelfActionHandler>();
                    newShelf.CopyStateFrom(oldShelf);
                    newShelf.skipSettingStartValues = true;
                });
            }
            else if(resumeNetworkObject.TryGetBehaviour<MovingGroundAction>(out var oldMovingGround))
            {
                Transform oldMovingTrans = oldMovingGround.gameObject.transform;
                runner.Spawn(resumeNetworkObject, position: oldMovingTrans.position, oldMovingTrans.rotation, onBeforeSpawned: (runner, newNetworkObject) =>
                {
                    newNetworkObject.CopyStateFrom(resumeNetworkObject);
                    MovingGroundAction newMovingGround = newNetworkObject.GetComponent<MovingGroundAction>();
                    newMovingGround.CopyStateFrom(oldMovingGround);
                    newMovingGround.skipSettingStartValues = true;
                });
            }
            else
            {
                Transform oldOne = resumeNetworkObject.gameObject.transform;
                runner.Spawn(resumeNetworkObject, position: oldOne.position, oldOne.rotation, onBeforeSpawned: (runner, newNetworkObject) =>
                {
                    newNetworkObject.CopyStateFrom(resumeNetworkObject);
                    //Copy Enemy HP state
                });
            }
        }
        StartCoroutine(CleanUpHostMigrationCO());

        runner.SetActiveScene(SceneManager.GetActiveScene().buildIndex);

        Debug.Log($"HostMigrationResum completed");
    }

    IEnumerator CleanUpHostMigrationCO()
    {
        yield return new WaitForSeconds(3.0f);
        FindObjectOfType<Spawner>().OnHostMigrationCleanUp();
    }
    public void OnJoinLobby()
    {
        var clientTask = JoinLobby();
    }
    private async Task JoinLobby()
    {
        Debug.Log("JoinLobby started");

        string lobbyID = "OurLobbyID";

        var result = await networkRunner.JoinSessionLobby(SessionLobby.Custom, lobbyID);

        if (!result.Ok)
        {
            Debug.LogError($"Unable to join lobby {lobbyID}");

        }
        else
        {
            Debug.Log("JoinLobby ok");
        }
    }

    public void CreateGame(String sessionName, string sceneName)
    {
        Debug.Log($"Create ssession {sessionName} scene {sceneName} build Index {SceneUtility.GetBuildIndexByScenePath($"scenes/{sceneName}")}");

        var clientTask = InitializeNetworkRunner(networkRunner,GameMode.Host, sessionName, GameManager.instance.GetConnectionToken(),NetAddress.Any(),SceneUtility.GetBuildIndexByScenePath($"scenes/{sceneName}"),null);
        
    }


    public void JoinGame(SessionInfo sessionInfo)
    {
        
        Debug.Log($"Join session {sessionInfo.Name}");

        var clientTask = InitializeNetworkRunner(networkRunner, GameMode.Client, sessionInfo.Name, GameManager.instance.GetConnectionToken(), NetAddress.Any(), SceneManager.GetActiveScene().buildIndex,null);

    }

    public void OutSession()
    {
        NetworkRunner networkRunnerInScene = FindObjectOfType<NetworkRunner>();
        networkRunnerInScene.Shutdown();
    }

    public void quitSession()
    {
        Application.Quit();
    }
}
