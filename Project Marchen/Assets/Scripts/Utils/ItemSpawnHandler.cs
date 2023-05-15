using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ItemSpawnHandler : NetworkBehaviour
{
    [Header("설정")]
    [SerializeField]
    private float delayTime;
    [SerializeField]
    private HeartHandler itemPrefab;
    [SerializeField]
    private Transform anchorPoint;
    public ItemSpawnHandler itemSpawnerPF;
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
                networkRunner.Spawn(itemSpawnerPF, tmp.position, Quaternion.identity);
            }
            else
            {
                if(skipSettingStartValues)
                    return;

                SpawnItem(networkRunner);
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
            SpawnItem(networkRunner);
    }
    private void SpawnItem(NetworkRunner networkRunner)
    {
        HeartHandler spawnedItem = networkRunner.Spawn(itemPrefab, anchorPoint.position, Quaternion.identity);
        spawnedItem.itemSpawner = Object;
        Debug.Log($"spawn item");
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
