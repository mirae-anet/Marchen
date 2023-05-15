using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class EnemySpawnHandler : NetworkBehaviour
{
    [Header("설정")]
    [SerializeField]
    private float delayTime;
    [SerializeField]
    private EnemyHPHandler enemyPrefab;
    [SerializeField]
    private Transform anchorPoint;
    public EnemySpawnHandler enemySpawnerPF;
    private bool spawnAble = true;
    public bool skipSettingStartValues = false;
    TickTimer respawnDelay = TickTimer.None;

    void Start()
    {
        if(skipSettingStartValues)
            return;
    }

    public override void FixedUpdateNetwork()
    {
        if(!skipSettingStartValues)
        {
            RespawnForHostMiragtion();
            skipSettingStartValues = true;
        }
    }

    public void RespawnForHostMiragtion()
    {
        NetworkRunner networkRunner = FindObjectOfType<NetworkRunner>();
        if(networkRunner != null)
        {
            if(!networkRunner.IsServer)
                return;
            
            if(Object.IsSceneObject)
            {
                Debug.Log("Respawn Spawner start");
                Transform tmp = anchorPoint.transform;
                // networkRunner.Despawn(Object);
                RPC_Despawn();
                networkRunner.Spawn(enemySpawnerPF, tmp.position, Quaternion.identity);
            }
            else
            {
                if(skipSettingStartValues)
                    return;

                SpawnEnemy(networkRunner);
                Debug.Log("first spawning");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!spawnAble)
            return;

        if (other.tag != "Player")
            return;

        NetworkRunner networkRunner = FindObjectOfType<NetworkRunner>();
        if(!networkRunner.IsServer)
            return;

        if(respawnDelay.ExpiredOrNotRunning(networkRunner))
            SpawnEnemy(networkRunner);
    }
    private void SpawnEnemy(NetworkRunner networkRunner)
    {
        EnemyHPHandler spawnedEnemy = networkRunner.Spawn(enemyPrefab, anchorPoint.position, Quaternion.identity);
        spawnedEnemy.enemySpawner = Object;
        Runner.SetPlayerObject(Object.StateAuthority, spawnedEnemy.Object);
        Debug.Log($"spawn enemy");
        spawnAble = false;
        gameObject.SetActive(false);
    }
    public void SetTimer()
    {
        NetworkRunner networkRunner = FindObjectOfType<NetworkRunner>();
        if(networkRunner != null && networkRunner.IsServer)
            respawnDelay = TickTimer.CreateFromSeconds(networkRunner, delayTime);
        spawnAble = true;
    }

    [Rpc (RpcSources.All, RpcTargets.All)]
    private void RPC_Despawn()
    {
        NetworkRunner networkRunner = FindObjectOfType<NetworkRunner>();
        if(networkRunner != null)
            networkRunner.Despawn(Object);
    }
}
