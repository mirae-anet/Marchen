using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.AI;

public class CountSpawnHandler : SpawnHandler
{
    [Networked]
    public int count{get; set;}

    [Header("설정")]
    public int targetCount;
    public NetworkBehaviour[] prefabs; 
    SphereCollider sphereCollider; 

    protected override void Start()
    {
        if(!Object.HasStateAuthority)
            return;

        if(!skipSettingStartValues)
        {
            count = 0;
        }

        sphereCollider = GetComponent<SphereCollider>();
    }
    protected override void OnTriggerStay (Collider other)
    {
        if (other.tag != "Player")
            return;

        if(!Object.HasStateAuthority)
            return;

        if(count >= targetCount)
            return;

        if(respawnDelay.ExpiredOrNotRunning(Runner))
            Spawn();
    }
    protected override void Spawn()
    {
        foreach(NetworkBehaviour prefab in prefabs)
        {
            Debug.Log($"Count Spawner");
            Vector3 randomSpawnPoint = Utils.GetRandomSpawnPoint(anchorPoint.position, sphereCollider.radius * 2/3);
            NetworkBehaviour spawned = Runner.Spawn(prefab, randomSpawnPoint, Quaternion.LookRotation(UnityEngine.Random.insideUnitCircle.normalized),null, (runner, networkObject) => {
                if(networkObject.TryGetComponent<NavMeshAgent>(out NavMeshAgent navMeshAgent))
                    navMeshAgent.Warp(randomSpawnPoint);
            });

            if(spawned.TryGetComponent<EnemyHPHandler>(out EnemyHPHandler enemyHPHandler))
                enemyHPHandler.Spawner = Object;
        }

        respawnDelay = TickTimer.CreateFromSeconds(Runner, delayTime);
    }
    public override void SetTimer()
    {
        if(Runner != null && Object.HasStateAuthority)
        {
            count++;
            if(count >= targetCount)
                GetBehaviour<MissionComplete>().OnMissionComplete(Object);
        }
    }
}
