using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.AI;
using System.Linq;

/// @brief 일정한 수만큼만 스폰하는 스포너.
public class CountSpawnHandler : SpawnHandler
{
    /// @brief 현재 카운트.
    [Networked]
    public int count{get; set;}

    /// @brief 목표 카운트.
    [Header("설정")]
    private int targetCount;

    /// @brief 스폰할 프리팹의 배열.
    /// @details 한 번에 여러 프리팹을 스폰할 수 있다.
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
        setSpawnbyPlayerCount();
        sphereCollider = GetComponent<SphereCollider>();
    }

    /// @brief 일정한 범위 내에 플레이어가 위치하면 동작하도록 함.
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

    /// @brief 스폰한다.
    protected override void Spawn()
    {
        foreach(NetworkBehaviour prefab in prefabs)
        {
            Debug.Log($"Count Spawner");
            Vector3 randomSpawnPoint = Utils.GetRandomSpawnPoint(anchorPoint.position, sphereCollider.radius * 1/2);
            NetworkBehaviour spawned = Runner.Spawn(prefab, randomSpawnPoint, Quaternion.LookRotation(UnityEngine.Random.insideUnitCircle.normalized),null, (runner, networkObject) => {
                if(networkObject.TryGetComponent<NavMeshAgent>(out NavMeshAgent navMeshAgent))
                    navMeshAgent.Warp(randomSpawnPoint);
            });

            if(spawned.TryGetComponent<EnemyHPHandler>(out EnemyHPHandler enemyHPHandler))
                enemyHPHandler.Spawner = Object;
        }

        respawnDelay = TickTimer.CreateFromSeconds(Runner, delayTime);
    }

    /// @brief 타이머를 설정한다. 
    /// @details 타이머를 설정하면 count가 증가한다. targetCount에 도달하면 게임 오브젝트에 포함된 MissionComplete의 자식 클래스를 실행한다.
    /// @see count, MissionComplete.OnMissionComplete()
    public override void SetTimer()
    {
        if(Runner != null && Object.HasStateAuthority)
        {
            count++;
            if(count >= targetCount)
                GetBehaviour<MissionComplete>().OnMissionComplete(Object);
        }
    }

    private void setSpawnbyPlayerCount()
    {
        int playerCount = Runner.ActivePlayers.Count();

        if (playerCount == 1)
        {
            targetCount = 10;
        }
        else if (playerCount == 2)
        {
            targetCount = 20;
        }
        else if (playerCount == 3)
        {
            targetCount = 30;
        }
        else if (playerCount == 4)
        {
            targetCount = 40;
        }
    }

}
