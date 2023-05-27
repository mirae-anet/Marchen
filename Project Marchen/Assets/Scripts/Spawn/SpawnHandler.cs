using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class SpawnHandler : NetworkBehaviour
{
    [Header("설정")]
    [SerializeField]
    private float delayTime;
    public NetworkBehaviour prefab; //EnemyHPHandler, HeartHandler
    public Transform anchorPoint;
    private bool spawnAble = true;
    public bool skipSettingStartValues = false;
    TickTimer respawnDelay = TickTimer.None;

    void Start()
    {
        if(!Object.HasStateAuthority)
            return;
        
        if(!skipSettingStartValues)
        {
            // Spawn();
            // skipSettingStartValues = true;
            // Debug.Log("first spawning");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!spawnAble)
            return;

        if (other.tag != "Player")
            return;

        if(!Object.HasStateAuthority)
            return;

        if(respawnDelay.ExpiredOrNotRunning(Runner))
            Spawn();
    }
    private void Spawn()
    {
        NetworkBehaviour spawned = Runner.Spawn(prefab, anchorPoint.position, Quaternion.identity);

        if(spawned.TryGetComponent<EnemyHPHandler>(out EnemyHPHandler enemyHPHandler))
            enemyHPHandler.Spawner = Object;
        if(spawned.TryGetComponent<HeartHandler>(out HeartHandler heartHandler))
            heartHandler.Spawner = Object;

        Debug.Log($"Spawner Spawned Something");
        spawnAble = false;
        gameObject.SetActive(false);
    }
    public void SetTimer()
    {
        if(Runner != null && Object.HasStateAuthority)
        {
            respawnDelay = TickTimer.CreateFromSeconds(Runner, delayTime);
            spawnAble = true;
        }
    }

    /*
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_Despawn()
    {
        NetworkRunner networkRunner = FindObjectOfType<NetworkRunner>();
        if(networkRunner != null)
            networkRunner.Despawn(Object);
    }
    */
}
