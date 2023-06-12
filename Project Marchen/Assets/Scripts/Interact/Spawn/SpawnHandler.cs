using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.AI;

public class SpawnHandler : NetworkBehaviour
{
    [Header("설정")]
    public float delayTime;
    public NetworkBehaviour prefab; //EnemyHPHandler, HeartHandler
    public Transform anchorPoint;
    protected bool spawnAble = true;
    public bool skipSettingStartValues = false;
    protected TickTimer respawnDelay = TickTimer.None;

    protected virtual void Start(){}
    protected virtual void OnTriggerStay (Collider other)
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
    protected virtual void Spawn()
    {
        NetworkBehaviour spawned = Runner.Spawn(prefab, anchorPoint.position, Quaternion.LookRotation(transform.forward),null, initSpawnPoint);

        if(spawned.TryGetComponent<EnemyHPHandler>(out EnemyHPHandler enemyHPHandler))
            enemyHPHandler.Spawner = Object;
        else if(spawned.TryGetComponent<HeartHandler>(out HeartHandler heartHandler))
            heartHandler.Spawner = Object;
        else if(spawned.TryGetComponent<PickUpAction>(out PickUpAction pickUpAction))
            pickUpAction.Spawner = Object;

        Debug.Log($"Spawner Spawned Something");
        spawnAble = false;
        gameObject.SetActive(false);
    }

    private void initSpawnPoint(NetworkRunner networkRunner, NetworkObject networkObject)
    {
        if(networkObject.TryGetComponent<NavMeshAgent>(out NavMeshAgent navMeshAgent))
            navMeshAgent.Warp(anchorPoint.position);
    }
    public virtual void SetTimer()
    {
        if(Runner != null && Object.HasStateAuthority)
        {
            respawnDelay = TickTimer.CreateFromSeconds(Runner, delayTime );
            spawnAble = true;
        }
    }

}
