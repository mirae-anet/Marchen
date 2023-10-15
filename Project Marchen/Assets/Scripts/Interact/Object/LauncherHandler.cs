using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.AI;

/// @brief 스폰너
public class LauncherHandler : NetworkBehaviour
{
    [Header("설정")]
    /// @brief 생성 딜레이
    public float delayTime;
    /// @brief 생성할 프리팹
    public NetworkBehaviour prefab; //EnemyHPHandler, HeartHandler
    public Transform anchorPoint;
    public bool skipSettingStartValues = false;
    protected TickTimer respawnDelay = TickTimer.None;

    NetworkObject networkObject;

    private void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
    }

    protected virtual void Start(){}

    protected virtual void OnTriggerStay (Collider other)
    {
        if (other.tag != "Player")
            return;

        if(!Object.HasStateAuthority)
            return;

        if(respawnDelay.ExpiredOrNotRunning(Runner))
            Spawn();
    }

    /// @brief 스폰한다.
    protected virtual void Spawn()
    {
        Runner.Spawn(prefab, anchorPoint.position, Quaternion.LookRotation(transform.forward), Object.InputAuthority, (runner, spawnedBullet) =>
        {
            spawnedBullet.GetComponent<BulletHandler>().Fire(Object.InputAuthority, networkObject, "Launcher");
        });
        if(Runner != null && Object.HasStateAuthority)
            respawnDelay = TickTimer.CreateFromSeconds(Runner, delayTime );
    }
}